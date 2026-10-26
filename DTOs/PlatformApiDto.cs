
namespace AemPlatformSync.DTOs;

public sealed class PlatformApiDto
{
    public int? Id { get; set; }

    public string? UniqueName { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<WellApiDto>? Well { get; set; }
}

public sealed class WellApiDto
{
    public int? Id { get; set; }

    public int? PlatformId { get; set; }

    public string? UniqueName { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
