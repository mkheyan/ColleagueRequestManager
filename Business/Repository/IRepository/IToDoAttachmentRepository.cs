using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Business.Repository.IRepository
{
    public interface IToDoAttachmentRepository
    {
        public Task<int> AddToDoAttachment(ToDoAttachmentDto attachment);
        public Task<int> DeleteToDoAttachmentByAttachmentId(int attachmentId);
        public Task<int> DeleteToDoAttachmentByItemId(int itemId);
        public Task<int> DeleteToDoAttachmentByResponseId(int responseId);
        public Task<int> DeleteToDoAttachmentByByUrl(string url);
        public Task<IEnumerable<ToDoAttachmentDto>> GetToDoAttachmentsByItemId(int itemId);
        public Task<IEnumerable<ToDoAttachmentDto>> GetToDoAttachmentsByResponseId(int responseId);

    }
}
