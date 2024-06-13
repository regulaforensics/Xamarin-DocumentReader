namespace DocumentReaderSample;

public partial class MainPage : ContentPage
{
    static readonly bool btDeviceSample = false;
    readonly IDocReaderScanner docReaderScanner;
    public MainPage()
    {
        InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        Application.Current.RequestedThemeChanged += (s, a) => { Application.Current.UserAppTheme = AppTheme.Light; };

        docReaderScanner = DependencyService.Get<IDocReaderScanner>();
        docReaderScanner.ResultsObtained += (object s, IDocReaderScannerEvent e) =>
        {
            NamesLabels.Text = e.SurnameAndGivenNames;
            if (e.PortraitField != null)
                PortraitImage.Source = ImageSource.FromStream(() => { return new MemoryStream(e.PortraitField); });
            if (e.DocumentField != null)
                DocumentImage.Source = ImageSource.FromStream(() => { return new MemoryStream(e.DocumentField); });
        };

        if (btDeviceSample)
        {
            NamesLabels.Text = "Connect btDevice.";
            StartServiceButton.IsVisible = true;
            BTDeviceName.IsVisible = true;
            RecognizeButton.IsVisible = false;
            return;
        }

        IDocReaderInit docReaderInit = DependencyService.Get<IDocReaderInit>();
        docReaderInit.ScenariosObtained += (object sender, IDocReaderInitEvent e) =>
        {
            if (e.IsSuccess)
            {
                BindingContext = new MainViewModel(e.Scenarios);
                ScenariosListView.SelectedItem = e.Scenarios[0];
                RfidLayout.IsVisible = e.IsRfidAvailable;
                NamesLabels.Text = "Ready";
            }
            else NamesLabels.Text = "Init failed";
        };
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
        (sender as Button).IsEnabled = false;
        Stream stream = await DependencyService.Get<IPhotoPickerService>().GetImageStreamAsync();
        if (stream != null)
        {
            NamesLabels.Text = "Recognize image...";
            docReaderScanner.RecognizeImage(stream, ReadRfidCb.IsChecked);
        }
        (sender as Button).IsEnabled = true;
    }
    void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        Scenario scenario = e.SelectedItem as Scenario;
        docReaderScanner.SelectScenario(scenario.Name);
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