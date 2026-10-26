
using AemPlatformSync.Data;
using AemPlatformSync.DTOs;
using AemPlatformSync.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AemPlatformSync.Services;

public sealed class SyncService
{
    private readonly AppDbContext _db;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SyncService(AppDbContext db)
    {
        _db = db;
    }

    public async Task SyncPlatformsAndWellsAsync(string json)
    {
        var platforms = JsonSerializer.Deserialize<List<PlatformApiDto>>(
            json, JsonOptions);

        if (platforms is null)
        {
            throw new InvalidOperationException(
                "The API response could not be read as a platform list.");
        }

        var platformCount = 0;
        var wellCount = 0;

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        foreach (var platformDto in platforms)
        {
            if (platformDto.Id is null || platformDto.Id <= 0)
            {
                Console.WriteLine(
                    "Skipping a platform because its ID is missing or invalid.");
                continue;
            }

            var platformId = platformDto.Id.Value;

            var platform = await _db.Platforms
                .SingleOrDefaultAsync(p => p.Id == platformId);

            if (platform is null)
            {
                platform = new Platform { Id = platformId };
                _db.Platforms.Add(platform);
            }

            platform.UniqueName = platformDto.UniqueName;
            platform.Latitude = platformDto.Latitude;
            platform.Longitude = platformDto.Longitude;
            platform.CreatedAt = platformDto.CreatedAt;
            platform.UpdatedAt = platformDto.UpdatedAt;

            platformCount++;

            foreach (var wellDto in platformDto.Well ?? [])
            {
                if (wellDto.Id is null || wellDto.Id <= 0)
                {
                    Console.WriteLine(
                        $"Skipping a well under platform {platformId}: " +
                        "its ID is missing or invalid.");
                    continue;
                }

                var wellId = wellDto.Id.Value;
                var wellPlatformId = wellDto.PlatformId ?? platformId;

                if (wellPlatformId != platformId)
                {
                    Console.WriteLine(
                        $"Skipping well {wellId}: its PlatformId does not " +
                        "match parent platform {platformId}.");
                    continue;
                }

                var well = await _db.Wells
                    .SingleOrDefaultAsync(w => w.Id == wellId);

                if (well is null)
                {
                    well = new Well { Id = wellId };
                    _db.Wells.Add(well);
                }

                well.PlatformId = wellPlatformId;
                well.UniqueName = wellDto.UniqueName;
                well.Latitude = wellDto.Latitude;
                well.Longitude = wellDto.Longitude;
                well.CreatedAt = wellDto.CreatedAt;
                well.UpdatedAt = wellDto.UpdatedAt;

                wellCount++;
            }
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine();
        Console.WriteLine("----- SYNCHRONIZATION SUMMARY -----");
        Console.WriteLine($"Platforms processed: {platformCount}");
        Console.WriteLine($"Wells processed: {wellCount}");
        Console.WriteLine("Database synchronization completed.");
    }
}
