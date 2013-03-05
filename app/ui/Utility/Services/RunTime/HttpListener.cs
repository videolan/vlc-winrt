using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VLC_WINRT.ViewModels;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace VLC_WINRT.Utility.Services
{
    public class HttpListener : IDisposable
    {
        private string _input = String.Empty;
        private StreamSocketListener _listener;
        private DataReader _reader;
        private DataWriter _writer;

        public HttpListener()
        {
            StartSocket();
        }

        public void Dispose()
        {
            CloseWriterAndReader();
        }

        private void StartSocket()
        {
            _listener = new StreamSocketListener();
            _listener.ConnectionReceived += OnConnection;

            try
            {
                IAsyncAction stuff = _listener.BindServiceNameAsync("8000");
                stuff.Completed += BindCompleted;
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Debug.WriteLine("Start listening failed with error: " + exception.Message);
            }
        }

        private void BindCompleted(IAsyncAction asyncinfo, AsyncStatus asyncstatus)
        {
            Debug.WriteLine(asyncstatus.ToString());
        }

        private void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                CloseWriterAndReader();

                _reader = new DataReader(args.Socket.InputStream);
                _writer = new DataWriter(args.Socket.OutputStream);

                DataReaderLoadOperation result = _reader.LoadAsync(1);
                result.Completed = LoadedAByte;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Hit an exception in tcp reconnection");
            }
        }

        private void CloseWriterAndReader()
        {
            if (_writer != null)
            {

                _writer.Dispose();
                _writer = null;
            }
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        private async void WriteVideoToOutput(ulong start, ulong end)
        {
            
            var mediaFile =ViewModelLocator.PlayVideoVM.CurrentFile;
            string mimetype = GetMimeType(mediaFile.FileType);
            IRandomAccessStreamWithContentType videoFile = await mediaFile.OpenReadAsync();
            videoFile.Seek(start);

            //construct http header 
            string header = string.Empty;
            header += "HTTP/1.1 206 Partial Content\r\n";
            //TODO: Get actual last mime type
            header += "Content-Type: "+mimetype+"\r\n";
            header += "Accept-Ranges: bytes\r\n";
            //TODO: Get actual last modified
            header += "Last-Modified: Fri, 16 Sep 2011 04:19:05 GMT\r\n";
            header += "Server: VlcTempD 1.0\r\n";
            //TODO: Get actual content length
            if (end != 0)
                header += "Content-Range: bytes " + start + "-" + end + "/" + videoFile.Size + "\r\n";
            else
            {
                header += "Content-Range: bytes " + start + "-" + (videoFile.Size - start - 1).ToString() + "/" +
                          videoFile.Size + "\r\n";
            }
            header += "Content-Length: " + (videoFile.Size - start).ToString() + "\r\n";
            header += "Connection: close\r\n";
            header += "\r\n";

            Debug.WriteLine("Sending header: " + Environment.NewLine);
            Debug.WriteLine(header);

            _writer.WriteString(header);


            ulong bytesSent = start;
            ulong chunkSize = 8192;
            try
            {
                while (bytesSent + chunkSize < videoFile.Size)
                {
                    await SendChunk(chunkSize, videoFile);
                    bytesSent += chunkSize;
                }

                if (bytesSent < videoFile.Size)
                {
                    await SendChunk(videoFile.Size - bytesSent, videoFile);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Output Connection Closed: " + ex);
                return;
            }
        }

        private string GetMimeType(string fileType)
        {
            string mimetype;
            switch (fileType)
            {
                case ".avi":
                    mimetype = "video/avi";
                    break;
                case ".mp4" :
                    mimetype = "video/mp4";
                    break;
                case ".m4v" :
                    mimetype = "video/mp4";
                    break;
                case ".mkv" :
                    mimetype = "video/x-matroska";
                    break;
                default :
                    mimetype = "video/mp4";
                    break;
            }


            return mimetype;
        }

        private async Task SendChunk(ulong chunkSize, IRandomAccessStreamWithContentType sr)
        {
            var buf = new Buffer((uint) chunkSize);
            IBuffer result = await sr.ReadAsync(buf, (uint) chunkSize, InputStreamOptions.None);

            _writer.WriteBuffer(result);
            await _writer.StoreAsync();
            await _writer.FlushAsync();
        }

        private void LoadedAByte(IAsyncOperation<uint> asyncinfo, AsyncStatus asyncstatus)
        {
            try
            {
                _input += _reader.ReadString(_reader.UnconsumedBufferLength);
                if (_input.Contains("\r\n\r\n"))
                {
                    ulong start = 0, end = 0;
                    Debug.WriteLine("Call output");

                    if (_input.ToLower().Contains("range"))
                    {
                        GetByteRanges(_input, ref start, ref end);
                    }

                    WriteVideoToOutput(start, end);
                    _input = String.Empty;
                }

                DataReaderLoadOperation result = _reader.LoadAsync(1);
                result.Completed = LoadedAByte; //recurse and read for eva
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Input Connection Closed: " + ex);
            }
        }

        private void GetByteRanges(string input, ref ulong start, ref ulong end)
        {
            string[] headers = input.Split('\n');

            foreach (string header in headers)
            {
                string lh = header.ToLower().Trim();
                if (lh.Contains("range"))
                {
                    lh = lh.Replace("range", "");
                    lh = lh.Replace(":", "");
                    lh = lh.Replace("bytes", "");
                    lh = lh.Replace("=", "");
                    lh = lh.Trim();

                    string[] ranges = lh.Split('-');
                    if (ranges.Length != 2)
                    {
                        throw new ArgumentOutOfRangeException("Ranges must have 1 or 2 values");
                    }

                    ulong.TryParse(ranges[0], out start);
                    if (!string.IsNullOrEmpty(ranges[1]))
                        ulong.TryParse(ranges[1], out end);
                }
            }
        }

        public void Stop()
        {
            CloseWriterAndReader();
        }
    }
}