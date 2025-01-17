﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using BaseLibS.Mol;
using BaseLibS.Table;
using BaseLibS.Util;

namespace BaseLib.Forms{
	public partial class IsobaricLabelsParamControl : UserControl{
		private readonly IsobaricLabelingDefault[] defaults = {
			new IsobaricLabelingDefault("4plex iTRAQ",
				new[]{"iTRAQ4plex-Lys114", "iTRAQ4plex-Lys115", "iTRAQ4plex-Lys116", "iTRAQ4plex-Lys117"},
				new[]{"iTRAQ4plex-Nter114", "iTRAQ4plex-Nter115", "iTRAQ4plex-Nter116", "iTRAQ4plex-Nter117"}),
			new IsobaricLabelingDefault("8plex iTRAQ",
				new[]{
					"iTRAQ8plex-Lys113", "iTRAQ8plex-Lys114", "iTRAQ8plex-Lys115", "iTRAQ8plex-Lys116",
					"iTRAQ8plex-Lys117", "iTRAQ8plex-Lys118", "iTRAQ8plex-Lys119", "iTRAQ8plex-Lys121"
				},
				new[]{
					"iTRAQ8plex-Nter113", "iTRAQ8plex-Nter114", "iTRAQ8plex-Nter115", "iTRAQ8plex-Nter116",
					"iTRAQ8plex-Nter117", "iTRAQ8plex-Nter118", "iTRAQ8plex-Nter119", "iTRAQ8plex-Nter121"
				}),
			new IsobaricLabelingDefault("2plex TMT", new[]{"TMT2plex-Lys126", "TMT2plex-Lys127"},
				new[]{"TMT2plex-Nter126", "TMT2plex-Nter127"}),
			new IsobaricLabelingDefault("6plex TMT",
				new[]{
					"TMT6plex-Lys126", "TMT6plex-Lys127", "TMT6plex-Lys128", "TMT6plex-Lys129", "TMT6plex-Lys130",
					"TMT6plex-Lys131"
				},
				new[]{
					"TMT6plex-Nter126", "TMT6plex-Nter127", "TMT6plex-Nter128", "TMT6plex-Nter129", "TMT6plex-Nter130",
					"TMT6plex-Nter131"
				}),
			new IsobaricLabelingDefault("8plex TMT",
				new[]{
					"TMT8plex-Lys126C", "TMT8plex-Lys127N", "TMT8plex-Lys127C", "TMT8plex-Lys128C", "TMT8plex-Lys129N",
					"TMT8plex-Lys129C", "TMT8plex-Lys130C", "TMT8plex-Lys131N"
				},
				new[]{
					"TMT8plex-Nter126C", "TMT8plex-Nter127N", "TMT8plex-Nter127C", "TMT8plex-Nter128C",
					"TMT8plex-Nter129N", "TMT8plex-Nter129C", "TMT8plex-Nter130C", "TMT8plex-Nter131N"
				}),
			new IsobaricLabelingDefault("10plex TMT",
				new[]{
					"TMT10plex-Lys126C", "TMT10plex-Lys127N", "TMT10plex-Lys127C", "TMT10plex-Lys128N",
					"TMT10plex-Lys128C", "TMT10plex-Lys129N", "TMT10plex-Lys129C", "TMT10plex-Lys130N",
					"TMT10plex-Lys130C", "TMT10plex-Lys131N"
				},
				new[]{
					"TMT10plex-Nter126C", "TMT10plex-Nter127N", "TMT10plex-Nter127C", "TMT10plex-Nter128N",
					"TMT10plex-Nter128C", "TMT10plex-Nter129N", "TMT10plex-Nter129C", "TMT10plex-Nter130N",
					"TMT10plex-Nter130C", "TMT10plex-Nter131N"
				}),
			new IsobaricLabelingDefault("11plex TMT",
				new[]{
					"TMT10plex-Lys126C", "TMT10plex-Lys127N", "TMT10plex-Lys127C", "TMT10plex-Lys128N",
					"TMT10plex-Lys128C", "TMT10plex-Lys129N", "TMT10plex-Lys129C", "TMT10plex-Lys130N",
					"TMT10plex-Lys130C", "TMT10plex-Lys131N", "TMT11plex-Lys131C"
				},
				new[]{
					"TMT10plex-Nter126C", "TMT10plex-Nter127N", "TMT10plex-Nter127C", "TMT10plex-Nter128N",
					"TMT10plex-Nter128C", "TMT10plex-Nter129N", "TMT10plex-Nter129C", "TMT10plex-Nter130N",
					"TMT10plex-Nter130C", "TMT10plex-Nter131N", "TMT11plex-Nter131C"
				}),
			new IsobaricLabelingDefault("16plex TMTpro",
				new[]{
					"TMTpro16plex-Lys126C", "TMTpro16plex-Lys127N", "TMTpro16plex-Lys127C", "TMTpro16plex-Lys128N",
					"TMTpro16plex-Lys128C", "TMTpro16plex-Lys129N", "TMTpro16plex-Lys129C", "TMTpro16plex-Lys130N",
					"TMTpro16plex-Lys130C", "TMTpro16plex-Lys131N", "TMTpro16plex-Lys131C", "TMTpro16plex-Lys132N",
					"TMTpro16plex-Lys132C", "TMTpro16plex-Lys133N", "TMTpro16plex-Lys133C", "TMTpro16plex-Lys134N"
				},
				new[]{
					"TMTpro16plex-Nter126C", "TMTpro16plex-Nter127N", "TMTpro16plex-Nter127C", "TMTpro16plex-Nter128N",
					"TMTpro16plex-Nter128C", "TMTpro16plex-Nter129N", "TMTpro16plex-Nter129C", "TMTpro16plex-Nter130N",
					"TMTpro16plex-Nter130C", "TMTpro16plex-Nter131N", "TMTpro16plex-Nter131C", "TMTpro16plex-Nter132N",
					"TMTpro16plex-Nter132C", "TMTpro16plex-Nter133N", "TMTpro16plex-Nter133C", "TMTpro16plex-Nter134N"
				}),
			new IsobaricLabelingDefault("iodo6plexTMT",
				new[]{
					"iodoTMT6plex-Cys126", "iodoTMT6plex-Cys127", "iodoTMT6plex-Cys128", "iodoTMT6plex-Cys129",
					"iodoTMT6plex-Cys130", "iodoTMT6plex-Cys131"
				}, new string[]{ }),
		};
		private DataTable2 table;

		public IsobaricLabelsParamControl(){
			InitializeComponent();
			InitializeComponent2();
		}

		private static readonly string[] header = {
			"Internal label", "Terminal label", "Correction factor -2 [%]", "Correction factor -1 [%]",
			"Correction factor +1 [%]", "Correction factor +2 [%]", "TMT like"
		};

		private void AddButtonOnClick(object sender, EventArgs eventArgs){
			DataRow2 row = table.NewRow();
			row[header[0]] = "";
			row[header[1]] = "";
			row[header[2]] = 0d;
			row[header[3]] = 0d;
			row[header[4]] = 0d;
			row[header[5]] = 0d;
			row[header[6]] = true;
			table.AddRow(row);
			tableView1.Invalidate(true);
		}

		private void RemoveButtonOnClick(object sender, EventArgs eventArgs){
			int[] sel = tableView1.GetSelectedRows();
			if (sel.Length == 0){
				MessageBox.Show(Loc.PleaseSelectSomeRows);
				return;
			}
			table.RemoveRows(sel);
			tableView1.Invalidate(true);
		}

		private void ImportButtonOnClick(object sender, EventArgs eventArgs){
			OpenFileDialog ofd = new OpenFileDialog{
				Multiselect = false,
				Title = @"Open a isobaric label tab-separated file",
				FileName = @"Select a isobaric label tab-separated file",
				Filter = @"Text file (*.txt)|*.txt",
			};
			if (ofd.ShowDialog() == DialogResult.OK){
				ImportLabelFile(ofd.FileName);
			}
		}

		private void ImportLabelFile(string fileName){
			using (StreamReader sr = new StreamReader(fileName)){
				string line = sr.ReadLine();
				if (string.IsNullOrEmpty(line)) return;
				string[] hh = line.Split('\t');
				if (hh.Length != header.Length) return;
				for (int i = 0; i < header.Length; i++){
					if (hh[i] != header[i]) return;
				}
				List<string[]> buf = new List<string[]>();
				while (!string.IsNullOrEmpty(line = sr.ReadLine())){
					string[] ll = line.Split('\t');
					if (ll.Length != header.Length) return;
					buf.Add(ll);
				}
				if (buf.Count == 0) return;
				table.Clear();
				foreach (string[] b in buf){
					AddLabel(b[0], b[1], b[2], b[3], b[4], b[5], b[6]);
				}
			}
			tableView1.Invalidate(true);
		}

		private void ExportButtonOnClick(object sender, EventArgs eventArgs){
			SaveFileDialog sfd = new SaveFileDialog(){
				Title = @"Save a isobaric label tab-separated file", Filter = @"Text file (*.txt)|*.txt"
			};
			if (sfd.ShowDialog() == DialogResult.OK){
				if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
				ExportLabelFile(sfd.FileName);
			}
		}

		private void ExportLabelFile(string fileName){
			string[][] value = Value;
			using (StreamWriter sw = new StreamWriter(fileName)){
				sw.WriteLine(string.Join("\t", header));
				foreach (string[] t in value){
					sw.WriteLine(string.Join("\t", t));
				}
			}
		}

		private void EditButtonOnClick(object sender, EventArgs eventArgs){
			int[] sel = tableView1.GetSelectedRows();
			if (sel.Length != 1){
				MessageBox.Show("Please select exactly one row.");
				return;
			}
			DataRow2 row = table.GetRow(sel[0]);
			IsobaricLabelsEditForm f = new IsobaricLabelsEditForm(new IsobaricLabelInfo((string) row[0],
				(string) row[1], (double) row[2], (double) row[3], (double) row[4], (double) row[5], (bool) row[6]));
			f.ShowDialog();
			if (f.DialogResult != DialogResult.OK){
				return;
			}
			IsobaricLabelInfo info = f.Info;
			row[0] = info.internalLabel;
			row[1] = info.terminalLabel;
			row[2] = info.correctionFactorM2;
			row[3] = info.correctionFactorM1;
			row[4] = info.correctionFactorP1;
			row[5] = info.correctionFactorP2;
			row[6] = info.tmtLike;
			tableView1.Invalidate(true);
		}

		private void InitializeComponent2(){
			TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel();
			TableLayoutPanel tableLayoutPanel3 = new TableLayoutPanel();
			Button addButton = new Button();
			Button removeButton = new Button();
			Button editButton = new Button();
			Button importButton = new Button();
			Button exportButton = new Button();
			tableLayoutPanel2.SuspendLayout();
			tableLayoutPanel3.SuspendLayout();
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 1);
			int firstRowButtons = 2;
			int nbuttons2 = 5 + firstRowButtons;
			int nbuttons3 = defaults.Length - firstRowButtons;
			tableLayoutPanel2.ColumnCount = 2 * nbuttons2;
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
			for (int i = 0; i < nbuttons2 - 1; i++){
				tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
				tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
			}
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 78F));
			tableLayoutPanel2.Controls.Add(addButton, 0, 0);
			tableLayoutPanel2.Controls.Add(removeButton, 2, 0);
			tableLayoutPanel2.Controls.Add(editButton, 4, 0);
			tableLayoutPanel2.Controls.Add(importButton, 6, 0);
			tableLayoutPanel2.Controls.Add(exportButton, 8, 0);
			for (int i = 0; i < firstRowButtons; i++){
				tableLayoutPanel2.Controls.Add(CreateDefaultButton(defaults[i]), 10 + 2 * i, 0);
			}
			tableLayoutPanel3.ColumnCount = 2 * nbuttons3;
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
			for (int i = 0; i < nbuttons3 - 1; i++){
				tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
				tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
			}
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 78F));
			for (int i = firstRowButtons; i < defaults.Length; i++){
				tableLayoutPanel3.Controls.Add(CreateDefaultButton(defaults[i]), 2 * (i - firstRowButtons), 0);
			}
			tableLayoutPanel2.Dock = DockStyle.Fill;
			tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel2.Margin = new Padding(0);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 1;
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel2.Size = new System.Drawing.Size(2135, 50);
			tableLayoutPanel2.TabIndex = 2;
			tableLayoutPanel3.Dock = DockStyle.Fill;
			tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel3.Margin = new Padding(0);
			tableLayoutPanel3.Name = "tableLayoutPanel2";
			tableLayoutPanel3.RowCount = 1;
			tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel3.Size = new System.Drawing.Size(2135, 50);
			tableLayoutPanel3.TabIndex = 2;
			// 
			// addButton
			// 
			addButton.Dock = DockStyle.Fill;
			addButton.Location = new System.Drawing.Point(0, 0);
			addButton.Margin = new Padding(0);
			addButton.Name = "addButton";
			addButton.Size = new System.Drawing.Size(220, 50);
			addButton.TabIndex = 0;
			addButton.Text = @"Add";
			addButton.UseVisualStyleBackColor = true;
			// 
			// removeButton
			// 
			removeButton.Dock = DockStyle.Fill;
			removeButton.Location = new System.Drawing.Point(230, 0);
			removeButton.Margin = new Padding(0);
			removeButton.Name = "removeButton";
			removeButton.Size = new System.Drawing.Size(220, 50);
			removeButton.TabIndex = 1;
			removeButton.Text = @"Remove";
			removeButton.UseVisualStyleBackColor = true;
			// 
			// editButton
			// 
			editButton.Dock = DockStyle.Fill;
			editButton.Location = new System.Drawing.Point(460, 0);
			editButton.Margin = new Padding(0);
			editButton.Name = "editButton";
			editButton.Size = new System.Drawing.Size(220, 50);
			editButton.TabIndex = 1;
			editButton.Text = @"Edit";
			editButton.UseVisualStyleBackColor = true;
			// 
			// importButton
			// 
			importButton.Dock = DockStyle.Fill;
			importButton.Location = new System.Drawing.Point(690, 0);
			importButton.Margin = new Padding(0);
			importButton.Name = "importButton";
			importButton.Size = new System.Drawing.Size(220, 50);
			importButton.TabIndex = 0;
			importButton.Text = @"Import";
			importButton.UseVisualStyleBackColor = true;
			// 
			// exportButton
			// 
			exportButton.Dock = DockStyle.Fill;
			exportButton.Location = new System.Drawing.Point(920, 0);
			exportButton.Margin = new Padding(0);
			exportButton.Name = "exportButton";
			exportButton.Size = new System.Drawing.Size(220, 50);
			exportButton.TabIndex = 0;
			exportButton.Text = @"Export";
			exportButton.UseVisualStyleBackColor = true;
			tableLayoutPanel2.ResumeLayout(false);
			tableLayoutPanel3.ResumeLayout(false);
			tableView1.TableModel = CreateTable();
			addButton.Click += AddButtonOnClick;
			removeButton.Click += RemoveButtonOnClick;
			editButton.Click += EditButtonOnClick;
			importButton.Click += ImportButtonOnClick;
			exportButton.Click += ExportButtonOnClick;
		}

		private Control CreateDefaultButton(IsobaricLabelingDefault def){
			Button button = new Button{
				Dock = DockStyle.Fill,
				Location = new System.Drawing.Point(230, 0),
				Margin = new Padding(0),
				Name = "button",
				Size = new System.Drawing.Size(220, 50),
				TabIndex = 1,
				Text = def.Name,
				UseVisualStyleBackColor = true
			};
			button.Click += (sender, args) => { SetDefaults(def); };
			return button;
		}

		private void SetDefaults(IsobaricLabelingDefault def){
			table.Clear();
			for (int i = 0; i < def.Count; i++){
				DataRow2 row = table.NewRow();
				row[0] = def.GetInternalLabel(i);
				row[1] = def.GetTerminalLabel(i);
				row[2] = 0d;
				row[3] = 0d;
				row[4] = 0d;
				row[5] = 0d;
				row[6] = def.IsLikelyTmtLike(i);
				table.AddRow(row);
			}
			tableView1.Invalidate(true);
		}

		private DataTable2 CreateTable(){
			table = new DataTable2("isobaric labels table");
			table.AddColumn(header[0], 130, ColumnType.Text, "");
			table.AddColumn(header[1], 130, ColumnType.Text, "");
			table.AddColumn(header[2], 80, ColumnType.Numeric);
			table.AddColumn(header[3], 80, ColumnType.Numeric);
			table.AddColumn(header[4], 80, ColumnType.Numeric);
			table.AddColumn(header[5], 80, ColumnType.Numeric);
			table.AddColumn(header[6], 60, ColumnType.Boolean);
			return table;
		}

		public string[][] Value{
			get{
				string[][] result = new string[table.RowCount][];
				for (int i = 0; i < result.Length; i++){
					result[i] = new[]{
						(string) table.GetEntry(i, header[0]), (string) table.GetEntry(i, header[1]),
						((double) table.GetEntry(i, header[2])).ToString(CultureInfo.InvariantCulture),
						((double) table.GetEntry(i, header[3])).ToString(CultureInfo.InvariantCulture),
						((double) table.GetEntry(i, header[4])).ToString(CultureInfo.InvariantCulture),
						((double) table.GetEntry(i, header[5])).ToString(CultureInfo.InvariantCulture),
						((bool) table.GetEntry(i, header[6])).ToString()
					};
				}
				return result;
			}
			set{
				table.Clear();
				foreach (string[] t in value){
					AddLabel(t[0], t[1], t[2], t[3], t[4], t[5], t[6]);
				}
			}
		}

		private void AddLabel(string internalLabel, string terminalLabel, string correctionFactorM2,
			string correctionFactorM1, string correctionFactorP1, string correctionFactorP2, string tmtLike){
			DataRow2 row = table.NewRow();
			row[0] = internalLabel;
			row[1] = terminalLabel;
			row[2] = Parser.Double(correctionFactorM2);
			row[3] = Parser.Double(correctionFactorM1);
			row[4] = Parser.Double(correctionFactorP1);
			row[5] = Parser.Double(correctionFactorP2);
			row[6] = Parser.Bool(tmtLike);
			table.AddRow(row);
		}
	}
}