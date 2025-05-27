using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;

namespace CV_2025.wwwroot.Image
{
    [SupportedOSPlatform("windows")]
    public class Image : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                string mapName = Path.GetFileNameWithoutExtension(file.FileName);

                MemoryMappedFile sourceMappedFile = MemoryMappedFile.CreateNew(mapName, file.Length);
                MemoryMappedViewStream sourceView = sourceMappedFile.CreateViewStream();
                await file.CopyToAsync(sourceView);

                Bitmap Image = new(sourceView);
                
                return Json(new { response = "OK", name = mapName, contentType = file.ContentType, width = Image.Width, height = Image.Height, length = file.Length });
            }
            catch (Exception ex)
            {
                return Json(new { response = ex.Message });
            }
        }

        public FileResult Display(string Name, string contentType, int length)
        {
            MemoryMappedFile sourceMappedFile = MemoryMappedFile.OpenExisting(Name, MemoryMappedFileRights.ReadWrite);
            MemoryMappedViewStream sourceView = sourceMappedFile.CreateViewStream();
            MemoryStream memoryStream = new();
            sourceView.CopyTo(memoryStream);

            byte[] buffer = new byte[length];
            byte[] source = memoryStream.ToArray();
            for (int index = 0; index < length; index++)
                buffer[index] = source[index];//Because file is mapped into memory in chunks of 4096 bytes => browser doesn't display SVG

            return File(buffer, contentType);
        }
    }
}
