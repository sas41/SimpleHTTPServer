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
            //List<string> prefixes = new List<string> {  };

            HTTPServer myServer = null;
            
            while (true)
            {
                if (args.Count() < 5)
                {
                    args = Console.ReadLine().Split().ToArray();
                }

                if (args[0] == "quit")
                {
                    break;
                }
                else if (args[0] == "start")
                {
                    if (args.Count() > 4)
                    {
                        // "D:\\HTTP" "/index.html" "/404.html" "http://localhost:8080/"
                        NetAclChecker.AddAddress(args.Skip(4).First().ToString());
                        myServer = new HTTPServer(args.Skip(4).ToList(), args[1], args[2], args[3]);
                    }
                    else
                    {
                        myServer = new HTTPServer(new List<string> { "http://localhost:8080/" }, "E:\\Servers\\Web", "/index.html", "/404.html");
                    }

                    myServer.StartListening();
                }
                else if (args[0] == "stop")
                {
                    if (myServer != null)
                    {
                        myServer.StopListening();
                    }
                }

                args = new string[5];

            }
        }
    }
}
