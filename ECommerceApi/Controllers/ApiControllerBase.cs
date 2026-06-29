using ECommerceApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkResponse<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResponse<T>(T data, string? message = null) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));

    protected IActionResult NotFoundResponse(string message = "Không tìm thấy dữ liệu") =>
        NotFound(ApiResponse<object>.Fail(message));

    protected IActionResult BadRequestResponse(string message, object? errors = null) =>
        BadRequest(ApiResponse<object>.Fail(message, errors));
}
