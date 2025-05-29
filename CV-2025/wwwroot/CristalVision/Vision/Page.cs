using CV_2025.CristalVision.Database;
using System.Drawing;
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
        public List<Word>? words;
        public List<Shape>? shapes = [];
        public List<Equation>? equations = [];
        public List<Table>? tables = [];
        public List<Paragraph>? Paragraphs = [];

        /// <summary>
        /// Distance factor to the next char relative to actual char width
        /// </summary>
        static Dictionary<char, float> distanceFactor = new() { { 'a', 3.0F }, { 'b', 1.0F }, { 'c', 100.0F }, { 'd', 3.7F }, { 'e', 3.7F }, { 'f', 100.0F }, { 'g', 100.0F }, { 'h', 1.0F }, { 'i', 2.1F }, { 'j', 100.0F }, { 'k', 100.0F }, { 'l', 2.1F }, { 'm', 100.0F }, { 'n', 3.5F }, { 'o', 3.5F }, { 'p', 3.5F }, { 'q', 1.0F }, { 'r', 2.1F }, { 's', 100.0F }, { 'ş', 100.0F }, { 't', 2.0F }, { 'ţ', 4.1F }, { 'u', 3.7F }, { 'v', 100.0F }, { 'w', 100.0F }, { 'x', 100.0F }, { 'y', 100.0F }, { 'z', 3.7F }, { 'A', 100.0F }, { 'B', 100.0F }, { 'D', 100.0F }, { 'E', 100.0F }, { 'O', 100.0F }, { 'T', 100.0F }, { 'V', 100.0F }, { 'α', 100.0F }, { '0', 100.0F }, { '2', 100.0F }, { '5', 100.0F }, { '6', 100.0F }, { '(', 100.0F }, { ')', 100.0F }, { ',', 100.0F }, { '/', 100.0F } };

        public Page(MemoryStream? memoryStream)
        {
            monochrome = new(memoryStream);
            bitmap256 = new(memoryStream);
        }

        public struct Word()
        {
            /// <summary>
            /// Relative positions to the image
            /// </summary>
            public int Top, Bottom, Left, Right;

            /// <summary>
            /// List of characters representing this word
            /// </summary>
            public List<char> value = [];

            /// <summary>
            /// List of Characters representing this word
            /// </summary>
            public List<Character> Characters = [];

            /// <summary>
            /// Get first word character relative to the reference
            /// </summary>
            public static Character GetFirstChar(List<Character> knownChars)
            {
                Character reference = knownChars[0];
                
                Character firstChar = new();
                while (firstChar.value != '␀')
                {
                    float distance = reference.Width / distanceFactor[reference.value];//Max distance to the previous character
                    List<Character> charsToLeft = [.. knownChars.Where(character => character.Right > reference.Left - distance && character.Right < reference.Left)];
                    List<Character> prevChars = [.. charsToLeft.Where(character => character.Top < reference.Bottom && character.Bottom > reference.Top)];

                    firstChar = (prevChars.Count > 0) ? prevChars[0] : new Character() { value = '␀' };

                    if (firstChar.value != '␀')
                    {
                        reference = firstChar;
                    }//Word continues to the left
                    else
                    {
                        firstChar = reference;
                        break;
                    }//Start of word
                }

                return firstChar;
            }

            /// <summary>
            /// Assembly characters in word chunks : loop to the right from reference
            /// </summary>
            public static Word GetWord(Character reference, List<Character> knownChars)
            {
                Word word = new() { Top = reference.Top, Left = reference.Left, Right = reference.Right, Bottom = reference.Bottom };
                word.Characters.Add(reference);
                word.value.Add(reference.value);
                knownChars.Remove(reference);

                Character nextChar = new();
                while (nextChar.value != '␀')
                {
                    //float distance = reference.Width / distanceFactor[reference.value];//Max distance to the next character
                    int distance = 10;
                    List<Character> charsToRight = [.. knownChars.Where(character => character.Left < reference.Right + distance && character.Left > reference.Right)];
                    List<Character> nextChars = [.. charsToRight.Where(character => character.Top < reference.Bottom && character.Bottom > reference.Top)];

                    if (nextChars.Count == 0)
                    {
                        charsToRight = [.. knownChars.Where(character => character.Left == reference.Right || character.Left == reference.Right - 1 || character.Left == reference.Right - 2 || character.Left == reference.Right - 3)];
                        nextChars = [.. charsToRight.Where(character => character.Top < reference.Bottom && character.Bottom > reference.Top)];
                    }

                    nextChar = (nextChars.Count > 0) ? nextChars[0] : new Character() { value = '␀' };

                    if (nextChar.value == '␀')
                        break;

                    if (nextChar.Top < word.Top) word.Top = nextChar.Top;
                    if (nextChar.Bottom > word.Bottom) word.Bottom = nextChar.Bottom;
                    word.Right = nextChar.Right;

                    word.Characters.Add(nextChar);
                    word.value.Add(nextChar.value);
                    knownChars.Remove(nextChar);
                    reference = nextChar;
                }

                return word;
            }
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
        /// Assembly chunks in words : loop to the left from reference
        /// </summary>
        public void GetWords()
        {
            List<Character>? knownChars = [.. this.knownChars];
            
            if (knownChars == null)
                return;

            words = [];
            while (knownChars.Count > 0)
            {
                Character firstChar = Word.GetFirstChar(knownChars);
                Word word = Word.GetWord(firstChar, knownChars);
                words.Add(word);
            }

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
            foreach (Word word in words)
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
            }
            //└─────────────────Outline words─────────────────┘

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
