using Adobe.PDFServicesSDK;
using Adobe.PDFServicesSDK.auth;
using Adobe.PDFServicesSDK.io;
using Adobe.PDFServicesSDK.pdfjobs.jobs;
using Adobe.PDFServicesSDK.pdfjobs.parameters.htmltopdf;
using Adobe.PDFServicesSDK.pdfjobs.results;
using CV_2025.CristalVision.Vision;
using System.IO.Compression;
using System.Xml;
using static CV_2025.CristalVision.Vision.Page;

namespace CV_2025.wwwroot.CristalVision.Output
{
    public class PDF
    {
        Page page;

        public PDF(Page page)
        {
            this.page = page;
        }

        /// <summary>
        /// Place characters/shapes/equations/tables on PDF
        /// </summary>
        public void Save()
        {
            // Initial setup, create credentials instance
            ICredentials credentials = new ServicePrincipalCredentials("20c01344b4e44beaa52a2b586697e24d", "p8e-tnh3WDy-PttJcq_GIkqlTGQnNvQ4ybbM");

            var pdfServices = new PDFServices(credentials);

            // Creates an asset(s) from source file(s) and upload
            Stream inputStream = CreateZip(CreateHTML());
            IAsset asset = pdfServices.Upload(inputStream, PDFServicesMediaType.ZIP.GetMIMETypeValue());

            // Create parameters for the job
            HTMLToPDFParams htmlToPDFParams = GetHTMLToPDFParams();

            // Creates a new job instance
            HTMLToPDFJob htmlToPDFJob = new HTMLToPDFJob(asset).SetParams(htmlToPDFParams);

            // Submits the job and gets the job result
            String location = pdfServices.Submit(htmlToPDFJob);

            PDFServicesResponse<HTMLToPDFResult> pdfServicesResponse = pdfServices.GetJobResult<HTMLToPDFResult>(location, typeof(HTMLToPDFResult));

            // Get content from the resulting asset(s)
            IAsset resultAsset = pdfServicesResponse.Result.Asset;
            StreamAsset streamAsset = pdfServices.GetContent(resultAsset);

            // Creating output streams and copying stream asset's content to it
            String outputFilePath = "/wwwroot/CristalVision/create.pdf";
            new FileInfo(Directory.GetCurrentDirectory() + outputFilePath).Directory.Create();
            Stream outputStream = File.OpenWrite(Directory.GetCurrentDirectory() + outputFilePath);
            streamAsset.Stream.CopyTo(outputStream);
            outputStream.Close();
        }

        static HTMLToPDFParams GetHTMLToPDFParams()
        {
            // Define the page layout, in this case an 20 x 25 inch page (effectively portrait orientation).
            PageLayout pageLayout = new PageLayout();
            pageLayout.SetPageSize(20, 25);

            // Set the desired HTML-to-PDF conversion options.
            HTMLToPDFParams htmlToPDFParams = HTMLToPDFParams.HTMLToPDFParamsBuilder()
                .IncludeHeaderFooter(true)
                .WithPageLayout(pageLayout)
                .Build();
            return htmlToPDFParams;
        }

        static MemoryStream CreateZip(string content)
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var fileName = $"index.html";
                using var inZipFile = archive.CreateEntry(fileName).Open();
                using var fileStreamWriter = new StreamWriter(inZipFile);
                fileStreamWriter.Write(content);
            }
            _ = stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        string CreateHTML()
        {
            XmlDocument document = new();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            var htmlRoot = document.CreateElement("html");
            document.AppendChild(htmlRoot);

            foreach (Row row in page.rows)
            {
                XmlElement paragraph = document.CreateElement("p");
                paragraph.InnerText = row.value;
                htmlRoot.AppendChild(paragraph);
            }//Place known characters as text

            string HTML = document.OuterXml;
            HTML = document.OuterXml.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>", String.Empty);

            return HTML;
        }
    }
}
