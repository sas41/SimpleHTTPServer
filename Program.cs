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
                List<string> inputList = Console.ReadLine().Split().ToList();

                if (inputList[0] == "quit")
                {
                    break;
                }
                else if(inputList[0] == "start")
                {
                    if (inputList.Count > 4)
                    {
                        // "D:\\HTTP" "/index.html" "/404.html" "http://localhost:8080/"
                        myServer = new HTTPServer(inputList.Skip(4).ToList(), inputList[1], inputList[2], inputList[3]);
                    }
                    else
                    {
                        myServer = new HTTPServer(new List<string> { "http://localhost:8080/" }, "D:\\HTTP", "/index.html", "/404.html");
                    }

                    myServer.StartListening();
                }
                else if (inputList[0] == "stop")
                {
                    if (myServer != null)
                    {
                        myServer.StopListening();
                    }
                }
            }
        }
    }
}
