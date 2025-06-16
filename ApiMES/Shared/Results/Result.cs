namespace ApiMES.Shared.Results
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public required string Message { get; set; }
        public required List<string> Errors { get; set; }

        public static Result Ok(string message = "Success")
        {
            return new Result { IsSuccess = true, Message = message, Errors = new List<string>() };
        }

        public static Result Fail(params string[] errors)
        {
            return new Result
            {
                IsSuccess = false,
                Message = "Failed",
                Errors = errors.ToList()
            };
        }

        public static Result Fail(IEnumerable<string> errors)
        {
            return new Result
            {
                IsSuccess = false,
                Message = "Failed",
                Errors = errors.ToList()
            };
        }
    }

    public class Result<T> : Result
    {
        public T? Data { get; set; }

        public static Result<T> Ok(T data, string message = "Success") =>
            new() { IsSuccess = true, Data = data, Message = message, Errors = new() };

        public static new Result<T> Fail(params string[] errors) =>
            new() { IsSuccess = false, Message = "Failed", Errors = errors.ToList() };
    }
}
