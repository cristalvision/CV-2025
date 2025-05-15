using MySqlX.XDevAPI.Relational;
using System.Xml;

namespace CV_2025.CristalVision.Vision
{
    public class Page(MemoryStream? memoryStream)
    {
        readonly Monochrome monochrome = new(memoryStream);
        readonly Bitmap256 bitmap256 = new(memoryStream);

        Characters? Characters;
        Shapes? Shapes;
        Equations? Equations;
        Tables? Table;

        List<Character>? characters;
        List<Shape>? shapes = [];
        List<Equation>? equations = [];
        List<Table>? tables = [];
        List<Paragraph>? Paragraphs = [];

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
        /// Get list of characters
        /// </summary>
        public void GetCharacters()
        {
            Characters = new(bitmap256);
            characters = Characters.GetText();

            characters = [.. characters.Where(character => character.value != '␀')];
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
            foreach (Character character in characters)
            {

            }
        }

        /// <summary>
        /// Assembly words/equations in rows
        /// </summary>
        public void GetRows()
        {

        }

        /// <summary>
        /// Assembly rows/shapes/tables in paragraphs
        /// </summary>
        public void GetParagraphs()
        {
            Paragraph paragraph = new();

            int counter = paragraph.Rows.Count;

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

            foreach (Character character in characters)
            {
                XmlElement text = document.CreateElement("text");
                text.SetAttribute("x", character.Left.ToString());
                text.SetAttribute("y", character.Bottom.ToString());
                text.SetAttribute("fill", "black");
                text.SetAttribute("font-size", "50px");
                text.SetAttribute("fill", "darkblue");
                text.InnerText = character.value.ToString();
                svg.AppendChild(text);
            }

            document.AppendChild(svg);

            return document;
        }
    }
}
