using System.ComponentModel.DataAnnotations;

namespace FinishHub.Admin.Models;

public class CityViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CityFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Arabic name is required.")]
    [Display(Name = "Name (Arabic)")]
    public string NameAr { get; set; } = string.Empty;

    [Required(ErrorMessage = "English name is required.")]
    [Display(Name = "Name (English)")]
    public string NameEn { get; set; } = string.Empty;

    [Display(Name = "Pinned city")]
    public bool IsPinned { get; set; }
}
