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
    public class ToDoAttachmentRepository : IToDoAttachmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ToDoAttachmentRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<int> AddToDoAttachment(ToDoAttachmentDto attachmentDto)
        {
            try
            {
                var attachment = _mapper.Map<ToDoAttachmentDto, ToDoAttachment>(attachmentDto);
                await _context.ToDoAttachments.AddAsync(attachment);
                return await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception();
            }
            
        }

        public async Task<int> DeleteToDoAttachmentByAttachmentId(int attachmentId)
        {
            try
            {
                var attachment = await _context.ToDoAttachments.FindAsync(attachmentId);
                _context.ToDoAttachments.Remove(attachment);
                return await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception();
            }
            
        }

        public async Task<int> DeleteToDoAttachmentByUrl(string url)
        {
            try
            {
                var attachment =
                    await _context.ToDoAttachments.FirstOrDefaultAsync(
                        x => x.AttachmentUrl.Equals(url, StringComparison.CurrentCultureIgnoreCase));
                if (attachment == null)
                {
                    return 0;
                }
                _context.ToDoAttachments.Remove(attachment);
                return await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<int> DeleteToDoAttachmentByItemId(int itemId)
        {
            try
            {
                var attachmentList = await _context.ToDoAttachments.Where(x => x.ToDoItemId == itemId).ToListAsync();
                _context.ToDoAttachments.RemoveRange(attachmentList);
                return await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<int> DeleteToDoAttachmentByResponseId(int responseId)
        {
            try
            {
                var attachmentList = await _context.ToDoAttachments.Where(x => x.ToDoItemId == responseId).ToListAsync();
                _context.ToDoAttachments.RemoveRange(attachmentList);
                return await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<IEnumerable<ToDoAttachmentDto>> GetToDoAttachmentsByItemId(int itemId)
        {
            return _mapper.Map<IEnumerable<ToDoAttachment>, IEnumerable<ToDoAttachmentDto>>(
                await _context.ToDoAttachments.Where(x => x.ToDoItemId == itemId).ToListAsync());
        }

        public async Task<IEnumerable<ToDoAttachmentDto>> GetToDoAttachmentsByResponseId(int responseId)
        {
            return _mapper.Map<IEnumerable<ToDoAttachment>, IEnumerable<ToDoAttachmentDto>>(
                await _context.ToDoAttachments.Where(x => x.ResponseId == responseId).ToListAsync());
        }
    }
}
