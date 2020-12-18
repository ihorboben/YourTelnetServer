using System;
using System.Net;

namespace YourTelnetServer
{
    class Program
    {
         private static Server server;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("specify port number");
                return ;
            }

            if (int.TryParse(args[0], out int port))
            {
                server = new Server(IPAddress.Any, port);
                server.Start();
                char read = Console.ReadKey(true).KeyChar;

                do
                {

                } while ((read = Console.ReadKey(true).KeyChar) != 'q');
            }
            else
            {
                Console.WriteLine("specify port number");
            }
        }
    }
}
