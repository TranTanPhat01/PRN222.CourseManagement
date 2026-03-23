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

            // ==============================================================
            // DEPENDENCY INJECTION CONFIGURATION
            // ==============================================================

            // 1. Register DbContext with SQL Server
            builder.Services.AddDbContext<CourseManagementContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("CourseManagementDB")
                    // sqlOptions => sqlOptions.EnableRetryOnFailure(
                    //     maxRetryCount: 3,
                    //     maxRetryDelay: TimeSpan.FromSeconds(5),
                    //     errorNumbersToAdd: null
                    // )
                )
            );

            // 2. Register Unit of Work (Repository Layer)
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Register Services (Service Layer)
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();

            // 4. Add MVC Controllers with Views
            builder.Services.AddControllersWithViews();

            // 5. Add TempData support for flash messages
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // ==============================================================
            // HTTP REQUEST PIPELINE CONFIGURATION
            // ==============================================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
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
