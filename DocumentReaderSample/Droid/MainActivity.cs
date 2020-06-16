using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Com.Regula.Documentreader.Api;
using System.IO;
using Android.Content;
using Com.Regula.Documentreader.Api.Results;
using System.Collections.Generic;
using static Android.Widget.AdapterView;
using Com.Regula.Documentreader.Api.Enums;
using Com.Regula.Documentreader.Api.Params;
using Android.Util;
using Android.Graphics;
using Android;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Com.Regula.Documentreader.Api.Completions;

namespace DocumentReaderSample.Droid
{
    [Activity(Label = "DocumentReaderSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FragmentActivity, IDocumentReaderInitCompletion, IDocumentReaderCompletion, IDocumentReaderPrepareCompletion
    {
        const int REQUEST_BROWSE_PICTURE = 11;
        const int PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE = 22;
        const String MY_SHARED_PREFS = "MySharedPrefs";
        const String DO_RFID = "doRfid";

        public TextView nameTv;
        public TextView showScanner;
        private TextView recognizeImage;

        private ImageView portraitIv;
        private ImageView docImageIv;

        private CheckBox doRfidCb;

        private ListView scenarioLv;

        private ISharedPreferences sharedPreferences;
        private Boolean doRfid;
        private Boolean isStartRfid;

        AlertDialog initDialog;
        AlertDialog updateDBDialog;
        AlertDialog loadingDialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);

            //global::Xamarin.Forms.Forms.Init(this, bundle);
            //LoadApplication(new App());

            nameTv = FindViewById<TextView>(Resource.Id.nameTv);
            showScanner = FindViewById<TextView>(Resource.Id.showScannerLink);
            recognizeImage = FindViewById<TextView>(Resource.Id.recognizeImageLink);

            portraitIv = FindViewById<ImageView>(Resource.Id.portraitIv);
            docImageIv = FindViewById<ImageView>(Resource.Id.documentImageIv);

            scenarioLv = FindViewById<ListView>(Resource.Id.scenariosList);

            doRfidCb = FindViewById<CheckBox>(Resource.Id.doRfidCb);

            sharedPreferences = GetSharedPreferences(MY_SHARED_PREFS, FileCreationMode.Private);

            updateDBDialog = showDialog("Updating DB");
            DocumentReader.Instance().PrepareDatabase(this, "Full", this);
        }

        protected void initReader()
        {
            var bytes = default(byte[]);
            using (var streamReader = new StreamReader(Assets.Open("regula.license")))
            {
                using (var memstream = new MemoryStream())
                {
                    streamReader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                }
            }

            initDialog = showDialog("Initializing");
            DocumentReader.Instance().InitializeReader(this, bytes, this);
        }


        protected AlertDialog showDialog(String msg)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            View dialogView = LayoutInflater.Inflate(Resource.Layout.simple_dialog, null);
            dialog.SetTitle(msg);
            dialog.SetView(dialogView);
            dialog.SetCancelable(false);
            return dialog.Show();
        }

        public void СlearResults()
        {
            nameTv.Text = "";
            portraitIv.SetImageResource(Resource.Drawable.portrait);
            docImageIv.SetImageResource(Resource.Drawable.id);
        }

        public void DisplayResults(DocumentReaderResults results)
        {
            if (results != null)
            {
                var name = results.GetTextFieldValueByType(EVisualFieldType.FtSurnameAndGivenNames);
                if (!String.IsNullOrEmpty(name))
                {
                    nameTv.Text = name;
                }

                // through all text fields
                if (results.TextResult != null && results.TextResult.Fields != null)
                {
                    foreach (DocumentReaderTextField textField in results.TextResult.Fields)
                    {
                        var value = results.GetTextFieldValueByType(textField.FieldType, textField.Lcid);
                        Log.Debug("MainActivity", value + "\n");
                    }
                }

                using (var portraitImage = results.GetGraphicFieldImageByType(EGraphicFieldType.GfPortrait))
                {
                    if (portraitImage != null)
                        portraitIv.SetImageBitmap(portraitImage);
                }


                using (var documentImage = results.GetGraphicFieldImageByType(EGraphicFieldType.GfDocumentImage))
                {
                    if (documentImage != null)
                        docImageIv.SetImageBitmap(documentImage);
                }

            }
        }

        // creates and starts image browsing intent
        // results will be handled in onActivityResult method
        private void CreateImageBrowsingRequest()
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.PutExtra(Intent.ExtraAllowMultiple, true);
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), REQUEST_BROWSE_PICTURE);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE)
            {
                // If request is cancelled, the result arrays are empty.
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    //access to gallery is allowed
                    CreateImageBrowsingRequest();
                }
                else
                {
                    Toast.MakeText(this, "Permission required, to browse images", ToastLength.Long).Show();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
                //Image browsing intent processed successfully
                if (requestCode == REQUEST_BROWSE_PICTURE && data.Data != null)
                {
                    var selectedImage = data.Data;
                    var bmp = GetBitmap(selectedImage, 1920, 1080);

                    loadingDialog = showDialog("Processing image");

                    DocumentReader.Instance().StopScanner(this);
                    DocumentReader.Instance().RecognizeImage(bmp, this);
                }
            }
        }

        public void OnInitCompleted(bool success, string error)
        {
            if (initDialog != null && initDialog.IsShowing)
            {
                initDialog.Dismiss();
            }
            //initialization successful
            if (success)
            {
                showScanner.Click += delegate {
                    СlearResults();

                    //starting video processing
                    DocumentReader.Instance().StopScanner(this);
                    DocumentReader.Instance().ShowScanner(this, this);
                };

                recognizeImage.Click += delegate {
                    СlearResults();

                    //checking for image browsing permissions
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE);
                    }
                    else
                    {
                        //start image browsing
                        CreateImageBrowsingRequest();
                    }
                };

                if (DocumentReader.Instance().IsRFIDAvailableForUse)
                {
                    doRfid = sharedPreferences.GetBoolean(DO_RFID, false);
                    doRfidCb.Checked = doRfid;
                    doRfidCb.CheckedChange += delegate {
                        doRfid = doRfidCb.Checked;
                        sharedPreferences.Edit().PutBoolean(DO_RFID, doRfid).Apply();
                    };
                }
                else
                {
                    doRfidCb.Visibility = ViewStates.Gone;
                }

                //getting current processing scenario and loading available scenarios to ListView
                var currentScenario = DocumentReader.Instance().ProcessParams().Scenario;
                var scenarios = new List<String>();
                foreach (DocumentReaderScenario scenario in DocumentReader.Instance().AvailableScenarios)
                {
                    scenarios.Add(scenario.Name);
                }

                //setting default scenario
                if (String.IsNullOrEmpty(currentScenario))
                {
                    currentScenario = scenarios[0];
                }

                DocumentReader.Instance().ProcessParams().Scenario = currentScenario;

                ScenarioAdapter adapter = new ScenarioAdapter(this, Android.Resource.Layout.SimpleListItem1, scenarios);
                adapter.SelectedPosition = adapter.GetPosition(currentScenario);
                scenarioLv.Adapter = adapter;
                scenarioLv.SetSelection(adapter.SelectedPosition);
                scenarioLv.ItemClick += (object sender, ItemClickEventArgs e) =>
                {
                    DocumentReader.Instance().ProcessParams().Scenario = scenarios[e.Position];
                    adapter.SelectedPosition = e.Position;
                    adapter.NotifyDataSetChanged();
                };
            }
            //Initialization was not successful
            else
            {
                Toast.MakeText(initDialog.Context, "Init failed:" + error, ToastLength.Long).Show();
            }

        }

        public void OnCompleted(int action, DocumentReaderResults results, string error)
        {
            if (action == DocReaderAction.Complete)
            {
                if (loadingDialog != null && loadingDialog.IsShowing)
                {
                    loadingDialog.Dismiss();
                }

                //Checking, if nfc chip reading should be performed
                if (!isStartRfid && doRfid && results != null && results.ChipPage != 0)
                {
                    //setting the chip's access key - mrz on car access number
                    string accessKey = null;
                    if ((accessKey = results.GetTextFieldValueByType(EVisualFieldType.FtMrzStringsIcaoRfid)) != null && !String.IsNullOrEmpty(accessKey))
                    {
                        accessKey = results.GetTextFieldValueByType(EVisualFieldType.FtMrzStringsIcaoRfid).Replace("^", "").Replace("\n", "");
                        DocumentReader.Instance().RfidScenario().Mrz = accessKey;
                        DocumentReader.Instance().RfidScenario().PacePasswordType = ERFID_Password_Type.PptMrz;
                    }
                    else if ((accessKey = results.GetTextFieldValueByType(EVisualFieldType.FtMrzStrings)) != null && !String.IsNullOrEmpty(accessKey))
                    {
                        accessKey = results.GetTextFieldValueByType(EVisualFieldType.FtMrzStrings).Replace("^", "").Replace("\n", "");
                        DocumentReader.Instance().RfidScenario().Mrz = accessKey;
                        DocumentReader.Instance().RfidScenario().PacePasswordType = ERFID_Password_Type.PptMrz;
                    }
                    else if ((accessKey = results.GetTextFieldValueByType(EVisualFieldType.FtCardAccessNumber)) != null && !String.IsNullOrEmpty(accessKey))
                    {
                        DocumentReader.Instance().RfidScenario().Password = accessKey;
                        DocumentReader.Instance().RfidScenario().PacePasswordType = ERFID_Password_Type.PptCan;
                    }

                    //starting chip reading
                    DocumentReader.Instance().StartRFIDReader(this, this);
                    isStartRfid = true;
                }
                else
                {
                    DisplayResults(results);
                    isStartRfid = false;
                }
            }
            else
            {
                //something happened before all results were ready
                if (action == DocReaderAction.Cancel)
                {
                    Toast.MakeText(this, "Scanning was cancelled", ToastLength.Long).Show();
                    isStartRfid = false;
                }
                else if (action == DocReaderAction.Error)
                {
                    Toast.MakeText(this, "Error:" + error, ToastLength.Long).Show();
                    isStartRfid = false;
                }
            }
        }

        public void OnPrepareCompleted(bool status, String error1)
        {
            if (updateDBDialog != null && updateDBDialog.IsShowing)
            {
                updateDBDialog.Dismiss();
            }

            initReader();
        }

        public void OnPrepareProgressChanged(int progress)
        {
            Console.WriteLine("Progress: " + progress + "%");
        }

        private Bitmap GetBitmap(Android.Net.Uri selectedImage, int targetWidth, int targetHeight)
        {
            var inputStream = ContentResolver.OpenInputStream(selectedImage);
            var options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeStream(inputStream, null, options);

            //Re-reading the input stream to move it's pointer to start
            inputStream = ContentResolver.OpenInputStream(selectedImage);
            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, targetWidth, targetHeight);
            options.InPreferredConfig = Bitmap.Config.Argb8888;
            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;

            return BitmapFactory.DecodeStream(inputStream, null, options);
        }

        // see https://developer.android.com/topic/performance/graphics/load-bitmap.html
        private int CalculateInSampleSize(BitmapFactory.Options options, int bitmapWidth, int bitmapHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > bitmapHeight || width > bitmapWidth)
            {

                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) > bitmapHeight
                        && (halfWidth / inSampleSize) > bitmapWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        class ScenarioAdapter : ArrayAdapter<String>
        {
            public int SelectedPosition
            {
                get; set;
            }
            public ScenarioAdapter(Context context, int textViewResourceId, IList<string> objects) : base(context, textViewResourceId, objects)
            {
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = base.GetView(position, convertView, parent);
                view.SetBackgroundColor(position == SelectedPosition ? Android.Graphics.Color.LightGray : Android.Graphics.Color.Transparent);
                return view;
            }
        }
    }
}

