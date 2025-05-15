using System.Collections;
using System.Drawing;
using System.Xml;

namespace CV_2025.CristalVision.Vision
{
    struct Section
    {
        public enum Format { Square, Vertical, Horizontal, Irregular }
        public int Width, Height, Area, PositionX, PositionY, chunk, index;

        /// <summary>
        /// Monochrome Image width + extra pixels
        /// </summary>
        public int FullWidth;

        /// <summary>
        /// Horizontal position relative to the image
        /// </summary>
        public int ReferenceX;

        /// <summary>
        /// Vertical position relative to the image
        /// </summary>
        public int ReferenceY;

        /// <summary>
        /// Brightness of each pixel for this section
        /// </summary>
        public List<float> Brightness;

        public float BrightAverage { get; set; }

        /// <summary>
        /// Set brightness values|average for this section
        /// </summary>
        public void SetBrightness(Format format, ref Bitmap ColorImage)
        {
            Brightness = new(Area);

            if (format == Format.Square)
            {
                for (int y = ReferenceY; y < ReferenceY + Height; y++)
                    for (int x = ReferenceX; x < ReferenceX + Width; x++)
                        Brightness.Add(ColorImage.GetPixel(x, y).GetBrightness());
            }//Full sections


            if (format == Format.Vertical)
            {
                for (int y = ReferenceY; y < ReferenceY + Height; y++)
                {
                    for (int x = ColorImage.Width - ColorImage.Width % 48; x < ColorImage.Width; x++)
                        Brightness.Add(ColorImage.GetPixel(x, y).GetBrightness());

                    for (int x = ColorImage.Width; x < FullWidth; x++)
                        Brightness.Add(0);
                }
            }//Right edge sections


            if (format == Format.Horizontal)
            {
                for (int y = ColorImage.Height - Height; y < ColorImage.Height; y++)
                    for (int x = ReferenceX; x < ReferenceX + Width; x++)
                        Brightness.Add(ColorImage.GetPixel(x, y).GetBrightness());
            }//Bottom edge sections


            if (format == Format.Irregular)
            {
                for (int y = ColorImage.Height - Height; y < ColorImage.Height; y++)
                {
                    for (int x = ColorImage.Width - ColorImage.Width % 48; x < ColorImage.Width; x++)
                        Brightness.Add(ColorImage.GetPixel(x, y).GetBrightness());

                    for (int x = ColorImage.Width; x < FullWidth; x++)
                        Brightness.Add(0);
                }
            }//Right-bottom section


            List<float> AvrageArea = [.. Brightness];
            AvrageArea.RemoveAll(value => value == 0);
            AvrageArea.RemoveAll(value => value == 1);
            BrightAverage = AvrageArea.Count != 0 ? AvrageArea.Average() - 0.1F : 0.5F;
            if (BrightAverage < 0) BrightAverage = 0.01F;
        }

        /// <summary>
        /// Turn section into black and white based on brightness average
        /// </summary>
        public void ColorToMonochrome(ref byte[] Content, Delegate setIndex)
        {
            for (int i = 0; i < Area; i += 8)
            {
                BitArray bitArray = new(8, true);
                for (int x_line = 0; x_line < 8; x_line++)//int x1 = (i + x_line) % 48;
                    if (Brightness[i + x_line] < BrightAverage) bitArray.Set(7 - x_line, false);

                PositionX = i % Width;
                PositionY = (i - PositionX) / Width;

                chunk = (PositionX - PositionX % 8 + 8) / 8 - 1;
                setIndex.DynamicInvoke();
                bitArray.CopyTo(Content, index);
            }
        }
    }

    public class Monochrome
    {
        public Bitmap ColorImage;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public int Size;

        /// <summary>
        /// Real width does not include ExtraPixels
        /// </summary>
        public int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public int Height;

        /// <summary>
        /// Total Area includes ExtraPixels
        /// </summary>
        public int TotalArea;

        /// <summary>
        /// Pixels outside image width
        /// </summary>
        public int ExtraPixels;

        /// <summary>
        /// FullWidth includes ExtraPixels
        /// </summary>
        public int FullWidth;

        /// <summary>
        /// Total width as 32 pixels chunks including ExtraPixels
        /// </summary>
        public int Chunks;

        /// <summary>
        /// Image file as bytes
        /// </summary>
        public byte[] Content;

        public Monochrome(Stream? stream)
        {
            ColorImage = new(stream);

            //┌───────────────────────Check header────────────────────────┐

            //└───────────────────────Check header────────────────────────┘


            //┌────────────────────────Image width────────────────────────┐
            Width = ColorImage.Width;
            int byte18 = 0, byte19 = 0;
            if (Width > 255)
            {
                byte18 = Width % 256;
                byte19 = (Width - Width % 256) / 256;
            }
            else
            {
                byte18 = Width;
            }
            if (Width > 8192)
                throw new Exception("Widh out of range");
            //└────────────────────────Image width────────────────────────┘


            //┌───────────────────────Image Height────────────────────────┐
            Height = ColorImage.Height;
            int byte22, byte23 = 0;
            if (Height > 255)
            {
                byte22 = Height % 256;
                byte23 = (Height - Height % 256) / 256;
            }
            else
            {
                byte22 = Height;
            }

            if (Height > 8192)
                throw new Exception("Height out of range");
            //└───────────────────────Image Height────────────────────────┘


            //┌───────────────────Remaining properties────────────────────┐
            ExtraPixels = Width % 32;
            if (ExtraPixels != 0) ExtraPixels = 32 - ExtraPixels;
            TotalArea = (Width + ExtraPixels) * Height;
            FullWidth = Width + ExtraPixels;
            Size = FullWidth / 8 * Height + 62;
            Chunks = FullWidth / 8;
            //└───────────────────Remaining properties────────────────────┘


            //┌─────────────────────────File size─────────────────────────┐
            int byte2, byte3, byte4;
            byte2 = Size % 256;
            byte3 = (Size - byte2) % 65536 / 256;
            byte4 = (Size - byte2 - byte3 * 256) / 65536;
            //└─────────────────────────File size─────────────────────────┘


            //┌────────────────────────Total area─────────────────────────┐
            int byte34, byte35, byte36;
            byte34 = TotalArea % 256;
            byte35 = (TotalArea - byte34) % 65536 / 256;
            byte36 = (TotalArea - byte34 - byte35 * 256) / 65536;
            //└────────────────────────Total area─────────────────────────┘


            //┌───────────────────────Write Content───────────────────────┐
            Content = new byte[Size];
            byte[] header = [66, 77, (byte)byte2, (byte)byte3, (byte)byte4, 0, 0, 0, 0, 0, 62, 0, 0, 0, 40, 0, 0, 0, (byte)byte18, (byte)byte19, 0, 0, (byte)byte22, (byte)byte23, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, (byte)byte34, (byte)byte35, (byte)byte36, 0, 195, 14, 0, 0, 195, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 0];
            header.CopyTo(Content, 0);
            for (int index = 62; index < Size; index++) Content[index] = 255;
            //└───────────────────────Write Content───────────────────────┘


            //┌─────────────────────24 bit to 2 colors────────────────────┐
            /// <summary>
            /// Color to black and white based on averge brightness of 48x48 sections
            /// </summary>
            Section section;

            for (int referenceY = 0; referenceY < Height - Height % 48; referenceY += 48)
            {
                for (int referenceX = 0; referenceX < Width - Width % 48; referenceX += 48)
                {
                    section = new() { Width = 48, Height = 48, ReferenceX = referenceX, ReferenceY = referenceY, Area = 2304 };
                    section.SetBrightness(Section.Format.Square, ref ColorImage);
                    section.ColorToMonochrome(ref Content, () => {
                        section.index = 62 + section.chunk + section.ReferenceX / 8 + Chunks * (Height - referenceY - section.PositionY - 1);
                    });
                }//48 ⨯ 48 →

                section = new() { Width = FullWidth - Width + Width % 48, Height = 48, ReferenceY = referenceY, FullWidth = FullWidth, Area = (FullWidth - Width + Width % 48) * 48 };
                section.SetBrightness(Section.Format.Vertical, ref ColorImage);
                section.ColorToMonochrome(ref Content, () => {
                    section.index = 62 + section.chunk + (Width - Width % 48) / 8 + Chunks * (Height - referenceY - section.PositionY - 1);
                });//width ⨯ 48

            }//48 ⨯ 48 ↓

            for (int referenceX = 0; referenceX < Width - Width % 48; referenceX += 48)
            {
                section = new() { Width = 48, Height = Height % 48, ReferenceX = referenceX, Area = Height % 48 * 48 };
                section.SetBrightness(Section.Format.Horizontal, ref ColorImage);
                section.ColorToMonochrome(ref Content, () => {
                    section.index = 62 + section.chunk + section.ReferenceX / 8 + Chunks * (Height % 48 - section.PositionY - 1);
                });

            }//48 ⨯ height →

            section = new() { Width = FullWidth - Width + Width % 48, Height = Height % 48, Area = (FullWidth - Width + Width % 48) * (Height % 48), FullWidth = FullWidth };
            section.SetBrightness(Section.Format.Irregular, ref ColorImage);
            section.ColorToMonochrome(ref Content, () => {
                section.index = 62 + section.chunk + (Width - Width % 48) / 8 + Chunks * (Height - (Height - Height % 48) - section.PositionY - 1);
            });//width ⨯ height
            //└─────────────────────24 bit to 2 colors────────────────────┘


            //┌──────────────────Ceate 1px white outline──────────────────┐

            //└──────────────────Ceate 1px white outline──────────────────┘
        }

        public Monochrome(int width, int height)
        {
            //┌────────────────────────Image width────────────────────────┐
            Width = width;
            int byte18 = 0, byte19 = 0;
            if (Width > 255)
            {
                byte18 = Width % 256;
                byte19 = (Width - Width % 256) / 256;
            }
            else
            {
                byte18 = Width;
            }
            if (Width > 8192)
                throw new Exception("Widh out of range");
            //└────────────────────────Image width────────────────────────┘


            //┌───────────────────────Image Height────────────────────────┐
            Height = height;
            int byte22, byte23 = 0;
            if (Height > 255)
            {
                byte22 = Height % 256;
                byte23 = (Height - Height % 256) / 256;
            }
            else
            {
                byte22 = Height;
            }

            if (Height > 8192)
                throw new Exception("Height out of range");
            //└───────────────────────Image Height────────────────────────┘


            //┌───────────────────Remaining properties────────────────────┐
            ExtraPixels = Width % 32;
            if (ExtraPixels != 0) ExtraPixels = 32 - ExtraPixels;
            TotalArea = (Width + ExtraPixels) * Height;
            FullWidth = Width + ExtraPixels;
            Size = FullWidth / 8 * Height + 62;
            Chunks = FullWidth / 8;
            //└───────────────────Remaining properties────────────────────┘


            //┌─────────────────────────File size─────────────────────────┐
            int byte2, byte3, byte4;
            byte2 = Size % 256;
            byte3 = (Size - byte2) % 65536 / 256;
            byte4 = (Size - byte2 - byte3 * 256) / 65536;
            //└─────────────────────────File size─────────────────────────┘


            //┌────────────────────────Total area─────────────────────────┐
            int byte34, byte35, byte36;
            byte34 = TotalArea % 256;
            byte35 = (TotalArea - byte34) % 65536 / 256;
            byte36 = (TotalArea - byte34 - byte35 * 256) / 65536;
            //└────────────────────────Total area─────────────────────────┘


            //┌───────────────────────Write Content───────────────────────┐
            Content = new byte[Size];
            byte[] header = [66, 77, (byte)byte2, (byte)byte3, (byte)byte4, 0, 0, 0, 0, 0, 62, 0, 0, 0, 40, 0, 0, 0, (byte)byte18, (byte)byte19, 0, 0, (byte)byte22, (byte)byte23, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, (byte)byte34, (byte)byte35, (byte)byte36, 0, 195, 14, 0, 0, 195, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 0];
            header.CopyTo(Content, 0);
            for (int index = 62; index < Size; index++) Content[index] = 255;
            //└───────────────────────Write Content───────────────────────┘
        }

        public Monochrome(int width, int height, Stream stream)
        {
            //┌─────────────────────────File size─────────────────────────┐
            string byte2 = Convert.ToString(Content[2], 2).PadLeft(8, '0');
            string byte3 = Convert.ToString(Content[3], 2).PadLeft(8, '0');
            string byte4 = Convert.ToString(Content[4], 2).PadLeft(8, '0');
            Size = Convert.ToInt32(byte4 + byte3 + byte2, 2);
            //└─────────────────────────File size─────────────────────────┘


            //┌────────────────────────Image width────────────────────────┐
            string byte18 = Convert.ToString(Content[18], 2).PadLeft(8, '0');
            string byte19 = Convert.ToString(Content[19], 2).PadLeft(8, '0');
            Width = Convert.ToInt32(byte19 + byte18, 2);

            if (Width > 8192)
                throw new Exception("Widh out of range");
            //└────────────────────────Image width────────────────────────┘


            //┌───────────────────────Image Height────────────────────────┐
            string byte22 = Convert.ToString(Content[22], 2).PadLeft(8, '0');
            string byte23 = Convert.ToString(Content[23], 2).PadLeft(8, '0');
            Height = Convert.ToInt32(byte23 + byte22, 2);

            if (Height > 8192)
                throw new Exception("Height out of range");
            //└───────────────────────Image Height────────────────────────┘


            //┌────────────────────────Total area─────────────────────────┐
            string byte34 = Convert.ToString(Content[34], 2).PadLeft(8, '0');
            string byte35 = Convert.ToString(Content[35], 2).PadLeft(8, '0');
            string byte36 = Convert.ToString(Content[36], 2).PadLeft(8, '0');
            TotalArea = Convert.ToInt32(byte36 + byte35 + byte34, 2);
            //└────────────────────────Total area─────────────────────────┘

            //┌───────────────────Remaining properties────────────────────┐
            ExtraPixels = Width % 32;
            if (ExtraPixels != 0) ExtraPixels = 32 - ExtraPixels;

            Chunks = (Size - 62) / Height;
            //└───────────────────Remaining properties────────────────────┘

            //Stream fileStream = new FileStream("C:\\Users\\user\\source\\repos\\OCRScore\\Test.bmp", FileMode.Create);
            //fileStream.Write(bytes, 0, bytes.Length);
            //fileStream.Close();
        }

        public int GetPixel(int x, int y)
        {
            if (!Enumerable.Range(0, Width).Contains(x) || !Enumerable.Range(0, Height).Contains(y))
                throw new Exception("Position out of range");

            int chunk = (x - x % 8 + 8) / 8 - 1;
            int index = Chunks * (Height - y - 1) + chunk + 62;

            string colors = Convert.ToString(Content[index], 2).PadLeft(8, '0');
            int position = x % 8;
            int color = colors[position] == '0' ? 0 : 255;

            return color;
        }

        public void SetPixel(int x, int y, byte color)
        {
            int chunk = (x - x % 8 + 8) / 8 - 1;
            int index = 62 + chunk + Chunks * (Height - y - 1);
            int position = x % 8;
            byte value = Content[index];

            Content[index] = 0;
        }

        /// <summary>
        /// Position x devides with 8
        /// </summary>
        public int[] GetPosition(int index)
        {
            if (!Enumerable.Range(62, Size).Contains(index))
                throw new Exception("Index out of range");

            int x = (index - 62) % Chunks * 8;
            int y = Height - 1 - (index - x / 8 - 62) / Chunks;

            return [x, y];
        }

        /// <summary>
        /// Index starts at 62
        /// </summary>
        public int GetIndex(int x, int y)
        {
            if (!Enumerable.Range(0, Width).Contains(x) || !Enumerable.Range(0, Height).Contains(y))
                throw new Exception("Position out of range");

            int chunk = (x - x % 8 + 8) / 8 - 1;
            int index = 62 + chunk + Chunks * (Height - y - 1);

            return index;
        }

        /// <summary>
        /// Get area as top left reference
        /// </summary>
        public byte[] GetSection(int reference, int width, int height)
        {
            //Sa verific si daca referinta este buna; ex: 62, 80, 10 nu este bine - este in afara imaginii

            if (!Enumerable.Range(62, Size).Contains(reference))
                throw new Exception("Reference out of range");

            if (!Enumerable.Range(0, Width + 1).Contains(width) || !Enumerable.Range(0, Height + 1).Contains(height))
                throw new Exception("Position out of range");

            if (width % 8 != 0)
                throw new Exception("Width not divisible to 8");

            int sourceIndex, destIndex = 0, chunks = width / 8, size = chunks * height;
            byte[] section = new byte[size];

            for (int indexH = 0; indexH < height; indexH++)
            {
                for (int chunk = 0; chunk < chunks; chunk++)
                {
                    sourceIndex = chunk + reference - indexH * Chunks;
                    section[destIndex] = Content[sourceIndex];
                    destIndex++;
                }
            }

            return section;
        }

        /// <summary>
        /// Fill section with given color
        /// </summary>
        public void FillSection(int reference, int width, int height, byte color)
        {
            if (!Enumerable.Range(62, Size).Contains(reference))
                throw new Exception("Reference out of range");

            if (!Enumerable.Range(0, Width).Contains(width) || !Enumerable.Range(0, Height).Contains(height))
                throw new Exception("Position out of range");

            if (width % 8 != 0)
                throw new Exception("Width not divisible to 8");

            if (!Enumerable.Range(0, 256).Contains(color))
                throw new Exception("Color out of range");

            int index, chunks = width / 8;
            for (int indexH = 0; indexH < height; indexH++)
            {
                for (int chunk = 0; chunk < chunks; chunk++)
                {
                    index = chunk + reference - indexH * Chunks;
                    Content[index] = color;
                }
            }
        }

        public XmlDocument ToSVG()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));

            XmlElement svg = document.CreateElement("svg");
            svg.SetAttribute("viewBox", "0 0 " + Width + " " + Height);
            svg.SetAttribute("version", "1.1");
            svg.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            svg.SetAttribute("xmlns:svg", "http://www.w3.org/2000/svg");
            svg.SetAttribute("style", "background: LightSkyBlue");
            document.AppendChild(svg);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int color = GetPixel(x, y);
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

            return document;
        }

        public static int CountBlackPixels(byte[] section)
        {
            int[] blackPixels = [8, 7, 7, 6, 7, 6, 6, 5, 7, 6, 6, 5, 6, 5, 5, 4, 7, 6, 6, 5, 6, 5, 5, 4, 6, 5, 5, 4, 5, 4, 4, 3, 7, 6, 6, 5, 6, 5, 5, 4, 6, 5, 5, 4, 5, 4, 4, 3, 6, 5, 5, 4, 5, 4, 4, 3, 5, 4, 4, 3, 4, 3, 3, 2, 7, 6, 6, 5, 6, 5, 5, 4, 6, 5, 5, 4, 5, 4, 4, 3, 6, 5, 5, 4, 5, 4, 4, 3, 5, 4, 4, 3, 4, 3, 3, 2, 6, 5, 5, 4, 5, 4, 4, 3, 5, 4, 4, 3, 4, 3, 3, 2, 5, 4, 4, 3, 4, 3, 3, 2, 4, 3, 3, 2, 3, 2, 2, 1, 7, 6, 6, 5, 6, 5, 5, 4, 6, 5, 5, 4, 5, 4, 4, 3, 6, 5, 5, 4, 5, 4, 4, 3, 5, 4, 4, 3, 4, 3, 3, 2, 6, 5, 5, 4, 5, 4, 4, 3, 5, 4, 4, 3, 4, 3, 3, 2, 5, 4, 4, 3, 4, 3, 3, 2, 4, 3, 3, 2, 3, 2, 2, 1, 6, 5, 5, 4, 5, 4, 4, 3, 5, 4, 4, 3, 4, 3, 3, 2, 5, 4, 4, 3, 4, 3, 3, 2, 4, 3, 3, 2, 3, 2, 2, 1, 5, 4, 4, 3, 4, 3, 3, 2, 4, 3, 3, 2, 3, 2, 2, 1, 4, 3, 3, 2, 3, 2, 2, 1, 3, 2, 2, 1, 2, 1, 1];

            int colorBytes = 0;
            for (int k = 0; k < section.Length; k++)
            {
                if (section[k] == 255) continue;

                colorBytes += blackPixels[section[k]];
            }

            return colorBytes;
        }
    }

    public class Bitmap256
    {
        public Bitmap ColorImage { get; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public int Size;

        /// <summary>
        /// Real width does not include ExtraPixels
        /// </summary>
        public int Width;

        /// <summary>
        /// FullWidth includes ExtraPixels
        /// </summary>
        public int FullWidth;

        /// <summary>
        /// Image height
        /// </summary>
        public int Height;

        /// <summary>
        /// Pixels outside image width
        /// </summary>
        public int ExtraPixels;

        /// <summary>
        /// Image file as bytes
        /// </summary>
        public byte[] Content;

        public Bitmap256(Stream? stream)
        {
            ColorImage = new(stream);

            //┌───────────────────────Check header────────────────────────┐

            //└───────────────────────Check header────────────────────────┘


            //┌────────────────────────Image width────────────────────────┐
            Width = ColorImage.Width;
            int byte18 = 0, byte19 = 0;
            if (Width > 255)
            {
                byte18 = Width % 256;
                byte19 = (Width - Width % 256) / 256;
            }
            else
            {
                byte18 = Width;
            }
            if (Width > 8192)
                throw new Exception("Widh out of range");
            //└────────────────────────Image width────────────────────────┘


            //┌───────────────────────Image Height────────────────────────┐
            Height = ColorImage.Height;
            int byte22, byte23 = 0;
            if (Height > 255)
            {
                byte22 = Height % 256;
                byte23 = (Height - Height % 256) / 256;
            }
            else
            {
                byte22 = Height;
            }

            if (Height > 8192)
                throw new Exception("Height out of range");
            //└───────────────────────Image Height────────────────────────┘


            //┌───────────────────Remaining properties────────────────────┐
            ExtraPixels = Width % 4;
            if (ExtraPixels != 0) ExtraPixels = 4 - ExtraPixels;
            FullWidth = Width + ExtraPixels;
            //└───────────────────Remaining properties────────────────────┘


            //┌─────────────────────────File size─────────────────────────┐
            Size = (Width + ExtraPixels) * Height + 54 + 1024;
            int byte2, byte3, byte4;
            byte2 = Size % 256;
            byte3 = (Size - byte2) % 65536 / 256;
            byte4 = (Size - byte2 - byte3 * 256) / 65536;
            //└─────────────────────────File size─────────────────────────┘


            //┌─────────────────────────Real area─────────────────────────┐
            int byte34 = (Size - 54 - 1024) % 256;
            int byte35 = (Size - 54 - 1024 - byte34) / 256;
            int byte36 = 0;
            if (byte35 > 255)
            {
                byte35 = byte35 % 256;
                byte36 = (Size - 54 - 124 - byte34 - byte35 * 256) / 65536;
            }
            //└─────────────────────────Real area─────────────────────────┘


            //┌───────────────────────Write Content───────────────────────┐
            Content = new byte[Size];
            byte[] header = [66, 77, (byte)byte2, (byte)byte3, (byte)byte4, 0, 0, 0, 0, 0, 54, 4, 0, 0, 40, 0, 0, 0, (byte)byte18, (byte)byte19, 0, 0, (byte)byte22, (byte)byte23, 0, 0, 1, 0, 8, 0, 0, 0, 0, 0, (byte)byte34, (byte)byte35, (byte)byte36, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            byte[] header256 = [0, 0, 0, 0, 0, 0, 128, 0, 0, 128, 0, 0, 0, 128, 128, 0, 128, 0, 0, 0, 128, 0, 128, 0, 128, 128, 0, 0, 192, 192, 192, 0, 192, 220, 192, 0, 240, 202, 166, 0, 0, 32, 64, 0, 0, 32, 96, 0, 0, 32, 128, 0, 0, 32, 160, 0, 0, 32, 192, 0, 0, 32, 224, 0, 0, 64, 0, 0, 0, 64, 32, 0, 0, 64, 64, 0, 0, 64, 96, 0, 0, 64, 128, 0, 0, 64, 160, 0, 0, 64, 192, 0, 0, 64, 224, 0, 0, 96, 0, 0, 0, 96, 32, 0, 0, 96, 64, 0, 0, 96, 96, 0, 0, 96, 128, 0, 0, 96, 160, 0, 0, 96, 192, 0, 0, 96, 224, 0, 0, 128, 0, 0, 0, 128, 32, 0, 0, 128, 64, 0, 0, 128, 96, 0, 0, 128, 128, 0, 0, 128, 160, 0, 0, 128, 192, 0, 0, 128, 224, 0, 0, 160, 0, 0, 0, 160, 32, 0, 0, 160, 64, 0, 0, 160, 96, 0, 0, 160, 128, 0, 0, 160, 160, 0, 0, 160, 192, 0, 0, 160, 224, 0, 0, 192, 0, 0, 0, 192, 32, 0, 0, 192, 64, 0, 0, 192, 96, 0, 0, 192, 128, 0, 0, 192, 160, 0, 0, 192, 192, 0, 0, 192, 224, 0, 0, 224, 0, 0, 0, 224, 32, 0, 0, 224, 64, 0, 0, 224, 96, 0, 0, 224, 128, 0, 0, 224, 160, 0, 0, 224, 192, 0, 0, 224, 224, 0, 64, 0, 0, 0, 64, 0, 32, 0, 64, 0, 64, 0, 64, 0, 96, 0, 64, 0, 128, 0, 64, 0, 160, 0, 64, 0, 192, 0, 64, 0, 224, 0, 64, 32, 0, 0, 64, 32, 32, 0, 64, 32, 64, 0, 64, 32, 96, 0, 64, 32, 128, 0, 64, 32, 160, 0, 64, 32, 192, 0, 64, 32, 224, 0, 64, 64, 0, 0, 64, 64, 32, 0, 64, 64, 64, 0, 64, 64, 96, 0, 64, 64, 128, 0, 64, 64, 160, 0, 64, 64, 192, 0, 64, 64, 224, 0, 64, 96, 0, 0, 64, 96, 32, 0, 64, 96, 64, 0, 64, 96, 96, 0, 64, 96, 128, 0, 64, 96, 160, 0, 64, 96, 192, 0, 64, 96, 224, 0, 64, 128, 0, 0, 64, 128, 32, 0, 64, 128, 64, 0, 64, 128, 96, 0, 64, 128, 128, 0, 64, 128, 160, 0, 64, 128, 192, 0, 64, 128, 224, 0, 64, 160, 0, 0, 64, 160, 32, 0, 64, 160, 64, 0, 64, 160, 96, 0, 64, 160, 128, 0, 64, 160, 160, 0, 64, 160, 192, 0, 64, 160, 224, 0, 64, 192, 0, 0, 64, 192, 32, 0, 64, 192, 64, 0, 64, 192, 96, 0, 64, 192, 128, 0, 64, 192, 160, 0, 64, 192, 192, 0, 64, 192, 224, 0, 64, 224, 0, 0, 64, 224, 32, 0, 64, 224, 64, 0, 64, 224, 96, 0, 64, 224, 128, 0, 64, 224, 160, 0, 64, 224, 192, 0, 64, 224, 224, 0, 128, 0, 0, 0, 128, 0, 32, 0, 128, 0, 64, 0, 128, 0, 96, 0, 128, 0, 128, 0, 128, 0, 160, 0, 128, 0, 192, 0, 128, 0, 224, 0, 128, 32, 0, 0, 128, 32, 32, 0, 128, 32, 64, 0, 128, 32, 96, 0, 128, 32, 128, 0, 128, 32, 160, 0, 128, 32, 192, 0, 128, 32, 224, 0, 128, 64, 0, 0, 128, 64, 32, 0, 128, 64, 64, 0, 128, 64, 96, 0, 128, 64, 128, 0, 128, 64, 160, 0, 128, 64, 192, 0, 128, 64, 224, 0, 128, 96, 0, 0, 128, 96, 32, 0, 128, 96, 64, 0, 128, 96, 96, 0, 128, 96, 128, 0, 128, 96, 160, 0, 128, 96, 192, 0, 128, 96, 224, 0, 128, 128, 0, 0, 128, 128, 32, 0, 128, 128, 64, 0, 128, 128, 96, 0, 128, 128, 128, 0, 128, 128, 160, 0, 128, 128, 192, 0, 128, 128, 224, 0, 128, 160, 0, 0, 128, 160, 32, 0, 128, 160, 64, 0, 128, 160, 96, 0, 128, 160, 128, 0, 128, 160, 160, 0, 128, 160, 192, 0, 128, 160, 224, 0, 128, 192, 0, 0, 128, 192, 32, 0, 128, 192, 64, 0, 128, 192, 96, 0, 128, 192, 128, 0, 128, 192, 160, 0, 128, 192, 192, 0, 128, 192, 224, 0, 128, 224, 0, 0, 128, 224, 32, 0, 128, 224, 64, 0, 128, 224, 96, 0, 128, 224, 128, 0, 128, 224, 160, 0, 128, 224, 192, 0, 128, 224, 224, 0, 192, 0, 0, 0, 192, 0, 32, 0, 192, 0, 64, 0, 192, 0, 96, 0, 192, 0, 128, 0, 192, 0, 160, 0, 192, 0, 192, 0, 192, 0, 224, 0, 192, 32, 0, 0, 192, 32, 32, 0, 192, 32, 64, 0, 192, 32, 96, 0, 192, 32, 128, 0, 192, 32, 160, 0, 192, 32, 192, 0, 192, 32, 224, 0, 192, 64, 0, 0, 192, 64, 32, 0, 192, 64, 64, 0, 192, 64, 96, 0, 192, 64, 128, 0, 192, 64, 160, 0, 192, 64, 192, 0, 192, 64, 224, 0, 192, 96, 0, 0, 192, 96, 32, 0, 192, 96, 64, 0, 192, 96, 96, 0, 192, 96, 128, 0, 192, 96, 160, 0, 192, 96, 192, 0, 192, 96, 224, 0, 192, 128, 0, 0, 192, 128, 32, 0, 192, 128, 64, 0, 192, 128, 96, 0, 192, 128, 128, 0, 192, 128, 160, 0, 192, 128, 192, 0, 192, 128, 224, 0, 192, 160, 0, 0, 192, 160, 32, 0, 192, 160, 64, 0, 192, 160, 96, 0, 192, 160, 128, 0, 192, 160, 160, 0, 192, 160, 192, 0, 192, 160, 224, 0, 192, 192, 0, 0, 192, 192, 32, 0, 192, 192, 64, 0, 192, 192, 96, 0, 192, 192, 128, 0, 192, 192, 160, 0, 240, 251, 255, 0, 164, 160, 160, 0, 128, 128, 128, 0, 0, 0, 255, 0, 0, 255, 0, 0, 0, 255, 255, 0, 255, 0, 0, 0, 255, 0, 255, 0, 255, 255, 0, 0, 255, 255, 255, 0];
            header.CopyTo(Content, 0);
            header256.CopyTo(Content, 54);
            for (int index = 1078; index < Size; index++) Content[index] = 255;
            //└───────────────────────Write Content───────────────────────┘


            //┌─────────────────────24 bit to 5 colors────────────────────┐
            /// <summary>
            /// Color to 5 colors based on averge brightness of 48x48 sections
            /// </summary>
            for (int top = 0; top < Height - Height % 40; top += 40)
            {
                for (int left = 0; left < Width - Width % 40; left += 40)
                {
                    List<float> brightness = new List<float>(1600);
                    for (int y = top; y < top + 40; y++)
                        for (int x = left; x < left + 40; x++)
                            brightness.Add(ColorImage.GetPixel(x, y).GetBrightness());

                    float averange = brightness.Average() - 0.1F;

                    for (int i = 0; i < 1600; i++)
                    {
                        int x = i % 40;
                        int y = (i - x) / 40;
                        int index = 1078 + (Height - 1 - y - top) * FullWidth + x + left;

                        if (brightness[i] < averange - 0.115F)
                        {
                            Content[index] = 0;
                            continue;
                        }

                        if (brightness[i] < averange - 0.05F)
                        {
                            Content[index] = 82;
                            continue;
                        }

                        if (brightness[i] < averange)
                        {
                            Content[index] = 164;
                            continue;
                        }

                        if (brightness[i] < averange + 0.015F)
                            Content[index] = 7;
                    }

                }//40 ⨯ 40 →
            }//40 ⨯ 40 ↓
            //└─────────────────────24 bit to 5 colors────────────────────┘


            //┌──────────────────Ceate 1px white outline──────────────────┐
            for (int y = 1; y < Height; y++) Content[1078 + y * FullWidth] = 255;//Left
            for (int y = 1; y < Height; y++) Content[1078 + y * FullWidth + Width - 1] = 255;//Right
            for (int index = Size - FullWidth; index < Size; index++) Content[index] = 255;//Top
            for (int index = 1078; index < 1078 + Width; index++) Content[index] = 255;//Bottom
            //└──────────────────Ceate 1px white outline──────────────────┘
        }

        public Bitmap256(int width, int height)
        {
            //┌────────────────────────Image width────────────────────────┐
            Width = width;
            int byte18 = 0, byte19 = 0;
            if (Width > 255)
            {
                byte18 = Width % 256;
                byte19 = (Width - Width % 256) / 256;
            }
            else
            {
                byte18 = Width;
            }
            if (Width > 8192)
                throw new Exception("Widh out of range");
            //└────────────────────────Image width────────────────────────┘


            //┌───────────────────────Image Height────────────────────────┐
            Height = height;
            int byte22, byte23 = 0;
            if (Height > 255)
            {
                byte22 = Height % 256;
                byte23 = (Height - Height % 256) / 256;
            }
            else
            {
                byte22 = Height;
            }

            if (Height > 8192)
                throw new Exception("Height out of range");
            //└───────────────────────Image Height────────────────────────┘


            //┌───────────────────Remaining properties────────────────────┐
            ExtraPixels = Width % 4;
            if (ExtraPixels != 0) ExtraPixels = 4 - ExtraPixels;
            FullWidth = Width + ExtraPixels;
            //└───────────────────Remaining properties────────────────────┘


            //┌─────────────────────────File size─────────────────────────┐
            Size = (Width + ExtraPixels) * Height + 54 + 1024;

            int byte2, byte3, byte4;
            byte2 = Size % 256;
            byte3 = (Size - byte2) % 65536 / 256;
            byte4 = (Size - byte2 - byte3 * 256) / 65536;
            //└─────────────────────────File size─────────────────────────┘


            //┌─────────────────────────Real area─────────────────────────┐
            int byte34 = (Size - 54 - 1024) % 256;
            int byte35 = (Size - 54 - 1024 - byte34) / 256;
            int byte36 = 0;
            if (byte35 > 255)
            {
                byte35 = byte35 % 256;
                byte36 = (Size - 54 - 124 - byte34 - byte35 * 256) / 65536;
            }
            //└─────────────────────────Real area─────────────────────────┘


            //┌───────────────────────Write Content───────────────────────┐
            Content = new byte[Size];
            byte[] header = [66, 77, (byte)byte2, (byte)byte3, (byte)byte4, 0, 0, 0, 0, 0, 54, 4, 0, 0, 40, 0, 0, 0, (byte)byte18, (byte)byte19, 0, 0, (byte)byte22, (byte)byte23, 0, 0, 1, 0, 8, 0, 0, 0, 0, 0, (byte)byte34, (byte)byte35, (byte)byte36, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            byte[] header256 = [0, 0, 0, 0, 0, 0, 128, 0, 0, 128, 0, 0, 0, 128, 128, 0, 128, 0, 0, 0, 128, 0, 128, 0, 128, 128, 0, 0, 192, 192, 192, 0, 192, 220, 192, 0, 240, 202, 166, 0, 0, 32, 64, 0, 0, 32, 96, 0, 0, 32, 128, 0, 0, 32, 160, 0, 0, 32, 192, 0, 0, 32, 224, 0, 0, 64, 0, 0, 0, 64, 32, 0, 0, 64, 64, 0, 0, 64, 96, 0, 0, 64, 128, 0, 0, 64, 160, 0, 0, 64, 192, 0, 0, 64, 224, 0, 0, 96, 0, 0, 0, 96, 32, 0, 0, 96, 64, 0, 0, 96, 96, 0, 0, 96, 128, 0, 0, 96, 160, 0, 0, 96, 192, 0, 0, 96, 224, 0, 0, 128, 0, 0, 0, 128, 32, 0, 0, 128, 64, 0, 0, 128, 96, 0, 0, 128, 128, 0, 0, 128, 160, 0, 0, 128, 192, 0, 0, 128, 224, 0, 0, 160, 0, 0, 0, 160, 32, 0, 0, 160, 64, 0, 0, 160, 96, 0, 0, 160, 128, 0, 0, 160, 160, 0, 0, 160, 192, 0, 0, 160, 224, 0, 0, 192, 0, 0, 0, 192, 32, 0, 0, 192, 64, 0, 0, 192, 96, 0, 0, 192, 128, 0, 0, 192, 160, 0, 0, 192, 192, 0, 0, 192, 224, 0, 0, 224, 0, 0, 0, 224, 32, 0, 0, 224, 64, 0, 0, 224, 96, 0, 0, 224, 128, 0, 0, 224, 160, 0, 0, 224, 192, 0, 0, 224, 224, 0, 64, 0, 0, 0, 64, 0, 32, 0, 64, 0, 64, 0, 64, 0, 96, 0, 64, 0, 128, 0, 64, 0, 160, 0, 64, 0, 192, 0, 64, 0, 224, 0, 64, 32, 0, 0, 64, 32, 32, 0, 64, 32, 64, 0, 64, 32, 96, 0, 64, 32, 128, 0, 64, 32, 160, 0, 64, 32, 192, 0, 64, 32, 224, 0, 64, 64, 0, 0, 64, 64, 32, 0, 64, 64, 64, 0, 64, 64, 96, 0, 64, 64, 128, 0, 64, 64, 160, 0, 64, 64, 192, 0, 64, 64, 224, 0, 64, 96, 0, 0, 64, 96, 32, 0, 64, 96, 64, 0, 64, 96, 96, 0, 64, 96, 128, 0, 64, 96, 160, 0, 64, 96, 192, 0, 64, 96, 224, 0, 64, 128, 0, 0, 64, 128, 32, 0, 64, 128, 64, 0, 64, 128, 96, 0, 64, 128, 128, 0, 64, 128, 160, 0, 64, 128, 192, 0, 64, 128, 224, 0, 64, 160, 0, 0, 64, 160, 32, 0, 64, 160, 64, 0, 64, 160, 96, 0, 64, 160, 128, 0, 64, 160, 160, 0, 64, 160, 192, 0, 64, 160, 224, 0, 64, 192, 0, 0, 64, 192, 32, 0, 64, 192, 64, 0, 64, 192, 96, 0, 64, 192, 128, 0, 64, 192, 160, 0, 64, 192, 192, 0, 64, 192, 224, 0, 64, 224, 0, 0, 64, 224, 32, 0, 64, 224, 64, 0, 64, 224, 96, 0, 64, 224, 128, 0, 64, 224, 160, 0, 64, 224, 192, 0, 64, 224, 224, 0, 128, 0, 0, 0, 128, 0, 32, 0, 128, 0, 64, 0, 128, 0, 96, 0, 128, 0, 128, 0, 128, 0, 160, 0, 128, 0, 192, 0, 128, 0, 224, 0, 128, 32, 0, 0, 128, 32, 32, 0, 128, 32, 64, 0, 128, 32, 96, 0, 128, 32, 128, 0, 128, 32, 160, 0, 128, 32, 192, 0, 128, 32, 224, 0, 128, 64, 0, 0, 128, 64, 32, 0, 128, 64, 64, 0, 128, 64, 96, 0, 128, 64, 128, 0, 128, 64, 160, 0, 128, 64, 192, 0, 128, 64, 224, 0, 128, 96, 0, 0, 128, 96, 32, 0, 128, 96, 64, 0, 128, 96, 96, 0, 128, 96, 128, 0, 128, 96, 160, 0, 128, 96, 192, 0, 128, 96, 224, 0, 128, 128, 0, 0, 128, 128, 32, 0, 128, 128, 64, 0, 128, 128, 96, 0, 128, 128, 128, 0, 128, 128, 160, 0, 128, 128, 192, 0, 128, 128, 224, 0, 128, 160, 0, 0, 128, 160, 32, 0, 128, 160, 64, 0, 128, 160, 96, 0, 128, 160, 128, 0, 128, 160, 160, 0, 128, 160, 192, 0, 128, 160, 224, 0, 128, 192, 0, 0, 128, 192, 32, 0, 128, 192, 64, 0, 128, 192, 96, 0, 128, 192, 128, 0, 128, 192, 160, 0, 128, 192, 192, 0, 128, 192, 224, 0, 128, 224, 0, 0, 128, 224, 32, 0, 128, 224, 64, 0, 128, 224, 96, 0, 128, 224, 128, 0, 128, 224, 160, 0, 128, 224, 192, 0, 128, 224, 224, 0, 192, 0, 0, 0, 192, 0, 32, 0, 192, 0, 64, 0, 192, 0, 96, 0, 192, 0, 128, 0, 192, 0, 160, 0, 192, 0, 192, 0, 192, 0, 224, 0, 192, 32, 0, 0, 192, 32, 32, 0, 192, 32, 64, 0, 192, 32, 96, 0, 192, 32, 128, 0, 192, 32, 160, 0, 192, 32, 192, 0, 192, 32, 224, 0, 192, 64, 0, 0, 192, 64, 32, 0, 192, 64, 64, 0, 192, 64, 96, 0, 192, 64, 128, 0, 192, 64, 160, 0, 192, 64, 192, 0, 192, 64, 224, 0, 192, 96, 0, 0, 192, 96, 32, 0, 192, 96, 64, 0, 192, 96, 96, 0, 192, 96, 128, 0, 192, 96, 160, 0, 192, 96, 192, 0, 192, 96, 224, 0, 192, 128, 0, 0, 192, 128, 32, 0, 192, 128, 64, 0, 192, 128, 96, 0, 192, 128, 128, 0, 192, 128, 160, 0, 192, 128, 192, 0, 192, 128, 224, 0, 192, 160, 0, 0, 192, 160, 32, 0, 192, 160, 64, 0, 192, 160, 96, 0, 192, 160, 128, 0, 192, 160, 160, 0, 192, 160, 192, 0, 192, 160, 224, 0, 192, 192, 0, 0, 192, 192, 32, 0, 192, 192, 64, 0, 192, 192, 96, 0, 192, 192, 128, 0, 192, 192, 160, 0, 240, 251, 255, 0, 164, 160, 160, 0, 128, 128, 128, 0, 0, 0, 255, 0, 0, 255, 0, 0, 0, 255, 255, 0, 255, 0, 0, 0, 255, 0, 255, 0, 255, 255, 0, 0, 255, 255, 255, 0];
            header.CopyTo(Content, 0);
            header256.CopyTo(Content, 54);
            for (int index = 1078; index < Size; index++) Content[index] = 255;
        }

        public int GetPixel(int x, int y)
        {
            if (!Enumerable.Range(0, Width).Contains(x) || !Enumerable.Range(0, Height).Contains(y))
                throw new Exception("Position out of range");

            int index = 1078 + (Height - 1 - y) * FullWidth + x;

            return Content[index];
        }

        public void SetPixel(int x, int y, byte color)
        {
            if (!Enumerable.Range(0, Width).Contains(x) || !Enumerable.Range(0, Height).Contains(y))
                throw new Exception("Position out of range");

            if (!Enumerable.Range(0, 256).Contains(color))
                throw new Exception("Color out of range");

            int index = 1078 + (Height - 1 - y) * FullWidth + x;
            Content[index] = color;
        }

        public int[] GetPosition(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Index starts at 1078
        /// </summary>
        public int GetIndex(int x, int y)
        {
            if (!Enumerable.Range(0, Width).Contains(x) || !Enumerable.Range(0, Height).Contains(y))
                throw new Exception("Position out of range");

            int index = 1078 + (Height - 1 - y) * FullWidth + x;

            return index;
        }

        /// <summary>
        /// Get area as top left reference
        /// </summary>
        public byte[] GetSection(int reference, int width, int height)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fill section with given color
        /// </summary>
        public void FillSection(int reference, int width, int height, byte color)
        {
            if (!Enumerable.Range(1078, Size).Contains(reference))
                throw new Exception("Reference out of range");

            if (!Enumerable.Range(0, Width).Contains(width) || !Enumerable.Range(0, Height).Contains(height))
                throw new Exception("Position out of range");

            if (!Enumerable.Range(0, 256).Contains(color))
                throw new Exception("Color out of range");

            int index;
            for (int indexH = 0; indexH < height; indexH++)
            {
                for (int indexW = 0; indexW < width; indexW++)
                {
                    index = reference - indexH * FullWidth + indexW;
                    Content[index] = color;
                }
            }
        }

        public XmlDocument ToSVG()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));

            XmlElement svg = document.CreateElement("svg");
            svg.SetAttribute("viewBox", "0 0 " + Width + " " + Height);
            svg.SetAttribute("version", "1.1");
            svg.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            svg.SetAttribute("xmlns:svg", "http://www.w3.org/2000/svg");
            svg.SetAttribute("style", "background: LightSkyBlue");
            document.AppendChild(svg);

            Dictionary<int, string> colors = new() { { 0, "black" }, { 7, "DimGrey" }, { 164, "Gray" }, { 82, "LightGray" } };

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int color = GetPixel(x, y);
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

            return document;
        }
    }
}
