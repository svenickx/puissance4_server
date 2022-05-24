using p4_server.Config;
using p4_server.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server
{
    class Program
    {
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Dictionary<string, Socket> _clientSockets = new Dictionary<string, Socket>();
        private static List<Game> games = new List<Game>();
        private static byte[] _buffer = new byte[1024];

        static void Main(string[] args)
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine();
        }

        private static void SetupServer() {
            Console.WriteLine("Seting up server...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 10000));
            _serverSocket.Listen(5);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void AcceptCallback(IAsyncResult AR) 
        {
            Socket socket = _serverSocket.EndAccept(AR);


            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            string req = Encoding.ASCII.GetString(dataBuf);
            Console.WriteLine("Text received: " + req);

            SelectActions(req, socket);
        }

        private static void SelectActions(string request, Socket socket) {
            string[] actions = request.Split(",");
            string response = "";

            if (actions[0] == "search") {
                SendTo(socket, "waiting");

                string player_name = actions[1];
                string uid = actions[2];

                _clientSockets.Add(uid, socket);
                string[] columns = new string[] { "id", "player" };
                string[] values = new string[] { uid, player_name };

                Insert.InsertIntoDB("users", columns, values);
                Game? game = CreateGame.checkPlayersAvailable();
                if (game != null) {
                    Console.WriteLine("Match created: " + game.player1.name + " VS " + game.player2.name);
                    games.Add(game);
                    SendTo(_clientSockets[game.player1.id], "matchFound:" + game.id + "," + game.player1.name + ":" + game.player1.id + "," + game.player2.name + ":" + game.player2.id);
                    SendTo(_clientSockets[game.player2.id], "matchFound:" + game.id + "," + game.player1.name + ":" + game.player1.id + "," + game.player2.name + ":" + game.player2.id);
                }
            }
        }

        private static void SendTo(Socket socket, string response) {
            byte[] data = Encoding.ASCII.GetBytes(response);

            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        private static void SendCallback(IAsyncResult AR) {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
    }
}