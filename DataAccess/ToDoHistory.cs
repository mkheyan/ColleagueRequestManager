using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Identity;

namespace DataAccess
{
    public class ToDoHistory
    {
        public int Id { get; set; }
        public int ToDoItemId { get; set; }
        [ForeignKey("ToDoItemId")]
        public virtual ToDoItem ToDoItem { get; set; }
        [Required]
        public ActionType ActionType { get; set; }
        [Required]
        public DateTime ActionDateTime { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    
}
