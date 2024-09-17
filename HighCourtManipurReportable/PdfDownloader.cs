using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighCourtManipurReportable
{
    public class PdfDownloader
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> DownloadPdfAsync(string url, string filePath)
        {
            try
            {
                // Send a GET request to the URL
                HttpResponseMessage response = await client.GetAsync(url);
                //if (response.Content.Headers.ContentType.MediaType != "application/pdf")
                //{
                //    return "The URL does not point to a PDF file.";
                //}
                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the content as a byte array
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                // Save the byte array to a file
                await File.WriteAllBytesAsync(filePath, fileBytes);

                Console.WriteLine("PDF downloaded and saved successfully.");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
