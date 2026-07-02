using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs.Review;

public class CreateReviewRequest
{
    [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
