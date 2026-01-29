namespace CourseManagement.Services.Common
{
    /// <summary>
    /// Represents the result of a service operation.
    /// All service methods return this instead of throwing exceptions (BR25).
    /// </summary>
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult Success(string message = "Operation completed successfully")
        {
            return new ServiceResult { IsSuccess = true, Message = message };
        }

        public static ServiceResult Failure(string message)
        {
            return new ServiceResult { IsSuccess = false, Message = message };
        }
    }

    /// <summary>
    /// Generic version that can return data along with the result.
    /// </summary>
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Success(T data, string message = "Operation completed successfully")
        {
            return new ServiceResult<T> { IsSuccess = true, Message = message, Data = data };
        }

        public new static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }
    }
}
