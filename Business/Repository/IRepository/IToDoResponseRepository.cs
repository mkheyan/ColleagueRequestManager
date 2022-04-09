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
        public Task<ToDoResponseDto> CreateToDoResponse(int itemId, ToDoResponseDto toDoResponseDto);
        public Task<ToDoResponseDto> UpdateToDoResponse(int responseId, ToDoResponseDto toDoResponseDto);
        public Task<int> DeleteToDoResponse(int responseId);
        public Task<IEnumerable<ToDoResponseDto>> GetAllToDoResponsesForItem(int itemId);
        public Task<ToDoResponseDto> GetToDoResponse(int responseId);
    }
}
