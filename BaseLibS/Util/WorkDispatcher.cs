﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using QueuingSystem;
using QueuingSystem.Kubernetes;
using Action = QueuingSystem.Action;


namespace BaseLibS.Util {
	public abstract class WorkDispatcher {
		private const int initialDelay = 6;
		internal readonly int nTasks;
		private Thread[] workThreads;
		private Process[] externalProcesses;
		private string[] queuedJobIds;
		private Stack<int> toBeProcessed;
		internal readonly string infoFolder;
		internal readonly bool dotNetCore;
		internal readonly int numInternalThreads;
		private ISession _session;

		private const string ClusterTypeDrmaa = "drmaa";
		private const string ClusterTypeGeneric = "generic";
		private const string ClusterTypeKubernetes = "kubernetes";
		
		private static ISession GetSession()
		{
			var type = Environment.GetEnvironmentVariable("MQ_CLUSTER_TYPE") ?? ClusterTypeDrmaa;
			switch (type)
			{
				case ClusterTypeDrmaa:
				{
					var s = QueuingSystem.Drmaa.DrmaaSession.GetInstance();
					s.Init();
					var nativeSpec = Environment.GetEnvironmentVariable("MQ_DRMAA_NATIVE_SPEC");
					if (nativeSpec != null)
					{
						s.NativeSpecificationTemplate = nativeSpec;
					}
					return s;
				}
				case ClusterTypeGeneric:
				{
					var submitCommand = Environment.GetEnvironmentVariable("MQ_CLUSTER_SUBMIT_CMD");
					return new QueuingSystem.GenericCluster.GenericClusterSession(submitCommand);
				}
				case ClusterTypeKubernetes:
				{
					var ns = Environment.GetEnvironmentVariable("MQ_KUBERNETES_NAMESPACE") ?? "default";
					// TODO: default container
					var containerId = Environment.GetEnvironmentVariable("MQ_KUBERNETES_CONTAINER");
					// TODO: config
					return new KubernetesSession(ns, containerId);
				}
				default:
					throw new Exception($"Unknown queueing system type: {type}");
			}
		}
		
		protected WorkDispatcher(int nThreads, int nTasks, string infoFolder, CalculationType calculationType,
			bool dotNetCore) : this(nThreads, nTasks, infoFolder, calculationType, dotNetCore, 1)
		{
			
		}

		protected WorkDispatcher(int nThreads, int nTasks, string infoFolder, CalculationType calculationType,
			bool dotNetCore, int numInternalThreads) {
			Nthreads = Math.Min(nThreads, nTasks);
			this.numInternalThreads = numInternalThreads;
			this.nTasks = nTasks;
			this.infoFolder = infoFolder;
			this.dotNetCore = dotNetCore;
			if (!string.IsNullOrEmpty(infoFolder) && !Directory.Exists(infoFolder)) {
				Directory.CreateDirectory(infoFolder);
			}
			CalculationType = calculationType;
			
			// TODO: remove in release
			if (Environment.GetEnvironmentVariable("MQ_CALC_TYPE") == "queue")
			{
				CalculationType = CalculationType.Queueing;
				_session = GetSession();
				Console.WriteLine($"Using queueing session type: {_session}");
			}
			
		}

		public int MaxHeapSizeGb { get; set; } 

		public int Nthreads { get; }

		public void Abort() {
			if (workThreads != null) {
				foreach (Thread t in workThreads.Where(t => t != null)) {
					t.Abort();
				}
			}
			if (CalculationType == CalculationType.ExternalProcess && externalProcesses != null) {
				foreach (Process process in externalProcesses) {
					if (process != null && IsRunning(process)) {
						try {
							process.Kill();
						} catch (Exception) { }
					}
				}
			}

			if (CalculationType == CalculationType.Queueing && queuedJobIds != null)
			{
				foreach (string jobId in queuedJobIds)
				{
					try
					{
						_session.JobControl(jobId, Action.Terminate);
					}
					catch (QueuingSystemException ex)
					{
						// TODO: handle DrmaaExceptions
						Console.Error.WriteLine(ex.ToString());
					}
				}
				// TODO: move Session Init/Exit code to upper level
//				Session.Exit();
			}
		}

		public static bool IsRunning(Process process) {
			if (process == null) return false;
			try {
				Process.GetProcessById(process.Id);
			} catch (Exception) {
				return false;
			}
			return true;
		}

		public void Start()
		{			
			// TODO: remove in release, move Session.Init() to upper level  
//			if (CalculationType == CalculationType.Queueing)
//			{
//				_session.Init();
//			}
			toBeProcessed = new Stack<int>();
			for (int index = nTasks - 1; index >= 0; index--) {
				toBeProcessed.Push(index);
			}
			workThreads = new Thread[Nthreads];
			externalProcesses = new Process[Nthreads];
			queuedJobIds = new string[Nthreads];
			
			for (int i = 0; i < Nthreads; i++) {
				workThreads[i] = new Thread(Work) {Name = "Thread " + i + " of " + GetType().Name};
				workThreads[i].Start(i);
				Thread.Sleep(initialDelay);
			}
			while (true) {
				Thread.Sleep(1000);
				bool busy = false;
				for (int i = 0; i < Nthreads; i++) {
					if (workThreads[i].IsAlive) {
						busy = true;
						break;
					}
				}
				if (!busy) {
					break;
				}
			}
			// TODO: move Session Init/Exit code to upper level
//			if (CalculationType == CalculationType.Queueing)
//			{
//				Session.Exit();	
//			}
			
			// TODO: waiting for fs sync
			// TODO: remove in release
			string sleepTime = Environment.GetEnvironmentVariable("MQ_WORK_SLEEP");
			if (sleepTime != null)
			{
				Thread.Sleep(int.Parse(sleepTime));	
			}
			

			
		}

		public string GetMessagePrefix() {
			return MessagePrefix + " ";
		}

		public abstract void Calculation(string[] args, Responder responder);
		public virtual bool IsFallbackPosition => true;

		protected virtual string GetComment(int taskIndex) {
			return "";
		}

		protected abstract string Executable { get; }
		protected abstract string ExecutableCore { get; }
		protected abstract object[] GetArguments(int taskIndex);
		protected abstract int Id { get; }
		protected abstract string MessagePrefix { get; }

		private void Work(object threadIndex) {
			while (toBeProcessed.Count > 0) {
				int x;
				lock (this) {
					if (toBeProcessed.Count > 0) {
						x = toBeProcessed.Pop();
					} else {
						x = -1;
					}
				}
				if (x >= 0) {
					DoWork(x, (int) threadIndex);
				}
			}
		}

		private void DoWork(int taskIndex, int threadIndex) {
			switch (CalculationType) {
				case CalculationType.ExternalProcess:
					ProcessSingleRunExternalProcess(taskIndex, threadIndex);
					break;
				case CalculationType.Thread:
					Calculation(GetStringArgs(taskIndex), null);
					break;
				case CalculationType.Queueing:
					ProcessSingleRunQueueing(taskIndex, threadIndex, numInternalThreads);
					break;
			}
		}

		private IJobTemplate MakeJobTemplate(int taskIndex, int threadIndex, int numInternalThreads)
		{
			string cmd = GetCommandFilename().Trim('"');
			
			// TODO: 
			cmd = "/opt/MaxQuantCmd/" + Executable;
			// TODO: refactor to a function?
			List<string> args = new List<string>{"mono", "--optimize=all,float32", "--server", cmd};
			args.AddRange(GetLogArgs(taskIndex, taskIndex));
			args.Add(Id.ToString());
			args.AddRange(GetStringArgs(taskIndex));

			string jobName = $"{GetFilename()}_{taskIndex}_{threadIndex}";
			
			string randSuffix = Guid.NewGuid().ToString();
			
			// TODO: Separate folder for job stdout/stderr?
			string outPath = Path.Combine(infoFolder, $"{jobName}.{randSuffix}.out"); 
			// TODO: Separate folder for job stdout/stderr?
			string errPath = Path.Combine(infoFolder, $"{jobName}.{randSuffix}.err"); 
			
			// Copying parent environment
			Dictionary<string, string> env = new Dictionary<string, string>();
			foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
			{
				env[entry.Key.ToString()] = entry.Value.ToString();
			}

			IJobTemplate jobTemplate = _session.AllocateJobTemplate();						
			jobTemplate.Arguments = args.ToArray();
			jobTemplate.OutputPath = $":{outPath}";
			jobTemplate.ErrorPath = $":{errPath}";
			jobTemplate.JobEnvironment = env;
			jobTemplate.Threads = numInternalThreads;
			jobTemplate.JobName = jobName;
			return jobTemplate;
		}
		
		private void ProcessSingleRunQueueing(int taskIndex, int threadIndex, int numInternalThreads)
		{
			IJobTemplate drmaaJobTemplate = MakeJobTemplate(taskIndex, threadIndex, numInternalThreads);

			// TODO: non atomic operation. When Abortvalled: job submmited, but queuedJobIds[threadIndex] not filled yet
			string jobId = _session.Submit(drmaaJobTemplate);
			queuedJobIds[threadIndex] = jobId;
			
			// TODO: remove debug messages from future release
			Console.WriteLine($@"Created jobTemplate:
  parent command line args: {string.Join(", ", Environment.GetCommandLineArgs())}
  jobName:    {drmaaJobTemplate.JobName}
  args:       {string.Join(" ", drmaaJobTemplate.Arguments.Select(x => $"\"{x}\""))}
  outPath:    {drmaaJobTemplate.OutputPath}
  errPath:    {drmaaJobTemplate.ErrorPath}
  threads: {drmaaJobTemplate.Threads}
Submitted job {drmaaJobTemplate.JobName} with id: {jobId}
");

			try
			{
				var status = _session.WaitForJobBlocking(jobId);
				if (status != Status.Success)
				{
					Console.Error.WriteLine($"{drmaaJobTemplate.JobName}, jobId: {jobId}: \n"+drmaaJobTemplate.ReadStderr());
					throw new Exception(
						$"Exception during execution of external job: {drmaaJobTemplate.JobName}, jobId: {jobId}, status: {status}");
				}
				else
				{
					Console.WriteLine($"Job \"{drmaaJobTemplate.JobName}\" with id {jobId} finished successfully");
				}
			}
			finally
			{
				// TODO: Maybe introduce flag (cleanup or not, for debugging purposes)
				drmaaJobTemplate.Cleanup();
			}

		
		}

		private void ProcessSingleRunExternalProcess(int taskIndex, int threadIndex) {
			bool isUnix = FileUtils.IsUnix();
			string cmd = GetCommandFilename();
			string args = GetLogArgsString(taskIndex, taskIndex) + GetCommandArguments(taskIndex);
			ProcessStartInfo psi = IsRunningOnMono() && !dotNetCore
			                       // http://www.mono-project.com/docs/about-mono/releases/4.0.0/#floating-point-optimizations
				? new ProcessStartInfo("mono", " --optimize=all,float32 --server " + cmd + " " + args)
				: new ProcessStartInfo(cmd, args);
			if (isUnix) {
				psi.WorkingDirectory = Directory.GetDirectoryRoot(cmd);
				if (MaxHeapSizeGb > 0) {
					psi.EnvironmentVariables["MONO_GC_PARAMS"] = "max-heap-size=" + MaxHeapSizeGb + "g";
				}
			}
//			Console.WriteLine($"Process run: {cmd} {args}");
			psi.WindowStyle = ProcessWindowStyle.Hidden;
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			var externalProcess = new Process {StartInfo = psi};
			externalProcess.OutputDataReceived += (sender, eventArgs) => { Console.WriteLine(eventArgs.Data); };
			externalProcess.ErrorDataReceived += (sender, eventArgs) => { Console.Error.WriteLine(eventArgs.Data); };
			externalProcesses[threadIndex] = externalProcess;
			externalProcesses[threadIndex].Start();
			int processid = externalProcesses[threadIndex].Id;
			externalProcesses[threadIndex].WaitForExit();
			int exitcode = externalProcesses[threadIndex].ExitCode;
			externalProcesses[threadIndex].Close();
			if (exitcode != 0) {
				throw new Exception("Exception during execution of external process: " + processid);
			}
		}

		/// <summary>
		/// http://www.mono-project.com/docs/gui/winforms/porting-winforms-applications/
		/// </summary>
		private static bool IsRunningOnMono() => Type.GetType("Mono.Runtime") != null;

		private string GetName(int taskIndex) {
			return GetFilename() + " (" + IntString(taskIndex + 1, nTasks) + "/" + nTasks + ")";
		}

		private static string IntString(int x, int n) {
			int npos = (int) Math.Ceiling(Math.Log10(n));
			string result = "" + x;
			if (result.Length >= npos) {
				return result;
			}
			return Repeat(npos - result.Length, "0") + result;
		}

		private static string Repeat(int n, string s) {
			StringBuilder b = new StringBuilder();
			for (int i = 0; i < n; i++) {
				b.Append(s);
			}
			return b.ToString();
		}
		
		private string[] GetLogArgs(int taskIndex, int id)
		{
			
			return new[]
			{
				infoFolder, GetFilename(), id.ToString(), GetName(taskIndex), GetComment(taskIndex), "Process",
			};
		}
		
		private string GetLogArgsString(int taskIndex, int id)
		{
			return string.Join(" ", GetLogArgs(taskIndex, id).Select(x => $"\"{x}\""))+" ";
		}

		private string GetFilename() {
			return GetMessagePrefix().Trim().Replace("/", "").Replace("(", "_").Replace(")", "_").Replace(" ", "_");
		}

		private string GetCommandFilename() {
			return "\"" + FileUtils.executablePath + Path.DirectorySeparatorChar + (dotNetCore ? ExecutableCore : Executable) +
			       "\"";
		}

		private CalculationType CalculationType { get; }

		private string GetCommandArguments(int taskIndex) {
			object[] o = GetArguments(taskIndex);
			string[] args = new string[o.Length + 1];
			args[0] = $"\"{Id}\"";
			for (int i = 0; i < o.Length; i++) {
				object o1 = o[i];
				string s = Parser.ToString(o1);
				args[i + 1] = "\"" + s + "\"";
			}
			return StringUtils.Concat(" ", args);
		}

		private string[] GetStringArgs(int taskIndex) {
			object[] o = GetArguments(taskIndex);
			string[] args = new string[o.Length];
			for (int i = 0; i < o.Length; i++) {
				args[i] = $"{o[i]}";
			}
			return args;
		}
	}
}