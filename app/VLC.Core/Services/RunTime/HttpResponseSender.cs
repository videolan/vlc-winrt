
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;


public class HttpResponseSender
{
    private StreamSocket socket;

    private readonly Dictionary<string, string> contentTypes = new Dictionary<string, string>
    {
        { "html", "text/html; charset=UTF-8" },
        { "css", "text/css; charset=UTF-8" },
        /* Javascript files have a "jstxt" extension to prevent being
         * removed from the Assets folder by the build system.
         * TODO: check why? */
        { "jstxt", "application/javascript; charset=UTF-8" },
        { "woff", "application/font-woff" }
    };

    private readonly String[] HTMLStrings =
    {
        "WEBINTF_TITLE",
        "WEBINTF_DROPFILES",
        "WEBINTF_DROPFILES_LONG",
        "WEBINTF_DOWNLOADFILES",
        "WEBINTF_DOWNLOADFILES_LONG"
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

        // Translate files if needed.
        if (ext == "html" || ext == "css")
            fileBuffer = translateBuffer(fileBuffer);

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

    IBuffer translateBuffer(IBuffer fileBuffer)
    {
        var res = new ResourceLoader();
        var fileStr = Encoding.UTF8.GetString(fileBuffer.ToArray());
        foreach (string key in HTMLStrings)
            fileStr = fileStr.Replace("%%" + key + "%%", res.GetString(key));

        return Encoding.UTF8.GetBytes(fileStr).AsBuffer();
    }

    public async Task simpleOK()
    {
        string msg = "\"OK\"\r\n";
        string header = String.Format("HTTP/1.1 200 OK\r\n" +
                            "Content-Type: application/json\r\n" +
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
