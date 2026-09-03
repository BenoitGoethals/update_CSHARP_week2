using System.ComponentModel.DataAnnotations;

namespace Week2Api.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Author { get; set; } = string.Empty;

    [Range(1450, 2100)]
    public int Year { get; set; }

    [StringLength(20)]
    public string? Isbn { get; set; }
}
