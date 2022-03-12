using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    public enum ActionType
    {
        CreateItem = 0,
        UpdateItem = 1,
        DeleteItem = 2,
        Respond = 3,
        MarkComplete = 4
    }
}
