using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN222.CourseManagement.Repository.Tests
{
    public class TestHelper
    {
        public static CourseManagementContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<CourseManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CourseManagementContext(options);
        }
    }
}
