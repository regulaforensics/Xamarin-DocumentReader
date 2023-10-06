namespace DocumentReaderSample;

public partial class MainPage : ContentPage
{
    static readonly bool btDeviceSample = false;


    readonly IDocReaderScanner docReaderScanner;
    bool dbPrepareFinished = false;

    public MainPage()
	{
		InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        Application.Current.RequestedThemeChanged += (s, a) =>
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        };

        StartServiceButton.IsVisible = btDeviceSample;
        RecognizeButton.IsVisible = !btDeviceSample;
        BTDeviceName.IsVisible = btDeviceSample;

        IDocReaderInit docReaderInit = DependencyService.Get<IDocReaderInit>();
        docReaderInit.ScenariosObtained += (object sender, IDocReaderInitEvent e) =>
        {
            // Preparing database
            if (!dbPrepareFinished)
            {
                if (e.dbPrepared)
                {
                    dbPrepareFinished = true;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if(!btDeviceSample)
                            NamesLabels.Text = "Initializing...";
                        else
                            NamesLabels.Text = "Database prepared, now connect btDevice.";
                    });
                } else
                {
                    if(e.dbProgress >= 0)
                    {
                        NamesLabels.Text = "Downloading database: " + e.dbProgress + "%";
                        if(e.dbProgress == 100)
                        {
                            NamesLabels.Text = "Preparing database...";
                        }
                    }
                    else
                    {
                        NamesLabels.Text = "Database preparation failed";
                    }
                }
            } else
            // Initializing
            {
                if (e.IsSuccess)
                {
                    BindingContext = new MainViewModel(e.Scenarios);
                    RfidLayout.IsVisible = e.IsRfidAvailable;
                    NamesLabels.Text = "Ready";
                }
                else
                {
                    NamesLabels.Text = "Init failed";
                }
            }
        };

        NamesLabels.Text = "Loading...";

        docReaderInit.InitDocReader(btDeviceSample);

        docReaderScanner = DependencyService.Get<IDocReaderScanner>();

        docReaderScanner.ResultsObtained += (object s, IDocReaderScannerEvent e) =>
        {
            if (e.IsSuccess)
            {
                Console.WriteLine("Scan with results");
                ShowResults(e);
            }
            else
            {
                Console.WriteLine("Scan with error: " + e.Error);
            }
        };
    }

    void ShowScanner_Clicked(object sender, EventArgs evt)
    {

        if (IsScenarioSelected())
            docReaderScanner.ShowScanner(ReadRfidCb.IsChecked);
    }

    async void RecognizeImage_Clicked(object sender, EventArgs evt)
    {
        if (!IsScenarioSelected())
            return;

        (sender as Button).IsEnabled = false;

        Stream stream = await DependencyService.Get<IPhotoPickerService>().GetImageStreamAsync();
        if (stream != null)
        {
            NamesLabels.Text = "Recognize image...";
            docReaderScanner.RecognizeImage(stream);
        }

        (sender as Button).IsEnabled = true;
    }

    void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        Scenario scenario = e.SelectedItem as Scenario;
        docReaderScanner.SelectScenario(scenario.Name);
    }

    void ShowResults(IDocReaderScannerEvent e)
    {
        NamesLabels.Text = e.SurnameAndGivenNames;
        if (e.PortraitField != null)
        {
            Stream stream = new MemoryStream(e.PortraitField);
            PortraitImage.Source = ImageSource.FromStream(() => { return stream; });
        }
        if (e.DocumentField != null)
        {
            Stream stream = new MemoryStream(e.DocumentField);
            DocumentImage.Source = ImageSource.FromStream(() => { return stream; });
        }

    }

    bool IsScenarioSelected()
    {
        ClearResults();
        if (ScenariosListView.SelectedItem == null)
        {
            DisplayAlert("Error", "Scenario was not selected", "OK");
            return false;
        }

        return true;
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