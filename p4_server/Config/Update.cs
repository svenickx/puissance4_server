using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Config {
    internal class Update {
        public static void UpdateFromDB(string table, string column, string value, string[] IDs) {
            using var con = new MySqlConnection(MySqlUtils.GetDBstring());
            con.Open();

            using var cmd = new MySqlCommand();
            cmd.Connection = con;

            foreach (var id in IDs) {
                cmd.CommandText = MakeUpdateQuery(table, column, value, id);
                cmd.ExecuteNonQuery();
            }
        }

        public static string MakeUpdateQuery(string table, string column, string value, string id) {
            return "UPDATE " + table + " SET " + column + " = " + value + " WHERE id = \'" + id + "\'";
        }
    }
}
