using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SimpleHTTPServer
{
    class HTTPServer
    {
        private List<string> aliases;
        private string rootPath;
        private string indexFile;

        public List<string> Aliases { get => aliases; set => aliases = value; }
        public string RootPath { get => rootPath; set => rootPath = value; }
        public string IndexFile { get => indexFile; set => indexFile = value; }

        HttpListener listener;
        private bool isLive = false;



        public HTTPServer(List<string> aliases,string rootPath, string indexFile)
        {
            Aliases = new List<string>();
            foreach (var alias in aliases)
            {
                Aliases.Add(alias);
            }
            RootPath = rootPath;
            IndexFile = indexFile;
        }

        public void StartListening()
        {
            if (isLive)
            {
                throw new Exception("Listener Already Running!");
            }

            else if (!HttpListener.IsSupported)
            {
                throw new Exception("OS Not Supported!");
            }

            listener = new HttpListener();

            if (Aliases == null || Aliases.Count() == 0)
            {
                throw new ArgumentException("You must add an Alias/Prefix!");
            }

            try
            {
                foreach (string alias in aliases)
                {
                    listener.Prefixes.Add(alias);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Alias(es)/Prefix(es), Example: http://localhost:8080");
            }

            listener.Start();
            Log("Listening...");
            Log("Root Path: " + RootPath);
            isLive = true;

            IAsyncResult Result = listener.BeginGetContext(new AsyncCallback(OnContextRequest), listener);
        }

        public void OnContextRequest(IAsyncResult result)
        {
            //The GetContext method blocks while waiting for a request.
            //We use EndGetContext and BeginGetContext instead.
            //As soon as we are done with the last context, we should be listening for the next.
            //That way we can reply to many at the same time.
            HttpListenerContext context;

            try
            {
                context = listener.EndGetContext(result);
            }
            catch (Exception)
            {
                Log("Listener Stopped, Stopping Thread");
                return;
            }

            listener.BeginGetContext(OnContextRequest, listener);

            //Simulate Delay
            //Thread.Sleep(5000);

            //Get the specifics of the request and extract the target file/directory.
            HttpListenerRequest request = context.Request;
            string target = request.RawUrl.Replace("/", "\\");

            //If no file/path is requested, serve the index file.
            if (target == "\\")
            {
                target = IndexFile;
            }

            string path = RootPath + target;
            Log("Target Path: " + target);
            Log("Final Path: " + path);

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
            catch (Exception)
            {
                Log("File Read Error: " + path);
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


        public void StopListening()
        {
            isLive = false;
            listener.Stop();
        }

        public void Log(string s)
        {
            string time = DateTime.UtcNow.ToString("HH:mm:ss.fff");
            string thread = Thread.CurrentThread.ManagedThreadId.ToString();
            Console.WriteLine(time + " - " + thread + " - " + s);
        }
    }
}
