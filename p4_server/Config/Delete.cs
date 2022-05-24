using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Config {
    internal class Delete {
        public static void DeleteFromDB(string table, string[] columns, string[] values) {
            using var con = new MySqlConnection(MySqlUtils.GetDBstring());
            con.Open();

            using var cmd = new MySqlCommand();
            cmd.Connection = con;

            cmd.CommandText = MakeDeleteQuery(table, columns, values);

            cmd.ExecuteNonQuery();
        }

        public static string MakeDeleteQuery(string table, string[] columns, string[] values) {
            return "";
        }
    }
}
