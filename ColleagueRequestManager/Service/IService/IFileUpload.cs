using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Models;

namespace ColleagueRequestManager.Service.IService
{
    public interface IFileUpload
    {
        Task<FileModel> UploadFile(IBrowserFile file);
        bool DeleteFile(string fileName);
    }
}
