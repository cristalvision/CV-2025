using CV_2025.CristalVision.Vision;
using System.IO.MemoryMappedFiles;
using System.Net.WebSockets;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Xml;

namespace CV_2025.wwwroot.Process
{
    [SupportedOSPlatform("windows")]
    public class Process
    {
        public WebSocket? webSocket;
        public WebSocketReceiveResult? result;
        public Page? page;


        MemoryMappedFile? mappedFile;
        MemoryMappedViewStream? viewStream;
        MemoryStream? memoryStream;

        public async Task Start(string mapName)
        {
            //┌──────────────────Read stream──────────────────┐
            await UpdateProgress("Monochrome", 10);
            mappedFile = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.ReadWrite);
            viewStream = mappedFile.CreateViewStream();

            memoryStream = new();
            viewStream.CopyTo(memoryStream);
            //└──────────────────Read stream──────────────────┘


            //┌─────────────Pixels to structures──────────────┐
            page = new(memoryStream);

            await UpdateProgress("Get Text", 30);
            page.GetCharacters();
            page.GetShapes();
            page.GetEquations();
            page.GetTables();

            page.GetWords();
            page.GetRows();
            page.GetParagraphs();
            //└─────────────Pixels to structures──────────────┘


            //┌─────────────────Bitmap to SVG─────────────────┐
            await UpdateProgress("SVG", 30);
            page.Characters = new(page.bitmap256);//All characters have been removed
            XmlDocument document = page.ToSVG();

            for (int y = 0; y < page.bitmap256.Height; y++)
            {
                for (int x = 0; x < page.bitmap256.Width; x++)
                {
                    int color = page.bitmap256.GetPixel(x, y);
                    if (color == 255) continue;

                    XmlElement rect = document.CreateElement("rect");
                    rect.SetAttribute("width", "1");
                    rect.SetAttribute("height", "1");
                    rect.SetAttribute("x", x.ToString());
                    rect.SetAttribute("y", y.ToString());
                    rect.SetAttribute("fill", "black");
                    document.ChildNodes[1].AppendChild(rect);
                }
            }
            //└─────────────────Bitmap to SVG─────────────────┘
            //character1.Left = 421
            //character1.Top = 38
            //┌─────────Color first unknown character─────────┐
            Character character1 = page.characters[0];
            XmlElement rectangle = document.CreateElement("rect");
            rectangle.SetAttribute("x", character1.Left.ToString());
            rectangle.SetAttribute("y", character1.Top.ToString());
            rectangle.SetAttribute("width", character1.Width.ToString());
            rectangle.SetAttribute("height", character1.Height.ToString());
            rectangle.SetAttribute("fill", "transparent");
            rectangle.SetAttribute("stroke", "darkblue");
            rectangle.InnerText = character1.value.ToString();
            document.ChildNodes[1].AppendChild(rectangle);
            //└─────────Color first unknown character─────────┘


            //┌─────────────────SVG to Stream─────────────────┐
            memoryStream = new();
            document.Save(memoryStream);

            int length = (int)memoryStream.Length;
            mappedFile = MemoryMappedFile.CreateNew(mapName + " - SVG", length);
            viewStream = mappedFile.CreateViewStream();
            await viewStream.WriteAsync(memoryStream.ToArray().AsMemory(0, length));
            //└─────────────────SVG to Stream─────────────────┘

            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[2] { "Start", "Image/Display?Name=" + mapName + " - SVG&contentType=image/svg+xml&length=" + length });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task UpdateProgress(string action, dynamic value)
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[3] { "Progress", action, value });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task DisplayUnwnownChar()
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[4] { "DisplayUnwnownChar", page.characters[2].Top, page.characters[2].Left, page.characters[2].Width });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task UpdateDatabase(char value, int width)
        {
            if (value == '␀') return;

            string tableName = width + "x" + 50;
            if (!page.Characters.database.tableNames.Contains(tableName))
            {
                string query = "CREATE TABLE " + tableName + " (ID AUTOINCREMENT PRIMARY KEY, `Black Pixels` Number, Charcater CHAR, Family CHAR, Style CHAR, CharSection VARBINARY);";
                //string MySQL = "CREATE TABLE " + tableName + " (ID int NOT NULL AUTO_INCREMENT, `Black Pixels` int NOT NULL, Value char(1) NOT NULL, Family varchar(15) NOT NULL, Style varchar(6), Section blob NOT NULL, PRIMARY KEY (ID))";
                page.Characters.database.ExecuteNonQuery(query);
            }

            byte[] section = page.characters[0].section;
            page.Characters.database.tableName = tableName;
            page.Characters.database.Insert(["Black Pixels", "Charcater", "Family", "Style", "CharSection"], new List<dynamic>() { Monochrome.CountBlackPixels(section), value, "Times New Roman", DBNull.Value, section });
        }
    }
}
