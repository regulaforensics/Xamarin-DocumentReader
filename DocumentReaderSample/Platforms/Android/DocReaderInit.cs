using Android.Bluetooth;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Com.Regula.Documentreader.Api;
using Com.Regula.Documentreader.Api.Ble;
using Com.Regula.Documentreader.Api.Completions;
using Com.Regula.Documentreader.Api.Errors;
using Com.Regula.Documentreader.Api.Internal.Permission;
using Com.Regula.Documentreader.Api.Params;
using Com.Regula.Documentreader.Api.Results;
using Android.App;
using Android.Content.PM;
using Android;
using Android.Provider;

namespace DocumentReaderSample.Platforms.Android
{
    public class DocReaderInitEvent : EventArgs, IDocReaderInitEvent
    {
        public IList<Scenario> Scenarios { get; set; }

        public bool IsSuccess { get; set; }

        public bool IsRfidAvailable { get; set; }

        public bool dbPrepared { get; set; }

        public int dbProgress { get; set; }
    }

    public class DocReaderInit : BleWrapperCallback, IDocReaderInit, IDocumentReaderInitCompletion, IDocumentReaderPrepareCompletion, IServiceConnection
    {
        public DocReaderInit()
        {
        }

        public event EventHandler<IDocReaderInitEvent> ScenariosObtained;

        public void InitDocReader(bool btDevice = false)
        {
            btDeviceSample = btDevice;
            if (btDeviceSample)
                DocumentReader.Instance().PrepareDatabase(Platform.AppContext, "FullAuth", this);
            else
                DocumentReader.Instance().PrepareDatabase(Platform.AppContext, "Full", this);
        }

        protected void InitReader()
        {
            var bytes = default(byte[]);
            using (var streamReader = new StreamReader(Platform.AppContext.Assets.Open("regula.license")))
            {
                using var memstream = new MemoryStream();
                streamReader.BaseStream.CopyTo(memstream);
                bytes = memstream.ToArray();
            }

            DocReaderConfig config = new(bytes)
            {
                DelayedNNLoad = true
            };
            DocumentReader.Instance().InitializeReader(Platform.AppContext, config, this);
        }

        //Document Reader Completions
        public void OnInitCompleted(bool success, DocumentReaderException error)
        {
            DocReaderInitEvent readerInitEvent = new() { IsSuccess = success };
            if (success)
            {
                Console.WriteLine("Initialized sucessfully");

                IList<Scenario> data = new List<Scenario>();
                foreach (DocumentReaderScenario scenario in DocumentReader.Instance().AvailableScenarios)
                {
                    var scenarion = new Scenario() { Name = scenario.Name, Description = scenario.Description };
                    data.Add(scenarion);
                }

                readerInitEvent.Scenarios = data;
                readerInitEvent.IsRfidAvailable = DocumentReader.Instance().IsRFIDAvailableForUse;
            }
            else
            {
                Console.WriteLine("Initialization failed:" + error);
            }

            ScenariosObtained(this, readerInitEvent);
        }

        void IDocumentReaderPrepareCompletion.OnPrepareCompleted(bool status, DocumentReaderException error)
        {
            DocReaderInitEvent readerInitEvent = new() { dbPrepared = status };
            if (status)
            {
                if(!btDeviceSample)
                    InitReader();
            }
            else
                readerInitEvent.dbProgress = -1;

            ScenariosObtained(this, readerInitEvent);
        }

        void IDocumentReaderPrepareCompletion.OnPrepareProgressChanged(int progress)
        {
            DocReaderInitEvent readerInitEvent = new() { dbProgress = progress };
            ScenariosObtained(this, readerInitEvent);
        }



        // BTDeviceSample

        static bool btDeviceSample = false;

        static readonly int REQUEST_ENABLE_LOCATION = 196;
        static readonly int REQUEST_ENABLE_BT = 197;

        static bool isBleServiceConnected = false;
        static BLEWrapper bleManager = null;

        public void CheckPermissionsAndConnect(string btDeviceName)
        {
            if (IsBlePermissionGranted())
            {
                Console.WriteLine("Loading...");
                if (!isBleServiceConnected)
                {
                    Console.WriteLine("connecting bleService");
                    DocumentReader.Instance().Functionality().Edit()
                        .SetUseAuthenticator(true)
                        .SetBtDeviceName(btDeviceName)
                        .Apply();
                    StartBluetoothService(Platform.CurrentActivity);
                }
                else
                    Console.WriteLine("bleService already connected");
            }
            else
            {
                Console.WriteLine("ble permissions denied");
            }
        }

        public static bool IsBlePermissionGranted()
        {
            if (!BluetoothSettingsHelper.IsLocationServiceEnabled(Platform.CurrentActivity))
            {
                RequestEnableLocationService(Platform.CurrentActivity);
                return false;
            }

            string[] deniedPermissions = DeniedBluetoothPermissions(Platform.CurrentActivity);
            if (deniedPermissions.Length > 0)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity, deniedPermissions, BluetoothPermissionHelper.BleAccessPermission);
                return false;
            }

            if (!BluetoothSettingsHelper.IsBluetoothEnabled(Platform.CurrentActivity))
            {
                RequestEnableBle(Platform.CurrentActivity);
                return false;
            }

            return true;
        }

        public static string[] DeniedBluetoothPermissions(Activity activity)
        {
            List<string> result = new();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                #pragma warning disable XA0001
                result.AddRange(DeniedBluetoothPermission(activity, Manifest.Permission.BluetoothScan));
                result.AddRange(DeniedBluetoothPermission(activity, Manifest.Permission.BluetoothConnect));
            }
            else
            {
                result.AddRange(DeniedBluetoothPermission(activity, Manifest.Permission.AccessFineLocation));
            }
            return result.ToArray<string>();
        }

        public static List<string> DeniedBluetoothPermission(Activity activity, string permission)
        {
            if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(activity, permission) != Permission.Granted)
                return new List<string>(new[] { permission });
            return new List<string>();
        }

        public static void RequestEnableBle(Activity activity)
        {
            Intent enableIntent = new(BluetoothAdapter.ActionRequestEnable);
            activity.StartActivityForResult(enableIntent, REQUEST_ENABLE_BT);
        }

        public static void RequestEnableLocationService(Activity activity)
        {
            Intent myIntent = new(Settings.ActionLocationSourceSettings);
            activity.StartActivityForResult(myIntent, REQUEST_ENABLE_LOCATION);
        }

        public void StartBluetoothService(Activity activity)
        {
            Intent bleIntent = new(activity, Java.Lang.Class.FromType(typeof(RegulaBleService)));
            activity.StartService(bleIntent);
            activity.BindService(bleIntent, this, 0);
        }

        void IServiceConnection.OnServiceConnected(ComponentName name, IBinder service)
        {
            bleManager = (service as RegulaBleService.LocalBinder).Service.BleManager;
            bool isBleManagerConnected = bleManager.IsConnected == true;
            if (!isBleManagerConnected)
                bleManager.AddCallback(this);

            isBleServiceConnected = true;
            Console.WriteLine("BleService connected, connecting to device...");
            Console.WriteLine("If nothing happens, then it failed to connect");
        }

        void IServiceConnection.OnServiceDisconnected(ComponentName name)
        {
            isBleServiceConnected = false;
            Console.WriteLine("bleService disconnected");
        }

        public override void OnDeviceReady()
        {
            bleManager.RemoveCallback(this);

            Console.WriteLine("device connected, initializing");
            if (bleManager == null)
            {
                Console.WriteLine("bleManager is null");
                return;
            }
            if (!DocumentReader.Instance().IsReady)
                DocumentReader.Instance().InitializeReader(Platform.CurrentActivity, new BleDeviceConfig(bleManager), this);
            else
                Console.WriteLine("already initialized");
        }
    }
}