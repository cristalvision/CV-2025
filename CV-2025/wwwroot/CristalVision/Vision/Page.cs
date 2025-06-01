using CV_2025.CristalVision.Database;
using System.Collections.Generic;
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
        public List<Row>? rows;
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

            Characters = new(bitmap256);
            Shapes = new Shapes(monochrome);
            Table = new Tables(monochrome);
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
            public string value = String.Empty;

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

                Character prevChar = new();
                while (prevChar.value != '␀')
                {
                    List<Character> charsToLeft = [.. knownChars.Where(character => character.Right < reference.Right)];
                    List<Character> prevChars = [.. charsToLeft.Where(character => character.Top < reference.Bottom && character.Bottom > reference.Top)];

                    int distance = 0;
                    foreach (Character character in prevChars)
                    {
                        if (character.Right > distance)
                        {
                            distance = character.Right;
                            prevChar = character;
                        }
                    }

                    if (prevChars.Count == 0) prevChar.value = '␀';
                    if (reference.Left - prevChar.Right > 10) prevChar.value = '␀';

                    if (prevChar.value != '␀')
                    {
                        reference = prevChar;
                    }//Word continues to the left
                    else
                    {
                        prevChar = reference;
                        break;
                    }//Start of word
                }

                return prevChar;
            }

            /// <summary>
            /// Assembly characters in word chunks : loop to the right from reference
            /// </summary>
            public static Word GetWord(Character reference, List<Character> knownChars)
            {
                Word word = new() { Top = reference.Top, Left = reference.Left, Right = reference.Right, Bottom = reference.Bottom };
                word.Characters.Add(reference);
                word.value += reference.value;

                Character nextChar = new();
                while (nextChar.value != '␀')
                {
                    List<Character> charsToRight = [.. knownChars.Where(character => character.Left > reference.Left)];
                    List<Character> nextChars = [.. charsToRight.Where(character => character.Top < reference.Bottom && character.Bottom > reference.Top)];

                    int distance = Bitmap256.MaxImageWidth;
                    foreach (Character character in nextChars)
                    {
                        if (character.Left > reference.Left && character.Left < distance)
                        {
                            distance = character.Left;
                            nextChar = character;
                        }
                    }

                    if (nextChars.Count == 0) nextChar.value = '␀';
                    if (nextChar.Left - reference.Right > 10) nextChar.value = '␀';

                    if (nextChar.value == '␀')
                        break;

                    if (nextChar.Top < word.Top) word.Top = nextChar.Top;
                    if (nextChar.Bottom > word.Bottom) word.Bottom = nextChar.Bottom;
                    word.Right = nextChar.Right;

                    word.Characters.Add(nextChar);
                    word.value += nextChar.value;
                    reference = nextChar;
                }

                foreach (Character character in word.Characters)
                    knownChars.Remove(character);
                
                return word;
            }
        }

        public struct Row()
        {
            /// <summary>
            /// Relative positions to the image
            /// </summary>
            public int Top, Bottom, Left, Right;

            /// <summary>
            /// List of words representing this row
            /// </summary>
            public string value = String.Empty;

            /// <summary>
            /// List of Words/Equantions representing this row
            /// </summary>
            public List<Object> Objects = [];

            /// <summary>
            /// Get first word character relative to the reference
            /// </summary>
            public static Word GetFirstWord(List<Word> knownWords)
            {
                Word reference = knownWords[0];

                Word prevWord = new();
                while (prevWord.value != null)
                {
                    List<Word> wordsToLeft = [.. knownWords.Where(word => word.Right < reference.Right)];
                    List<Word> prevWords = [.. wordsToLeft.Where(word => word.Top < reference.Bottom && word.Bottom > reference.Top)];

                    int distance = 0;
                    foreach (Word word in prevWords)
                    {
                        if (word.Right > distance)
                        {
                            distance = word.Right;
                            prevWord = word;
                        }
                    }

                    if (prevWords.Count == 0) prevWord.value = null;
                    //if (reference.Left - prevWord.Right > 10) prevWord.value = null;

                    if (prevWord.value != null)
                    {
                        reference = prevWord;
                    }//Word continues to the left
                    else
                    {
                        prevWord = reference;
                        break;
                    }//Start of word
                }

                return prevWord;
            }

            /// <summary>
            /// Assembly characters in word chunks : loop to the right from reference
            /// </summary>
            public static Row GetRow(Word reference, List<Word> knownWords)
            {

                Row row = new() { Top = reference.Top, Left = reference.Left, Right = reference.Right, Bottom = reference.Bottom };
                row.Objects.Add(reference);
                row.value = reference.value;

                Word nextWord = new();
                while (nextWord.value != null)
                {
                    List<Word> charsToRight = [.. knownWords.Where(word => word.Left > reference.Characters.Last().Left)];
                    List<Word> nextWords = [.. charsToRight.Where(word => word.Characters.First().Top < reference.Characters.Last().Bottom && word.Characters.First().Bottom > reference.Characters.Last().Top)];

                    int distance = Bitmap256.MaxImageWidth;
                    foreach (Word word in nextWords)
                    {
                        if (word.Left > reference.Left && word.Left < distance)
                        {
                            distance = word.Left;
                            nextWord = word;
                        }
                    }

                    if (nextWords.Count == 0) nextWord.value = null;
                    //if (nextWord.Characters.First().Left - reference.Characters.Last().Right > 10) nextWord.value = null;

                    if (nextWord.value == null)
                        break;

                    if (nextWord.Top < row.Top) row.Top = nextWord.Top;
                    if (nextWord.Bottom > row.Bottom) row.Bottom = nextWord.Bottom;
                    row.Right = nextWord.Right;

                    row.Objects.Add(nextWord);
                    row.value += " " +  nextWord.value;
                    reference = nextWord;
                }

                foreach (Word word in row.Objects)
                {
                    knownWords.Remove(word);
                }

                return row;
            }
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
            List<Character> characters = Characters.GetCharacters();
            Characters.database.Close();

            unknownChars = [.. characters.Where(character => character.value == '␀')];
            knownChars = [.. characters.Where(character => character.value != '␀')];
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
            if (knownChars == null)
                return;

            List<Fraction> fractions = Equations.GetFractions(unknownChars, knownChars);
            equations.Add(fractions[0]);
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
            if (words == null)
                return;

            rows = [];
            while (words.Count > 0)
            {
                Word firstWord = Row.GetFirstWord(words);
                //Word firstEquation = Row.GetFirstEquation(equations);
                Row row = Row.GetRow(firstWord, words);
                rows.Add(row);
            }
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


            foreach (Word word in words)
            {
                XmlElement text = document.CreateElement("text");
                text.SetAttribute("x", word.Left.ToString());
                text.SetAttribute("y", word.Bottom.ToString());
                text.SetAttribute("fill", "black");
                text.SetAttribute("font-size", "50px");
                text.SetAttribute("fill", "darkblue");
                text.InnerText = word.value;
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
