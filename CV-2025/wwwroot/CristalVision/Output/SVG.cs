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


            /*foreach (Character character in unknownChars)
            {
                for (int y = character.Top; y < character.Bottom; y++)
                {
                    for (int x = character.Left; x < character.Right; x++)
                    {
                        int color = bitmap256.GetPixel(x, y);
                        if (color == 255) continue;

                        XmlElement rect = document.CreateElement("rect");
                        rect.SetAttribute("width", "1");
                        rect.SetAttribute("height", "1");
                        rect.SetAttribute("x", x.ToString());
                        rect.SetAttribute("y", y.ToString());
                        rect.SetAttribute("fill", "black");
                        svg.AppendChild(rect);
                    }
                }
            }//Place unknown characters as rectangle pixels*/

            document.AppendChild(svg);

            //┌────────Outline first unknown character────────┐
            /*List<Character> displayChars = [knownChars[0], knownChars[4]];

            foreach (Character character in displayChars)
            {
                XmlElement rectangle = document.CreateElement("rect");
                rectangle.SetAttribute("x", character.Left.ToString());
                rectangle.SetAttribute("y", character.Top.ToString());
                rectangle.SetAttribute("width", character.Width.ToString());
                rectangle.SetAttribute("height", character.Height.ToString());
                rectangle.SetAttribute("fill", "transparent");
                rectangle.SetAttribute("stroke", "darkblue");
                rectangle.InnerText = character.value.ToString();
                document.ChildNodes[1].AppendChild(rectangle);
            }*/
            //└────────Outline first unknown character────────┘

            //┌─────────────────Outline words─────────────────┐
            /*foreach (Word word in words)
            {
                int width = word.Right - word.Left;
                int height = word.Bottom - word.Top;

                XmlElement rectangle = document.CreateElement("rect");
                rectangle.SetAttribute("x", word.Left.ToString());
                rectangle.SetAttribute("y", word.Top.ToString());
                rectangle.SetAttribute("width", width.ToString());
                rectangle.SetAttribute("height", height.ToString());
                rectangle.SetAttribute("fill", "transparent");
                rectangle.SetAttribute("stroke", "darkblue");
                rectangle.InnerText = word.value.ToString();
                document.ChildNodes[1].AppendChild(rectangle);
            }*/
            //└─────────────────Outline words─────────────────┘

            /*if (page.unknownChars.Count == 0)
                return document;*/

            //┌─────────Color first unknown character─────────┐
            /*Character character1 = unknownChars[0];
            XmlElement rectangle = document.CreateElement("rect");
            rectangle.SetAttribute("x", character1.Left.ToString());
            rectangle.SetAttribute("y", character1.Top.ToString());
            rectangle.SetAttribute("width", character1.Width.ToString());
            rectangle.SetAttribute("height", character1.Height.ToString());
            rectangle.SetAttribute("fill", "transparent");
            rectangle.SetAttribute("stroke", "darkblue");
            rectangle.InnerText = character1.value.ToString();
            document.ChildNodes[1].AppendChild(rectangle);*/
            //└─────────Color first unknown character─────────┘

            //document.AppendChild(document.ChildNodes[1]);

            MemoryStream memoryStream = new();
            /*byte[] filePDF = File.ReadAllBytes("C:\\Users\\user\\source\\repos\\CV-2025\\CV-2025\\wwwroot\\CristalVision\\Test Images\\TEST.pdf");
            Stream stream = new MemoryStream(filePDF);
            stream.CopyTo(memoryStream);*/
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
