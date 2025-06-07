using CV_2025.CristalVision.Vision;
using CV_2025.wwwroot.CristalVision.Output;
using System.IO.MemoryMappedFiles;
using System.Net.WebSockets;
using System.Runtime.Versioning;
using System.Text.Json;

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

            MemoryStream memoryStream = new();
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
            page.GetSections();

            //page.database.Close();
            //└─────────────Pixels to structures──────────────┘


            //┌─────────────────Bitmap to SVG─────────────────┐
            await UpdateProgress("SVG", 30);
            page.Characters = new(page.bitmap256);//All characters have been removed
            SVG SVG = new(page);
            memoryStream = SVG.ToMemeoryStream(mapName);

            PDF PDF = new PDF(page);
            PDF.Save();

            Word word = new Word(page);
            word.Save();
            //└─────────────────Bitmap to SVG─────────────────┘

            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[2] { "Start", "Image/Display?Name=" + mapName + " - SVG&contentType=image/svg+xml&length=" + memoryStream.Length });
            //byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[2] { "Start", "Image/Display?Name=" + mapName + " - SVG&contentType=application/pdf&length=" + length });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task UpdateProgress(string action, dynamic value)
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[3] { "Progress", action, value });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task DisplayUnwnownChar()
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[4] { "DisplayUnwnownChar", page.unknownChars[2].Top, page.unknownChars[0].Left, page.unknownChars[0].DBWidth });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task UpdateDatabase(bool style, char value, int DBWidth)
        {
            if (value == '␀') return;
            
            string tableName = DBWidth + "x" + 50;//"88"
            if (!page.Characters.database.tableNames.Contains(tableName))
            {
                string query = "CREATE TABLE " + tableName + " (ID AUTOINCREMENT PRIMARY KEY, `Black Pixels` Number, Charcater CHAR(1), Family CHAR(15), Style CHAR(6), CharSection LONGBINARY);";
                //string MySQL = "CREATE TABLE " + tableName + " (ID int NOT NULL AUTO_INCREMENT, `Black Pixels` int NOT NULL, Value char(1) NOT NULL, Family varchar(15) NOT NULL, Style varchar(6), Section blob NOT NULL, PRIMARY KEY (ID))";
                page.Characters.database.ExecuteNonQuery(query);
            }

            byte[] section = page.unknownChars[0].section;
            page.Characters.database.tableName = tableName;
            dynamic fontStyle = (style) ? "Italic" : DBNull.Value;
            int blackPixels = Monochrome.CountBlackPixels(section);//1800
            page.Characters.database.Insert(["Black Pixels", "Charcater", "Family", "Style", "CharSection"], [Monochrome.CountBlackPixels(section), value, "Times New Roman", fontStyle, section]);
        }
    }
}
