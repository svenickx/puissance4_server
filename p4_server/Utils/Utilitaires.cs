using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_server.Utils
{
    internal class Utilitaires
    {

        /// <summary>
        /// Performs a fake loading
        /// </summary>
        public static void FakeLoading()
        {
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(1000);
                Console.Write(".");
            }
            Thread.Sleep(1000);
        }
    }
}
