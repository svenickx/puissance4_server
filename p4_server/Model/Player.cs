using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Model {
    internal class Player {
        public string id { get; set; }
        public string name { get; set; }
        public Player(string id, string name) {
            this.id = id;
            this.name = name;
        }
    }
}
