
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

    public async Task error500()
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
