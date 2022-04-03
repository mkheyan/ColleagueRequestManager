using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColleagueRequestManager.Models;
using DataAccess;

namespace Business.UserManager
{
    public interface IUserManager
    {
        public Task<ApplicationUserModel> GetApplicationUser(string id);
        public Task EditApplicationUser(string id, ApplicationUserModel user);
        public Task DeleteApplicationUser(string id);
    }
}
