using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ToDoItem
    {
        public int Id { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Description { get; set; }
        public string CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public virtual ApplicationUser Creator { get; set; }
        public string AssigneeId { get; set; }
        [ForeignKey("AssigneeId")]
        public virtual ApplicationUser Assignee { get; set; }
        
        public virtual ICollection<ToDoAttachment> Attachments { get; set; }
        public virtual ICollection<ToDoResponse> Responses { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }
        public DateTime NecessaryCompletionDate { get; set; }

        public bool IsComplete { get; set; } = false;
        public bool IsDeleted { get; set; } = false;


    }
}
