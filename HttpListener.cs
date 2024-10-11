using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace HttpListenerExample
{
  class HttpServer
  {
    public static HttpListener listener;
    public static string url = "http://localhost:8000/";
    public static int pageViews = 0;
    public static int requestCount = 0;
    public static string pageData =
        "<!DOCTYPE>" +
        "<html>" +
        "  <head>" +
        "    <title>HttpListener Example</title>" +
        "  </head>" +
        "  <body>" +
        "    <p>Page Views: {0}</p>" +
        "    <form method=\"post\" action=\"shutdown\">" +
        "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
        "    </form>" +
        "    <form method=\"get\" action=\"javascript\">" +
        "      <input type=\"submit\" value=\"javascript\" {1}>" +
        "    </form>" +
        "  </body>" +
        "</html>";


    public static async Task HandleIncomingConnections()
    {
      bool runServer = true;

      // While a user hasn't visited the `shutdown` url, keep on handling requests
      while (runServer)
      {
        // Will wait here until we hear from a connection
        HttpListenerContext ctx = await listener.GetContextAsync();

        // Peel out the requests and response objects
        HttpListenerRequest req = ctx.Request;
        HttpListenerResponse resp = ctx.Response;

        // Print out some info about the request
        Console.WriteLine("================================================");
        Console.WriteLine("Request #: {0}", ++requestCount);
        Console.WriteLine(req.Url.ToString());
        Console.WriteLine(req.HttpMethod);
        Console.WriteLine(req.UserHostName);
        Console.WriteLine(req.UserAgent);
        Console.WriteLine("================================================");

        // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
        if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
        {
          Console.WriteLine("Shutdown requested");
          runServer = false;
        }

        // Testing for javascript
        if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/javascript"))
        {
          Console.WriteLine("Javascript requested");
          resp.ContentType = "text/html";
          string script = "<!DOCTYPE>" +
                            "<html>" +
                            "  <head>" +
                            "    <title>JAVASCRIPT</title>" +
                            "  </head>" +
                            "  <body>" +
                            "<h1>Hello, world!</h1>" +
                            "  </body>" +
                            "    <script>console.log('Hello, world!');</script>" +
                            "</html>";
          byte[] scriptBytes = Encoding.UTF8.GetBytes(script);
          await resp.OutputStream.WriteAsync(scriptBytes, 0, scriptBytes.Length);
          return;
        }

        // Make sure we don't increment the page views counter if `favicon.ico` is requested
        if (req.Url.AbsolutePath != "/favicon.ico")
          pageViews += 1;

        // Write the response info
        string disableSubmit = !runServer ? "disabled" : "";
        byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews, disableSubmit));
        resp.ContentType = "text/html";
        resp.ContentEncoding = Encoding.UTF8;
        resp.ContentLength64 = data.LongLength;

        // Write out to the response stream (asynchronously), then close it
        await resp.OutputStream.WriteAsync(data, 0, data.Length);
        resp.Close();

      }
    }


    public static void Run()
    {
      // Create a Http server and start listening for incoming connections
      listener = new HttpListener();
      listener.Prefixes.Add(url);
      listener.Start();
      Console.WriteLine("Listening for connections on {0}", url);

      // Handle requests
      Task listenTask = HandleIncomingConnections();
      listenTask.GetAwaiter().GetResult();

      // Close the listener
      listener.Close();
    }
  }
}
