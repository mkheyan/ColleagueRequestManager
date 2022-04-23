using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColleagueRequestManager.Models;

namespace Models
{
    public class ToDoResponseDto
    {
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter a response")]
        public string ResponseText { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        [Required]
        public string ResponderId { get; set; }
        public virtual ApplicationUserModel Responder { get; set; }
        [Required]
        public int ToDoItemId { get; set; }
        public virtual ToDoItemDto ToDoItem { get; set; }
        public virtual ICollection<ToDoAttachmentDto> Attachments { get; set; }
        public virtual List<string> FileUrls { get; set; }
    }
}
