using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColleagueRequestManager.Models;
using Common;

namespace Models
{
    public class ToDoHistoryDto
    {
        public int Id { get; set; }
        public int ToDoItemId { get; set; }
        public virtual ToDoItemDto ToDoItem { get; set; }
        [Required]
        public ActionType ActionType { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUserModel User { get; set; }
    }
}
