using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Config {
    internal class MySqlUtils {
        private static readonly string ip = "127.0.0.1";
        private static readonly string port = "3300";
        private static readonly string dbName = "puissance4";
        private static readonly string user = "root";
        private static readonly string mdp = "";

        public static string GetDBstring() {
            return $"Server={ip};Port={port};Database={dbName};Uid={user};Pwd={mdp};";
        }
        public static void CreateDB() {
            string script = File.ReadAllText(path: "../../../puissance4.sql");

            MySqlConnection con = new(GetDBstring());
            con.Open();
            using var cmd = new MySqlCommand();
            cmd.Connection = con;

            cmd.CommandText = script;
            cmd.ExecuteNonQuery();
        }
    }
}
