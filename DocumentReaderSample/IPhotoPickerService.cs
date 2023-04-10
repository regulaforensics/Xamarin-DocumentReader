using System;
namespace DocumentReaderSample
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}

