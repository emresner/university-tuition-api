using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace University.Tuition.Api.Models;

public class TuitionBatchUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
