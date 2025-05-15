using CV_2025.CristalVision.Database;

namespace CV_2025.CristalVision.Vision
{
    public class Score
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

        /// <summary>
        /// Monochrome extends 8 pixels to the right to make shift
        /// </summary>
        public Score(Monochrome monochrome)
        {
            //┌────────────────────────Image width────────────────────────┐
            Width = monochrome.Width + 8;
            int byte18 = 0, byte19 = 0;
            if (Width > 255)
            {
                byte18 = Width % 256;
                byte19 = (Width - (Width % 256)) / 256;
            }
            else
            {
                byte18 = Width;
            }
            //└────────────────────────Image width────────────────────────┘

            //┌───────────────────────Image Height────────────────────────┐
            Height = monochrome.Height;
            int byte22, byte23 = 0;
            if (Height > 255)
            {
                byte22 = Height % 256;
                byte23 = (Height - (Height % 256)) / 256;
            }
            else
            {
                byte22 = Height;
            }

            if (Height > 8192)
                throw new Exception("Height out of range");
            //└───────────────────────Image Height────────────────────────┘

            //┌───────────────────Remaining properties────────────────────┐
            int ExtraPixels = Width % 32;
            if (ExtraPixels != 0) ExtraPixels = 32 - ExtraPixels;
            int TotalArea = (Width + ExtraPixels) * Height;
            int FullWidth = Width + ExtraPixels;
            Size = (FullWidth / 8) * Height + 62;
            Chunks = FullWidth / 8;
            //└───────────────────Remaining properties────────────────────┘

            //┌─────────────────────────File size─────────────────────────┐
            int byte2, byte3, byte4;
            byte2 = Size % 256;
            byte3 = ((Size - byte2) % 65536) / 256;
            byte4 = (Size - byte2 - (byte3 * 256)) / 65536;
            //└─────────────────────────File size─────────────────────────┘

            //┌────────────────────────Total area─────────────────────────┐
            int byte34, byte35, byte36;
            byte34 = TotalArea % 256;
            byte35 = ((TotalArea - byte34) % 65536) / 256;
            byte36 = (TotalArea - byte34 - (byte35 * 256)) / 65536;
            //└────────────────────────Total area─────────────────────────┘

            //┌───────────────────────Write Content───────────────────────┐
            Content = new byte[Size];
            byte[] header = [66, 77, (byte)byte2, (byte)byte3, (byte)byte4, 0, 0, 0, 0, 0, 62, 0, 0, 0, 40, 0, 0, 0, (byte)byte18, (byte)byte19, 0, 0, (byte)byte22, (byte)byte23, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, (byte)byte34, (byte)byte35, (byte)byte36, 0, 195, 14, 0, 0, 195, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 0];
            header.CopyTo(Content, 0);

            for (int index = 62; index < Size; index++) Content[index] = 255;

            int reference = monochrome.Chunks * (Height - 1) + 62;
            byte[] section = monochrome.GetSection(reference, monochrome.Width - (monochrome.Width % 8), Height);
            reference = 62 + Chunks * (Height - 1);
            int sourceIndex = 0, destIndex;
            int fullChunks = (monochrome.Width - (monochrome.Width % 8)) / 8;//Chunks fully inside image

            for (int indexH = 0; indexH < Height; indexH++)
            {
                for (int chunk = 0; chunk < fullChunks; chunk++)
                {
                    destIndex = chunk + reference - indexH * Chunks;
                    Content[destIndex] = section[sourceIndex];
                    sourceIndex++;
                }
            }
            //└───────────────────────Write Content───────────────────────┘
        }

        /// <summary>
        /// Shift all bits to the right
        /// </summary>
        private void ShiftRight()
        {
            /*byte[] LastPixelW = [128, 128, 129, 129, 130, 130, 131, 131, 132, 132, 133, 133, 134, 134, 135, 135, 136, 136, 137, 137, 138, 138, 139, 139, 140, 140, 141, 141, 142, 142, 143, 143, 144, 144, 145, 145, 146, 146, 147, 147, 148, 148, 149, 149, 150, 150, 151, 151, 152, 152, 153, 153, 154, 154, 155, 155, 156, 156, 157, 157, 158, 158, 159, 159, 160, 160, 161, 161, 162, 162, 163, 163, 164, 164, 165, 165, 166, 166, 167, 167, 168, 168, 169, 169, 170, 170, 171, 171, 172, 172, 173, 173, 174, 174, 175, 175, 176, 176, 177, 177, 178, 178, 179, 179, 180, 180, 181, 181, 182, 182, 183, 183, 184, 184, 185, 185, 186, 186, 187, 187, 188, 188, 189, 189, 190, 190, 191, 191, 192, 192, 193, 193, 194, 194, 195, 195, 196, 196, 197, 197, 198, 198, 199, 199, 200, 200, 201, 201, 202, 202, 203, 203, 204, 204, 205, 205, 206, 206, 207, 207, 208, 208, 209, 209, 210, 210, 211, 211, 212, 212, 213, 213, 214, 214, 215, 215, 216, 216, 217, 217, 218, 218, 219, 219, 220, 220, 221, 221, 222, 222, 223, 223, 224, 224, 225, 225, 226, 226, 227, 227, 228, 228, 229, 229, 230, 230, 231, 231, 232, 232, 233, 233, 234, 234, 235, 235, 236, 236, 237, 237, 238, 238, 239, 239, 240, 240, 241, 241, 242, 242, 243, 243, 244, 244, 245, 245, 246, 246, 247, 247, 248, 248, 249, 249, 250, 250, 251, 251, 252, 252, 253, 253, 254, 254, 255, 255];
            byte[] LastPixelB = [0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 22, 23, 23, 24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 29, 30, 30, 31, 31, 32, 32, 33, 33, 34, 34, 35, 35, 36, 36, 37, 37, 38, 38, 39, 39, 40, 40, 41, 41, 42, 42, 43, 43, 44, 44, 45, 45, 46, 46, 47, 47, 48, 48, 49, 49, 50, 50, 51, 51, 52, 52, 53, 53, 54, 54, 55, 55, 56, 56, 57, 57, 58, 58, 59, 59, 60, 60, 61, 61, 62, 62, 63, 63, 64, 64, 65, 65, 66, 66, 67, 67, 68, 68, 69, 69, 70, 70, 71, 71, 72, 72, 73, 73, 74, 74, 75, 75, 76, 76, 77, 77, 78, 78, 79, 79, 80, 80, 81, 81, 82, 82, 83, 83, 84, 84, 85, 85, 86, 86, 87, 87, 88, 88, 89, 89, 90, 90, 91, 91, 92, 92, 93, 93, 94, 94, 95, 95, 96, 96, 97, 97, 98, 98, 99, 99, 100, 100, 101, 101, 102, 102, 103, 103, 104, 104, 105, 105, 106, 106, 107, 107, 108, 108, 109, 109, 110, 110, 111, 111, 112, 112, 113, 113, 114, 114, 115, 115, 116, 116, 117, 117, 118, 118, 119, 119, 120, 120, 121, 121, 122, 122, 123, 123, 124, 124, 125, 125, 126, 126, 127, 127];

            bool lastPixel = true;//Last pixel for previous byte
            for (int index = 62; index < Size; index++)
            {
                byte value = Content[index];
                Content[index] = (lastPixel == false) ? LastPixelB[value] : LastPixelW[value];
                lastPixel = value % 2 != 0;
            }*/
        }

        public string GetText()
        {
            /*int reference = 62 + Chunks * (Height - 1);
            for (int y = 0; y < Height - 10; y++)
            {
                for (int chunk = 0; chunk < Chunks - 4; chunk++)
                {
                    int index = reference - y * Chunks + chunk;
                    if (Content[index] == 255)
                        continue;

                    char character = GetCharacter(index);

                    if (character == '␀')
                        continue;

                }//→
            }//↓

            //for (int shift = 0; shift < 8; shift++)
            //{
            //ShiftRight();

            //}//Shift Right*/

            return String.Empty;
        }
    }
}
