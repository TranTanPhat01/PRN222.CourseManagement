using CourseManagement.Models;
using CourseManagement.Repositories.Interfaces;
using CourseManagement.Services.Common;
using CourseManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseManagement.Services.Implementations
{
    /// <summary>
    /// Department service implementation.
    /// Implements business rules:
    /// BR01: Department name must be unique
    /// BR02: Department name not empty and length >= 3
    /// BR03: Cannot delete department if it has students
    /// BR04: Cannot delete department if it has courses
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<IEnumerable<Department>> GetAllDepartments()
        {
            try
            {
                var departments = _unitOfWork.Departments.GetAll();
                return ServiceResult<IEnumerable<Department>>.Success(departments);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Department>>.Failure($"Error retrieving departments: {ex.Message}");
            }
        }

        public ServiceResult<Department> GetDepartmentById(int id)
        {
            try
            {
                var department = _unitOfWork.Departments.GetById(id);
                if (department == null)
                {
                    return ServiceResult<Department>.Failure($"Department with ID {id} not found");
                }
                return ServiceResult<Department>.Success(department);
            }
            catch (Exception ex)
            {
                return ServiceResult<Department>.Failure($"Error retrieving department: {ex.Message}");
            }
        }

        public ServiceResult AddDepartment(Department department)
        {
            try
            {
                // BR02: Name not empty and length >= 3
                if (string.IsNullOrWhiteSpace(department.Name))
                {
                    return ServiceResult.Failure("Department name cannot be empty");
                }

                if (department.Name.Trim().Length < 3)
                {
                    return ServiceResult.Failure("Department name must be at least 3 characters long");
                }

                // BR01: Unique name
                var existingDepartment = _unitOfWork.Departments
                    .Find(d => d.Name.ToLower() == department.Name.ToLower().Trim())
                    .FirstOrDefault();

                if (existingDepartment != null)
                {
                    return ServiceResult.Failure($"Department with name '{department.Name}' already exists");
                }

                department.Name = department.Name.Trim();
                _unitOfWork.Departments.Add(department);
                _unitOfWork.Save();

                return ServiceResult.Success($"Department '{department.Name}' added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error adding department: {ex.Message}");
            }
        }

        public ServiceResult UpdateDepartment(Department department)
        {
            try
            {
                var existing = _unitOfWork.Departments.GetById(department.DepartmentId);
                if (existing == null)
                {
                    return ServiceResult.Failure($"Department with ID {department.DepartmentId} not found");
                }

                // BR02: Name not empty and length >= 3
                if (string.IsNullOrWhiteSpace(department.Name))
                {
                    return ServiceResult.Failure("Department name cannot be empty");
                }

                if (department.Name.Trim().Length < 3)
                {
                    return ServiceResult.Failure("Department name must be at least 3 characters long");
                }

                // BR01: Unique name (excluding current department)
                var duplicateName = _unitOfWork.Departments
                    .Find(d => d.Name.ToLower() == department.Name.ToLower().Trim() 
                               && d.DepartmentId != department.DepartmentId)
                    .FirstOrDefault();

                if (duplicateName != null)
                {
                    return ServiceResult.Failure($"Department with name '{department.Name}' already exists");
                }

                existing.Name = department.Name.Trim();
                existing.Description = department.Description;

                _unitOfWork.Departments.Update(existing);
                _unitOfWork.Save();

                return ServiceResult.Success($"Department '{existing.Name}' updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error updating department: {ex.Message}");
            }
        }

        public ServiceResult DeleteDepartment(int id)
        {
            try
            {
                var department = _unitOfWork.Departments.GetById(id);
                if (department == null)
                {
                    return ServiceResult.Failure($"Department with ID {id} not found");
                }

                // BR03: Cannot delete if has students
                var hasStudents = _unitOfWork.Students
                    .Find(s => s.DepartmentId == id)
                    .Any();

                if (hasStudents)
                {
                    return ServiceResult.Failure("Cannot delete department: it has students enrolled");
                }

                // BR04: Cannot delete if has courses
                var hasCourses = _unitOfWork.Courses
                    .Find(c => c.DepartmentId == id)
                    .Any();

                if (hasCourses)
                {
                    return ServiceResult.Failure("Cannot delete department: it has courses assigned");
                }

                _unitOfWork.Departments.Delete(id);
                _unitOfWork.Save();

                return ServiceResult.Success($"Department deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error deleting department: {ex.Message}");
            }
        }
    }
}
