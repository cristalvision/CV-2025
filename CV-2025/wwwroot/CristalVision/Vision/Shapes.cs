using CV_2025.CristalVision.Database;
using NetTopologySuite.Geometries;

namespace CV_2025.CristalVision.Vision
{
    public struct Shape
    {

    }

    public class Shapes
    {
        /// <summary>
        /// Real image width
        /// </summary>
        public int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public int Height;

        /// <summary>
        /// Pixels outside image width
        /// </summary>
        public int ExtraPixels;

        /// <summary>
        /// FullWidth includes ExtraPixels
        /// </summary>
        public int FullWidth;

        /// <summary>
        /// Total width including ExtraPixels
        /// </summary>
        public int Chunks;

        /// <summary>
        /// Image file as bytes
        /// </summary>
        public byte[] Content;

        /// <summary>
        /// File size
        /// </summary>
        public int Size;

        MySQL database;

        Monochrome monochrome;

        /// <summary>
        /// Monochrome image as stream
        /// </summary>
        public Shapes(Monochrome monochrome)
        {
            /*database = new MySQL("cvdrawings");
            this.monochrome = monochrome;
            Size = monochrome.Size;
            Content = monochrome.Content;*/
        }

        /// <summary>
        /// Get length starting at index
        /// </summary>
        public int GetLine(int index)
        {
            if (index < 62 || monochrome.GetPosition(index)[0] + 80 > Width)
                throw new Exception("Index out of range");//Trebuie luat in calcul top si bottom

            //┌─────────────────────Check start point─────────────────────┐
            if (Content[index + 1] != 0)
                return 0;

            bool endLine = false; byte[] section;
            int reference = index + 2 * Chunks, length = 0;
            //└─────────────────────Check start point─────────────────────┘

            //┌───────────────────────────80 x 5──────────────────────────┐
            while (!endLine)
            {
                section = monochrome.GetSection(reference, 80, 5);
                List<dynamic> rows = database.Filter("Black Pixels", Monochrome.CountBlackPixels(section), section);

                if (rows.Count == 0)
                {
                    int[] position = monochrome.GetPosition(reference);
                    database.Insert(new List<string>() { "Black Pixels", "Section", "Positions" }, new List<dynamic>() { Monochrome.CountBlackPixels(section), section, MySQL.GetLineString(new LineSegment(4088, 4089, 4090, 4091)) });

                    endLine = true;
                    continue;
                }

                //int nextPosition = -1;
                //if (nextPosition == -1) foundLine = false;//End of line
                monochrome.FillSection(reference, 40, 5, 255);
                reference += 5;
                length += 40;

            }
            //└───────────────────────────80 x 5──────────────────────────┘

            //┌───────────────────────────80 x 6──────────────────────────┐
            //int reference2 = index + 2 * Chunks;
            //byte[] section2 = GetSection(reference2, 80, 6);
            //└───────────────────────────80 x 6──────────────────────────┘

            return length;
        }

        public int[] GetLongestLine()
        {
            byte[] bytes = Content;
            int index = 63, lastIndex = Size, maxLine = 0, maxIndex = 0;

            while (index < lastIndex)
            {
                if (bytes[index] == 255)
                {
                    index++;
                    continue;
                }

                if (bytes[index] == 0)
                {
                    //int[] pos = GetPosition(2645);
                    int length = GetLine(index);
                    if (length == 0)
                    {
                        index++;
                        continue;
                    }

                    if (length > maxLine)
                    {
                        //File.WriteAllBytes("C:\\Users\\user\\source\\repos\\OCRScore\\OCRScore\\wwwroot\\Images\\DoriN.bmp", Content);

                        maxLine = length;
                        maxIndex = index;
                        //int[] position = GetPosition(index);
                        index++;
                        continue;
                    }
                }

                index++;
            }

            return [0, 0];
        }
    }
}
