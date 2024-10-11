using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientExample
{
  class Downloader
  {
    // Where to download from, and where to save it to
    public static string urlToDownload = "https://artoodeeto.github.io/portfolio/pdf/resume.pdf";
    public static string filename = "samp.pdf";

    public static async Task DownloadWebPage()
    {
      Console.WriteLine("Starting download...");

      // Setup the HttpClient
      using (HttpClient httpClient = new HttpClient())
      {
        // Get the webpage asynchronously
        HttpResponseMessage resp = await httpClient.GetAsync(urlToDownload);

        // If we get a 200 response, then save it
        if (resp.IsSuccessStatusCode)
        {
          Console.WriteLine("Download Success!");

          // Get the data
          byte[] data = await resp.Content.ReadAsByteArrayAsync();

          // Save it to a file
          Console.WriteLine("Saving to file...");
          FileStream fStream = File.Create(filename);
          await fStream.WriteAsync(data, 0, data.Length);
          fStream.Close();

          Console.WriteLine("Done!");
        }
      }
    }

    // Run this in main
    // Downloader.DownloadWebPage().GetAwaiter().GetResult();
  }
}