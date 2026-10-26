namespace AemPlatformSync.Models;

public class Platform
{
    public int Id { get; set; }

    public string? UniqueName { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<Well> Wells { get; set; } = new();
}