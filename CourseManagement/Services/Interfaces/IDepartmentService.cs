using CourseManagement.Models;
using CourseManagement.Services.Common;
using System.Collections.Generic;

namespace CourseManagement.Services.Interfaces
{
    /// <summary>
    /// Department service interface.
    /// Implements business rules: BR01, BR02, BR03, BR04
    /// </summary>
    public interface IDepartmentService
    {
        ServiceResult<IEnumerable<Department>> GetAllDepartments();
        ServiceResult<Department> GetDepartmentById(int id);
        ServiceResult AddDepartment(Department department);
        ServiceResult UpdateDepartment(Department department);
        ServiceResult DeleteDepartment(int id);
    }
}
