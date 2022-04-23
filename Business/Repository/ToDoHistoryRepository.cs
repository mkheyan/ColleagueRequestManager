using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Business.Repository.IRepository;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Business.Repository
{
    public class ToDoHistoryRepository : IToDoHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ToDoHistoryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ToDoHistoryDto> AddRecordToHistory(ToDoHistoryDto toDoHistoryDto)
        {
            try
            {
                ToDoHistory historyRecord = _mapper.Map<ToDoHistoryDto, ToDoHistory>(toDoHistoryDto);
                historyRecord.ActionDateTime = DateTime.Now;
                historyRecord.User = null;
                var addedRecord = await _context.ToDoHistory.AddAsync(historyRecord);
                await _context.SaveChangesAsync();
                return _mapper.Map<ToDoHistory, ToDoHistoryDto>(addedRecord.Entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IEnumerable<ToDoHistoryDto>> GetAllToDoHistoryForItem(int itemId)
        {
            try
            {
                IEnumerable<ToDoHistoryDto> historyDtos =
                    _mapper.Map<IEnumerable<ToDoHistory>, IEnumerable<ToDoHistoryDto>>(
                        _context.ToDoHistory.Include(x => x.User)
                            .Where(x => x.ToDoItemId == itemId)
                            .AsNoTracking());
                return historyDtos;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
