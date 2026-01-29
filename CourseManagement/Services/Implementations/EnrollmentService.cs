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
        /// Enrollment service implementation.
        /// Implements business rules:
        /// BR16: Cannot enroll in the same course twice
        /// BR17: Maximum 5 courses per student
        /// BR18: Enroll date cannot be in the past
        /// BR19: Student can enroll only in courses of the same department
        /// BR20: Student and course must exist
        /// BR21: Can only assign grade after enrollment exists
        /// BR22: Grade must be in range 0..10
        /// BR23: Grade cannot be updated once finalized
        /// BR24: Enrollment operations must be transactional
        /// </summary>
        public class EnrollmentService : IEnrollmentService
        {
            private readonly IUnitOfWork _unitOfWork;

            public EnrollmentService(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            public ServiceResult<IEnumerable<Enrollment>> GetAllEnrollments()
            {
                try
                {
                    var enrollments = _unitOfWork.Enrollments.GetAll();
                    return ServiceResult<IEnumerable<Enrollment>>.Success(enrollments);
                }
                catch (Exception ex)
                {
                    return ServiceResult<IEnumerable<Enrollment>>.Failure($"Error retrieving enrollments: {ex.Message}");
                }
            }

            public ServiceResult<Enrollment> GetEnrollment(int studentId, int courseId)
            {
                try
                {
                    var enrollment = _unitOfWork.Enrollments
                        .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                        .FirstOrDefault();

                    if (enrollment == null)
                    {
                        return ServiceResult<Enrollment>.Failure($"Enrollment not found for student {studentId} in course {courseId}");
                    }
                    return ServiceResult<Enrollment>.Success(enrollment);
                }
                catch (Exception ex)
                {
                    return ServiceResult<Enrollment>.Failure($"Error retrieving enrollment: {ex.Message}");
                }
            }

            public ServiceResult<IEnumerable<Enrollment>> GetEnrollmentsByStudent(int studentId)
            {
                try
                {
                    var enrollments = _unitOfWork.Enrollments
                        .Find(e => e.StudentId == studentId)
                        .ToList();

                    return ServiceResult<IEnumerable<Enrollment>>.Success(enrollments);
                }
                catch (Exception ex)
                {
                    return ServiceResult<IEnumerable<Enrollment>>.Failure($"Error retrieving enrollments: {ex.Message}");
                }
            }

            public ServiceResult<IEnumerable<Enrollment>> GetEnrollmentsByCourse(int courseId)
            {
                try
                {
                    var enrollments = _unitOfWork.Enrollments
                        .Find(e => e.CourseId == courseId)
                        .ToList();

                    return ServiceResult<IEnumerable<Enrollment>>.Success(enrollments);
                }
                catch (Exception ex)
                {
                    return ServiceResult<IEnumerable<Enrollment>>.Failure($"Error retrieving enrollments: {ex.Message}");
                }
            }

            public ServiceResult EnrollStudent(int studentId, int courseId, DateTime enrollDate)
            {
                // BR24: Enrollment operations must be transactional
                using var transaction = _unitOfWork.BeginTransaction();
                try
                {
                    // BR20: Student and course must exist
                    var student = _unitOfWork.Students.GetById(studentId);
                    if (student == null)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure($"Student with ID {studentId} does not exist");
                    }

                    var course = _unitOfWork.Courses.GetById(courseId);
                    if (course == null)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure($"Course with ID {courseId} does not exist");
                    }

                    // BR18: Enroll date cannot be in the past
                    if (enrollDate.Date < DateTime.Now.Date)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Enrollment date cannot be in the past");
                    }

                    // BR16: Cannot enroll in the same course twice
                    var existingEnrollment = _unitOfWork.Enrollments
                        .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                        .FirstOrDefault();

                    if (existingEnrollment != null)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure($"Student is already enrolled in this course");
                    }

                    // BR17: Maximum 5 courses per student
                    var studentEnrollments = _unitOfWork.Enrollments
                        .Find(e => e.StudentId == studentId)
                        .Count();

                    if (studentEnrollments >= 5)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Student cannot enroll in more than 5 courses");
                    }

                    // BR19: Student can enroll only in courses of the same department
                    if (student.DepartmentId != course.DepartmentId)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Student can only enroll in courses from their own department");
                    }
                    // BR29: student must be active
                    if (!student.IsActive)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Student is inactive");
                    }

                    // BR28: course must be active
                    if (course.Status != CourseStatus.Active)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Course is inactive");
                    }

                    // BR27: course must have at least 1 credit
                    if (course.Credits < 1)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Course credit must be at least 1");
                    }

                    // BR26: student age >= 18 at enrollment time (use enrollDate)
                    var age = CalculateAge(student.DateOfBirth, enrollDate);
                    if (age < 18)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Student must be at least 18");
                    }
                    var enrollment = new Enrollment
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        EnrollDate = enrollDate,
                        Grade = null,
                        IsGradeFinalized = false
                    };

                    _unitOfWork.Enrollments.Add(enrollment);
                    _unitOfWork.Save();
                    _unitOfWork.Commit();

              
                

                    return ServiceResult.Success($"Student enrolled successfully in course");
                }
                catch (Exception ex)
                {
                    _unitOfWork.Rollback();
                    return ServiceResult.Failure($"Error enrolling student: {ex.Message}");
                }
            }
            private static int CalculateAge(DateTime dob, DateTime atDate)
            {
                var age = atDate.Year - dob.Year;
                if (dob.Date > atDate.Date.AddYears(-age)) age--;
                return age;
            }
            public ServiceResult AssignGrade(int studentId, int courseId, decimal grade)
            {
                try
                {
                    // BR21: Enrollment must exist
                    var enrollment = _unitOfWork.Enrollments
                        .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                        .FirstOrDefault();

                    if (enrollment == null)
                    {
                        return ServiceResult.Failure("Enrollment does not exist");
                    }

                    // BR30: Grade assignment allowed only within grading period
                    const int GradingPeriodDays = 30; // sẽ refactor ra config/const sau
                    
                    var now = DateTime.UtcNow;
                    var daysSinceEnroll = (now - enrollment.EnrollDate.ToUniversalTime()).TotalDays;

                if (daysSinceEnroll > GradingPeriodDays)
                    {
                        return ServiceResult.Failure("Outside grading period");
                    }

                    // BR23: Grade cannot be updated once finalized

                    if (enrollment.IsGradeFinalized)
                    {
                        return ServiceResult.Failure("Grade is finalized and cannot be modified");
                    }

                    // BR22: Grade must be in range 0..10
                    if (grade < 0 || grade > 10)
                    {
                        return ServiceResult.Failure("Grade must be between 0 and 10");
                    }

                    enrollment.Grade = grade;
                    _unitOfWork.Enrollments.Update(enrollment);
                    _unitOfWork.Save();

                    return ServiceResult.Success($"Grade {grade} assigned successfully");
                }
                catch (Exception ex)
                {
                    return ServiceResult.Failure($"Error assigning grade: {ex.Message}");
                }
            }

            public ServiceResult FinalizeGrade(int studentId, int courseId)
            {
                try
                {
                    var enrollment = _unitOfWork.Enrollments
                        .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                        .FirstOrDefault();

                    if (enrollment == null)
                    {
                        return ServiceResult.Failure("Enrollment does not exist");
                    }

                    if (!enrollment.Grade.HasValue)
                    {
                        return ServiceResult.Failure("Cannot finalize: no grade has been assigned");
                    }

                    if (enrollment.IsGradeFinalized)
                    {
                        return ServiceResult.Failure("Grade is already finalized");
                    }

                    enrollment.IsGradeFinalized = true;
                    _unitOfWork.Enrollments.Update(enrollment);
                    _unitOfWork.Save();

                    return ServiceResult.Success("Grade finalized successfully");
                }
                catch (Exception ex)
                {
                    return ServiceResult.Failure($"Error finalizing grade: {ex.Message}");
                }
            }

            public ServiceResult UnenrollStudent(int studentId, int courseId)
            {
                // BR24: Enrollment operations must be transactional
                using var transaction = _unitOfWork.BeginTransaction();
                try
                {
                    var enrollment = _unitOfWork.Enrollments
                        .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                        .FirstOrDefault();

                    if (enrollment == null)
                    {
                        _unitOfWork.Rollback();
                        return ServiceResult.Failure("Enrollment does not exist");
                    }

                    // Use Delete(entity) for composite key entities
                    _unitOfWork.Enrollments.Delete(enrollment);
                    _unitOfWork.Save();
                    _unitOfWork.Commit();

                    return ServiceResult.Success("Student unenrolled successfully");
                }
                catch (Exception ex)
                {
                    _unitOfWork.Rollback();
                    return ServiceResult.Failure($"Error unenrolling student: {ex.Message}");
                }
            }
        }
    }
