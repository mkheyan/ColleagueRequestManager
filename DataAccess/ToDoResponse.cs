using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ToDoResponse
    {
        public int Id { get; set; }
        public string ResponseText { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string ResponderId { get; set; }
        [ForeignKey("ResponderId")]
        public virtual ApplicationUser Responder { get; set; }
        public int ToDoItemId { get; set; }
        [ForeignKey("ToDoItemId")]
        public virtual ToDoItem ToDoItem { get; set; }
        public virtual ICollection<ToDoAttachment> Attachments { get; set; }
    }
}
