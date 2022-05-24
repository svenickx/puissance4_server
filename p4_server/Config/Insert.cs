using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Config {
    internal class Insert {
        public static void InsertIntoDB(string table, string[] columns, string[] values) {
            using var con = new MySqlConnection(MySqlUtils.GetDBstring());
            con.Open();

            using var cmd = new MySqlCommand();
            cmd.Connection = con;

            cmd.CommandText = MakeInsertQuery(table, columns, values);

            cmd.ExecuteNonQuery();
        }

        public static string MakeInsertQuery(string table, string[] columns, string[] values) {
            string result = "INSERT INTO " + table + "(";
            for (int i = 0; i < columns.Length; i++) {
                if (i == columns.Length - 1) {
                    result += columns[i];
                    break;
                }
                result += columns[i] + ",";
            }

            result += ") VALUES (";

            for (int j = 0; j < values.Length; j++) {
                if (j == columns.Length - 1) {
                    result += "\'" + values[j] + "\')";
                    break;
                }
                result += "\'" + values[j] + "\',";
            }

            return result;
        }
    }
}
