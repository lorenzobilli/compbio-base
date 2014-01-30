﻿using System.Collections.Generic;
using System.Windows;
using BaseLib.Param;

namespace BaseLib.Wpf{
	/// <summary>
	/// Interaction logic for DictionaryIntValuePopup.xaml
	/// </summary>
	public partial class DictionaryIntValuePopup : Window{
		public DictionaryIntValuePopup(){
			InitializeComponent();
		}

		internal void SetData(Dictionary<string, int> v, string[] keys, int d){
			Parameter[] p = new Parameter[keys.Length];
			for (int i = 0; i < p.Length; i++){
				p[i] = new IntParam(keys[i], v.ContainsKey(keys[i]) ? v[keys[i]] : d);
			}
			ParameterPanel.Init(new Parameters(p));
		}

		internal Dictionary<string, int> GetData(string[] keys){
			Dictionary<string, int> result = new Dictionary<string, int>();
			foreach (string key in keys){
				int y = ParameterPanel.Parameters.GetIntParam(key).Value;
				result.Add(key, y);
			}
			return result;
		}

		private void CancelButton_OnClick(object sender, RoutedEventArgs e){
			Close();
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e){
			DialogResult = true;
			Close();
		}
	}
}