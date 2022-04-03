using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Business.Repository.IRepository
{
    public interface IToDoHistoryRepository
    {
        public Task<IEnumerable<ToDoHistoryDto>> GetAllToDoHistoryForItem(int itemId);
        public Task<ToDoHistoryDto> GetToDoHistoryEntryById(int id);
    }
}
