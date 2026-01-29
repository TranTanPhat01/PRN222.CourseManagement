using CourseManagement.Models;
using CourseManagement.Services.Common;
using System.Collections.Generic;

namespace CourseManagement.Services.Interfaces
{
    /// <summary>
    /// Course service interface.
    /// Implements business rules: BR11, BR12, BR13, BR14, BR15
    /// </summary>
    public interface ICourseService
    {
        ServiceResult<IEnumerable<Course>> GetAllCourses();
        ServiceResult<Course> GetCourseById(int id);
        ServiceResult<Course> GetCourseByCode(string courseCode);
        ServiceResult AddCourse(Course course);
        ServiceResult UpdateCourse(Course course);
        ServiceResult DeleteCourse(int id);
    }
}
