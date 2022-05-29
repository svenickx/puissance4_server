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

        /// <summary>
        /// Make the server listen to the port
        /// </summary>
        public static void SetupServer()
        {
            Console.Write("Setting up server");
            Utilitaires.FakeLoading();
            Program._serverSocket.Bind(new IPEndPoint(IPAddress.Any, 10000));
            Program._serverSocket.Listen(5);
            Program._serverSocket.BeginAccept(new AsyncCallback(SrvSocket.AcceptCallback), null);
            Console.WriteLine("\nServer ready.");
        }

        /// <summary>
        /// Send data to a remote client
        /// </summary>
        /// <param name="socket">The remote client</param>
        /// <param name="response">The data to send</param>
        public static void SendTo(Socket socket, string response)
        {
            byte[] data = Encoding.ASCII.GetBytes(response);

            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        /// <summary>
        /// Create a new socket with an incoming connection
        /// </summary>
        /// <param name="AR">The data from the client</param>
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = Program._serverSocket.EndAccept(AR);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            Program._serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        /// <summary>
        /// Listen to the port for datas
        /// </summary>
        /// <param name="AR">The data from the remote client</param>
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

        /// <summary>
        /// Send to the remote client
        /// </summary>
        /// <param name="AR"></param>
        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState!;
            socket.EndSend(AR);
        }
    }
}
