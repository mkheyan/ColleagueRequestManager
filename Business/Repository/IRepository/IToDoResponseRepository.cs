using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Business.Repository.IRepository
{
    public interface IToDoResponseRepository
    {
        public Task<ToDoResponseDto> CreateToDoResponse(int itemId, ToDoResponseDto toDoResponse);
        public Task<ToDoResponseDto> UpdateToDoResponse(int responseId, ToDoItemDto toDoItem);
        public Task<int> DeleteToDoResponse(int responseId);
        public Task<IEnumerable<ToDoResponseDto>> GetAllToDoResponsesForItem(int itemId);
        public Task<ToDoItemDto> GetToDoResponse(int responseId);
    }
}
