using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Config {
    internal class Delete {
        public static void DeleteFromDB(string table, string id) {
            using var con = new MySqlConnection(MySqlUtils.GetDBstring());
            con.Open();

            using var cmd = new MySqlCommand();
            cmd.Connection = con;

            cmd.CommandText = MakeDeleteQuery(table, id);

            cmd.ExecuteNonQuery();
        }

        public static string MakeDeleteQuery(string table, string id) {
            return "DELETE FROM " + table + " WHERE id = \'" + id + "\'";
        }

        public static void ClearDB() {
            using var con = new MySqlConnection(MySqlUtils.GetDBstring());
            con.Open();
            using var cmd = new MySqlCommand();
            cmd.Connection = con;

            cmd.CommandText = "TRUNCATE users";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "TRUNCATE game";
            cmd.ExecuteNonQuery();
        }
    }
}
