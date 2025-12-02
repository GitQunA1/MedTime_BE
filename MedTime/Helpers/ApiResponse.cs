namespace MedTime.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message, int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errors, string errorMessage, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> ErrorResponse(string error, string errorMessage, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Errors = new List<string> { error },
                StatusCode = statusCode
            };
        }
    }
}
