using System.Collections.ObjectModel;

namespace DocumentReaderSample
{
    public class MainViewModel
    {
        public ObservableCollection<Scenario> Scenarios { get; set; } = [];
        public MainViewModel(IList<Scenario> scenarios)
        {
            foreach (Scenario scenario in scenarios)
                Scenarios.Add(scenario);
        }
    }
}
