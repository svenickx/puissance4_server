using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Config {
    internal class MySqlUtils {
        public static string GetDBstring() {
            string cs = "Server=127.0.0.1;Port=3300;Database=puissance4;Uid=root;Pwd=;";
            return cs;
        }
    }
}
