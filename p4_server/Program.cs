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
            else if (actions[0] == "move")
            {
                string sender_id = actions[2];
                string col = actions[3];

                Game? game = games.Find(G => G.id == actions[1]);
                if (game != null)
                {
                    string receiver_id = (game.player1.id == sender_id) ? game.player2.id : game.player1.id;
                    SendTo(_clientSockets[receiver_id], "move," + col);
                }
            }
            else if (actions[0] == "endGame")
            {
                string game_id = actions[1];
                string sender_id = actions[2];
                string state = actions[3];

                Game? game = games.Find(G => G.id == game_id);
                if (game != null)
                {
                    string receiver_id = (game.player1.id == sender_id) ? game.player2.id : game.player1.id;
                    SendTo(_clientSockets[receiver_id], "endGame," + state);
                    Delete.DeleteFromDB("game", game_id);
                    games.Remove(game);
                }
            }
            else if (actions[0] == "quit")
            {
                string game_id = actions[1];
                string player_id = actions[2];

                Game? game = games.Find(G => G.id == game_id);
                if (game != null)
                {
                    string receiver_id = (game.player1.id == player_id) ? game.player2.id : game.player1.id;
                    SendTo(_clientSockets[receiver_id], "endGame," + "victory");
                    Delete.DeleteFromDB("game", game_id);
                    games.Remove(game);
                }
                Delete.DeleteFromDB("users", player_id);
                _clientSockets.Remove(player_id);
            }
            else if (actions[0] == "newGame")
            {
                string playerID = actions[1];

                Update.UpdateFromDB("users", "isInGame", "0", new string[] {playerID});
                Game? game = CreateGame.checkPlayersAvailable();
                if (game != null)
                {
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