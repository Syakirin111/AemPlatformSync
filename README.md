# AEM Platform Sync

A .NET 10 console application that retrieves platform and well data from the AEM Enersol REST API and synchronizes it with a SQL Server LocalDB database using Entity Framework Core.

## Features

- Authenticates with the API and receives a bearer token.
- Retrieves platform and well data from the actual API endpoint.
- Tests the dummy platform/well endpoint.
- Inserts new platforms and wells.
- Updates existing platforms and wells by their IDs.
- Uses Entity Framework Core Code First migrations.
- Handles optional JSON fields and skips records with missing or invalid IDs.
- Includes a SQL query to find the most recently updated well for each platform.

## Technologies

- C#
- .NET 10
- Entity Framework Core 10
- SQL Server LocalDB
- REST API
- PowerShell

## Prerequisites

- Windows
- .NET 10 SDK
- SQL Server LocalDB
- Network access to the AEM Enersol API

## Configuration

The application reads the database connection string from `appsettings.json`.

The default configuration uses:

- Server: `(localdb)\MSSQLLocalDB`
- Database: `AemPlatformSyncDb`
- Authentication: Windows Authentication

Update the connection string if you use a different SQL Server instance.

## Database Setup

Open PowerShell in the project directory.

Restore the packages:

```powershell
dotnet restore
```

Apply the Entity Framework Core migrations:

```powershell
dotnet ef database update
```

If the `dotnet ef` command is unavailable, install the EF Core command-line tool:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.14
```

## Run the Application

Run the application:

```powershell
dotnet run
```

Enter your API username and password when prompted.

The application will:

1. Authenticate with the API.
2. Test the dummy platform/well endpoint.
3. Retrieve actual platform and well data.
4. Synchronize the data with SQL Server LocalDB.
5. Display a synchronization summary.

API credentials are entered at runtime and should not be committed to source control.

## API Endpoints

- Login: `POST /api/Account/Login`
- Actual data: `GET /api/PlatformWell/GetPlatformWellActual`
- Dummy data: `GET /api/PlatformWell/GetPlatformWellDummy`

The API base URL is configured in `Services/ApiService.cs`.

## Verify the Database

Run these queries in SQL Server Management Studio (SSMS):

```sql
USE AemPlatformSyncDb;
GO

SELECT * FROM dbo.Platform;
SELECT * FROM dbo.Well;
```

## SQL: Latest Updated Well per Platform

The following query returns the latest-updated well for each platform. Platforms without wells are also included.

```sql

USE AemPlatformSyncDb;
GO

WITH RankedWells AS
(
    SELECT
        w.*,
        ROW_NUMBER() OVER (
            PARTITION BY w.PlatformId
            ORDER BY w.UpdatedAt DESC, w.Id DESC
        ) AS RowNum
    FROM dbo.Well AS w
)
SELECT
    p.UniqueName AS PlatformName,
    rw.Id,
    rw.PlatformId,
    rw.UniqueName,
    rw.Latitude,
    rw.Longitude,
    rw.CreatedAt,
    rw.UpdatedAt
FROM dbo.Platform AS p
INNER JOIN RankedWells AS rw
    ON rw.PlatformId = p.Id
WHERE rw.RowNum = 1
ORDER BY p.Id;
```

## Validation

The application was tested successfully with the assessment API.

The actual data synchronization processed:

- 10 platforms
- 15 wells

The dummy endpoint was also tested successfully. The package vulnerability scan reported no vulnerable packages, and the project built successfully without warnings.

## Security Notes

- Do not commit API usernames, passwords, bearer tokens, or other secrets.
- Keep environment-specific configuration out of source control where appropriate.
- The assessment API currently uses HTTP; production systems should use HTTPS.

## License

Created as part of the AEM Enersol technical assessment.
