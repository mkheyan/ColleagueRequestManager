using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ToDoAttachment
    {
        public int Id { get; set; }
        public string AttachmentUrl { get; set; }
        public int ToDoItemId { get; set; }
        [ForeignKey("ToDoItemId")]
        public virtual ToDoItem ToDoItem { get; set; }
        public int? ResponseId { get; set; }
        [ForeignKey("ResponseId")]
        public virtual ToDoResponse ToDoResponse { get; set; }
        public string UploaderId { get; set; }
        [ForeignKey("UploaderId")]
        public virtual ApplicationUser Uploader { get; set; }

        public string OriginalName { get; set; }
            
    }
}
