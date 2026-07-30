using ECommerceApi.DTOs.Payment;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[Route("api/payments")]
public class PaymentsController(IPaymentService paymentService) : ApiControllerBase
{
    /// <summary>
    /// SePay gọi vào đây khi tài khoản có tiền vào. KHÔNG dùng JWT — tự xác thực bằng Apikey.
    /// Phải trả 2xx + {success:true} trong 5 giây, nếu không SePay sẽ retry.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("sepay/webhook")]
    public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookPayload payload)
    {
        if (!paymentService.VerifyApiKey(Request.Headers.Authorization.ToString()))
            return Unauthorized(new { success = false });

        await paymentService.HandleSepayWebhookAsync(payload);
        return Ok(new { success = true });
    }
}
