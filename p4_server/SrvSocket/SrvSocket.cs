using server;
using System.Text;
using System.Net.Sockets;
using System.Net;
using p4_server.Utils;

namespace p4_server.SrvSocket
{
    public class SrvSocket
    {
        private static readonly byte[] _buffer = new byte[1024];
        public static void SetupServer()
        {
            Console.Write("Setting up server");
            Utilitaires.FakeLoading();
            Program._serverSocket.Bind(new IPEndPoint(IPAddress.Any, 10000));
            Program._serverSocket.Listen(5);
            Program._serverSocket.BeginAccept(new AsyncCallback(SrvSocket.AcceptCallback), null);
            Console.WriteLine("\nServer ready.");
        }
        public static void SendTo(Socket socket, string response)
        {
            byte[] data = Encoding.ASCII.GetBytes(response);

            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = Program._serverSocket.EndAccept(AR);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            Program._serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState!;
            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            string req = Encoding.ASCII.GetString(dataBuf);
            Console.WriteLine("Request received: " + req);

            Program.SelectActions(req, socket);
        }
        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState!;
            socket.EndSend(AR);
        }
    }
}
