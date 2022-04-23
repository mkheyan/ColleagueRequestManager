using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Business.Repository.IRepository
{
    public interface IToDoItemRepository
    {
        public Task<ToDoItemDto> CreateToDoItem (ToDoItemDto toDoItemDto);
        public Task<ToDoItemDto> UpdateToDoItem (int itemId, ToDoItemDto toDoItemDto);
        public Task<int> DeleteToDoItem (int itemId);
        public Task<IEnumerable<ToDoItemDto>> GetAllToDoItems ();
        public Task<IEnumerable<ToDoItemDto>> GetAllToDoItems (string userId);
        public Task<ToDoItemDto> GetToDoItem (int itemId);
    }
}
