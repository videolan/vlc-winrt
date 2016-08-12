
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;


public class HttpResponseSender
{
    private StreamSocket socket;

    public HttpResponseSender(StreamSocket s)
    {
        socket = s;
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
