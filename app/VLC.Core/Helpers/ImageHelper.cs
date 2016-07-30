using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC.Utils;

namespace VLC.Helpers
{
    public class ImageHelper
    {
        public static async Task<byte[]> ResizedImage(byte[] img, int maxWidth, int maxHeight)
        {
            var memStream = new MemoryStream(img);

            IRandomAccessStream inputstream = memStream.AsRandomAccessStream();

            var decoder = await BitmapDecoder.CreateAsync(inputstream);
            if (decoder.PixelHeight > maxHeight || decoder.PixelWidth > maxWidth)
            {
                using (inputstream)
                {
                    var resizedStream = new InMemoryRandomAccessStream();

                    var biEncoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                    double widthRatio = (double)maxWidth / decoder.PixelWidth;
                    double heightRatio = (double)maxHeight / decoder.PixelHeight;

                    double scaleRatio = Math.Min(widthRatio, heightRatio);

                    if (maxWidth == 0)
                        scaleRatio = heightRatio;

                    if (maxHeight == 0)
                        scaleRatio = widthRatio;

                    uint aspectHeight = (uint)Math.Floor(decoder.PixelHeight * scaleRatio);
                    uint aspectWidth = (uint)Math.Floor(decoder.PixelWidth * scaleRatio);

                    biEncoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;

                    biEncoder.BitmapTransform.ScaledHeight = aspectHeight;
                    biEncoder.BitmapTransform.ScaledWidth = aspectWidth;
                    
                    await biEncoder.FlushAsync();
                    resizedStream.Seek(0);
                    var outBuffer = new byte[resizedStream.Size];
                    await resizedStream.ReadAsync(outBuffer.AsBuffer(), (uint)resizedStream.Size, InputStreamOptions.None);

                    return outBuffer;
                }
            }
            return img;
        }
    }
}
