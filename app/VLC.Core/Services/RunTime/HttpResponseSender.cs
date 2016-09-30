
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.Model.Music;
using VLC.Model.Video;
using VLC.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

public class HttpResponseSender
{
    private const uint BufferSize = 8192;
    private StreamSocket socket;

    private readonly Dictionary<string, string> contentTypes = new Dictionary<string, string>
    {
        { "html", "text/html; charset=UTF-8" },
        { "css", "text/css; charset=UTF-8" },
        /* Javascript files have a "jstxt" extension to prevent being
         * removed from the Assets folder by the build system.
         * TODO: check why? */
        { "jstxt", "application/javascript; charset=UTF-8" },
        { "woff", "application/font-woff" },
        { "jpg", "image/jpeg" },
        { "png", "image/png" },
        { "mp4", "video/mp4" },
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

        Uri uri;
        try
        {
            if (path.StartsWith("/thumbnails/"))
                uri = getThumbnail(path);
            else
                uri = new Uri("ms-appx:///Assets/WebInterface" + path);
        }
        catch
        {
            throw new System.IO.FileNotFoundException();
        }

        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
        IBuffer fileBuffer = await FileIO.ReadBufferAsync(file);

        // Translate files if needed.
        if (ext == "html" || ext == "css")
            fileBuffer = translateBuffer(fileBuffer);

        // Fill media list if we serve the main page.
        if (path == "/index.html")
            fileBuffer = fillMediaList(fileBuffer);

        string header = String.Format("HTTP/1.1 200 OK\r\n" +
                            "Content-Type: {0}\r\n" +
                            "Content-Length: {1}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n", contentType, fileBuffer.Length);
        using (IOutputStream output = socket.OutputStream)
        {
            byte[] headerArray = Encoding.UTF8.GetBytes(header);
            IBuffer buffer = headerArray.AsBuffer();
            await output.WriteAsync(buffer);
            await output.WriteAsync(fileBuffer);
        }
    }


    public async Task serveMediaFile(string path)
    {
        var decomp = path.Substring(1).Split('/');
        string localPath;
        if (decomp[1] == "track")
        {
            TrackItem track = Locator.MediaLibrary.LoadTrackById(int.Parse(decomp[4]));
            localPath = track.Path;
        }
        else if (decomp[1] == "video")
        {
            VideoItem video = Locator.MediaLibrary.LoadVideoById(int.Parse(decomp[3]));
            localPath = video.Path;
        }
        else
            throw new System.IO.FileNotFoundException();

        string fileName = GetFileName(localPath);
        StorageFile file = await StorageFile.GetFileFromPathAsync(localPath);

        using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
        {
            using (IOutputStream output = socket.OutputStream)
            {
                {
                    // Send the header.
                    string header = String.Format("HTTP/1.1 200 OK\r\n" +
                            "Content-Type: application/octet-stream\r\n" +
                            "Content-Disposition: attachment; filename=\"{1}\";\r\n" +
                            "Content-Length: {0}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n", stream.Size, fileName);

                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    IBuffer buffer = headerArray.AsBuffer();
                    await output.WriteAsync(buffer);
                }

                {
                    // Send the file.
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();

                    while (true)
                    {
                        buffer = await stream.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);

                        if (buffer.Length == 0)
                            break; // Connection closed.

                        await output.WriteAsync(buffer);
                    }
                }
            }
        }
    }

    IBuffer fillMediaList(IBuffer fileBuffer)
    {
        var fileStr = Encoding.UTF8.GetString(fileBuffer.ToArray());
        var mediaList = "";

        List<VideoItem> videos = Locator.MediaLibrary.LoadVideos(x => true);
        foreach (VideoItem v in videos)
        {
            mediaList += String.Format("<div style='background-image: url(\"/thumbnails/video/{1}/{2}/art.jpg\"); height: 174px;'>"
                + "<a href='/downloads/video/{1}/{2}/{3}' class='inner'><div class='down icon bgz'></div><div class='infos'>"
                + "<span class='first-line'>{0}</span><span class='second-line'>{4}</span>"
                + "</div></a></div>",
                v.Name, Uri.EscapeDataString(v.Name), v.Id,
                Uri.EscapeDataString(GetFileName(v.Path)), DurationToString(v.Duration));
        }

        List<TrackItem> tracks = Locator.MediaLibrary.LoadTracks();
        foreach (TrackItem t in tracks)
        {
            mediaList += String.Format("<div style='background-image: url(\"/thumbnails/track/{2}/{3}/{4}/art.jpg\"); height: 174px;'>"
                + "<a href='/downloads/track/{2}/{3}/{4}/{5}' class='inner'><div class='down icon bgz'></div><div class='infos'>"
                + "<span class='first-line'>{0} - {1}</span><span class='second-line'>{6}</span>"
                + "</div></a></div>",
                t.ArtistName, t.Name, Uri.EscapeDataString(t.ArtistName), Uri.EscapeDataString(t.Name), t.Id,
                Uri.EscapeDataString(GetFileName(t.Path)), DurationToString(t.Duration));
        }

        fileStr = fileStr.Replace("%%FILES%%", mediaList);

        return Encoding.UTF8.GetBytes(fileStr).AsBuffer();
    }

    string GetFileName(string path)
    {
        return path.Substring(path.LastIndexOf('\\') + 1);
    }

    string DurationToString(TimeSpan d)
    {
        string ret = "";
        if (d.Hours != 0)
            ret += d.Hours.ToString("D2") + ":";
        ret += d.Minutes.ToString("D2") + ":";
        ret += d.Seconds.ToString("D2");
        return ret;
    }

    Uri getThumbnail(string path)
    {
        Uri ret = null;
        var decomp = path.Substring(1).Split('/');
        if (decomp[1] == "track")
        {
            TrackItem track = Locator.MediaLibrary.LoadTrackById(int.Parse(decomp[4]));
            AlbumItem album = Locator.MediaLibrary.LoadAlbum(track.AlbumId);
            ret = new Uri(album.AlbumCoverFullUri);
        }
        else if (decomp[1] == "video")
        {
            VideoItem video = Locator.MediaLibrary.LoadVideoById(int.Parse(decomp[3]));
            ret = new Uri(video.PictureUri);
        }
        else
            throw new ArgumentException("Wrong URL path");

        return ret;
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
            byte[] headerArray = Encoding.UTF8.GetBytes(answer);
            IBuffer buffer = headerArray.AsBuffer();
            await output.WriteAsync(buffer);
        }
    }
}
