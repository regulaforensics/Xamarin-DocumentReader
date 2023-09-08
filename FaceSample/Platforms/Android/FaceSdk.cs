using Android.Graphics;
using Com.Regula.Facesdk;
using Com.Regula.Facesdk.Callback;
using Com.Regula.Facesdk.Enums;
using Com.Regula.Facesdk.Model.Results;
using Com.Regula.Facesdk.Model.Results.Matchfaces;
using Com.Regula.Facesdk.Configuration;
using Com.Regula.Facesdk.Request;

namespace FaceSample.Platforms.Android
{
    public class MatchFacesEvent : EventArgs, IMatchFacesEvent
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public double Similarity { get; set; }
    }

    public class LivenessEvent : EventArgs, ILivenessEvent
    {
        public LivenessStatus LivenessStatus { get; set; }
        public byte[] LivenessImage { get; set; }
    }

    public class FaceCaptureImageEvent : EventArgs, IFaceCaptureImageEvent
    {
        public byte[] Image { get; set; }
    }

    public class FaceSdk : Java.Lang.Object, IFaceSdk, ILivenessCallback, IMatchFaceCallback, IFaceCaptureCallback
    {
        private readonly ImageType FirstImage = ImageType.Printed;
        private readonly ImageType SecondImage = ImageType.Rfid;
        public FaceSdk() { }
        public event EventHandler<IMatchFacesEvent> MatchFacesResultsObtained;
        public event EventHandler<ILivenessEvent> LivenessResultsObtained;
        public event EventHandler<IFaceCaptureImageEvent> FaceCaptureResultsObtained;

        public void MatchFaces(byte[] firstStream, byte[] secondStream)
        {
            Bitmap firstBitmap = BitmapFactory.DecodeByteArray(firstStream, 0, firstStream.Length);
            Bitmap secondBitmap = BitmapFactory.DecodeByteArray(secondStream, 0, secondStream.Length);
            IList<Com.Regula.Facesdk.Model.MatchFacesImage> listImages = new List<Com.Regula.Facesdk.Model.MatchFacesImage>{
                new Com.Regula.Facesdk.Model.MatchFacesImage(firstBitmap, FirstImage),
                new Com.Regula.Facesdk.Model.MatchFacesImage(secondBitmap, SecondImage)
            };
            var matchFacesRequest = new MatchFacesRequest(listImages);
            FaceSDK.Instance().MatchFaces(matchFacesRequest, this);
        }

        public void OnFaceMatched(MatchFacesResponse matchFacesResponse)
        {
            MatchFacesEvent matchFacesEvent = new();
            if (matchFacesResponse.Exception != null)
            {
                matchFacesEvent.IsSuccess = false;
                matchFacesEvent.Error = matchFacesResponse.Exception.Message;
            }
            else
            {
                if (matchFacesResponse.Results.Count > 0)
                {
                    double similarity = matchFacesResponse.Results[0].Similarity;
                    matchFacesEvent.Similarity = similarity;
                    matchFacesEvent.IsSuccess = true;
                }
                else
                {
                    matchFacesEvent.IsSuccess = false;
                    matchFacesEvent.Error = "Faces are not equals";
                }

            }

            MatchFacesResultsObtained(this, matchFacesEvent);
        }

        public void FaceCaptureImage()
        {
            FaceSDK.Instance().PresentFaceCaptureActivity(Platform.AppContext, this);
        }

        public void StartLiveness()
        {
            // example of liveness configuration

            //LivenessConfiguration.Builder config = new();
            //config.SetCopyright(false);
            //FaceSDK.Instance().StartLiveness(Platform.AppContext, config.Build(), this);

            FaceSDK.Instance().StartLiveness(Platform.AppContext, this);
        }

        public void OnLivenessCompete(LivenessResponse response)
        {
            LivenessEvent livenessEvent = new()
            {
                LivenessStatus = response.Liveness == Com.Regula.Facesdk.Enums.LivenessStatus.Passed ? LivenessStatus.PASSED : LivenessStatus.UNKNOWN
            };
            if (response.Exception == null)
                Console.WriteLine("No exception");

            if (response.Bitmap != null)
            {
                livenessEvent.LivenessImage = ConvertBitmap(response.Bitmap);
            }

            LivenessResultsObtained(this, livenessEvent);
        }

        public void OnFaceCaptured(FaceCaptureResponse captureResponse)
        {
            if (captureResponse.Image == null || captureResponse.Image.Bitmap == null)
                return;

            FaceCaptureImageEvent faceCaptureImageEvent = new() { Image = ConvertBitmap(captureResponse.Image.Bitmap) };
            FaceCaptureResultsObtained(this, faceCaptureImageEvent);
        }

        private static byte[] ConvertBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            byte[] bitmapData = null;
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                bitmapData = stream.ToArray();
            }
            return bitmapData;
        }
    }
}