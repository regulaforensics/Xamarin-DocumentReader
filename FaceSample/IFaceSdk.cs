namespace FaceSample
{
    public interface IFaceSdk
    {
        void MatchFaces(byte[] firstStream, byte[] secondStream);
        void StartLiveness();
        void FaceCaptureImage();
        event EventHandler<IMatchFacesEvent> MatchFacesResultsObtained;
        event EventHandler<ILivenessEvent> LivenessResultsObtained;
        event EventHandler<IFaceCaptureImageEvent> FaceCaptureResultsObtained;
    }

    public interface IMatchFacesEvent
    {
        bool IsSuccess { get; set; }
        string Error { get; set; }
        double Similarity { get; set; }
    }

    public interface ILivenessEvent
    {
        LivenessStatus LivenessStatus { get; set; }
        byte[] LivenessImage { get; set; }
    }

    public interface IFaceCaptureImageEvent
    {
        byte[] Image { get; set; }
    }

    public enum LivenessStatus
    {
        PASSED,
        UNKNOWN
    }
}