using CourseManagement.Models;
using CourseManagement.Services.Common;
using System;
using System.Collections.Generic;

namespace CourseManagement.Services.Interfaces
{
    /// <summary>
    /// Enrollment service interface.
    /// Implements business rules: BR16, BR17, BR18, BR19, BR20, BR21, BR22, BR23, BR24
    /// </summary>
    public interface IEnrollmentService
    {
        ServiceResult<IEnumerable<Enrollment>> GetAllEnrollments();
        ServiceResult<Enrollment> GetEnrollment(int studentId, int courseId);
        ServiceResult<IEnumerable<Enrollment>> GetEnrollmentsByStudent(int studentId);
        ServiceResult<IEnumerable<Enrollment>> GetEnrollmentsByCourse(int courseId);
        ServiceResult EnrollStudent(int studentId, int courseId, DateTime enrollDate);
        ServiceResult AssignGrade(int studentId, int courseId, decimal grade);
        ServiceResult FinalizeGrade(int studentId, int courseId);
        ServiceResult UnenrollStudent(int studentId, int courseId);
    }
}
