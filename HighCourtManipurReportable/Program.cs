
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

using System.Drawing;
using System.Drawing.Imaging;
using Patagames.Ocr.Exceptions;
using Patagames.Ocr;



namespace HighCourtManipurReportable;

class Program
{
    static async Task Main(string[] args)
    {
        // Initialize the Chrome WebDriver
        using (IWebDriver driver = new ChromeDriver())
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
                        "Hon'ble The Chief Justice",
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


                    int maxCaptchaRetries = 5;
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
                                string imgFolderPath = Path.Combine(projectDirectory, "img");

                                if (!Directory.Exists(imgFolderPath))
                                {
                                    Directory.CreateDirectory(imgFolderPath);
                                }

                                captchaImagePath = Path.Combine(imgFolderPath, "captcha.png");
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

                        // Click the button to submit the form
                        submitButton.Click();


                        // Wait for the next page to load
                        //await Task.Delay(10000);


                        // Check if the invalid captcha message appears
                        try
                        {

                            // Check if the invalid CAPTCHA message appears
                            IWebElement txtMsgElement = wait.Until(d => d.FindElement(By.Id("txtmsg")));
                            string title = txtMsgElement.GetAttribute("title");
                            // Read the value of the input element
                            string textValue = txtMsgElement.GetAttribute("value");
                            Console.WriteLine("Text inside the input element: " + textValue);


                            if (textValue == "Invalid Captcha")
                            {
                                Console.WriteLine("Invalid CAPTCHA detected, retrying...");
                            }
                            else
                            {
                                // If no invalid CAPTCHA message, it means CAPTCHA was solved
                                Console.WriteLine("CAPTCHA solved. Proceeding to next judge...");
                                captchaCracked = true;
                            }
                        }
                        catch (NoSuchElementException)
                        {
                            // CAPTCHA cracked, no invalid message found
                            Console.WriteLine("CAPTCHA solved successfully.");
                            captchaCracked = true;
                        }

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



