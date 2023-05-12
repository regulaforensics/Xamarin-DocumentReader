using System;
using System.ComponentModel;
using System.IO;

namespace DocumentReaderSample;

public partial class MainPage : ContentPage
{
    IDocReaderScanner docReaderScanner;

    public MainPage()
	{
		InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        Application.Current.RequestedThemeChanged += (s, a) =>
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        };

        IDocReaderInit docReaderInit = DependencyService.Get<IDocReaderInit>();
        docReaderInit.ScenariosObtained += (object sender, IDocReaderInitEvent e) =>
        {
            if (e.IsSuccess)
            {
                BindingContext = new MainViewModel(e.Scenarios);
                RfidLayout.IsVisible = e.IsRfidAvailable;
                NamesLabels.Text = "";
            }
            else
            {
                DisplayAlert("Error", "Initialization failed", "OK");
            }
        };
        docReaderInit.InitDocReader();

        NamesLabels.Text = "Initialization Document Reader...";

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

    void ShowScanner_Clicked(System.Object sender, System.EventArgs evt)
    {

        if (IsScenarioSelected())
            docReaderScanner.ShowScanner(ReadRfidCb.IsChecked);
    }

    async void RecognizeImage_Clicked(System.Object sender, System.EventArgs evt)
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

    void ListView_ItemSelected(System.Object sender, SelectedItemChangedEventArgs e)
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
        PortraitImage.Source = "portrait.png";
        DocumentImage.Source = "id.png";
    }
}