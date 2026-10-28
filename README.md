# Books Management API

A secure ASP.NET Core Web API for managing books. The API supports book CRUD operations, user registration, JWT authentication, role-based authorization, and TOTP-based two-factor authentication.

## Features

- Built with ASP.NET Core and .NET 10
- SQL Server database integration using Entity Framework Core
- Book management:
  - Retrieve all books
  - Retrieve a book by ID
  - Add a new book
  - Update an existing book
  - Delete a book
- JWT bearer authentication
- Role-based authorization for administrative operations
- TOTP-based two-factor authentication using authenticator applications
- QR code generation for TOTP enrollment
- OpenAPI documentation with Scalar during development
- Dependency injection using service interfaces
- Seed data for sample books

## Technology Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server
- JWT Bearer Authentication
- `Otp.NET`
- `QRCoder`
- Scalar OpenAPI
- Visual Studio

## Project Structure

```text
Books-Managment/
├── API Practise/
│   ├── Controllers/
│   │   └── BooksController.cs
│   ├── Data/
│   │   └── APIContext.cs
│   ├── Entities/
│   │   ├── Book.cs
│   │   └── User.cs
│   ├── Models/
│   │   ├── BooksDto.cs
│   │   ├── LoginChallengeDto.cs
│   │   ├── TotpSetupResponseDto.cs
│   │   ├── VerifyLoginOtpDto.cs
│   │   ├── VerifyTotpDto.cs
│   │   ├── newBookDto.cs
│   │   └── userDto.cs
│   ├── Service/
│   │   ├── AuthService.cs
│   │   ├── BookSerice.cs
│   │   ├── IAuthService.cs
│   │   └── IBookService.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── Books managment.csproj
├── Books managment.slnx
└── README.md
```

## Prerequisites

Install the following software before running the project:

- .NET 10 SDK
- SQL Server or SQL Server Express
- Visual Studio with ASP.NET and web development support
- An authenticator application such as Microsoft Authenticator or Google Authenticator

Verify the installed .NET SDK:

```bash
dotnet --version
```

## Configuration

The application uses the following configuration values:

- SQL Server connection string
- JWT signing key
- JWT issuer
- JWT audience

Configure these values using user secrets or environment-specific configuration. Do not commit production credentials, database passwords, or JWT signing keys to source control.

Example configuration (`appsettings.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BooksManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "AppSettings": {
    "Token": "replace-with-a-long-random-secret",
    "Issuer": "BooksManagementApi",
    "Audience": "BooksManagementClient"
  }
}
```

For local development, user secrets can be configured with:

```bash
dotnet user-secrets init --project ".\API Practise\Books managment.csproj"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string" --project ".\API Practise\Books managment.csproj"
dotnet user-secrets set "AppSettings:Token" "your-long-random-jwt-secret" --project ".\API Practise\Books managment.csproj"
dotnet user-secrets set "AppSettings:Issuer" "BooksManagementApi" --project ".\API Practise\Books managment.csproj"
dotnet user-secrets set "AppSettings:Audience" "BooksManagementClient" --project ".\API Practise\Books managment.csproj"
```

## Database Setup

Create a SQL Server database and update the `DefaultConnection` connection string.

If Entity Framework Core migrations are available, apply them with:

```bash
dotnet ef database update --project ".\API Practise\Books managment.csproj"
```

The application seeds the following books when the database is initialized:

- The Great Gatsby
- To Kill a Mockingbird
- 1984

## Running the Application

Restore dependencies:

```bash
dotnet restore ".\Books managment.slnx"
```

Build the solution:

```bash
dotnet build ".\Books managment.slnx"
```

Run the API:

```bash
dotnet run --project ".\API Practise\Books managment.csproj"
```

The default development URLs are:

- HTTP: `http://localhost:5177`
- HTTPS: `https://localhost:7229`

When running in the Development environment, OpenAPI documentation is available through Scalar.

## Authentication Flow

The API uses a two-step authentication flow to enforce multi-factor security.

### 1. Register a User

```http
POST /api/Books/register
Content-Type: application/json
```

**Example request:**

```json
{
  "username": "admin",
  "password": "StrongPassword123!",
  "role": "Admin"
}
```

New users are assigned either the `Admin` or `User` role.

### 2. Configure TOTP

After signing in with the user's credentials:

```http
POST /api/Books/setup-otp
Authorization: Bearer <access-token>
```

The response contains a Base64-encoded QR code. Scan the QR code using an authenticator application.

Verify the authenticator setup:

```http
POST /api/Books/verify-otp-setup
Authorization: Bearer <access-token>
Content-Type: application/json
```

**Example request:**

```json
{
  "otp": "123456"
}
```

### 3. Login

Submit the username and password:

```http
POST /api/Books/login
Content-Type: application/json
```

**Example request:**

```json
{
  "username": "admin",
  "password": "StrongPassword123!"
}
```

If TOTP is enabled, the response contains a temporary challenge token. Submit that token together with the six-digit authenticator code:

```http
POST /api/Books/verify-login-otp
Content-Type: application/json
```

**Example request:**

```json
{
  "challengeToken": "<challenge-token>",
  "otp": "123456"
}
```

The response contains an access token:

```json
{
  "accessToken": "<jwt-access-token>"
}
```

Use the access token in the `Authorization` header for protected endpoints:

```http
Authorization: Bearer <jwt-access-token>
```

## API Endpoints

### Authentication

- `POST /api/Books/register` – Register a new user
- `POST /api/Books/login` – Validate username and password
- `POST /api/Books/verify-login-otp` – Verify the login TOTP code
- `POST /api/Books/setup-otp` – Generate a TOTP QR code
- `POST /api/Books/verify-otp-setup` – Enable TOTP after verification

### Books

- `GET /api/Books` – Retrieve all books
- `GET /api/Books/{id}` – Retrieve a book by ID
- `POST /api/Books` – Add a book
- `PUT /api/Books/{id}` – Update a book
- `DELETE /api/Books/{id}` – Delete a book; requires the `Admin` role

**Example book request body:**

```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "yearPublished": 2008
}
```

## Authorization

The following operations require authentication:

- Retrieving books
- Adding books
- Updating books
- Deleting books
- Configuring TOTP

Book deletion requires the `Admin` role.

## Error Responses

The API returns standard HTTP status codes, including:

- `200 OK` — Request completed successfully
- `201 Created` — Resource created successfully
- `400 Bad Request` — Invalid request data
- `401 Unauthorized` — Missing or invalid authentication
- `403 Forbidden` — Insufficient permissions
- `404 Not Found` — Resource does not exist
- `409 Conflict` — Resource cannot be created or updated due to its current state
