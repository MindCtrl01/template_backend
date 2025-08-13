namespace TemplateBackend.API.Common.Models;

/// <summary>
/// Standardized API response model
/// </summary>
/// <typeparam name="T">The type of data in the response</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error details (if any)
    /// </summary>
    public object? Errors { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response
    /// </summary>
    /// <param name="data">The response data</param>
    /// <param name="message">The response message</param>
    /// <returns>A successful API response</returns>
    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errors">The error details</param>
    /// <returns>An error API response</returns>
    public static ApiResponse<T> ErrorResult(string? message, object? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "",
            Errors = errors
        };
    }
} 