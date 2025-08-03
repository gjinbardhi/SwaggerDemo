# SwaggerDemo Message Processing Service

A .NET (9.0/8.0) Web API demonstrating:

* **Message CRUD** endpoints via Swagger/OpenAPI
* Background processing of messages (retry, dead-letter)
* Global exception handling middleware
* Dependency injection with interfaces
* xUnit + Moq unit tests (≥50% coverage)
* Code metrics collection

---

## Requirements

* .NET SDK 9.0
* MySQL server (or change connection string to your provider)

## Getting Started

1. **Clone** this repo:

   ```bash
   git clone <your-repo-url>
   cd SwaggerDemo
   ```

2. **Configure** your database in `appsettings.json`:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=localhost;port=3306;database=message_processor;user=root;password=***"
   }
   ```

3. **Build & Run** the API:

   ```bash
   dotnet build
   dotnet run
   ```

   * The server will listen on `http://localhost:5000`
   * Swagger UI: `http://localhost:5000/swagger`

4. **Initialize** database tables (automatically on startup).

## Testing & Coverage

```bash
cd SwaggerDemo
# run tests + code coverage
dotnet test --collect:"XPlat Code Coverage"
```

* Results in `TestResults/*/coverage.cobertura.xml`
* Aim: ≥50% line coverage.


## Project Structure

```
SwaggerDemo/           # API project
├─ Controllers/        # Web API controllers
├─ Interfaces/         # Abstractions (IMessageRepository)
├─ Middleware/         # Global exception handler
├─ Models/             # DTOs and domain models
├─ Services/           # Repository, background service, initializer
├─ Program.cs          # Startup & DI
├─ appsettings.json    # Config (DB, throttling)
└─ Dockerfile          # Container setup

SwaggerDemo.Tests/     # xUnit + Moq unit tests
SwaggerDemo.sln        # Solution file


