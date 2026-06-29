using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerceApi.Common;

/// <summary>
/// Bắt mọi exception chưa xử lý, trả về ApiResponse thống nhất + log lại.
/// Đăng ký: AddExceptionHandler&lt;GlobalExceptionHandler&gt;() + UseExceptionHandler().
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        int status;
        string message;
        object? errors = null;

        switch (exception)
        {
            case ValidationException ve:   // FluentValidation
                status = StatusCodes.Status400BadRequest;
                message = "Dữ liệu không hợp lệ";
                errors = ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                break;
            case System.Collections.Generic.KeyNotFoundException:
                status = StatusCodes.Status404NotFound;
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
                status = StatusCodes.Status401Unauthorized;
                message = "Không có quyền truy cập";
                break;
            default:
                status = StatusCodes.Status500InternalServerError;
                message = "Lỗi hệ thống";
                break;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(message, errors), cancellationToken);

        return true;   // đã xử lý
    }
}
