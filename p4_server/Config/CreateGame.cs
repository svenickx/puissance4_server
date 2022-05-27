using MySqlConnector;
using p4_server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Config {
    internal class CreateGame {
        public static Game? CheckPlayersAvailable() {

            MySqlConnection connection = new (MySqlUtils.GetDBstring());
            MySqlCommand command = connection.CreateCommand();
            Game? game = null;

            command.CommandText = "SELECT users.id, users.player FROM users WHERE users.isInGame = 0 LIMIT 2";

            try {
                connection.Open();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            try {
                MySqlDataReader reader;
                reader = command.ExecuteReader();

                List<Player> players = new();
                while (reader.Read()) {
                    Player player = new(reader["id"].ToString(), reader["player"].ToString());
                    players.Add(player);
                }
                if (players.Count > 1) {
                    game = CreateNewGame(players);
                }
                connection.Close();
            } catch (Exception)
            {
                connection.Close();
            }
            return game;
        }

        public static Game CreateNewGame(List<Player> players) {
            Game game = new(Guid.NewGuid().ToString(), players[0], players[1]);

            Update.UpdateFromDB("users", "isInGame", "1", new string[] { players[0].id, players[1].id });
            Insert.InsertIntoDB("game", new string[] { "id", "player1", "player2" }, new string[] { game.id, game.player1.id, game.player2.id });
            return game;
        }
    }
}
