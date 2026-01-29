using CourseManagement.Models;
using CourseManagement.Services.Common;
using System.Collections.Generic;

namespace CourseManagement.Services.Interfaces
{
    /// <summary>
    /// Student service interface.
    /// Implements business rules: BR05, BR06, BR07, BR08, BR09, BR10
    /// </summary>
    public interface IStudentService
    {
        ServiceResult<IEnumerable<Student>> GetAllStudents();
        ServiceResult<Student> GetStudentById(int id);
        ServiceResult<Student> GetStudentByCode(string studentCode);
        ServiceResult AddStudent(Student student);
        ServiceResult UpdateStudent(Student student);
        ServiceResult DeleteStudent(int id);
    }
}
