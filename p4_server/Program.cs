using p4_server.Config;
using p4_server.Model;
using p4_server.SrvSocket;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server
{
    class Program
    {
        public static readonly Socket _serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static readonly Dictionary<string, Socket> _clientSockets = new();
        public static readonly List<Game> games = new();

        static void Main(string[] args)
        {
            Console.Title = "Server";
            SrvSocket.SetupServer();
            Console.ReadLine();
        }

        public static void SelectActions(string request, Socket socket) {
            string[] actions = request.Split(",");

            if (actions[0] == "search") SearchMatch(actions, socket);
            else if (actions[0] == "move") MovePiece(actions);
            else if (actions[0] == "endGame") EndGame(actions);
            else if (actions[0] == "quit") LeaveGame(actions);
            else if (actions[0] == "newGame") NewGame(actions);
            else if (actions[0] == "") Console.WriteLine("Socket closed");
            else Console.WriteLine("Invalid Request");
        }
        private static void SearchMatch(string[] actions, Socket socket)
        {
            SrvSocket.SendTo(socket, "waiting");

            string player_name = actions[1];
            string uid = actions[2];
            _clientSockets.Add(uid, socket);
            
            string[] columns = new string[] { "id", "player" };
            string[] values = new string[] { uid, player_name };
            Insert.InsertIntoDB("users", columns, values);
            
            Game? game = CreateGame.CheckPlayersAvailable();
            if (game != null) MatchCreated(game);
        }
        private static void MovePiece(string[] actions)
        {
            string game_id = actions[1];
            string sender_id = actions[2];
            string column = actions[3];

            Game? game = games.Find(G => G.id == game_id);
            if (game != null)
            {
                string receiver_id = (game.player1.id == sender_id) ? game.player2.id : game.player1.id;
                SrvSocket.SendTo(_clientSockets[receiver_id], "move," + column);
            }
        }
        private static void EndGame(string[] actions)
        {
            string game_id = actions[1];
            string sender_id = actions[2];
            string state = actions[3];

            Game? game = games.Find(G => G.id == game_id);
            if (game != null)
            {
                string receiver_id = (game.player1.id == sender_id) ? game.player2.id : game.player1.id;
                SrvSocket.SendTo(_clientSockets[receiver_id], "endGame," + state);
                Delete.DeleteFromDB("game", game_id);
                games.Remove(game);
            }
        }
        private static void LeaveGame(string[] actions)
        {
            string game_id = actions[1];
            string player_id = actions[2];

            Game? game = games.Find(G => G.id == game_id);
            if (game != null)
            {
                string receiver_id = (game.player1.id == player_id) ? game.player2.id : game.player1.id;
                SrvSocket.SendTo(_clientSockets[receiver_id], "endGame," + "victory");
                Delete.DeleteFromDB("game", game_id);
                games.Remove(game);
            }
            Delete.DeleteFromDB("users", player_id);
            _clientSockets.Remove(player_id);
        }
        private static void NewGame(string[] actions)
        {
            string playerID = actions[1];

            Update.UpdateFromDB("users", "isInGame", "0", new string[] { playerID });
            Game? game = CreateGame.CheckPlayersAvailable();
            if (game != null) MatchCreated(game);
        }
        private static void MatchCreated(Game game)
        {
            Console.WriteLine("Match created: " + game.player1.name + " VS " + game.player2.name);
            games.Add(game);
            SrvSocket.SendTo(_clientSockets[game.player1.id], "matchFound:" + game.id + "," + game.player1.name + ":" + game.player1.id + "," + game.player2.name + ":" + game.player2.id);
            SrvSocket.SendTo(_clientSockets[game.player2.id], "matchFound:" + game.id + "," + game.player1.name + ":" + game.player1.id + "," + game.player2.name + ":" + game.player2.id);
        }
    }
}