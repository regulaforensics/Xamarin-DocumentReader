using System;
using System.Collections.Generic;
using DocReaderApi.iOS;
using UIKit;

namespace DocumentReaderSample.iOS
{
    public class ScenarioPickerModel: UIPickerViewModel
    {
        private List<Scenario> scenarios;
        public EventHandler ValueChanged;
        public string SelectedValue;
        public ScenarioPickerModel(Scenario[] scenarios)
        {
            this.scenarios = new List<Scenario>(scenarios);
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component) {  
            return scenarios.Count;  
        } 

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component) {  
            return scenarios[(int) row].Identifier;  
        } 

        public override void Selected(UIPickerView pickerView, nint row, nint component) {  
            var scenario = scenarios[(int) row].Identifier;  
            SelectedValue = scenario;  
            ValueChanged ? .Invoke(null, null);  
        }
    }
}
