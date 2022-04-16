using System;
using System.IO;
using System.Threading.Tasks;
using ColleagueRequestManager.Service.IService;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ColleagueRequestManager.Service
{
    public class FileUpload : IFileUpload
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public FileUpload(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
        public async Task<string> UploadFile(IBrowserFile file)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file.Name);
                var fileName = Guid.NewGuid().ToString() + fileInfo.Extension;
                var folderDirectory = $"{_webHostEnvironment.WebRootPath}\\Attachments";
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "Attachments", fileName);

                var memoryStream = new MemoryStream();
                await file.OpenReadStream(maxAllowedSize: 160000000).CopyToAsync(memoryStream);

                if (!Directory.Exists(folderDirectory))
                {
                    Directory.CreateDirectory(folderDirectory);
                }

                await using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fs);
                }

                var url =
                    $"{_configuration.GetValue<string>("ServerUrl")}";
                var fullPath = $"{url}Attachments/{fileName}";
                return fullPath;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        public bool DeleteFile(string fileName)
        {
            
            try
            {
                var path = $"{_webHostEnvironment.WebRootPath}\\Attachments\\{fileName}";
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
    }
}
