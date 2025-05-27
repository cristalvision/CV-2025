using CV_2025.CristalVision.Database;
using System.Runtime.Versioning;
using System.Xml;

namespace CV_2025.CristalVision.Vision
{
    [SupportedOSPlatform("windows")]
    public class Page
    {
        Monochrome monochrome;
        public Bitmap256 bitmap256;
        public Access database;

        public Characters? Characters;
        Shapes? Shapes;
        Equations? Equations;
        Tables? Table;

        public List<Character> knownChars = [], unknownChars = [];
        List<Shape>? shapes = [];
        List<Equation>? equations = [];
        List<Table>? tables = [];
        List<Paragraph>? Paragraphs = [];

        public Page(MemoryStream? memoryStream)
        {
            monochrome = new(memoryStream);
            bitmap256 = new(memoryStream);
            database = new Access("cvpage.accdb");
        }

        public struct Word
        {

        }
        public struct Row
        {

        }

        public struct Paragraph
        {
            public List<Row> Rows;

            public Paragraph()
            {
                Rows = [];
            }
        }

        /// <summary>
        /// Get list of all characters (recognized &amp; unrecognized)
        /// </summary>
        public void GetCharacters()
        {
            Characters = new(bitmap256);
            List<Character> characters = Characters.GetText();
            Characters.database.Close();

            unknownChars = [.. characters.Where(character => character.Width > 7 && character.Height > 15 && character.value == '␀')];
            knownChars = [.. characters.Where(character => character.Width > 7 && character.Height > 15 && character.value != '␀')];
        }

        /// <summary>
        /// Get list of geometric shapes
        /// </summary>
        public void GetShapes()
        {
            Shapes = new(monochrome);
        }

        /// <summary>
        /// Get list of equations
        /// </summary>
        public void GetEquations()
        {

        }

        /// <summary>
        /// Get list of tables
        /// </summary>
        public void GetTables()
        {

        }

        /// <summary>
        /// Assembly characters in words
        /// </summary>
        public void GetWords()
        {
            database.tableName = "DistanceBetweenChars";
            List<dynamic>? rows = database.Filter("ID", "\"d-30\"");

            Character firstChar = knownChars[0];
            //30 -> 7

        }

        /// <summary>
        /// Assembly words/equations in rows
        /// </summary>
        public void GetRows()
        {

        }

        /// <summary>
        /// Assembly rows in paragraphs
        /// </summary>
        public void GetParagraphs()
        {
            Paragraph paragraph = new();

            int counter = paragraph.Rows.Count;

        }

        /// <summary>
        /// Assembly paragraphs/shapes/tables in sections
        /// </summary>
        public void GetSections()
        {

        }

        /// <summary>
        /// Place characters/shapes/equations/tables on SVG
        /// </summary>
        public XmlDocument ToSVG()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));

            XmlElement svg = document.CreateElement("svg");
            svg.SetAttribute("viewBox", "0 0 " + bitmap256.Width + " " + bitmap256.Height);
            svg.SetAttribute("version", "1.1");
            svg.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            svg.SetAttribute("xmlns:svg", "http://www.w3.org/2000/svg");
            svg.SetAttribute("style", "background: LightSkyBlue");


            foreach (Character character in knownChars)
            {
                XmlElement text = document.CreateElement("text");
                text.SetAttribute("x", character.Left.ToString());
                text.SetAttribute("y", character.Bottom.ToString());
                text.SetAttribute("fill", "black");
                text.SetAttribute("font-size", "50px");
                text.SetAttribute("fill", "darkblue");
                text.InnerText = character.value.ToString();
                svg.AppendChild(text);

            }//Place known characters as text


            foreach (Character character in unknownChars)
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
            }//Place unknown characters as rectangle pixels

            document.AppendChild(svg);

            //┌─────────Color first unknown character─────────┐
            List<Character> displayChars = [knownChars[0], knownChars[12]];
            char firstChar = displayChars[0].value;//d
            int right = displayChars[0].Right;//707
            int width = displayChars[0].Width;//30
            char secondChar = displayChars[1].value;//e
            int left = displayChars[1].Left;//714

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
            }
            //└─────────Color first unknown character─────────┘

            if (unknownChars.Count == 0)
                return document;
            
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

            document.AppendChild(document.ChildNodes[1]);

            return document;
        }
    }
}
