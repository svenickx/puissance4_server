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

        /// <summary>
        /// Performs an action depending on the data received by a client
        /// </summary>
        /// <param name="request">The data received</param>
        /// <param name="socket">The client who sent the data</param>
        public static void SelectActions(string request, Socket socket) {
            string[] actions = request.Split(",");

            if (actions[0] == "search") SearchMatch(actions, socket);
            else if (actions[0] == "move") MovePiece(actions);
            else if (actions[0] == "endGame") EndGame(actions);
            else if (actions[0] == "quit") LeaveGame(actions);
            else if (actions[0] == "newGame") NewGame(actions);
            else if (actions[0] == "message") TransferMessage(actions);
            else if (actions[0] == "") Console.WriteLine("Socket closed");
            else Console.WriteLine("Invalid Request");
        }

        /// <summary>
        /// Add the client to the waiting list and check if he can starts a match with another waiting client
        /// </summary>
        /// <param name="actions">The data from the client</param>
        /// <param name="socket">The client itself</param>
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

        /// <summary>
        /// Transfer the information of a new piece on the grid
        /// </summary>
        /// <param name="actions">The data such as the game ID and the column where the new piece is located</param>
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

        /// <summary>
        /// Transfer the information of a game that ended
        /// </summary>
        /// <param name="actions">The data such as the game ID, the state of the game (victory, lose, draw)</param>
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

        /// <summary>
        /// Transfer the information that a player left the app. Remove it from the DB and the local list and if the game was finished, the remote player receive a victory
        /// </summary>
        /// <param name="actions">The data such as the game ID and the remote Player</param>
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

        /// <summary>
        /// Get the information that a player wants a new Game. Puts it in the waiting list
        /// </summary>
        /// <param name="actions">Datas such as the player ID</param>
        private static void NewGame(string[] actions)
        {
            string playerID = actions[1];

            Update.UpdateFromDB("users", "isInGame", "0", new string[] { playerID });
            Game? game = CreateGame.CheckPlayersAvailable();
            if (game != null) MatchCreated(game);
        }

        /// <summary>
        /// Send data to the two players from the newly created match
        /// </summary>
        /// <param name="game">The game datas</param>
        private static void MatchCreated(Game game)
        {
            Console.WriteLine("Match created: " + game.player1.name + " VS " + game.player2.name);
            games.Add(game);
            SrvSocket.SendTo(_clientSockets[game.player1.id], "matchFound:" + game.id + "," + game.player1.name + ":" + game.player1.id + "," + game.player2.name + ":" + game.player2.id);
            SrvSocket.SendTo(_clientSockets[game.player2.id], "matchFound:" + game.id + "," + game.player1.name + ":" + game.player1.id + "," + game.player2.name + ":" + game.player2.id);
        }

        /// <summary>
        /// Send the message from one player to the other
        /// </summary>
        /// <param name="actions">Datas such as the message to transfer and the ID</param>
        private static void TransferMessage(string[] actions)
        {
            string game_id = actions[1];
            string player_id = actions[2];
            string message = actions[3];

            Game? game = games.Find(G => G.id == game_id);
            if (game != null)
            {
                Player sender = (game.player1.id == player_id) ? game.player1 : game.player2;
                Player receiver = (game.player1.id == player_id) ? game.player2 : game.player1;
                SrvSocket.SendTo(_clientSockets[receiver.id], "message," + sender.name + ": " + message);
            }
        }
    }
}