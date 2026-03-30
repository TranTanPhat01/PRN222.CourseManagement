using Microsoft.EntityFrameworkCore;
using CourseManagement.Models;
using CourseManagement.Repositories.Interfaces;
using CourseManagement.Repositories.Implementations;
using CourseManagement.Services.Interfaces;
using CourseManagement.Services.Implementations;

namespace PRN222.CourseManagement.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Register DbContext with SQL Server
            builder.Services.AddDbContext<CourseManagementContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("CourseManagementDB")));

            // 2. Register Unit of Work (Repository Layer)
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Register Services (Service Layer)
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();

            // 4. Add MVC Controllers with Views
            builder.Services.AddControllersWithViews();

            // 5. Configure Session with secure cookie settings
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // 6. Configure HSTS
            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Students}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
