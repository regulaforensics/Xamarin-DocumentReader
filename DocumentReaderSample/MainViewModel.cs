using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DocumentReaderSample
{
    public class MainViewModel
    {
        public ObservableCollection<Scenario> Scenarios { get; set; } = new ObservableCollection<Scenario>();

        public MainViewModel(IList<Scenario> scenarios)
        {
            foreach (Scenario scenario in scenarios)
            {
                Console.WriteLine("Name: " + scenario.Name + ", description: " + scenario.Description);
                Scenarios.Add(scenario);
            }
        }
    }
}
