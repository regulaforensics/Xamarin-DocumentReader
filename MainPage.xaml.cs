namespace DocumentReaderSample;

public partial class MainPage : ContentPage
{
    static readonly bool btDeviceSample = false;
    readonly IDocReaderScanner docReaderScanner;
    static readonly List<string> Scenarios = [];
    public MainPage()
    {
        InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        Application.Current.RequestedThemeChanged += (s, a) => { Application.Current.UserAppTheme = AppTheme.Light; };

        // Fix disappearing selection in iOS
        Loaded += (sender, e) =>
        {
            if (Scenarios.Count > 0)
                ScenariosListView.UpdateSelectedItems([ScenariosListView.SelectedItem]);
        };

        docReaderScanner = DependencyService.Get<IDocReaderScanner>();
        docReaderScanner.ResultsObtained += (_, result) =>
        {
            NamesLabels.Text = result.SurnameAndGivenNames;
            if (result.PortraitField != null)
                PortraitImage.Source = ImageSource.FromStream(() => { return new MemoryStream(result.PortraitField); });
            if (result.DocumentField != null)
                DocumentImage.Source = ImageSource.FromStream(() => { return new MemoryStream(result.DocumentField); });
        };

        IDocReaderInit docReaderInit = DependencyService.Get<IDocReaderInit>();
        docReaderInit.ScenariosObtained += (sender, e) =>
        {
            if (e.IsSuccess)
            {
                foreach (Scenario scenario in e.Scenarios) { Scenarios.Add(scenario.Name); }
                ScenariosListView.ItemsSource = Scenarios;
                ScenariosListView.SelectedItem = Scenarios[0];
                ScenariosListView.SelectionChanged += (sender, e) =>
                {
                    string scenario = e.CurrentSelection.FirstOrDefault() as string;
                    docReaderScanner.SelectScenario(scenario);
                };
                RfidLayout.IsVisible = e.IsRfidAvailable;
                NamesLabels.Text = "Ready";
            }
            else NamesLabels.Text = "Init failed";
        };

        if (btDeviceSample)
        {
            NamesLabels.Text = "Connect btDevice.";
            StartServiceButton.IsVisible = true;
            BTDeviceName.IsVisible = true;
            RecognizeButton.IsVisible = false;
            return;
        }

        NamesLabels.Text = "Initializing...";
        docReaderInit.InitDocReader();
    }
    void ShowScanner_Clicked(object sender, EventArgs evt)
    {
        ClearResults();
        docReaderScanner.ShowScanner(ReadRfidCb.IsChecked);
    }
    async void RecognizeImage_Clicked(object sender, EventArgs evt)
    {
        ClearResults();
        Stream stream = await DependencyService.Get<IPhotoPickerService>().GetImageStreamAsync();
        if (stream != null)
        {
            NamesLabels.Text = "Recognize image...";
            docReaderScanner.RecognizeImage(stream, ReadRfidCb.IsChecked);
        }
        (sender as Button).IsEnabled = true;
    }
    void ClearResults()
    {
        NamesLabels.Text = "";
        PortraitImage.Source = "mainpage_portrait_icon.png";
        DocumentImage.Source = "mainpage_id_icon.png";
    }
    void StartService_Clicked(object sender, EventArgs evt)
    {
        IDocReaderInit docReaderInit = DependencyService.Get<IDocReaderInit>();
        docReaderInit.CheckPermissionsAndConnect(BTDeviceName.Text);
    }
}