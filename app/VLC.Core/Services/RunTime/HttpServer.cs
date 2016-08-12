using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VLC.Services.RunTime
{
    public class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;
        private static readonly StorageFolder LocalFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

        private readonly StreamSocketListener listener;

        public HttpServer()
        {
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        public async void bind(int port)
        {
            await this.listener.BindServiceNameAsync(port.ToString());
        }

        public void Dispose()
        {
            this.listener.Dispose();
        }

        private class RequestParameters
        {
            public string method;
            public string endPoint;
            public ulong payloadOffset;
            public ulong payloadLength;
            public string boundary;
        }


        private async void ProcessRequestAsync(StreamSocket socket)
        {
            try
            {
                using (IInputStream input = socket.InputStream)
                {
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();

                    MemoryStream headerStream = new MemoryStream();
                    UploadFileStream uploadFileStream = null;

                    RequestParameters requestParameters = null;
                    bool hasHeader = false;
                    ulong payloadLengthReceived = 0;
                    while (true)
                    {
                        buffer = await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);

                        if (buffer.Length == 0)
                            break; // Connection closed.

                        if (hasHeader)
                        {
                            if (uploadFileStream != null)
                                await uploadFileStream.WriteAsync(buffer);
                            payloadLengthReceived += buffer.Length;
                        }
                        else
                        {
                            await headerStream.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length);
                            hasHeader = parseHeader(headerStream.ToArray(), out requestParameters);
                            Debug.WriteLine("hasHeader: " + hasHeader);

                            if (hasHeader)
                            {
                                uploadFileStream = new UploadFileStream(requestParameters.boundary);
                                ulong length = buffer.Length - requestParameters.payloadOffset;
                                if (length > 0)
                                {
                                    IBuffer tempPayloadBuffer = headerStream.ToArray().AsBuffer((int)requestParameters.payloadOffset, (int)length);
                                    await uploadFileStream.WriteAsync(tempPayloadBuffer);
                                    payloadLengthReceived += length;
                                }
                            }
                        }

                        Debug.WriteLine("received: " + payloadLengthReceived);
                        if (requestParameters != null
                            && payloadLengthReceived == requestParameters.payloadLength)
                            break;
                    }
                }
                await answerSimpleOK(socket);
            }
            catch
            {
                await answerError500(socket);
            }
        }


        private class UploadFileStream
        {
            private IOutputStream payloadOutputStream;
            private bool hasHeader = false;
            MemoryStream headerStream;

            ulong payloadOffset;
            string boundary;
            string fileName;

            public UploadFileStream(string b)
            {
                headerStream = new MemoryStream();
                boundary = b;
            }

            public async Task WriteAsync(IBuffer buffer)
            {
                if (hasHeader)
                {
                    writeUntilEnd(buffer);
                }
                else
                {
                    Debug.WriteLine("mutipart header: " + Encoding.ASCII.GetString(buffer.ToArray()));

                    await headerStream.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length);
                    parseHeader(buffer.ToArray());
                    if (hasHeader)
                    {
                        payloadOutputStream = await getUploadOutputStream();
                        ulong lenToWrite = buffer.Length - payloadOffset;
                        if (lenToWrite > 0)
                            writeUntilEnd(buffer.ToArray().AsBuffer((int)payloadOffset, (int)lenToWrite));
                    }
                }
            }

            private async void writeUntilEnd(IBuffer buffer)
            {
                var lenToWrite = buffer.Length;
                var fileData = Encoding.ASCII.GetString(buffer.ToArray());

                // Try to find the end of the file
                var ind = fileData.IndexOf("\r\n--" + boundary);
                if (ind != -1)
                    lenToWrite = (uint)ind;

                byte[] base64data = new byte[lenToWrite];
                Array.Copy(buffer.ToArray(), 0, base64data, 0, (int)lenToWrite);
                await payloadOutputStream.WriteAsync(base64data.AsBuffer());

                if (ind != -1)
                {
                    // Close the file.
                    hasHeader = false;
                    payloadOutputStream.Dispose();
                }
            }

            public void parseHeader(byte[] data)
            {
                var headerBuilder = new StringBuilder();
                headerBuilder.Append(Encoding.ASCII.GetString(data));
                var header = headerBuilder.ToString();

                var ind = header.IndexOf("\r\n\r\n");
                if (ind != -1)
                {
                    payloadOffset = (ulong)ind + 4;
                    hasHeader = true;

                    header = header.Substring(0, ind);
                    var headerLines = header.Split('\n');
                    for (int i = 0; i < headerLines.Length; ++i)
                    {
                        // Remove the '\r'
                        headerLines[i] = headerLines[i].Substring(0, headerLines[i].Length - 1);

                        const string CONTENT_DISPOSITION_HEADER = "Content-Disposition: ";

                        if (headerLines[i].StartsWith(CONTENT_DISPOSITION_HEADER))
                        {
                            var d = HttpServer.getHeaderLineDict(headerLines[i].Substring(CONTENT_DISPOSITION_HEADER.Length));
                            fileName = d["filename"];
                        }
                        Debug.WriteLine("header: " + headerLines[i]);
                    }
                }
            }

            private async Task<IOutputStream> getUploadOutputStream()
            {
                var testFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                var writeStream = await testFile.OpenAsync(FileAccessMode.ReadWrite);
                return writeStream;
            }
        }


        private bool parseHeader(byte[] data, out RequestParameters requestParams)
        {
            requestParams = new RequestParameters();
            var ret = false;

            // Look for the end of the header.
            var headerBuilder = new StringBuilder();
            headerBuilder.Append(Encoding.ASCII.GetString(data));
            var header = headerBuilder.ToString();

            var ind = header.IndexOf("\r\n\r\n");
            if (ind != -1)
            {
                ret = true;
                requestParams.payloadOffset = (ulong)ind + 4;
                header = header.Substring(0, ind);
                var headerLines = header.Split('\n');
                // Remove the '\r'
                for (int i = 0; i < headerLines.Length; ++i)
                {
                    headerLines[i] = headerLines[i].Substring(0, headerLines[i].Length - 1);

                    const string CONTENT_LENGTH_HEADER = "Content-Length: ";
                    const string CONTENT_TYPE_HEADER = "Content-Type: ";

                    if (i == 0)
                    {
                        // Get the method and the endpoint.
                        var lineParts = headerLines[i].Split();
                        requestParams.method = lineParts[0];
                        requestParams.endPoint = lineParts[1];
                    }
                    else if (headerLines[i].StartsWith(CONTENT_LENGTH_HEADER))
                    {
                        requestParams.payloadLength =
                            UInt64.Parse(headerLines[i].Substring(CONTENT_LENGTH_HEADER.Length));
                    }
                    else if (headerLines[i].StartsWith(CONTENT_TYPE_HEADER))
                    {
                        var d = getHeaderLineDict(headerLines[i].Substring(CONTENT_TYPE_HEADER.Length));
                        if (d.ContainsKey("multipart/form-data"))
                            requestParams.boundary = d["boundary"];
                        else
                            throw new HttpServerException("Not a \"multipart/form-data\" content");
                    }
                    Debug.WriteLine("header: " + headerLines[i]);
                }
            }
            else
                ret = false;

            return ret;
        }

        private static Dictionary<string, string> getHeaderLineDict(string headerLine)
        {
            var ret = new Dictionary<string, string>();
            var lineParts = headerLine.Split(';');
            foreach (string p in lineParts)
            {
                var pParts = p.Trim().Split('=');
                if (pParts.Length == 1)
                    ret[pParts[0]] = null;
                else if (pParts.Length >= 2)
                    ret[pParts[0]] = pParts[1].Trim('"');
            }
            return ret;
        }

        private async Task answerSimpleOK(StreamSocket socket)
        {
            using (IOutputStream output = socket.OutputStream)
            {
                string msg = "OK\r\n";
                string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                    "Content-Length: {0}\r\n" +
                                    "Connection: close\r\n" +
                                    "\r\n", msg.Length);

                byte[] headerArray = Encoding.ASCII.GetBytes(header + msg);
                IBuffer buffer = headerArray.AsBuffer();
                await output.WriteAsync(buffer);
            }
        }

        private async Task answerError500(StreamSocket socket)
        {
            using (IOutputStream output = socket.OutputStream)
            {
                string msg = "Internal Server Error\r\n";
                string header = String.Format("HTTP/1.1 500 Internal Server Error\r\n" +
                                    "Content-Length: {0}\r\n" +
                                    "Connection: close\r\n" +
                                    "\r\n", msg.Length);

                byte[] headerArray = Encoding.ASCII.GetBytes(header + msg);
                IBuffer buffer = headerArray.AsBuffer();
                await output.WriteAsync(buffer);
            }
        }
    }

    public class HttpServerException : System.Exception
    {
        public HttpServerException() : base() { }
        public HttpServerException(string message) : base(message) { }
        public HttpServerException(string message, System.Exception inner) : base(message, inner) { }
    }
}
