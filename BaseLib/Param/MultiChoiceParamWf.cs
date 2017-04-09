using System;
using BaseLibC.Forms;
using BaseLibS.Param;

namespace BaseLibC.Param{
	[Serializable]
	internal class MultiChoiceParamWf : MultiChoiceParam{
		[NonSerialized] private ListSelectorControl control;
		internal MultiChoiceParamWf(string name) : base(name){}
		internal MultiChoiceParamWf(string name, int[] value) : base(name, value){}
		public override ParamType Type => ParamType.WinForms;

		public override void SetValueFromControl(){
			Value = control.SelectedIndices;
		}

		public override void UpdateControlFromValue(){
			if (control == null){
				return;
			}
			control.SelectedIndices = Value;
		}

		public override object CreateControl(){
			control = new ListSelectorControl{HasMoveButtons = true};
			foreach (string value in Values){
				control.Items.Add(value);
			}
			control.Repeats = Repeats;
			control.SelectedIndices = Value;
			control.SetDefaultSelectors(DefaultSelectionNames, DefaultSelections);
			return control;
		}
	}
}