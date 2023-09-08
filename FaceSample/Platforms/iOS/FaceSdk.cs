using Foundation;
using UIKit;
using FaceApi.iOS;

#pragma warning disable CA1422

namespace FaceSample.Platforms.iOS
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

    public class FaceSdk : IFaceSdk
    {
        public FaceSdk()
        {
        }

        private static UIViewController CurrentPresenter
        {
            get
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }
                return vc;
            }
        }


        public event EventHandler<IMatchFacesEvent> MatchFacesResultsObtained;
        public event EventHandler<ILivenessEvent> LivenessResultsObtained;
        public event EventHandler<IFaceCaptureImageEvent> FaceCaptureResultsObtained;

        public void MatchFaces(byte[] firstStream, byte[] secondStream)
        {
            var firsImage = UIImage.LoadFromData(NSData.FromArray(firstStream));
            var matchFacesRequest = new RFSMatchFacesRequest(new RFSMatchFacesImage[]
            {
                new RFSMatchFacesImage(UIImage.LoadFromData(NSData.FromArray(firstStream)), RFSImageType.Printed),
                new RFSMatchFacesImage(UIImage.LoadFromData(NSData.FromArray(secondStream)), RFSImageType.Printed)
            });
            RFSFaceSDK.Service.MatchFaces(matchFacesRequest, (RFSMatchFacesResponse matchFacesResponse) => {
                MatchFacesEvent matchFacesEvent = new();
                if (matchFacesResponse.Error != null)
                {
                    matchFacesEvent.IsSuccess = false;
                    matchFacesEvent.Error = matchFacesResponse.Error.ToString();
                }
                else
                {
                    RFSMatchFacesSimilarityThresholdSplit split = RFSMatchFacesSimilarityThresholdSplit.SplitPairs(matchFacesResponse.Results, 0.75);
                    if (split.MatchedFaces.Length > 0)
                    {
                        double similarity = ((float)split.MatchedFaces[0].Similarity);
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
            });
        }

        public void StartLiveness()
        {

            // example of liveness configuration

            //RFSFaceSDK.Service.StartLivenessFrom(CurrentPresenter, true, RFSLivenessConfiguration.ConfigurationWithBuilder((RFSLivenessConfigurationBuilder builder) =>
            //{
            //    builder.Copyright = false;
            //}), (RFSLivenessResponse response) =>
            //{
            //    if (response.Error == null)
            //        Console.WriteLine("No errors");

            //}, () => { });

            RFSFaceSDK.Service.StartLivenessFrom(CurrentPresenter, true, (RFSLivenessResponse response) => {
                if (response.Error == null)
                    Console.WriteLine("No errors");

                LivenessEvent livenessEvent = new()
                {
                    LivenessStatus = response.Liveness == RFSLivenessStatus.Passed ? LivenessStatus.PASSED : LivenessStatus.UNKNOWN
                };
                if (response.Error == null)
                    Console.WriteLine("No error");

                if (response.Image != null)
                {
                    livenessEvent.LivenessImage = ConvertImage(response.Image);
                }

                LivenessResultsObtained(this, livenessEvent);
            }, () => { });
        }

        public void FaceCaptureImage()
        {
            RFSFaceSDK.Service.PresentFaceCaptureViewControllerFrom(CurrentPresenter, true, (RFSFaceCaptureResponse captureResponse) => {
                if (captureResponse.Image == null || captureResponse.Image.Image == null)
                    return;


                FaceCaptureImageEvent faceCaptureImageEvent = new()
                {
                    Image = ConvertImage(captureResponse.Image.Image)
                };
                FaceCaptureResultsObtained(this, faceCaptureImageEvent);
            }, null);
        }

        protected static byte[] ConvertImage(UIImage image)
        {
            if (image == null)
                return null;

            using NSData imageData = image.AsPNG();
            byte[] myByteArray = new byte[imageData.Length];
            System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
            return myByteArray;
        }
    }
}