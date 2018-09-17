using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SimpleHTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> prefixes = new List<string> { "http://localhost:8080/" };

            HTTPServer myServer = new HTTPServer(prefixes, "D:\\HTTP", "\\index.html");

            string command;

            while (true)
            {
                command = Console.ReadLine();

                if (command == "quit")
                {
                    break;
                }
                else if(command == "start")
                {
                    myServer.StartListening();
                }
                else if (command == "stop")
                {
                    myServer.StopListening();
                }
            }
        }
    }
}
