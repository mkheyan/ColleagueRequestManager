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
    public class ToDoResponseRepository : IToDoResponseRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ToDoResponseRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ToDoResponseDto> CreateToDoResponse(int itemId, ToDoResponseDto toDoResponseDto)
        {
            try
            {
                ToDoResponse response = _mapper.Map<ToDoResponseDto, ToDoResponse>(toDoResponseDto);
                response.CreationDate = DateTime.Now;
                response.LastModifiedDate = DateTime.Now;
                response.ToDoItemId = itemId;
                response.Responder = null;
                var addedResponse = await _context.ToDoResponses.AddAsync(response);
                await _context.SaveChangesAsync();
                return _mapper.Map<ToDoResponse, ToDoResponseDto>(addedResponse.Entity);
            }
            catch (Exception e)
            {
                throw new Exception();
            }
            
        }

        public async Task<int> DeleteToDoResponse(int responseId)
        {
            try
            {
                var responseDetails = await _context.ToDoResponses.FindAsync(responseId);
                if (responseDetails != null)
                {
                    var allAttachments =
                        await _context.ToDoAttachments.Where(x => x.ResponseId == responseId).ToListAsync();
                    _context.ToDoAttachments.RemoveRange(allAttachments);
                    _context.ToDoResponses.Remove(responseDetails);
                    return await _context.SaveChangesAsync();
                }

                return 0;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<IEnumerable<ToDoResponseDto>> GetAllToDoResponsesForItem(int itemId)
        {
            try
            {
                IEnumerable<ToDoResponseDto> responseDtos = _mapper.Map<IEnumerable<ToDoResponse>, IEnumerable<ToDoResponseDto>>(
                    _context.ToDoResponses.Include(x => x.Attachments).Where(x => x.ToDoItemId == itemId));
                return responseDtos;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<ToDoResponseDto> GetToDoResponse(int responseId)
        {
            try
            {
                ToDoResponseDto response = _mapper.Map<ToDoResponse, ToDoResponseDto>(await _context.ToDoResponses
                    .Include(x => x.Attachments)
                    .FirstOrDefaultAsync(x => x.Id == responseId));
                return response;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<ToDoResponseDto> UpdateToDoResponse(int responseId, ToDoResponseDto toDoResponseDto)
        {
            try
            {
                if (responseId == toDoResponseDto.Id)
                {
                    //valid
                    ToDoResponse responseDetails = await _context.ToDoResponses.FindAsync(responseId);
                    ToDoResponse response = _mapper.Map<ToDoResponseDto, ToDoResponse>(toDoResponseDto, responseDetails);
                    response.LastModifiedDate = DateTime.Now;
                    var updatedResponse = _context.ToDoResponses.Update(response);
                    return _mapper.Map<ToDoResponse, ToDoResponseDto>(updatedResponse.Entity);
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
