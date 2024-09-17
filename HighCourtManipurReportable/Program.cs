
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

using System.Drawing;
using System.Drawing.Imaging;
using Patagames.Ocr.Exceptions;
using Patagames.Ocr;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;

using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using System.Text.RegularExpressions;

namespace HighCourtManipurReportable;

class Program
{
    
    static async Task Main(string[] args)
    {
        // Set up Chrome options
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddUserProfilePreference("download.default_directory", @"C:\CourtDetails\ManipurCourt"); // Set your download folder path
        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false); // Disable download prompt
        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true); // Download PDFs directly instead of opening in Chrome

        // Initialize the Chrome WebDriver
        using (IWebDriver driver = new ChromeDriver(chromeOptions))
        {

            string url = "https://hcservices.ecourts.gov.in/ecourtindiaHC/cases/s_order.php?state_cd=25&dist_cd=1&court_code=1&stateNm=Manipur"; // Replace with your URL
            int maxRetries = 3;
            int retryCount = 0;
            bool success = false;

            // Retry navigation in case of failure
            while (retryCount < maxRetries && !success)
            {
                try
                {
                    driver.Navigate().GoToUrl(url);
                    success = true;
                }
                catch (WebDriverException ex)
                {
                    Console.WriteLine($"Error navigating to URL: {ex.Message}");
                    retryCount++;
                    if (retryCount < maxRetries)
                    {
                        Console.WriteLine("Retrying...");
                        await Task.Delay(3000); // Wait for 3 seconds before retrying
                    }
                    else
                    {
                        Console.WriteLine("Max retries reached. Exiting...");
                    }
                }
            }

            if (success)
            {

                string[] judgeNames = new string[]
                     {
                        
                        "Hon'ble The Acting Chief Justice",
                        "Hon'ble Mr Justice MV Muralidaran",
                        "Hon'ble Mr Justice Ahanthem Bimol Singh",
                        "Hon'ble Mr Justice A.Guneshwar Sharma",
                        "Hon'ble Mrs. Justice Golmei Gaiphulshillu Kabui",
                        "W. TONEN MEITEI",
                        "YUMKHAM ROTHER - Presiding Officer",
                        "K BRAJAKUMAR SHARMA - Member",
                        "MAIBAM BINOYKUMAR SINGH",
                        "OJESH MUTUM - MEMBER",
                        "Hon'ble Mr Justice Kh Nobin Singh",
                        "Hon'ble Mr Justice N Kotiswar Singh",
                        "Honble Mr Justice Lanusungkum Jamir",
                        "Hon'ble Mr Justice Songkhupchung Serto",
                        "Maibam Manojkumar Singh - PRESIDING OFFICER",
                        "R.K. MEMCHA DEVI - MEMBER",
                        "Hon'ble Registrar",
                        "KH. AJIT SINGH - MEMBER"
                     };

                int judgeIndex = 0;



                while (judgeIndex < judgeNames.Length)
                {
                    DateTime today = DateTime.Today;
                    DateTime oneMonthAgo = today.AddMonths(-1);
                    string fromDate = "01-01-1997";  // As per your example
                    string toDate = "16-09-2024";    // As per your example

                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    // Select the judge from the dropdown
                    SelectElement judgeDropdown = new SelectElement(wait.Until(d => d.FindElement(By.Id("nnjudgecode1"))));
                    judgeDropdown.SelectByText(judgeNames[judgeIndex]);


                    // Fill the 'from_date' field
                    IWebElement fromDateElement = wait.Until(d => d.FindElement(By.Id("from_date")));
                    fromDateElement.Clear();
                    fromDateElement.SendKeys(fromDate);

                    // Fill the 'to_date' field
                    IWebElement toDateElement = wait.Until(d => d.FindElement(By.Id("to_date")));
                    toDateElement.Clear();
                    toDateElement.SendKeys(toDate);
                    IWebElement bodyElement = driver.FindElement(By.TagName("body"));
                    bodyElement.Click();

                    // Select "Yes" for Reportable Judgements
                    SelectElement reportableJudgesDropdown = new SelectElement(wait.Until(d => d.FindElement(By.Id("reportableJudges"))));
                    reportableJudgesDropdown.SelectByValue("Y");

                    // Select "Judgment" for Type of Orders
                    SelectElement typeOfOrdersDropdown = new SelectElement(wait.Until(d => d.FindElement(By.Id("typeOfOrders"))));
                    typeOfOrdersDropdown.SelectByValue("283");



                    bool captchaCracked = false;

                    while(!captchaCracked)
                    {
                        // Capture the CAPTCHA image element
                        IWebElement captchaImageElement = wait.Until(d => d.FindElement(By.Id("captcha_image")));

                        // Capture a screenshot of the entire page
                        Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();

                        string captchaImagePath = string.Empty;
                        using (var fullImage = new Bitmap(new MemoryStream(screenshot.AsByteArray)))
                        {
                            
                            // Get the location and size of the CAPTCHA element
                            var elementLocation = captchaImageElement.Location;
                            var elementSize = captchaImageElement.Size;

                            // Create a new bitmap with the size of the CAPTCHA image
                            using (var elementScreenshot = new Bitmap(elementSize.Width, elementSize.Height))
                            {
                                using (var graphics = Graphics.FromImage(elementScreenshot))
                                {
                                    // Draw the portion of the screenshot that corresponds to the CAPTCHA element
                                    graphics.DrawImage(fullImage, new Rectangle(0, 0, elementSize.Width, elementSize.Height),
                                        new Rectangle(elementLocation.X, elementLocation.Y, elementSize.Width, elementSize.Height),
                                        GraphicsUnit.Pixel);
                                }

                                // Save the CAPTCHA image to a file
                                string projectDirectory = Directory.GetCurrentDirectory();
                                string imgFolderPath = System.IO.Path.Combine(projectDirectory, "img");

                                if (!Directory.Exists(imgFolderPath))
                                {
                                    Directory.CreateDirectory(imgFolderPath);
                                }

                                captchaImagePath = System.IO.Path.Combine(imgFolderPath, "captcha.png");
                                elementScreenshot.Save(captchaImagePath, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                        

                        // Process the CAPTCHA image to get the text (you need to implement this OCR function)
                        string captchaText = ScanTextFromImage(captchaImagePath);

                        // Fill the CAPTCHA input field
                        IWebElement captchaInputElement = wait.Until(d => d.FindElement(By.Id("captcha")));
                        captchaInputElement.SendKeys(captchaText);

                        // Locate the button by its attributes and submit the form
                        IWebElement submitButton = wait.Until(d => d.FindElement(By.XPath("//input[@name='submit1']")));

                        // Scroll into view to ensure it's visible
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", submitButton);
                        Thread.Sleep(1000);
                        // Click the button to submit the form
                        submitButton.Click();
                        wait.Until(d => string.IsNullOrEmpty(d.FindElement(By.Id("captcha")).GetAttribute("value")));

                        // Wait for the next page to load
                        //await Task.Delay(10000);

                        await Task.Delay(1000);

                        try
                        {
                            // Check if the table with id="showList3" is present (CAPTCHA solved and records found)
                            try
                            {
                                IWebElement tableElement = wait.Until(d => d.FindElement(By.Id("showList3")));
                                Thread.Sleep(1000);
                                if (tableElement.Displayed)
                                {
                                    Console.WriteLine("CAPTCHA solved and records found for the judge.");

                                    // Extract table headings
                                    var headings = tableElement.FindElement(By.TagName("thead"))
                                                               .FindElement(By.TagName("tr"))
                                                               .FindElements(By.TagName("th"))
                                                               .Select(th => th.Text.Trim())
                                                               .ToArray();

                                    // Extract table rows
                                    var rows = tableElement.FindElement(By.TagName("tbody"))
                                                           .FindElements(By.TagName("tr"));

                                    var tableData = new List<Dictionary<string, string>>();

                                    foreach (var row in rows)
                                    {
                                        var casedetail = new caseDetail();
                                        casedetail.Court = "Manipur High Court";
                                        casedetail.Abbr = "MAN";
                                        var cells = row.FindElements(By.TagName("td"));
                                        if (cells.Count >= 4)
                                        {
                                            var pdfLink = cells[3].FindElement(By.TagName("a")).GetAttribute("href");
                                            var pdfLinkElement = cells[3].FindElement(By.TagName("a"));
                                            var caseNo = cells[1].Text.Trim();
                                            var rowData = new Dictionary<string, string>
                                            {
                                                { headings[0], cells[0].Text.Trim() },
                                                { headings[1], cells[1].Text.Trim() },
                                                { headings[2], cells[2].Text.Trim() },
                                                { headings[3], cells[3].FindElement(By.TagName("a")).GetAttribute("href") }
                                            };
                                            var caseInfo = rowData["Case Type/Case Number/Case Year"].Split('/');

                                            if (caseInfo.Length == 3)
                                            {
                                                casedetail.Type = rowData["Case Type/Case Number/Case Year"];
                                                casedetail.CaseNo = $"WA {caseInfo[1]} of {caseInfo[2]}";
                                            }
                                            casedetail.Dated = rowData["Order Date"];

                                            // Print the key-value pairs for the row
                                            foreach (var kvp in rowData)
                                            {
                                                Console.WriteLine($"{kvp.Key} - {kvp.Value}");
                                            }
                                            Console.WriteLine(); // New line for better readability

                                            // Add the row data to the list
                                            tableData.Add(rowData);


                                            pdfLinkElement.Click();

                                            await Task.Delay(4000);

                                            string downloadDirectory = @"C:\CourtDetails\ManipurCourt";
                                            string sanitizedCaseNo = caseNo.Replace("/", "_"); // Replace / with _ or any valid character
                                            string newFileName = $"{sanitizedCaseNo}.pdf";

                                            

                                            string downloadedFile = GetLatestDownloadedFile(downloadDirectory);

                                            if (!string.IsNullOrEmpty(downloadedFile))
                                            {
                                                string newFilePath = System.IO.Path.Combine(downloadDirectory, newFileName);



                                                if (File.Exists(newFilePath))
                                                {
                                                    // Delete the existing file
                                                    File.Delete(newFilePath);
                                                    Console.WriteLine($"Existing file at {newFilePath} deleted.");
                                                }

                                                File.Move(downloadedFile, newFilePath); // Rename the file
                                                Console.WriteLine($"Downloaded file renamed to: {newFilePath}");


                                                casedetail.PdfLink = newFilePath;

                                                using(var db=new AppDbContext())
                                                {
                                                    db.CaseDetails.Add(casedetail);
                                                    db.SaveChanges();
                                                }

                                                // Step 2: Extract text from the renamed PDF file
                                                //string pdfText = ExtractTextFromPdf(newFilePath);

                                                //Console.WriteLine(pdfText);

                                                //// Step 3: Extract case details from the extracted text
                                                //var caseDetail = ExtractCaseDetails(pdfText);

                                                //// Step 4: Display or handle the case details
                                                //Console.WriteLine($"Court: {caseDetail.Court}");
                                                //Console.WriteLine($"Case No: {caseDetail.CaseNo}");
                                                //Console.WriteLine($"Dated: {caseDetail.Dated}");
                                                //Console.WriteLine($"Case Name: {caseDetail.CaseName}");
                                                //Console.WriteLine($"Counsel: {caseDetail.Counsel}");
                                                //Console.WriteLine($"Coram: {string.Join(", ", caseDetail.Coram ?? new string[0])}");
                                                //Console.WriteLine($"Coram Count: {caseDetail.CoramCount}");
                                                //Console.WriteLine($"Petitioner: {caseDetail.Petitioner}");
                                                //Console.WriteLine($"Respondent: {caseDetail.Respondent}");
                                                //Console.WriteLine($"Result: {caseDetail.Result}");
                                                //Console.WriteLine($"Filename: {caseDetail.Filename}");

                                            }
                                            else
                                            {
                                                Console.WriteLine("No downloaded file found.");
                                            }

                                            //await ProcessPdfAsync(pdfLink);
                                        }
                                    }



                                    //database operation 

                                    captchaCracked = true; // Move to the next judge
                                }
                                else
                                {
                                    IWebElement txtMsgElement = wait.Until(d => d.FindElement(By.Id("txtmsg")));
                                    string textValue = txtMsgElement.GetAttribute("value");

                                    // Handle "Invalid Captcha"
                                    if (textValue == "Invalid Captcha")
                                    {
                                        Console.WriteLine("Invalid CAPTCHA detected, retrying...");
                                        await Task.Delay(1000); // Small delay before retrying
                                    }
                                    // Handle "No records found" (CAPTCHA cracked but no records for the judge)
                                    else if (textValue == "No records found")
                                    {
                                        Console.WriteLine("CAPTCHA cracked but no records found for the judge. Moving to next judge...");
                                        captchaCracked = true; // Proceed to the next judge
                                    }
                                }
                            }
                            catch (WebDriverTimeoutException)
                            {
                                
                                    Console.WriteLine("Unexpected error: No element found.");
                                
                            }
                        }
                        catch (NoSuchElementException)
                        {
                            Console.WriteLine("Unexpected error: No element found.");
                        }
                        Thread.Sleep(2000);

                        // Move to the next judge if CAPTCHA is solved
                        if (captchaCracked)
                        {
                            judgeIndex++;
                        }
                    }
                    if (judgeIndex >= judgeNames.Length)
                    {
                        Console.WriteLine("All judges processed.");
                    }

                }






            }
        }
    }

    private static caseDetail ExtractCaseDetails(string pdfText)
    {
       
            caseDetail caseDetails = new caseDetail();

            // Extract Court
            string courtPattern = @"IN THE (.*?)\n";
            Match courtMatch = Regex.Match(pdfText, courtPattern);
            caseDetails.Court = courtMatch.Success ? courtMatch.Groups[1].Value.Trim() : null;

            // Extract Case Number
            string caseNoPattern = @"WA No\.\s*(.*?)\s*Ref";
            Match caseNoMatch = Regex.Match(pdfText, caseNoPattern);
            caseDetails.CaseNo = caseNoMatch.Success ? caseNoMatch.Groups[1].Value.Trim() : null;


            // Extract Date of Judgment & Order
            string datePattern = @"Date of Judgment & Order\s*::\s*(.*?)\n";
            Match dateMatch = Regex.Match(pdfText, datePattern);
            caseDetails.Dated = dateMatch.Success ? dateMatch.Groups[1].Value.Trim() : null;

            // Extract Case Name
            string caseNamePattern = @"Shri (.*?)(?:\s*\.\.\s*Writ Appellant|(?:\s*Versus\s*|\s*For the|))";
            Match caseNameMatch = Regex.Match(pdfText, caseNamePattern);
            caseDetails.CaseName = caseNameMatch.Success ? caseNameMatch.Groups[1].Value.Trim() : null;

            // Extract Counsel
            string counselPattern = @"For the appellants\s*::\s*(.*?)\s*For the Respondents";
            Match counselMatch = Regex.Match(pdfText, counselPattern, RegexOptions.Singleline);
            caseDetails.Counsel = counselMatch.Success ? counselMatch.Groups[1].Value.Trim() : null;

            // Extract Coram
            string coramPattern = @"BEFORE\s*(.*?)\n";
            Match coramMatch = Regex.Match(pdfText, coramPattern, RegexOptions.Singleline);
            caseDetails.Coram = coramMatch.Success ? coramMatch.Groups[1].Value.Trim().Split('\n') : null;

            // Extract Petitioner and Respondents
            string petitionerPattern = @"Shri (.*?),\s*aged about";
            Match petitionerMatch = Regex.Match(pdfText, petitionerPattern);
            caseDetails.Petitioner = petitionerMatch.Success ? petitionerMatch.Groups[1].Value.Trim() : null;

            string respondentPattern = @"(?:1\.|2\.|3\.)\s*(.*?)(?=\s*\.\.\s*Official Respondents|\s*\.\.\s*Private Respondent|$)";
            MatchCollection respondentMatches = Regex.Matches(pdfText, respondentPattern);
            caseDetails.Respondent = string.Join(", ", respondentMatches.Select(m => m.Groups[1].Value.Trim()));

            return caseDetails;
        
    }

 
    private static string ExtractTextFromPdf(string pdfPath)
    {
       


        StringBuilder text = new StringBuilder();

        using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(pdfPath))
        {
            // Determine the number of pages to process (maximum of 2 or the actual number of pages)
            int maxPages = Math.Min(reader.NumberOfPages, 2);

            // Loop through the first two pages or up to the total number of pages, whichever is smaller
            for (int i = 1; i <= maxPages; i++)
            {
                text.Append(iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, i));
            }
        }

        return text.ToString();
    }

    private static string GetLatestDownloadedFile(string downloadDirectory)
    {
        var directoryInfo = new DirectoryInfo(downloadDirectory);

        // Get the most recent PDF file
        var file = directoryInfo.GetFiles("*.pdf")
                                .OrderByDescending(f => f.LastWriteTime)
                                .FirstOrDefault();

        return file?.FullName;
    }


    //private static async Task<string> DownloadPdfAsync(string pdfLink, string caseNo)
    //{
    //    // Define the folder where you want to save the PDF
    //    string folderPath = @"C:\CourtDetails\ManipurCourt";

    //    try
    //    {
    //        // Ensure the directory exists
    //        if (!Directory.Exists(folderPath))
    //        {
    //            Directory.CreateDirectory(folderPath);
    //        }

    //        // Ensure caseNo is not null and sanitize it if needed
    //        var sanitizedCaseNo = string.IsNullOrWhiteSpace(caseNo) ? "DefaultName" : caseNo;
    //        sanitizedCaseNo = new string(sanitizedCaseNo.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

    //        // Create the full file path with the sanitized case number
    //        var filePath = Path.Combine(folderPath, $"{sanitizedCaseNo}.pdf");

    //        using (var httpClient = new HttpClient())
    //        {
    //            // Send a GET request to the PDF URL
    //            var response = await httpClient.GetAsync(pdfLink);

    //            // Check if the response indicates success
    //            if (!response.IsSuccessStatusCode)
    //            {
    //                throw new Exception($"Failed to download PDF. Status code: {response.StatusCode}");
    //            }

    //            // Ensure the content type is PDF
    //            if (response.Content.Headers.ContentType.MediaType != "application/pdf")
    //            {
    //                throw new Exception("The URL does not point to a PDF file.");
    //            }

    //            // Read and save the PDF file
    //            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
    //            {
    //                await response.Content.CopyToAsync(fileStream);
    //            }
    //        }

    //        // Return the path to the downloaded file
    //        return filePath;
    //    }
    //    catch (Exception ex)
    //    {
    //        // Handle exceptions, such as network errors or file access issues
    //        Console.WriteLine($"Error downloading PDF: {ex.Message}");
    //        return string.Empty;
    //    }
    //}
    //private static async Task ProcessPdfAsync(string pdfLink)
    //{
    //    try
    //    {
    //        var pdfText = await ExtractTextFromPdfAsync(pdfLink);

    //        // Process the extracted text
    //        Console.WriteLine("Text from PDF:");
    //        Console.WriteLine(pdfText);

           
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error processing PDF: {ex.Message}");
    //    }
    //}

//    private static async Task<string> ExtractTextFromPdfAsync(string pdfUrl)
//{
//        try
//        {
//            using var httpClient = new HttpClient();

//            // Download the PDF as a byte array
//            var pdfBytes = await httpClient.GetByteArrayAsync(pdfUrl);

//            // Check if the downloaded content is a valid PDF
//            if (pdfBytes.Length < 5 || !pdfBytes.Take(5).SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2d })) // Check for "%PDF-" header
//            {
//                throw new InvalidDataException("Downloaded file is not a valid PDF.");
//            }

//            using var pdfStream = new MemoryStream(pdfBytes);

//            // Create a PdfDocument from the stream
//            using var pdfDoc = new iText.Kernel.Pdf.PdfDocument(new PdfReader(pdfStream));

//            // Extract text from the first page
//            var page = pdfDoc.GetPage(1);
//            var strategy = new SimpleTextExtractionStrategy();
//            var text = PdfTextExtractor.GetTextFromPage(page, strategy);

//            return text;
//        }
//        catch (HttpRequestException httpEx)
//        {
//            // Handle HTTP request errors
//            Console.WriteLine($"HTTP Request error: {httpEx.Message}");
//            throw;
//        }
//        catch (InvalidDataException dataEx)
//        {
//            // Handle invalid PDF data errors
//            Console.WriteLine($"Invalid data error: {dataEx.Message}");
//            throw;
//        }
//        catch (Exception ex)
//        {
//            // Handle other errors
//            Console.WriteLine($"Unexpected error: {ex.Message}");
//            throw;
//        }
//    }

    private static string ScanTextFromImage(string imagePath)
    {
        if (!System.IO.File.Exists(imagePath))
        {
            Console.WriteLine("Image file not found.");
            return string.Empty;
        }

        try
        {
            // Preprocess the image to enhance OCR accuracy (grayscale, resize)
            string preprocessedImagePath = PreprocessImage(imagePath);

            using (var objOcr = OcrApi.Create())
            {
                // Initialize the OCR engine with the English language (detects alphabets and numbers by default)
                objOcr.Init(Patagames.Ocr.Enums.Languages.English);

                // Extract and return the text from the preprocessed image
                string plainText = objOcr.GetTextFromImage(preprocessedImagePath);
                string cleanedText = System.Text.RegularExpressions.Regex.Replace(plainText, @"[^a-zA-Z0-9]", "");

                return string.IsNullOrEmpty(cleanedText) ? "No text found." : cleanedText.Length > 5 ? cleanedText.Substring(0, 5) : cleanedText;
            }
        }
        catch (OcrException ocrEx)
        {
            Console.WriteLine($"OCR error occurred: {ocrEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return string.Empty;
    }

    private static string PreprocessImage(string imagePath)
    {
        // Define the directory for preprocessed images
        string directoryPath = System.IO.Path.GetDirectoryName(imagePath);
        string preprocessedPath = System.IO.Path.Combine(directoryPath, "preprocessed_" + System.IO.Path.GetFileName(imagePath));

        try
        {
            using (Bitmap originalImage = new Bitmap(imagePath))
            {
                // Convert the image to grayscale for better contrast
                using (Bitmap grayscaleImage = ConvertToGrayscale(originalImage))
                {
                    // Optionally resize the image to improve small text detection
                    using (Bitmap resizedImage = ResizeImage(grayscaleImage, 2.0)) // Resize 2x
                    {
                        // Save the preprocessed image
                        resizedImage.Save(preprocessedPath, ImageFormat.Png);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during image preprocessing: {ex.Message}");
        }

        return preprocessedPath;
    }

    /// <summary>
    /// Converts an image to grayscale to reduce noise and improve OCR accuracy.
    /// </summary>
    private static Bitmap ConvertToGrayscale(Bitmap originalImage)
    {
        Bitmap grayscaleImage = new Bitmap(originalImage.Width, originalImage.Height);

        for (int x = 0; x < originalImage.Width; x++)
        {
            for (int y = 0; y < originalImage.Height; y++)
            {
                Color originalColor = originalImage.GetPixel(x, y);
                int grayValue = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11);
                Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                grayscaleImage.SetPixel(x, y, grayColor);
            }
        }

        return grayscaleImage;
    }

    /// <summary>
    /// Resizes the image by the given scale factor to help the OCR engine detect smaller text.
    /// </summary>
    private static Bitmap ResizeImage(Bitmap originalImage, double scale)
    {
        int newWidth = (int)(originalImage.Width * scale);
        int newHeight = (int)(originalImage.Height * scale);
        Bitmap resizedImage = new Bitmap(newWidth, newHeight);

        using (Graphics g = Graphics.FromImage(resizedImage))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
        }

        return resizedImage;
    }


}



