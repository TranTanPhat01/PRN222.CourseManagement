using CourseManagement.Models;
using CourseManagement.Repositories.Interfaces;
using CourseManagement.Services.Common;
using CourseManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseManagement.Services.Implementations
{
    /// <summary>
    /// Student service implementation.
    /// Implements business rules:
    /// BR05: StudentCode must be unique
    /// BR06: Must belong to exactly one department (department must exist)
    /// BR07: Full name not null/empty
    /// BR08: Full name length >= 3
    /// BR09: Email if provided must be unique
    /// BR10: Cannot delete if has enrollments
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<IEnumerable<Student>> GetAllStudents()
        {
            try
            {
                var students = _unitOfWork.Students.GetAll();
                return ServiceResult<IEnumerable<Student>>.Success(students);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Student>>.Failure($"Error retrieving students: {ex.Message}");
            }
        }

        public ServiceResult<Student> GetStudentById(int id)
        {
            try
            {
                var student = _unitOfWork.Students.GetById(id);
                if (student == null)
                {
                    return ServiceResult<Student>.Failure($"Student with ID {id} not found");
                }
                return ServiceResult<Student>.Success(student);
            }
            catch (Exception ex)
            {
                return ServiceResult<Student>.Failure($"Error retrieving student: {ex.Message}");
            }
        }

        public ServiceResult<Student> GetStudentByCode(string studentCode)
        {
            try
            {
                var student = _unitOfWork.Students
                    .Find(s => s.StudentCode == studentCode)
                    .FirstOrDefault();

                if (student == null)
                {
                    return ServiceResult<Student>.Failure($"Student with code '{studentCode}' not found");
                }
                return ServiceResult<Student>.Success(student);
            }
            catch (Exception ex)
            {
                return ServiceResult<Student>.Failure($"Error retrieving student: {ex.Message}");
            }
        }

        public ServiceResult AddStudent(Student student)
        {
            try
            {
                // BR07: Full name not null/empty
                if (string.IsNullOrWhiteSpace(student.FullName))
                {
                    return ServiceResult.Failure("Student full name cannot be empty");
                }

                // BR08: Full name length >= 3
                if (student.FullName.Trim().Length < 3)
                {
                    return ServiceResult.Failure("Student full name must be at least 3 characters long");
                }

                // BR05: StudentCode unique
                var existingStudent = _unitOfWork.Students
                    .Find(s => s.StudentCode == student.StudentCode)
                    .FirstOrDefault();

                if (existingStudent != null)
                {
                    return ServiceResult.Failure($"Student with code '{student.StudentCode}' already exists");
                }

                // BR09: Email if provided must be unique
                if (!string.IsNullOrWhiteSpace(student.Email))
                {
                    var duplicateEmail = _unitOfWork.Students
                        .Find(s => s.Email.ToLower() == student.Email.ToLower().Trim())
                        .FirstOrDefault();

                    if (duplicateEmail != null)
                    {
                        return ServiceResult.Failure($"Email '{student.Email}' is already in use");
                    }
                }

                // BR06: Must belong to exactly one department (department must exist)
                var department = _unitOfWork.Departments.GetById(student.DepartmentId);
                if (department == null)
                {
                    return ServiceResult.Failure($"Department with ID {student.DepartmentId} does not exist");
                }

                student.FullName = student.FullName.Trim();
                if (!string.IsNullOrWhiteSpace(student.Email))
                {
                    student.Email = student.Email.Trim();
                }

                _unitOfWork.Students.Add(student);
                _unitOfWork.Save();

                return ServiceResult.Success($"Student '{student.FullName}' added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error adding student: {ex.Message}");
            }
        }

        public ServiceResult UpdateStudent(Student student)
        {
            try
            {
                var existing = _unitOfWork.Students.GetById(student.StudentId);
                if (existing == null)
                {
                    return ServiceResult.Failure($"Student with ID {student.StudentId} not found");
                }

                // BR07: Full name not null/empty
                if (string.IsNullOrWhiteSpace(student.FullName))
                {
                    return ServiceResult.Failure("Student full name cannot be empty");
                }

                // BR08: Full name length >= 3
                if (student.FullName.Trim().Length < 3)
                {
                    return ServiceResult.Failure("Student full name must be at least 3 characters long");
                }

                // BR05: StudentCode unique (excluding current student)
                var duplicateCode = _unitOfWork.Students
                    .Find(s => s.StudentCode == student.StudentCode && s.StudentId != student.StudentId)
                    .FirstOrDefault();

                if (duplicateCode != null)
                {
                    return ServiceResult.Failure($"Student with code '{student.StudentCode}' already exists");
                }

                // BR09: Email if provided must be unique (excluding current student)
                if (!string.IsNullOrWhiteSpace(student.Email))
                {
                    var duplicateEmail = _unitOfWork.Students
                        .Find(s => s.Email.ToLower() == student.Email.ToLower().Trim() 
                                   && s.StudentId != student.StudentId)
                        .FirstOrDefault();

                    if (duplicateEmail != null)
                    {
                        return ServiceResult.Failure($"Email '{student.Email}' is already in use");
                    }
                }

                // BR06: Must belong to exactly one department (department must exist)
                var department = _unitOfWork.Departments.GetById(student.DepartmentId);
                if (department == null)
                {
                    return ServiceResult.Failure($"Department with ID {student.DepartmentId} does not exist");
                }

                existing.StudentCode = student.StudentCode;
                existing.FullName = student.FullName.Trim();
                existing.Email = string.IsNullOrWhiteSpace(student.Email) ? student.Email : student.Email.Trim();
                existing.DepartmentId = student.DepartmentId;

                _unitOfWork.Students.Update(existing);
                _unitOfWork.Save();

                return ServiceResult.Success($"Student '{existing.FullName}' updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error updating student: {ex.Message}");
            }
        }

        public ServiceResult DeleteStudent(int id)
        {
            try
            {
                var student = _unitOfWork.Students.GetById(id);
                if (student == null)
                {
                    return ServiceResult.Failure($"Student with ID {id} not found");
                }

                // BR10: Cannot delete if has enrollments
                var hasEnrollments = _unitOfWork.Enrollments
                    .Find(e => e.StudentId == id)
                    .Any();

                if (hasEnrollments)
                {
                    return ServiceResult.Failure("Cannot delete student: student has active enrollments");
                }

                _unitOfWork.Students.Delete(id);
                _unitOfWork.Save();

                return ServiceResult.Success("Student deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error deleting student: {ex.Message}");
            }
        }
    }
}
