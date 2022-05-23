using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Repository;
using Business.Repository.IRepository;
using Business.UserManager;
using ColleagueRequestManager.Service;
using DataAccess;
using ColleagueRequestManager.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace ColleagueRequestManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()
                .AddDefaultUI();
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IFileUpload,FileUpload>();
            services.AddScoped<IToDoItemRepository, ToDoItemRepository>();
            services.AddScoped<IToDoAttachmentRepository, ToDoAttachmentRepository>();
            services.AddScoped<IToDoResponseRepository, ToDoResponseRepository>();
            services.AddScoped<IToDoHistoryRepository, ToDoHistoryRepository>();
            services.AddScoped<ContextMenuService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddSignalR().AddAzureSignalR(options =>
            {
                options.ServerStickyMode =
                    Microsoft.Azure.SignalR.ServerStickyMode.Required;
            });
            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    if (!context.Context.User.Identity.IsAuthenticated && context.Context.Request.Path.StartsWithSegments("/Attachments"))
                    {
                        throw new Exception("Not authenticated");
                    }
                }
            });
            app.UseStaticFiles();
            dbInitializer.Initialize();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
