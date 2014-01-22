using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace VLC_WINRT.Utility.Helpers
{
    public static class DoesFileExistHelper
    {
        public static async Task<bool> DoesFileExistAsync(string fileName)
        {
            try
            {
                await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
