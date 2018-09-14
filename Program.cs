using System;
using System.IO;
using System.Linq;
using System.Net;

namespace SimpleHTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] prefixes = new string[] { "http://localhost:8080/" };
            SimpleListenerExample(prefixes, "D:\\HTTP", "\\index.html");
        }

        public static void SimpleListenerExample(string[] prefixes, string rootPath, string indexFile)
        {
            if (!HttpListener.IsSupported)
            {
                Log("OS Not supported!");
                return;
            }

            HttpListener listener = new HttpListener();

            if (prefixes == null || prefixes.Length == 0)
            {
                throw new ArgumentException("Invalid Prefix(es), Example: http://localhost:8080/");
            }

            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            listener.Start();
            Log("Listening...");
            Log("Root Path: " + rootPath);

            bool listening = true;

            while (listening)
            {
                //The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                string target = request.RawUrl.Replace("/","\\");

                //If no file/path is requested, serve the index file.
                if (target == "\\")
                {
                    target = indexFile;
                }

                string path = rootPath + target;
                Log("Target Path: " + target + Environment.NewLine + "Final Path: " + path);

                Log("Opening File...");
                StreamReader sr;
                string responseString = "";

                try
                {
                    sr = new StreamReader(path);

                    Log("Reading File...");
                    while (!sr.EndOfStream)
                    {
                        responseString += sr.ReadLine() + Environment.NewLine;
                    }
                    Log("Done!");

                    sr.Close();
                    sr.Dispose();
                }
                catch (Exception e)
                {
                    Log("Requested File was not found: " + path);
                    responseString = "404, File not Found!";
                }
                

                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // You must close the output stream.
                output.Close();
            }

            listener.Stop();
        }

        public static void Log(string s)
        {
            Console.WriteLine(s);
        }
    }
}
