using FaceApi.iOS;
using Foundation;

namespace FaceSample.Platforms.iOS
{
    public class FaceSdkInit : RFSURLRequestInterceptingDelegate, IFaceSdkInit
    {
        public FaceSdkInit()
        {
        }

        public void InitFaceSdk()
        {
            RFSFaceSDK.Service.InitializeWithCompletion((bool success, NSError error) =>
            {
                if (success)
                {
                    RFSFaceSDK.Service.RequestInterceptingDelegate = this;
                    Console.WriteLine("Init complete");
                }
                else
                {
                    Console.WriteLine("Init failed:");
                    Console.WriteLine(error);
                }
            });
        }

        public override NSUrlRequest InterceptorPrepareRequest(NSUrlRequest request)
        {
            NSString key = new("Authorization");
            NSString value = new("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmYWNlLXNkayI6IkhhdmUgYSBncmVhdCBkYXkhIn0.IPoW0D0LnMv_pL4U22MuIhDNGIdK34TaHhqhKBAaBEs");
            NSMutableUrlRequest interceptedRequest = request.MutableCopy() as NSMutableUrlRequest;
            NSMutableDictionary headers = interceptedRequest.Headers.MutableCopy() as NSMutableDictionary;
            headers.Add(key, value);
            interceptedRequest.Headers = headers;

            return interceptedRequest;
        }
    }
}