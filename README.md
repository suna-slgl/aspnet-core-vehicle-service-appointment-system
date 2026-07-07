# Vehicle Service Appointment System

English | [Türkçe](README.tr.md)

Vehicle Service Appointment System is an ASP.NET Core MVC application for managing vehicle service appointments. It includes customer-facing vehicle and appointment workflows, an admin area for operational management, ASP.NET Core Identity authentication, Entity Framework Core persistence, and automated tests.

## Features

- User registration, login, profile, password change, forgot password, and reset password flows
- Role-based access control with `Admin` and `User` roles
- Vehicle management with license plate validation, ownership, soft-active state, and optional image upload
- Appointment creation and listing for registered users
- Appointment status workflow: pending, confirmed, in progress, completed, and cancelled
- Technician assignment and technician availability checks
- Service type management with estimated duration, price, icons, colors, and active state
- Admin area for appointments, service types, technicians, users, dashboard, and reports
- Dashboard/report services for appointment, technician, and service type statistics
- Turkish request localization configured with `tr-TR`
- SQL Server persistence through Entity Framework Core migrations
- xUnit test project using EF Core InMemory where appropriate

## Tech Stack

- .NET 9
- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core 9
- SQL Server
- Bootstrap, jQuery, jQuery Validation
- xUnit
- Docker

## Project Structure

```text
.
+-- src/
|   +-- VehicleServiceApp/        Main ASP.NET Core MVC application
+-- tests/
|   +-- VehicleServiceApp.Tests/  Automated tests
+-- Dockerfile                    Docker build definition
+-- aspnet-core-vehicle-service-appointment-system.sln
```

## Prerequisites

- .NET 9 SDK
- SQL Server or SQL Server Express
- Optional: Docker

## Configuration

The default connection string is defined in `src/VehicleServiceApp/appsettings.json`:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=AracServisDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Update this value if your SQL Server instance, database name, or authentication method is different.

Development configuration enables automatic migration application at startup:

```json
"Database": {
  "ApplyMigrationsOnStartup": true
}
```

For other environments, the value can be controlled through configuration.

## Getting Started

Restore dependencies:

```bash
dotnet restore
```

Apply database migrations:

```bash
dotnet ef database update --project src/VehicleServiceApp
```

Run the application:

```bash
dotnet run --project src/VehicleServiceApp
```

The development launch settings use:

- `http://localhost:5189`
- `https://localhost:7123`

## Testing

Run the test suite from the repository root:

```bash
dotnet test
```

## Docker

Build the image:

```bash
docker build -t vehicle-service-appointment-system .
```

Run the container:

```bash
docker run -p 8080:80 vehicle-service-appointment-system
```

Make sure the application can reach a configured SQL Server instance when running in a container.

## Notes

- The application seeds service types and technicians through the Entity Framework model configuration.
- The application creates `Admin` and `User` roles at startup.
- Admin and demo users are seeded only when the related `SeedUsers` configuration values are provided.
- Appointment records use a soft-delete style active filter.

## License

This project is licensed under the terms included in the [LICENSE](LICENSE) file.
