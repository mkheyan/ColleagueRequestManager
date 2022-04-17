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
    public class ToDoItemDto
    {
        public int Id { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Description { get; set; }
        public string CreatorId { get; set; }
        public virtual ApplicationUserModel Creator { get; set; } = new ApplicationUserModel();
        public string AssigneeId { get; set; }
        public virtual ApplicationUserModel Assignee { get; set; }

        public virtual ICollection<ToDoAttachmentDto> Attachments { get; set; }
        public virtual List<string> FileUrls { get; set; }
        public virtual ICollection<ToDoResponseDto> Responses { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }
        public DateTime NecessaryCompletionDate { get; set; }

        public bool IsComplete { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
