using test.Models;

namespace test.DTOs;

// ---------- Portfolio ----------

public class PortfolioDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<PortfolioMediaDto> Media { get; set; } = new();
}

public class PortfolioMediaDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
}

public class AddPortfolioMediaRequest
{
    public List<IFormFile>? Images { get; set; }
    public List<IFormFile>? Videos { get; set; }
}
