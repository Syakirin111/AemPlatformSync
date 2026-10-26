
using AemPlatformSync.Data;
using AemPlatformSync.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

Console.WriteLine("====================================");
Console.WriteLine("       AEM PLATFORM SYNC");
Console.WriteLine("====================================");
Console.WriteLine();

Console.Write("Enter API username: ");
var username = Console.ReadLine();

Console.Write("Enter API password: ");
var password = ReadPassword();

if (string.IsNullOrWhiteSpace(username) ||
    string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Username and password are required.");
    return;
}

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    var connectionString =
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Database connection string is missing.");

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    using var apiService = new ApiService();

    Console.WriteLine();
    Console.WriteLine("Step 1: Connecting to API...");

    await apiService.LoginAsync(username, password);

    Console.WriteLine();
    Console.WriteLine("Step 2: Testing dummy platform/well endpoint...");

    var dummyJson = await apiService.GetPlatformWellDummyAsync();

    Console.WriteLine("Dummy API test successful.");
    Console.WriteLine("Dummy API response:");
    Console.WriteLine(dummyJson);

    Console.WriteLine("Step 2: Retrieving actual platform/well data...");

    var actualJson = await apiService.GetPlatformWellActualAsync();

    Console.WriteLine("Step 3: Synchronizing database...");

    await using var db = new AppDbContext(options);

    var syncService = new SyncService(db);

    await syncService.SyncPlatformsAndWellsAsync(actualJson);

    Console.WriteLine();
    Console.WriteLine("SUCCESS: API synchronization completed.");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("----- ERROR DETAILS -----");
    Console.WriteLine(ex.ToString());
}

static string ReadPassword()
{
    var password = new System.Text.StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
                password.Length--;

            continue;
        }

        if (!char.IsControl(key.KeyChar))
            password.Append(key.KeyChar);
    }

    return password.ToString();
}
