namespace Backend.Models.Dtos
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static Result<T> SuccessResult(T data, string? message = null)
        {
            return new Result<T> { Success = true, Data = data, Message = message };
        }

        public static Result<T> Failure(string message)
        {
            return new Result<T> { Success = false, Message = message };
        }
    }

    public class Result
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static Result SuccessResult(string? message = null)
        {
            return new Result { Success = true, Message = message };
        }

        public static Result Failure(string message)
        {
            return new Result { Success = false, Message = message };
        }
    }
}