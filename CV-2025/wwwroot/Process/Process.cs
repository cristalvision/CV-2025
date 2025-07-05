using CV_2025.CristalVision.Database;
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

            MemoryStream source = new();
            viewStream.CopyTo(source);
            //└──────────────────Read stream──────────────────┘

            //=====TEST=====
            /*const int WIDTH = 21;
            const int AREA = WIDTH * 50;
            string folderPath = "C:\\Users\\user\\source\\repos\\CV-2025\\CV-2025\\wwwroot\\CristalVision\\Characters\\" + WIDTH;
            List<string> allFiles = [.. Directory.GetFiles(folderPath)];

            List<byte[]> contents = [];
            for (int index = 0; index < allFiles.Count; index++) 
            {
                string fileName = allFiles[index];
                FileStream source = new(fileName, FileMode.Open);
                
                Bitmap256 bitmap256 = new(source);
                contents.Add(bitmap256.GetSection2(WIDTH, 50));
            }//Files to arrays

            byte[] reference = new byte[AREA];
            Array.Fill(reference, (byte)255);
            for (int index = 0; index < (AREA); index++)
            {
                List<byte> pixels = [.. contents.Select(content => content[index])];
                if (pixels.All(pixel => pixel == 0))
                    reference[index] = 0;
            }//Write reference pattern

            Bitmap256 pattern = new(WIDTH, 50);
            for (int index = 0; index < (AREA); index++)
            {
                int x = index % WIDTH;
                int y = (index - x) / WIDTH;
                pattern.SetPixel(x, y, reference[index]);
            }

            var database = new SQLServer("OCRDrawings");


            //database.tableName = "dbo.Table_1";
            //List<dynamic>? rows = database.Filter("Test1", "Test");


            File.WriteAllBytes("C:\\Users\\user\\source\\repos\\CV-2025\\CV-2025\\wwwroot\\CristalVision\\Characters\\Pattern.bmp", pattern.Content);

            string breakPoint = null;*/
            //=====TEST=====

            //┌─────────────Pixels to structures──────────────┐
            page = new(source, CVDatabase.Type.Acess, "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + Directory.GetCurrentDirectory() + "\\wwwroot\\CristalVision\\Database\\cvcharacters.accdb");
            //page = new(source, CVDatabase.Type.SQLServer, "Server=DESKTOP-IUG3E2G\\MSSQLSERVER01;Database=OCRDrawings;Trusted_connection=True;TrustServerCertificate=True;");
            
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
            await UpdateProgress("SVG", 75);
            page.Characters = new(page);//All characters have been removed
            SVG SVG = new(page);
            memoryStream = SVG.ToMemeoryStream(mapName);

            PDF PDF = new(page);
            PDF.Save();

            /*Word word = new Word(page);
            word.Save();*/
            //└─────────────────Bitmap to SVG─────────────────┘

            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[2] { "Start", "Image/Display?Name=" + mapName + " - SVG&contentType=image/svg+xml&length=" + memoryStream.Length });
            //byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[2] { "Start", "Image/Display?Name=" + mapName + " - SVG&contentType=application/pdf&length=" + length });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task UpdateProgress(string descriptiion, dynamic value)
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(new dynamic[3] { "Progress", descriptiion, value });
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
}
