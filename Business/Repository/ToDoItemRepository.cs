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
    public class ToDoItemRepository : IToDoItemRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ToDoItemRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ToDoItemDto> CreateToDoItem(ToDoItemDto toDoItemDto)
        {
            ToDoItem item = _mapper.Map<ToDoItemDto, ToDoItem>(toDoItemDto);
            item.CreationDate = DateTime.Now;
            item.LastModifiedDate = DateTime.Now;
            var addedItem = await _context.ToDoItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return _mapper.Map<ToDoItem, ToDoItemDto>(addedItem.Entity);
        }

        public async Task<int> DeleteToDoItem(int itemId)
        {
            try
            {
                var itemDetails = await _context.ToDoItems.FindAsync(itemId);
                if (itemDetails != null)
                {
                    var allAttachments =
                        await _context.ToDoAttachments.Where(x => x.ToDoItemId == itemId).ToListAsync();
                    var allResponses = await _context.ToDoResponses.Where(x => x.ToDoItemId == itemId).ToListAsync();
                    _context.ToDoAttachments.RemoveRange(allAttachments);
                    _context.ToDoResponses.RemoveRange(allResponses);
                    _context.ToDoItems.Remove(itemDetails);
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
                     _context.ToDoItems.Include(x => x.Attachments).Include(x => x.Responses));
                return itemDtos;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<ToDoItemDto> GetToDoItem(int itemId)
        {
            try
            {
                ToDoItemDto item = _mapper.Map<ToDoItem, ToDoItemDto>(await _context.ToDoItems
                    .Include(x => x.Attachments)
                    .Include(x => x.Responses)
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
