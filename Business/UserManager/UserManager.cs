using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ColleagueRequestManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Business.UserManager
{
    public class UserManager : IUserManager
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public UserManager(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApplicationUserModel> GetApplicationUser (string id)
        {
            var userEntity = await _context.ApplicationUser.FindAsync(id);
            return _mapper.Map<ApplicationUser, ApplicationUserModel>(userEntity);
        }
     
        public async Task EditApplicationUser(string id, ApplicationUserModel user)
        {
            var userDetails = await _context.ApplicationUser.FindAsync(id);
            var userEntity = _mapper.Map<ApplicationUserModel, ApplicationUser>(user, userDetails);
            _context.ApplicationUser.Update(userEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteApplicationUser(string id)
        {
            var user = await _context.ApplicationUser.Where(x => x.Id == id).FirstOrDefaultAsync();
            _context.ApplicationUser.Remove(user);
            await _context.SaveChangesAsync();
        }

    }
}
