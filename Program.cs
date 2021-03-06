﻿using System;
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

                if (args[0] == "start")
                {
                    if (args.Count() > 5)
                    {
                        // Example: D:/HTTPServer /index.html /404.html false http://localhost:8080/
                        myServer = new HTTPServer(args.Skip(5).ToList(), args[1], args[2], args[3], bool.Parse(args[4]));
                        myServer.StartListening();
                    }
                    else
                    {
                        Logger.Log("Missing input data!");
                        Logger.Log("Example: D:/HTTPServer /index.html /404.html false http://localhost:8080/");
                    }
                }
                else if(args[0] == "netacl")
                {
                    NetAclChecker.AddAddresses(args.Skip(1).ToList());
                }
                else if (args[0] == "stop")
                {
                    if (myServer != null)
                    {
                        myServer.StopListening();
                    }
                }
                else if (args[0] == "quit")
                {
                    break;
                }

                args = new string[5];

            }
        }
    }
}
