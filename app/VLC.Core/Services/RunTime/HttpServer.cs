using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.ViewModels;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace VLC.Services.RunTime
{
    public class HttpServer : IDisposable
    {
        public delegate void UploadRequestCallbackHandler(object current, string ip);
        public event UploadRequestCallbackHandler UploadRequestCallback;
        private const uint BufferSize = 8192;
        private static readonly StorageFolder LocalFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

        private readonly StreamSocketListener listener;

        public HttpServer(int port)
        {
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            this.listener.BindServiceNameAsync(port.ToString());
        }

        public void Dispose()
        {
            this.listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            var request = new StringBuilder();
            using (IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;

                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            using (IOutputStream output = socket.OutputStream)
            {
                string requestMethod = request.ToString().Split('\n')[0];
                string[] requestParts = requestMethod.Split(' ');

                if (requestParts[0] == "GET")
                    await WriteResponseAsync(requestParts[1], socket.Information.RemoteAddress.CanonicalName, output);
                throw new InvalidDataException("HTTP method not supported: "
                                                   + requestParts[0]);
            }
        }

        private async Task WriteResponseAsync(string payload, string remoteAdress, IOutputStream os)
        {
            using (Stream resp = os.AsStreamForWrite())
            {
                if (payload == "prepUploadRequest")
                {
                    string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                    "Content-Length: {0}\r\n" +
                                    "Connection: close\r\n\r\n",
                                    "OK");
                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                    UploadRequestCallback?.Invoke(this, remoteAdress);
                }
                else if (payload == "fileRequest")
                {

                    bool exists = true;
                    try
                    {
                        using (Stream fs = await Locator.UploaderVM.MediaToUpload.File.OpenStreamForReadAsync())
                        {
                            string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                            "Content-Length: {0}\r\n" +
                                            "Connection: close\r\n\r\n",
                                            fs.Length);
                            byte[] headerArray = Encoding.UTF8.GetBytes(header);
                            await resp.WriteAsync(headerArray, 0, headerArray.Length);
                            await fs.CopyToAsync(resp);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        exists = false;
                    }

                    if (!exists)
                    {
                        byte[] headerArray = Encoding.UTF8.GetBytes(
                                              "HTTP/1.1 404 Not Found\r\n" +
                                              "Content-Length:0\r\n" +
                                              "Connection: close\r\n\r\n");
                        await resp.WriteAsync(headerArray, 0, headerArray.Length);
                    }
                }

                await resp.FlushAsync();
            }
        }

        public async Task Upload()
        {
            //if (DeviceTypeHelper.GetDeviceType() == DeviceTypeEnum.Tablet)
            //{
            //    try
            //    {
            //        var video = (await Locator.MediaLibrary.LoadVideos(x => x.Name == "AR_Concorde1-1"))[0];
            //        var testUpload = new HttpClient();
            //        await video.LoadFileFromPath();
            //        var randomAccessStream = await video.File.OpenReadAsync();
            //        var stream = new HttpStreamContent(randomAccessStream);

            //        await testUpload.PostAsync(new Uri("http://localhost:8080/", UriKind.RelativeOrAbsolute), stream);
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.WriteLine(e);
            //    }
            //}
        }
    }
}
