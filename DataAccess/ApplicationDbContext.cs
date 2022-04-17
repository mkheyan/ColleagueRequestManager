using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ToDoItem> ToDoItems { get; set; }
        public DbSet<ToDoResponse> ToDoResponses { get; set; }
        public DbSet<ToDoAttachment> ToDoAttachments { get; set; }
        public DbSet<ToDoHistory> ToDoHistory { get; set; }
    }
}
