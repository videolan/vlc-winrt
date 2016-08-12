
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;


public class HttpResponseSender
{
    private StreamSocket socket;

    private readonly Dictionary<string, string> contentTypes = new Dictionary<string, string>
    {
        { "html", "text/html; charset=UTF-8" },
        { "css", "text/css; charset=UTF-8" }
    };

    public HttpResponseSender(StreamSocket s)
    {
        socket = s;
    }

    public async Task serveStaticFile(string path)
    {
        if (path == "/")
            path = "/index.html";

        /* Security check: remove the trailing "/.." to prevent the user from accessing forbiden files.
           This is probably not needed since the system should restrict access to only authorized files. */
        path = path.Replace("/..", "");

        // Try to determince Content-type from the extension.
        var startExt = path.LastIndexOf(".");
        if (startExt == -1)
            throw new System.IO.FileNotFoundException();
        var ext = path.Substring(startExt + 1);
        string contentType;
        try
        {
            contentType = contentTypes[ext];
        }
        catch (KeyNotFoundException)
        {
            throw new System.IO.FileNotFoundException();
        }

        Uri appUri = new Uri("ms-appx:///Assets/WebInterface" + path);
        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(appUri);
        IBuffer fileBuffer = await FileIO.ReadBufferAsync(file);

        string header = String.Format("HTTP/1.1 200 OK\r\n" +
                            "Content-Type: {0}\r\n" +
                            "Content-Length: {1}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n", contentType, fileBuffer.Length);
        using (IOutputStream output = socket.OutputStream)
        {
            byte[] headerArray = Encoding.ASCII.GetBytes(header);
            IBuffer buffer = headerArray.AsBuffer();
            await output.WriteAsync(buffer);
            await output.WriteAsync(fileBuffer);
        }
    }

    public async Task simpleOK()
    {
        string msg = "OK\r\n";
        string header = String.Format("HTTP/1.1 200 OK\r\n" +
                            "Content-Length: {0}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n", msg.Length);
        await send(header + msg);
    }

    public async Task error500()
    {
        string msg = "Internal Server Error\r\n";
        string header = String.Format("HTTP/1.1 500 Internal Server Error\r\n" +
                            "Content-Length: {0}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n", msg.Length);
        await send(header + msg);
    }

    public async Task error400()
    {
        string msg = "Bad Request\r\n";
        string header = String.Format("HTTP/1.1 400 Bad Request\r\n" +
                            "Content-Length: {0}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n", msg.Length);
        await send(header + msg);
    }

    public async Task error404()
    {
        string msg = "Not Found\r\n";
        string header = String.Format("HTTP/1.1 404 Not Found\r\n" +
                            "Content-Length: {0}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n", msg.Length);
        await send(header + msg);
    }

    private async Task send(string answer)
    {
        using (IOutputStream output = socket.OutputStream)
        {
            byte[] headerArray = Encoding.ASCII.GetBytes(answer);
            IBuffer buffer = headerArray.AsBuffer();
            await output.WriteAsync(buffer);
        }
    }
}
