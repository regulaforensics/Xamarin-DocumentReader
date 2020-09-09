using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentReaderSample
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}
