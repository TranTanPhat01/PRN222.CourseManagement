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
    /// Course service implementation.
    /// Implements business rules:
    /// BR11: CourseCode must be unique
    /// BR12: Must belong to exactly one department (department must exist)
    /// BR13: Credits must be between 1 and 6
    /// BR14: Cannot delete if has enrollments
    /// BR15: Cannot update if inactive or archived
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<IEnumerable<Course>> GetAllCourses()
        {
            try
            {
                var courses = _unitOfWork.Courses.GetAll();
                return ServiceResult<IEnumerable<Course>>.Success(courses);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Course>>.Failure($"Error retrieving courses: {ex.Message}");
            }
        }

        public ServiceResult<Course> GetCourseById(int id)
        {
            try
            {
                var course = _unitOfWork.Courses.GetById(id);
                if (course == null)
                {
                    return ServiceResult<Course>.Failure($"Course with ID {id} not found");
                }
                return ServiceResult<Course>.Success(course);
            }
            catch (Exception ex)
            {
                return ServiceResult<Course>.Failure($"Error retrieving course: {ex.Message}");
            }
        }

        public ServiceResult<Course> GetCourseByCode(string courseCode)
        {
            try
            {
                var course = _unitOfWork.Courses
                    .Find(c => c.CourseCode == courseCode)
                    .FirstOrDefault();

                if (course == null)
                {
                    return ServiceResult<Course>.Failure($"Course with code '{courseCode}' not found");
                }
                return ServiceResult<Course>.Success(course);
            }
            catch (Exception ex)
            {
                return ServiceResult<Course>.Failure($"Error retrieving course: {ex.Message}");
            }
        }

        public ServiceResult AddCourse(Course course)
        {
            try
            {
                // BR13: Credits must be between 1 and 6
                if (course.Credits < 1 || course.Credits > 6)
                {
                    return ServiceResult.Failure("Course credits must be between 1 and 6");
                }

                // BR11: CourseCode unique
                var existingCourse = _unitOfWork.Courses
                    .Find(c => c.CourseCode == course.CourseCode)
                    .FirstOrDefault();

                if (existingCourse != null)
                {
                    return ServiceResult.Failure($"Course with code '{course.CourseCode}' already exists");
                }

                // BR12: Must belong to exactly one department (department must exist)
                var department = _unitOfWork.Departments.GetById(course.DepartmentId);
                if (department == null)
                {
                    return ServiceResult.Failure($"Department with ID {course.DepartmentId} does not exist");
                }

                _unitOfWork.Courses.Add(course);
                _unitOfWork.Save();

                return ServiceResult.Success($"Course '{course.Title}' added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error adding course: {ex.Message}");
            }
        }

        public ServiceResult UpdateCourse(Course course)
        {
            try
            {
                var existing = _unitOfWork.Courses.GetById(course.CourseId);
                if (existing == null)
                {
                    return ServiceResult.Failure($"Course with ID {course.CourseId} not found");
                }

                // BR15: Cannot update if inactive or archived
                if (existing.Status == CourseStatus.Inactive || existing.Status == CourseStatus.Archived)
                {
                    return ServiceResult.Failure($"Cannot update course: course status is {existing.Status}");
                }

                // BR13: Credits must be between 1 and 6
                if (course.Credits < 1 || course.Credits > 6)
                {
                    return ServiceResult.Failure("Course credits must be between 1 and 6");
                }

                // BR11: CourseCode unique (excluding current course)
                var duplicateCode = _unitOfWork.Courses
                    .Find(c => c.CourseCode == course.CourseCode && c.CourseId != course.CourseId)
                    .FirstOrDefault();

                if (duplicateCode != null)
                {
                    return ServiceResult.Failure($"Course with code '{course.CourseCode}' already exists");
                }

                // BR12: Must belong to exactly one department (department must exist)
                var department = _unitOfWork.Departments.GetById(course.DepartmentId);
                if (department == null)
                {
                    return ServiceResult.Failure($"Department with ID {course.DepartmentId} does not exist");
                }

                existing.CourseCode = course.CourseCode;
                existing.Title = course.Title;
                existing.Credits = course.Credits;
                existing.DepartmentId = course.DepartmentId;
                existing.Status = course.Status;

                _unitOfWork.Courses.Update(existing);
                _unitOfWork.Save();

                return ServiceResult.Success($"Course '{existing.Title}' updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error updating course: {ex.Message}");
            }
        }

        public ServiceResult DeleteCourse(int id)
        {
            try
            {
                var course = _unitOfWork.Courses.GetById(id);
                if (course == null)
                {
                    return ServiceResult.Failure($"Course with ID {id} not found");
                }

                // BR14: Cannot delete if has enrollments
                var hasEnrollments = _unitOfWork.Enrollments
                    .Find(e => e.CourseId == id)
                    .Any();

                if (hasEnrollments)
                {
                    return ServiceResult.Failure("Cannot delete course: course has active enrollments");
                }

                _unitOfWork.Courses.Delete(id);
                _unitOfWork.Save();

                return ServiceResult.Success("Course deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error deleting course: {ex.Message}");
            }
        }
    }
}
