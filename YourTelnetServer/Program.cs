using System;
using System.Net;

namespace YourTelnetServer
{
    class Program
    {
        private static Server server;
        static void Main(string[] args)
        {
            server = new Server(IPAddress.Any, 23);
            server.Start();
            char read = Console.ReadKey(true).KeyChar;

            do
            {

            } while ((read = Console.ReadKey(true).KeyChar) != 'q');
        }
    }
}
