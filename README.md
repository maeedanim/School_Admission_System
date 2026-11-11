# School Admission System

A high-concurrency online school admission system built with **.NET 10, ASP.NET Core Web API, PostgreSQL 17, React 19, and TypeScript**.

The system follows a First-Come, First-Served (FCFS) admission model. Students can temporarily reserve a seat in a class through a time-limited hold. A hold can be confirmed during its reservation period; otherwise, it expires and its reserved seat becomes available again.

## Live Demo

- **Frontend:** https://school-admission-system-lemon.vercel.app/classes
- **Backend API:** https://school-admission-system-t609.onrender.com
- **Health Check:** https://school-admission-system-t609.onrender.com/health

The frontend is hosted on Vercel. The backend API and PostgreSQL can run locally using Docker Compose.

### Demo Credentials

The following demo accounts are provided for reviewers to explore the live application.

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@gmail.com` | `Admin123` |
| Student | `student@gmail.com` | `Sudent123` |

> These credentials are provided exclusively for demonstration and assignment evaluation. Please do not use these accounts for personal or sensitive information.

## Table of Contents

- [Features](#features)
- [Architecture and Technology Stack](#architecture-and-technology-stack)
- [Project Structure](#project-structure)
- [Business Rules](#business-rules)
- [Authentication and Authorization](#authentication-and-authorization)
- [Additional Enhancements (Beyond Assignment Requirements)](#additional-enhancements-beyond-assignment-requirements)
- [Environment Configuration](#environment-configuration)
- [Quick Start with Docker](#quick-start-with-docker)
- [Local Development](#local-development)
- [Frontend Routes](#frontend-routes)
- [Automated Tests](#automated-tests)
- [API Reference](#api-reference)
- [Admission Hold Lifecycle](#admission-hold-lifecycle)
- [Deployment](#deployment)
- [AI Usage Documentation](#ai-usage-documentation)
- [License](#license)

---

## Features

### Admission Management

- Create classes with configurable seat capacities.
- View class information and current seat availability.
- Temporarily reserve seats using time-limited holds.
- Confirm or cancel an active hold.
- View a student's own admission applications.
- Allow administrators to view admission applications.

### Concurrency and Data Integrity

- PostgreSQL transactions and row-level locking for competing seat reservations.
- Database constraints to protect data integrity.
- Prevent seat overbooking.
- Prevent duplicate active or confirmed applications for the same student and class.
- Idempotent confirmation and cancellation operations where supported by the business rules.
- Automatically release expired reservations during admission operations.

### Authentication and Authorization

- Student registration using a 17-digit birth registration number.
- Separate student and administrator login.
- JWT access-token authentication.
- Refresh-token rotation using an HttpOnly cookie.
- Role-based authorization for student and administrator operations.
- Administrator bootstrap configuration for initial setup.

### User Experience

- Periodic class availability refresh.
- Reservation countdown.
- Separate student and administrator interfaces.
- Protected frontend routes based on authenticated roles.

### Infrastructure

- Dockerized ASP.NET Core API.
- PostgreSQL running through Docker Compose.
- Persistent PostgreSQL named volume.
- EF Core migrations applied during application startup.
- Production hosting through Render and Vercel.

---

## Architecture and Technology Stack

### Backend

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Backend development |
| ASP.NET Core Web API | REST API and HTTP pipeline |
| Entity Framework Core 10 | Object-relational mapping |
| Npgsql | PostgreSQL database provider |
| PostgreSQL 17 | Relational database |
| ASP.NET Core Identity | User and role management |
| JWT Bearer Authentication | Access-token authentication |
| Swagger / OpenAPI | API documentation in Development |
| xUnit | Automated integration testing |

### Three-Tier Architecture

The backend separates HTTP handling, business rules, and data access.

**1. Presentation Layer — `SchoolAdmission.API`**

- REST API controllers.
- HTTP request and response handling.
- Authentication and authorization.
- Dependency injection and middleware.
- CORS configuration.

**2. Business Logic Layer — `SchoolAdmission.BLL`**

- Admission business rules.
- Service interfaces and implementations.
- Request and response DTOs.
- Business validation and exception handling.

**3. Data Access Layer — `SchoolAdmission.DAL`**

- Entity Framework Core DbContext.
- Database entities and configurations.
- Repository implementations.
- PostgreSQL queries and transactions.
- EF Core migrations.

### Frontend

| Technology | Purpose |
|---|---|
| React 19 | User interface |
| TypeScript | Type-safe application development |
| Vite | Development server and production build |
| React Router | Client-side routing |
| TanStack Query | Server-state management and polling |
| Axios | HTTP communication |
| Lucide React | Icons |

### Infrastructure

- Docker and Docker Compose.
- Multi-stage Dockerfile for the API.
- PostgreSQL named volume.
- Render for backend hosting.
- Vercel for frontend hosting.

---

## Project Structure

```text
School_Admission_System/
|
|-- .env.example
|-- .gitignore
|-- Dockerfile
|-- docker-compose.yml
|-- README.md
|-- AI_USAGE.md
|-- SchoolAdmissionSystem.sln
|
|-- backend/
|   |-- SchoolAdmission.API/
|   |   |-- Controllers/
|   |   |-- Middleware/
|   |   |-- Services/
|   |   |-- Models/
|   |   `-- Program.cs
|   |
|   |-- SchoolAdmission.BLL/
|   |   |-- DTOs/
|   |   |-- Enums/
|   |   |-- Exceptions/
|   |   |-- Interfaces/
|   |   |-- Models/
|   |   `-- Services/
|   |
|   `-- SchoolAdmission.DAL/
|       |-- Data/
|       |-- Entities/
|       |-- Repositories/
|       `-- Migrations/
|
|-- frontend/
|   `-- school-admission-ui/
|       |-- public/
|       |-- src/
|       |   |-- app/
|       |   |-- assets/
|       |   |-- components/
|       |   |-- features/
|       |   |-- hooks/
|       |   |-- services/
|       |   |-- types/
|       |   `-- utils/
|       |-- package.json
|       `-- vercel.json
|
`-- tests/
    `-- SchoolAdmission.Tests/
        |-- AdmissionIntegrationTests.cs
        `-- PostgresTestFixture.cs
```

---

## Business Rules

1. **Seat capacity:** A class cannot have more active holds and confirmed admissions than its total seat capacity.
2. **Reservation duration:** A new hold remains active for the configured duration. The default is 600 seconds.
3. **Expiration:** An expired hold cannot be confirmed. Its reserved seat becomes available again.
4. **Student identification:** A student's birth registration number must contain exactly 17 ASCII digits.
5. **Duplicate applications:** A student cannot create another active hold or confirmed admission for the same class.
6. **Ownership:** Only the student who owns a hold can retrieve, confirm, or cancel it.
7. **Confirmation:** Only an active, unexpired hold can be confirmed.
8. **Cancellation:** Students can cancel their own active holds. Confirmed admissions cannot be cancelled.
9. **Idempotency:** Repeating a successful confirmation does not increase the confirmed seat count.
10. **Concurrency:** Database transactions and row-level locking protect seat allocation when multiple students apply concurrently.
11. **Persistence:** PostgreSQL data is retained across container restarts through a Docker named volume.

### Admission Statuses

| Status | Description |
|---|---|
| `HELD` | Seat temporarily reserved |
| `CONFIRMED` | Admission successfully confirmed |
| `EXPIRED` | Reservation period elapsed |
| `CANCELLED` | Applicant cancelled an active reservation |

---

## Authentication and Authorization

The API uses JWT Bearer authentication for protected endpoints.

After a successful student or administrator login:

- The API returns an access token in the response body.
- The API sets a refresh token in an HttpOnly cookie.
- Protected API requests use the access token in the `Authorization` header.
- The refresh endpoint rotates the refresh token and returns a new access token.
- Logout revokes the refresh token and clears the cookie.

The refresh cookie is named `school_admission_refresh` and is scoped to `/api/auth`. Refresh and logout requests must originate from the configured frontend origin.

### Authorization Roles

| Role | Permissions |
|---|---|
| Student | View classes, manage their own holds, and view their applications |
| Admin | View classes, create classes, and view all admission applications |

A student's identity is derived from the authenticated JWT claims. The API does not use a client-supplied `X-Student-Id` header to identify the applicant.

---

## Additional Enhancements (Beyond Assignment Requirements)

All mandatory requirements specified in the original assignment have been implemented and tested.

Although authentication and authorization were not mandatory requirements of the original assignment, I independently implemented these features as a learning opportunity to strengthen my understanding of authentication, authorization, and application security in ASP.NET Core.

These enhancements were developed beyond the assigned scope and are not presented as mandatory assignment requirements.

### 1. Authentication

- Student registration (signup).
- Student login.
- Administrator login.
- JWT-based authentication.
- Secure password hashing.
- Refresh-token rotation using an HttpOnly cookie.

### 2. Role-Based Access Control (RBAC)

- Introduced separate `Admin` and `Student` roles.
- Restricted administrative operations to authorized administrators.
- Restricted student-specific operations to authenticated students.
- Implemented server-side role-based authorization using ASP.NET Core authorization attributes.
- Enforced ownership checks for student-specific admission records.

### 3. Additional Security Considerations

- Protected API endpoints using authentication and authorization middleware.
- Derived student identity from authenticated JWT claims instead of trusting client-supplied student identifiers.
- Prevented unauthorized access to administrative functionality.
- Used an HttpOnly refresh-token cookie to reduce direct JavaScript access to refresh-token values.

### Design Decision

These enhancements were implemented independently to explore practical authentication and authorization patterns beyond the original assignment scope.

The core admission workflow and its mandatory business rules remain the primary focus of the application.

---

## Environment Configuration

Create a root `.env` file using `.env.example` as a template.

**PowerShell:**

```powershell
Copy-Item .env.example .env
```

Configure the following variables:

| Variable | Description |
|---|---|
| `POSTGRES_DB` | PostgreSQL database name |
| `POSTGRES_USER` | PostgreSQL username |
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `POSTGRES_PORT` | Host port mapped to PostgreSQL |
| `JWT_SECRET` | Base64-encoded JWT signing secret; must decode to at least 32 bytes |
| `JWT_ISSUER` | JWT issuer |
| `JWT_AUDIENCE` | JWT audience |
| `ADMIN_BOOTSTRAP_FULL_NAME` | Initial administrator's name |
| `ADMIN_BOOTSTRAP_EMAIL` | Initial administrator's email |
| `ADMIN_BOOTSTRAP_PASSWORD` | Initial administrator's password |
| `HOLD_DURATION_SECONDS` | Reservation duration in seconds |
| `FRONTEND_ORIGIN` | Allowed frontend origin for refresh and logout requests |

The supplied example uses a 600-second reservation duration.

### Initial Administrator

Configure the administrator bootstrap variables when initializing an appropriate fresh database.

Use a strong password and keep these values private. Do not commit credentials to source control.

Do not assume that changing bootstrap variables will reset or replace an administrator in an existing database.

### Frontend Environment

The frontend uses Vite environment variables.

Development:

```env
VITE_API_BASE_URL=http://localhost:8080
```

Production:

```env
VITE_API_BASE_URL=https://school-admission-system-t609.onrender.com
```

Configure the production value in the Vercel project environment settings as well.

Vite reads environment variables at build time. Restart the development server after changing local environment files.

---

## Quick Start with Docker

Docker Compose starts PostgreSQL and the ASP.NET Core API. The frontend runs separately.

### Prerequisites

- Docker Desktop with Docker Compose.
- Git.
- Node.js and npm (for the frontend).
- .NET 10 SDK (for local backend development).

### 1. Clone the Repository

```powershell
git clone https://github.com/maeedanim/School_Admission_System.git
cd School_Admission_System
```

### 2. Configure Environment Variables

```powershell
Copy-Item .env.example .env
```

Open `.env` and configure the database credentials, JWT secret, and other required settings.

### 3. Build and Start the Services

```powershell
docker compose up --build -d
```

This command builds the API image and starts PostgreSQL and the API. The API waits for the database health check and applies pending EF Core migrations during startup.

### 4. Verify the Containers

```powershell
docker compose ps
```

PostgreSQL should report a healthy status.

### 5. Verify the API

- API: http://localhost:8080
- Health check: http://localhost:8080/health

The health endpoint should return HTTP 200 when the application is healthy.

Swagger UI is enabled only in the Development environment.

### 6. Run the Frontend

Open another terminal:

```powershell
cd frontend/school-admission-ui
npm install
npm run dev
```

Open http://localhost:5173.

### 7. View Logs

API:

```powershell
docker compose logs -f api
```

PostgreSQL:

```powershell
docker compose logs -f postgres
```

### 8. Stop the Services

```powershell
docker compose down
```

This preserves the PostgreSQL named volume.

**Warning:** Avoid `docker compose down -v` unless you intentionally want to delete the database volume and all data stored in it.

---

## Local Development

You can run the API and frontend outside Docker while using Docker for PostgreSQL.

### 1. Start PostgreSQL

From the repository root:

```powershell
docker compose up -d postgres
```

### 2. Configure the Backend Database Connection

The backend reads its connection string from:

`ConnectionStrings:DefaultConnection`

Use .NET User Secrets or an appropriate local environment variable to configure it.

Example:

```text
Host=localhost;Port=5434;Database=school_admission;Username=school_app;Password=YOUR_PASSWORD
```

Use the host port and credentials configured in your local `.env` file. The example above assumes `POSTGRES_PORT=5434`.

### 3. Run the Backend

From the repository root:

```powershell
dotnet run --project backend/SchoolAdmission.API/SchoolAdmission.API.csproj --urls http://localhost:8080
```

The application applies pending database migrations during startup.

### 4. Run the Frontend

In another terminal:

```powershell
cd frontend/school-admission-ui
npm install
npm run dev
```

Frontend URL: http://localhost:5173

---

## Frontend Routes

| Route | Access | Purpose |
|---|---|---|
| `/login` | Public | Student login |
| `/register` | Public | Student registration |
| `/admin/login` | Public | Administrator login |
| `/classes` | Student and Admin | Browse classes |
| `/my-applications` | Student | View personal applications |
| `/admin/classes` | Admin | Manage classes |
| `/admin/applications` | Admin | View admission applications |

The frontend uses protected routes to restrict access based on the authenticated user's role. The API independently enforces authorization; frontend route protection is not a replacement for backend security.

---

## Automated Tests

The project includes PostgreSQL integration tests for admission operations, business rules, expiration handling, idempotency, and concurrent seat reservations.

### 1. Prepare a Separate Test Database

Create a dedicated PostgreSQL database:

```sql
CREATE DATABASE school_admission_test;
```

Never point integration tests at a database containing data you need to preserve.

### 2. Configure the Test Connection String

Set `TEST_DATABASE_CONNECTION_STRING` to the test database connection string.

PowerShell example:

```powershell
$env:TEST_DATABASE_CONNECTION_STRING = "Host=localhost;Port=5434;Database=school_admission_test;Username=school_app;Password=YOUR_PASSWORD"
```

Use your actual database credentials and host port.

### 3. Run the Tests

From the repository root:

```powershell
dotnet test
```

For detailed output:

```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Integration Test Coverage

The existing test suite contains 12 PostgreSQL integration tests covering:

- Class creation and seat initialization.
- Seat-capacity enforcement.
- Duplicate application prevention.
- Expired hold confirmation rejection.
- Successful and idempotent confirmation.
- Unauthorized confirmation attempts.
- Successful cancellation and seat release.
- Idempotent cancellation.
- Unauthorized cancellation attempts.
- Prevention of confirmed-admission cancellation.
- Expired hold cancellation rejection.
- Concurrent seat reservation without overselling.

### Concurrency Test

The concurrency integration test submits 10 simultaneous hold requests for a class with 3 available seats.

It verifies that:

- Exactly 3 requests successfully reserve seats.
- The remaining 7 requests receive `409 Conflict`.
- The class does not exceed its configured capacity.
- The invariant `heldSeats + confirmedSeats <= totalSeats` is maintained.

---

## API Reference

### Base URLs

Local:

```text
http://localhost:8080
```

Production:

```text
https://school-admission-system-t609.onrender.com
```

All endpoints use JSON where a request or response body is applicable.

### Endpoint Overview

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/student/register` | Public |
| POST | `/api/auth/student/login` | Public |
| POST | `/api/auth/admin/login` | Public |
| POST | `/api/auth/refresh` | Refresh cookie and allowed origin |
| POST | `/api/auth/logout` | Refresh cookie and allowed origin |
| GET | `/api/auth/me` | Authenticated |
| GET | `/classes` | Student or Admin |
| GET | `/classes/{classId}` | Student or Admin |
| POST | `/classes` | Admin |
| POST | `/classes/{classId}/holds` | Student |
| GET | `/holds/{holdId}` | Student |
| POST | `/holds/{holdId}/confirm` | Student |
| DELETE | `/holds/{holdId}` | Student |
| GET | `/me/holds` | Student |
| GET | `/admin/applications` | Admin |

### Authentication Header

Protected endpoints require:

```http
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Use the access token returned by the login or refresh endpoint.

### 1. Register a Student

**Request**

```http
POST /api/auth/student/register
Content-Type: application/json
```

```json
{
  "fullName": "Example Student",
  "email": "student@example.com",
  "password": "StrongPassword123",
  "confirmPassword": "StrongPassword123",
  "birthRegistrationNumber": "20152691234567890"
}
```

The birth registration number must contain exactly 17 digits.

**Response — `201 Created`**

The response includes a success message, the new user ID, full name, and email.

Duplicate email or birth registration numbers return `409 Conflict`.

### 2. Student Login

**Request**

```http
POST /api/auth/student/login
Content-Type: application/json
```

```json
{
  "email": "student@example.com",
  "password": "StrongPassword123"
}
```

**Response — `200 OK`**

The response contains:

- `accessToken`
- `expiresAtUtc`
- `userId`
- `fullName`
- `email`
- `role`

The refresh token is set separately as an HttpOnly cookie.

### 3. Administrator Login

**Request**

```http
POST /api/auth/admin/login
Content-Type: application/json
```

```json
{
  "email": "admin@example.com",
  "password": "StrongPassword123"
}
```

The response uses the same authentication response structure as student login, with the administrator role.

### 4. Refresh Access Token

**Request**

```http
POST /api/auth/refresh
```

The browser must send the refresh cookie and an allowed `Origin` header. A successful response contains a new access token and rotates the refresh token cookie.

An invalid or expired refresh token returns `401 Unauthorized`.

### 5. Logout

**Request**

```http
POST /api/auth/logout
```

The browser must send the refresh cookie and an allowed `Origin` header.

The API revokes the refresh token and clears the cookie.

### 6. Get Current User

**Request**

```http
GET /api/auth/me
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Returns the authenticated user's ID, name, email, birth registration number, and roles.

### 7. Create a Class

**Request**

```http
POST /classes
Authorization: Bearer ADMIN_ACCESS_TOKEN
Content-Type: application/json
```

```json
{
  "name": "Class 6 - Morning Shift",
  "totalSeats": 120
}
```

**Response — `201 Created`**

```json
{
  "id": "b30f36d1-3f0c-4ce2-9b2c-23fd8e1324a5",
  "name": "Class 6 - Morning Shift",
  "totalSeats": 120
}
```

Only administrators can create classes.

### 8. Get All Classes

**Request**

```http
GET /classes
Authorization: Bearer YOUR_ACCESS_TOKEN
```

**Response — `200 OK`**

```json
[
  {
    "id": "b30f36d1-3f0c-4ce2-9b2c-23fd8e1324a5",
    "name": "Class 6 - Morning Shift",
    "totalSeats": 120,
    "heldSeats": 15,
    "confirmedSeats": 80,
    "availableSeats": 25
  }
]
```

### 9. Get Class Details

**Request**

```http
GET /classes/{classId}
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Returns the class information and current seat breakdown.

### 10. Create a Seat Hold

**Request**

```http
POST /classes/{classId}/holds
Authorization: Bearer STUDENT_ACCESS_TOKEN
```

**Response — `201 Created`**

```json
{
  "holdId": "8b2d1be9-801d-4163-a538-977870d6f166",
  "classId": "b30f36d1-3f0c-4ce2-9b2c-23fd8e1324a5",
  "studentId": "00000000-0000-0000-0000-000000000000",
  "status": "HELD",
  "expiresAt": "2026-11-10T12:00:00Z"
}
```

The `studentId` is the authenticated user's ID. The example GUID and timestamps are illustrative.

A `409 Conflict` response can occur when no seats are available or the student already has an active application for that class.

### 11. Get Hold Status

**Request**

```http
GET /holds/{holdId}
Authorization: Bearer STUDENT_ACCESS_TOKEN
```

**Response — `200 OK`**

```json
{
  "holdId": "8b2d1be9-801d-4163-a538-977870d6f166",
  "classId": "b30f36d1-3f0c-4ce2-9b2c-23fd8e1324a5",
  "status": "HELD",
  "expiresAt": "2026-11-10T12:00:00Z"
}
```

The authenticated student must own the requested hold.

### 12. Confirm Admission

**Request**

```http
POST /holds/{holdId}/confirm
Authorization: Bearer STUDENT_ACCESS_TOKEN
```

**Response — `200 OK`**

```json
{
  "holdId": "8b2d1be9-801d-4163-a538-977870d6f166",
  "status": "CONFIRMED"
}
```

Only the owner of an active, unexpired hold can confirm it. Repeated successful confirmation requests are idempotent.

This project simulates admission confirmation; it does not integrate with an actual payment gateway.

### 13. Cancel a Hold

**Request**

```http
DELETE /holds/{holdId}
Authorization: Bearer STUDENT_ACCESS_TOKEN
```

**Response — `204 No Content`**

Cancellation releases the reserved seat. Confirmed admissions cannot be cancelled.

### 14. Get My Applications

**Request**

```http
GET /me/holds
Authorization: Bearer STUDENT_ACCESS_TOKEN
```

Returns the authenticated student's applications, including class name, status, and expiration information.

### 15. Get All Applications

**Request**

```http
GET /admin/applications
Authorization: Bearer ADMIN_ACCESS_TOKEN
```

Returns admission application information for administrator review.

---

## Admission Hold Lifecycle

```text
                 Student applies
                       |
                       v
                 +-----------+
                 |   HELD    |
                 +-----------+
                   /       \
                  /         \
       Confirmed /           \ Reservation expires
                /             \
               v               v
        +-----------+     +-----------+
        | CONFIRMED |     |  EXPIRED  |
        +-----------+     +-----------+

                 HELD
                   |
                   | Student cancels
                   v
              +-----------+
              | CANCELLED |
              +-----------+
```

The system uses database-level concurrency control to prevent seat allocation beyond the configured class capacity.

---

## Deployment

### Backend — Render

The backend is deployed at:

https://school-admission-system-t609.onrender.com

The backend requires the production database connection string and the appropriate JWT and frontend-origin configuration.

### Frontend — Vercel

The frontend is deployed at:

https://school-admission-system-lemon.vercel.app

Configure `VITE_API_BASE_URL` as a Vercel environment variable with the production backend URL.

The backend's `FRONTEND_ORIGIN` must match the frontend's origin:

```text
https://school-admission-system-lemon.vercel.app
```

Use the origin only; do not append `/classes`.

### Production Notes

- Never commit `.env` files or production credentials.
- Use strong, unique JWT and database secrets.
- Keep the PostgreSQL database persistent.
- Do not expose administrator bootstrap credentials.
- Swagger is configured for Development, not Production.
- The API health endpoint is `/health`.

---

## AI Usage Documentation

For transparency, the project includes a separate document describing how AI assistance supported development.

See [AI_USAGE.md](./AI_USAGE.md).

---

## License

This project was developed as a software engineering assignment demonstrating backend architecture, REST API development, relational database design, authentication, concurrency control, automated integration testing, and containerized deployment.

