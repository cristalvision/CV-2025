using CV_2025.CristalVision.Vision;
using System.IO.MemoryMappedFiles;
using System.Xml;
using static CV_2025.CristalVision.Vision.Page;

namespace CV_2025.wwwroot.CristalVision.Output
{
    public class SVG
    {
        Page page;

        public SVG(Page page)
        {
            this.page = page;
        }

        /// <summary>
        /// Place characters/shapes/equations/tables on SVG
        /// </summary>
        public MemoryStream ToMemeoryStream(string mapName)
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));

            XmlElement svg = document.CreateElement("svg");
            svg.SetAttribute("viewBox", "0 0 " + page.bitmap256.Width + " " + page.bitmap256.Height);
            svg.SetAttribute("version", "1.1");
            svg.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            svg.SetAttribute("xmlns:svg", "http://www.w3.org/2000/svg");
            svg.SetAttribute("style", "background: LightSkyBlue");


            foreach (Row row in page.rows)
            {
                XmlElement text = document.CreateElement("text");
                text.SetAttribute("x", row.Left.ToString());
                text.SetAttribute("y", row.Bottom.ToString());
                text.SetAttribute("fill", "black");
                text.SetAttribute("font-size", "50px");
                text.SetAttribute("fill", "darkblue");
                text.InnerText = row.value;
                svg.AppendChild(text);

            }//Place known characters as text

            document.AppendChild(svg);
            MemoryStream memoryStream = new();
            document.Save(memoryStream);

            //┌─────────────────SVG to Stream─────────────────┐
            int length = (int)memoryStream.Length;
            MemoryMappedFile mappedFile = MemoryMappedFile.CreateNew(mapName + " - SVG", length);
            MemoryMappedViewStream viewStream = mappedFile.CreateViewStream();
            viewStream.Write(memoryStream.ToArray(), 0, length);
            //└─────────────────SVG to Stream─────────────────┘

            return memoryStream;
        }
    }
}
