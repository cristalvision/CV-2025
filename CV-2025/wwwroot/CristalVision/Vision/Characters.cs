using CV_2025.CristalVision.Database;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.Versioning;

namespace CV_2025.CristalVision.Vision
{
    public struct Character()
    {
        /// <summary>
        /// Relative positions to the image
        /// </summary>
        public int Top = 8192, Bottom = 0, Right = 0, Left = 8192;

        /// <summary>
        /// Database width is based on 50px height
        /// </summary>
        public int DBWidth, Width, Height;

        /// <summary>
        /// Shape as character
        /// </summary>
        public char value;

        /// <summary>
        /// Remove this
        /// </summary>
        public byte[] section;
    }

    public struct ShapeChar
    {
        /// <summary>
        /// Color Image properties
        /// </summary>
        public int Chunks, ImageHeight, Reference;

        /// <summary>
        /// ShapeChar properties
        /// </summary>
        public int Width, Height, FullWidth;

        /// <summary>
        /// Shape as character
        /// </summary>
        public char value;

        /// <summary>
        /// Character as Bitmap256
        /// </summary>
        public byte[] Bitmap256;

        /// <summary>
        /// Character as Bitmap Monochrome
        /// </summary>
        public byte[] Monochrome;

        /// <summary>
        /// Character as 8pixel/byte array
        /// </summary>
        public byte[] Section;

        /// <summary>
        /// Character indexes relative to original image
        /// </summary>
        public List<int> Indexes;

        /// <summary>
        /// Relative positions to the image
        /// </summary>
        public int Top = 8192, Bottom = 0, Right = 0, Left = 8192;

        /// <summary>
        /// H WB | V WB | V BW | H BW
        /// </summary>
        public bool[] Pairs;

        public List<int[]> Segments = [];

        /// <summary>
        /// Outline 2 pixel pairs to extract character
        /// </summary>
        public ShapeChar(int reference, int chunks, int height)
        {
            Pairs = [true, false, false, false];
            Reference = reference - 1;
            Chunks = chunks;
            ImageHeight = height;
            Indexes = new List<int>();
        }

        /// <summary>
        /// Remain in Horizontal White Black pair and advance 1 pixel down
        /// </summary>
        public void StayHorizontalWB()
        {
            Reference -= Chunks;
        }//□■↓

        /// <summary>
        /// Horizontal white black to vertical black white
        /// </summary>
        public void HorizontalWBToVerticalBW()
        {
            Pairs = [false, false, true, true];
            Reference -= Chunks - 1;
        }//□■ → ▀

        /// <summary>
        /// Horizontal white black to vertical white black
        /// </summary>
        public void HorizontalWBToVerticalWB()
        {
            Pairs = [false, true, false, false];
        }//□■ → ▄

        /// <summary>
        /// Remain in Vertical Black White pair and advance 1 pixel right
        /// </summary>
        public void StayVerticalBW()
        {
            Reference += 1;
        }//▀ → ▀

        /// <summary>
        /// Vertical black white to horizontal black white
        /// </summary>
        public void VerticalBWToHorizontalBW()
        {
            Pairs = [false, false, false, true];
            Reference += Chunks + 1;
        }//▀ → ■□

        /// <summary>
        /// Vertical black white to horizontal white black
        /// </summary>
        public void VerticalBWToHorizontalWB()
        {
            Pairs = [true, false, false, false];
        }//▀ → □■

        /// <summary>
        /// Remain in Horizontal Black White pair and advance 1 pixel up
        /// </summary>
        public void StayHorizontalBW()
        {
            Reference += Chunks;
        }//■□↑

        /// <summary>
        /// Horizontal black white to vertical white black
        /// </summary>
        public void HorizontalBWToVerticalWB()
        {
            Pairs = [false, true, false, false];
            Reference += Chunks - 1;
        }//■□ → ▄

        /// <summary>
        /// Horizontal black white to vertical black white
        /// </summary>
        public void HorizontalBWToVerticalBW()
        {
            Pairs = [false, false, true, false];
        }//■□ → ▀

        /// <summary>
        /// Remain in Vertical White Black pair and advance 1 pixel left
        /// </summary>
        public void StayVerticalWB()
        {
            Reference--;
        }//▄ ← ▄

        /// <summary>
        /// Vertical white black to horizontal black white
        /// </summary>
        public void VerticalWBToHorizontalBW()
        {
            Pairs = [false, false, false, true];
        }//▄ ← ■□

        /// <summary>
        /// Vertical white black to horizontal white black
        /// </summary>
        public void VerticalWBToHorizontalWB()
        {
            Pairs = [true, false, false, false];
            Reference -= Chunks + 1;
        }//▄ ← □■

        //int x = (Reference - 1078) % Chunks;
        //int y = (ImageHeight - 1) - (Reference - 1078 - x) / Chunks;

        /// <summary>
        /// Write start position for this segment
        /// </summary>
        public void SetStartSegment()
        {
            int x = (Reference - 1078) % Chunks;
            int y = (ImageHeight - 1) - (Reference - 1078 - x) / Chunks;

            Segments.Add([x + 1, y]);
        }

        /// <summary>
        /// Update highest pixel for this shape
        /// </summary>
        public void UpdateTop()
        {
            int x = (Reference - 1078) % Chunks;
            int y = (ImageHeight - 1) - (Reference - 1078 - x) / Chunks;
            if (y < Top) Top = y;
        }

        /// <summary>
        /// Update lowest pixel for this shape
        /// </summary>
        public void UpdateBottom()
        {
            int x = (Reference - 1078) % Chunks;
            int y = (ImageHeight - 1) - (Reference - 1078 - x) / Chunks;
            if (y > Bottom) Bottom = y;
        }

        /// <summary>
        /// Update furthest right pixel for this shape
        /// </summary>
        public void UpdateRight()
        {
            int x = (Reference - 1078) % Chunks;
            if (x > Right) Right = x;
        }

        /// <summary>
        /// Update furthest left pixel for this shape
        /// </summary>
        public void UpdateLeft()
        {
            int x = (Reference - 1078) % Chunks;
            if (x < Left) Left = x;
        }
    }

    [SupportedOSPlatform("windows")]
    public class Characters
    {
        /// <summary>
        /// Character shape
        /// </summary>
        ShapeChar ShapeChar;

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

        public Access database;

        public Characters(Bitmap256 bitmap256)
        {
            Width = bitmap256.Width;
            Height = bitmap256.Height;
            Size = bitmap256.Size;
            Chunks = (Size - 1078) / Height;
            ExtraPixels = Width % 4;
            if (ExtraPixels != 0) ExtraPixels = 4 - ExtraPixels;
            FullWidth = Width + ExtraPixels;

            Content = new byte[Size];
            bitmap256.Content.CopyTo(Content, 0);

            database = new Access("cvcharacters.accdb");
        }

        /// <summary>
        /// Get list of characters
        /// </summary>
        public List<Character> GetText()
        {
            int reference = 1078 + Chunks * (Height - 1);
            List<Character> characters = new List<Character>();

            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    int index = reference - y * Chunks + x;

                    if (Content[index] == 255)
                        continue;

                    ShapeChar = new(index, Chunks, Height);
                    SetIndexes(index);
                    Extract();
                    Character character = new() { Top = ShapeChar.Top, Bottom = ShapeChar.Bottom, Left = ShapeChar.Left, Right = ShapeChar.Right, Width = ShapeChar.Width, Height = ShapeChar.Height };
                    Resize();
                    BMP256ToMonochrome();
                    GetValue();
                    character.DBWidth = ShapeChar.Width;
                    character.value = ShapeChar.value;
                    character.section = ShapeChar.Section;

                    characters.Add(character);
                }//→
            }//↓

            return characters;
        }

        /// <summary>
        /// Set start/end references for enclosed segments
        /// </summary>
        private void SetIndexes(int reference)
        {
            do
            {
                if (ShapeChar.Pairs[0] && Content[ShapeChar.Reference - Chunks] == 255 && Content[ShapeChar.Reference - Chunks + 1] != 255)
                {
                    ShapeChar.StayHorizontalWB();
                    ShapeChar.SetStartSegment();
                }

                if (ShapeChar.Pairs[0] && Content[ShapeChar.Reference - Chunks] != 255)
                {
                    ShapeChar.HorizontalWBToVerticalWB();
                    ShapeChar.UpdateTop();
                }

                if (ShapeChar.Pairs[0] && Content[ShapeChar.Reference - Chunks] == 255 && Content[ShapeChar.Reference - Chunks + 1] == 255)
                {
                    ShapeChar.HorizontalWBToVerticalBW();
                    ShapeChar.UpdateBottom();
                }

                if (ShapeChar.Pairs[1] && Content[ShapeChar.Reference - 1] == 255 && Content[ShapeChar.Reference - Chunks - 1] != 255)
                    ShapeChar.StayVerticalWB();

                if (ShapeChar.Pairs[1] && Content[ShapeChar.Reference - 1] != 255)
                {
                    ShapeChar.VerticalWBToHorizontalBW();
                    MarkEndSegment();
                }

                if (ShapeChar.Pairs[1] && Content[ShapeChar.Reference - 1] == 255 && Content[ShapeChar.Reference - Chunks - 1] == 255)
                {
                    ShapeChar.VerticalWBToHorizontalWB();
                    ShapeChar.SetStartSegment();

                    ShapeChar.UpdateLeft();
                }

                if (ShapeChar.Pairs[2] && Content[ShapeChar.Reference + 1] == 255 && Content[ShapeChar.Reference + Chunks + 1] != 255)
                    ShapeChar.StayVerticalBW();

                if (ShapeChar.Pairs[2] && Content[ShapeChar.Reference + 1] == 255 && Content[ShapeChar.Reference + Chunks + 1] == 255)
                {
                    ShapeChar.VerticalBWToHorizontalBW();
                    MarkEndSegment();
                    ShapeChar.UpdateRight();
                }

                if (ShapeChar.Pairs[2] && Content[ShapeChar.Reference + 1] != 255)
                {
                    ShapeChar.VerticalBWToHorizontalWB();
                    ShapeChar.SetStartSegment();
                    ShapeChar.UpdateLeft();
                }

                if (ShapeChar.Pairs[3] && Content[ShapeChar.Reference + Chunks] == 255 && Content[ShapeChar.Reference + Chunks - 1] != 255)
                {
                    ShapeChar.StayHorizontalBW();
                    MarkEndSegment();
                }

                if (ShapeChar.Pairs[3] && Content[ShapeChar.Reference + Chunks] == 255 && Content[ShapeChar.Reference + Chunks - 1] == 255)
                {
                    ShapeChar.HorizontalBWToVerticalWB();
                    ShapeChar.UpdateTop();
                }

                if (ShapeChar.Pairs[3] && Content[ShapeChar.Reference + Chunks] != 255)
                {
                    ShapeChar.HorizontalBWToVerticalBW();
                    ShapeChar.UpdateBottom();
                }

            }
            while (reference - 1 != ShapeChar.Reference);

            ShapeChar.Left++;
            ShapeChar.Right--;
            ShapeChar.Top++;
            ShapeChar.Bottom--;
        }

        /// <summary>
        /// Mark end position for this segment
        /// </summary>
        private void MarkEndSegment()
        {
            int x = (ShapeChar.Reference - 1078) % Chunks;
            int y = (Height - 1) - (ShapeChar.Reference - 1078 - x) / Chunks;
            int index = 1078 + (Height - 1 - y) * FullWidth + x - 1;

            Content[index] = 201;
        }

        /// <summary>
        /// Extract character shape &amp; remove from Bitmap256
        /// </summary>
        private void Extract()
        {
            ShapeChar.Width = ShapeChar.Right - ShapeChar.Left + 1;
            ShapeChar.Height = ShapeChar.Bottom - ShapeChar.Top + 1;
            Bitmap256 character256 = new(ShapeChar.Width, ShapeChar.Height);

            foreach (int[] positions in ShapeChar.Segments)
            {
                int xStart = positions[0] - ShapeChar.Left;
                int y = positions[1] - ShapeChar.Top;

                bool endLine = false;
                for (int x = xStart; !endLine; x++)
                {
                    int index = 1078 + (Height - 1 - positions[1]) * FullWidth + positions[0] + x - xStart;
                    character256.SetPixel(x, y, Content[index]);
                    ShapeChar.Indexes.Add(index);

                    if (Content[index] == 201)
                        endLine = true;

                    Content[index] = 255;
                }
            }//Copy each segment from Bitmap256 to Character256

            ShapeChar.FullWidth = character256.FullWidth;
            ShapeChar.Bitmap256 = new byte[character256.Size];
            character256.Content.CopyTo(ShapeChar.Bitmap256, 0);
        }

        /// <summary>
        /// Resize character to 70px height
        /// </summary>
        private void Resize()
        {
            decimal ZoomWidth = (ShapeChar.Width * 50) / ShapeChar.Height;
            Bitmap256 CharImage = new(ShapeChar.Width, ShapeChar.Height);
            ShapeChar.Bitmap256.CopyTo(CharImage.Content, 0);
            Bitmap256 ZoomImage = new((int)Math.Round(ZoomWidth), 50);

            float raport_pixel = ShapeChar.Height / 50.0F;
            float raport_pixel_x = 0, raport_pixel_y = 0; //Am nevoie de raport_pixel_x si y pt parcurgerea imaginii originale functia de zoom
            
            for (int y_zoom = 0; y_zoom < ZoomImage.Height; y_zoom++)
            {
                for (int x_zoom = 0; x_zoom < ZoomImage.Width; x_zoom++)
                {
                    int sourceIndex = 1078 + (CharImage.Height - 1 - (int)raport_pixel_y) * CharImage.FullWidth + (int)raport_pixel_x;
                    int destIndex = 1078 + (50 - 1 - y_zoom) * ZoomImage.FullWidth + x_zoom;
                    ZoomImage.Content[destIndex] = CharImage.Content[sourceIndex];
                    
                    raport_pixel_x += raport_pixel;
                }
                raport_pixel_x = 0;
                raport_pixel_y += raport_pixel;
            }

            ShapeChar.Bitmap256 = new byte[ZoomImage.Content.Length];
            ZoomImage.Content.CopyTo(ShapeChar.Bitmap256, 0);

            ShapeChar.Width = ZoomImage.Width;
            ShapeChar.Height = 50;
            ShapeChar.FullWidth = ZoomImage.FullWidth;
        }

        /// <summary>
        /// Turn 5 colors into 8 pixel/byte array &amp; center image
        /// </summary>
        private void BMP256ToMonochrome()
        {
            byte[] leftPixels = [0, 1, 1, 2, 2, 3, 3, 4, 0];//Center charcter to fit 8 pixel/byte width
            int offset = 8 - ShapeChar.Width % 8;
            int leftOffset = leftPixels[offset];
            int rightOffset = offset - leftOffset;
            string strLine = String.Empty;

            //┌─────Center image & turn color to monochrome────┐
            for (int y = 0; y < ShapeChar.Height; y++)
            {
                for (int x = 0; x < leftOffset; x++) strLine += '1';
                for (int x = 0; x < ShapeChar.Width; x++)
                {
                    int index = 1078 + (ShapeChar.Height - 1 - y) * ShapeChar.FullWidth + x;
                    strLine += (ShapeChar.Bitmap256[index] == 255) ? '1' : '0';
                }
                for (int x = 0; x < rightOffset; x++) strLine += '1';
            }
            //└─────Center image & turn color to monochrome256────┘


            //┌───Convert monochrome256 into 8 pixel/byte array───┐
            byte[] content = new byte[strLine.Length / 8];
            int k = 0;
            for (int chunk = 0; chunk < strLine.Length; chunk += 8)
            {
                string result = strLine.Substring(chunk, 8);
                content[k] = Convert.ToByte(result, 2);
                k++;
            }
            //└───Convert monochrome256 into 8 pixel/byte array───┘

            ShapeChar.Width = strLine.Length / 50;
            ShapeChar.Section = new byte[content.Length];
            content.CopyTo(ShapeChar.Section, 0);
        }

        /// <summary>
        /// Check database for given bytes
        /// </summary>
        private void GetValue()
        {
            ShapeChar.value = '␀';

            string tableName = ShapeChar.Width + "x" + ShapeChar.Height;
            if (!database.tableNames.Contains(tableName))
                return;

            byte[] section = ShapeChar.Section;
            if (Monochrome.CountBlackPixels(section) == 2500)
            {

            }
            database.tableName = tableName;
            List<dynamic>? rows = database.Filter("Black Pixels", Monochrome.CountBlackPixels(section), section);

            if (rows == null)
                return;

            ShapeChar.value = (Enumerable.SequenceEqual(section, rows[5])) ? rows[2][0] : '␀';
        }
    }
}
