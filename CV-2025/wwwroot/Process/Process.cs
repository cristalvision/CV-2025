using CV_2025.CristalVision.Vision;
using System.IO.MemoryMappedFiles;
using System.Net.WebSockets;
using System.Text.Json;
using System.Xml;

namespace CV_2025.wwwroot.Process
{
    public class Process
    {
        public WebSocket? webSocket;
        public WebSocketReceiveResult? result;
        public List<Character>? characters;
        public Characters? Characters;
        public Bitmap256 bitmap256;

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
            Page page = new(memoryStream);

            await UpdateProgress("Get Text", 30);
            page.GetCharacters();
            page.GetShapes();
            page.GetEquations();
            page.GetTables();

            page.GetWords();
            page.GetRows();
            page.GetParagraphs();

            await UpdateProgress("SVG", 30);
            XmlDocument document = page.ToSVG();
            //└─────────────Pixels to structures──────────────┘


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

        /*public async Task DisplayUnwnownChar()
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[3] { "DisplayUnwnownChar", characters[0].Top, characters[0].Left });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }*/

        /*public async Task UpdateDatabase(char value)
        {
            if (value == '␀') return;

            string tableName = characters[0].Width + "x" + characters[0].Height;
            if (!Characters.database.tableNames.Contains(tableName))
            {
                string query = "CREATE TABLE " + tableName + " (ID int NOT NULL AUTO_INCREMENT, `Black Pixels` int NOT NULL, Value char(1) NOT NULL, Family varchar(15) NOT NULL, Style varchar(6), Section blob NOT NULL, PRIMARY KEY (ID))";
                Characters.database.ExecuteNonQuery(query);
            }

            byte[] section = characters[0].Section;
            Characters.database.tableName = tableName;
            Characters.database.Insert(new List<string>() { "Black Pixels", "Value", "Family", "Style", "Section" }, new List<dynamic>() { Monochrome.CountBlackPixels(section), value, "Times New Roman", DBNull.Value, section });
        }*/
    }
}
