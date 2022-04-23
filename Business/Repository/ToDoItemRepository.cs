using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Business.Repository.IRepository;
using Common;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Business.Repository
{
    public class ToDoItemRepository : IToDoItemRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToDoHistoryRepository _historyRepository;

        public ToDoItemRepository(ApplicationDbContext context, IMapper mapper, IToDoHistoryRepository historyRepository)
        {
            _context = context;
            _mapper = mapper;
            _historyRepository = historyRepository;
        }

        public async Task<ToDoItemDto> CreateToDoItem(ToDoItemDto toDoItemDto)
        {
            try
            {
                ToDoItem item = _mapper.Map<ToDoItemDto, ToDoItem>(toDoItemDto);
                item.CreationDate = DateTime.Now;
                item.LastModifiedDate = DateTime.Now;
                item.Assignee = null;
                item.Creator = null;
                var addedItem = await _context.ToDoItems.AddAsync(item);
                await _context.SaveChangesAsync();
                await _historyRepository.AddRecordToHistory(
                    new ToDoHistoryDto()
                    {
                        ActionDateTime = DateTime.Now,
                        ActionType = ActionType.Create_Item,
                        ToDoItemId = addedItem.Entity.Id,
                        UserId = addedItem.Entity.CreatorId

                    });
                await _context.SaveChangesAsync();
                return _mapper.Map<ToDoItem, ToDoItemDto>(addedItem.Entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public async Task<int> DeleteToDoItem(int itemId)
        {
            try
            {
                var itemDetails = await _context.ToDoItems.FindAsync(itemId);
                if (itemDetails != null)
                {
                    itemDetails.IsDeleted = true;
                    itemDetails.LastModifiedDate = DateTime.Now;
                    _context.ToDoItems.Update(itemDetails);
                    await _context.SaveChangesAsync();
                    await _historyRepository.AddRecordToHistory(
                        new ToDoHistoryDto()
                        {
                            ActionDateTime = DateTime.Now,
                            ActionType = ActionType.Delete_Item,
                            ToDoItemId = itemDetails.Id,
                            UserId = itemDetails.CreatorId

                        });
                    return await _context.SaveChangesAsync();
                }

                return 0;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<IEnumerable<ToDoItemDto>> GetAllToDoItems()
        {
            try
            {
                IEnumerable<ToDoItemDto> itemDtos = _mapper.Map<IEnumerable<ToDoItem>, IEnumerable<ToDoItemDto>>(
                     _context.ToDoItems.Include(x => x.Attachments)
                         .Include(x => x.Responses)
                         .Include(x=>x.Creator)
                         .Include(x=>x.Assignee)
                         .Where(x=>x.IsDeleted == false)
                         .AsNoTracking());
                return itemDtos;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<IEnumerable<ToDoItemDto>> GetAllToDoItems(string userId)
        {
            try
            {
                IEnumerable<ToDoItemDto> itemDtos = _mapper.Map<IEnumerable<ToDoItem>, IEnumerable<ToDoItemDto>>(
                    _context.ToDoItems.Include(x => x.Attachments)
                        .Include(x => x.Responses)
                        .Include(x => x.Creator)
                        .Include(x => x.Assignee)
                        .Where((x=>x.CreatorId == userId || x.AssigneeId == userId ))
                        .Where(x=>x.IsDeleted == false)
                        .AsNoTracking());
                return itemDtos;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<ToDoItemDto> GetToDoItem(int itemId)
        {
            try
            {
                ToDoItemDto item = _mapper.Map<ToDoItem, ToDoItemDto>(await _context.ToDoItems
                    .Include(x => x.Attachments)
                    .Include(x => x.Responses)
                    .Include(x => x.Creator)
                    .Include(x => x.Assignee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == itemId));
                return item;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<ToDoItemDto> UpdateToDoItem(int itemId, ToDoItemDto toDoItemDto)
        {
            try
            {
                if (itemId == toDoItemDto.Id)
                {
                    //valid
                    ToDoItem itemDetails = await _context.ToDoItems.FindAsync(itemId);
                    ToDoItem item = _mapper.Map<ToDoItemDto, ToDoItem>(toDoItemDto, itemDetails);
                    item.LastModifiedDate = DateTime.Now;
                    var updatedItem = _context.ToDoItems.Update(item);
                    await _context.SaveChangesAsync();
                    await _historyRepository.AddRecordToHistory(
                        new ToDoHistoryDto()
                        {
                            ActionDateTime = DateTime.Now,
                            ActionType = ActionType.Update_Item,
                            ToDoItemId = updatedItem.Entity.Id,
                            UserId = updatedItem.Entity.CreatorId

                        });
                    await _context.SaveChangesAsync();
                    return _mapper.Map<ToDoItem, ToDoItemDto>(updatedItem.Entity);
                }
                else
                {
                    //invalid
                    return null;
                }
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }
    }
}
