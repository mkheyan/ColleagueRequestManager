using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColleagueRequestManager.Models;

namespace Models
{
    public class ToDoAttachmentDto
    {
        public int Id { get; set; }
        public string AttachmentUrl { get; set; }
        public int ToDoItemId { get; set; }
        public virtual ToDoItemDto ToDoItem { get; set; }
        public int? ResponseId { get; set; }
        public virtual ToDoResponseDto ToDoResponse { get; set; }
        public string UploaderId { get; set; }
        public virtual ApplicationUserModel Uploader { get; set; }
    }
}
