# School Admission System

A high-concurrency online school admission system built with **.NET 10 (ASP.NET Core Web API)**, **PostgreSQL 17**, and **React + TypeScript**.

The system follows a First-Come, First-Served (FCFS) admission model without a lottery. Applicants can temporarily reserve a seat in a class by creating a time-limited hold. When the admission fee is confirmed within the reservation period, the seat becomes permanently confirmed. Otherwise, the expired reservation is automatically released and becomes available to another applicant.

## Live Demo

- **Frontend:** https://school-admission-system-lemon.vercel.app/classes
- **Backend API:** https://school-admission-system-t609.onrender.com
- **API Health Check:** https://school-admission-system-t609.onrender.com/health

> The frontend is hosted on Vercel, while the backend API and PostgreSQL can also run locally through Docker Compose.

---

## Table of Contents

- [Features](#features)
- [Architecture and Technology Stack](#architecture-and-technology-stack)
- [Project Structure](#project-structure)
- [System Requirements and Business Rules](#system-requirements-and-business-rules)
- [Environment Configuration](#environment-configuration)
- [Quick Start with Docker](#quick-start-with-docker)
- [Running Locally in Development Mode](#running-locally-in-development-mode)
- [Running Automated Tests](#running-automated-tests)
- [API Specification](#api-specification)
- [Admission Hold Lifecycle](#admission-hold-lifecycle)

---

## Features

- **Class Management:** Create classes with configurable seat capacities. Different shifts can be represented as separate classes, such as `Class 6 - Morning Shift` and `Class 6 - Day Shift`.
- **Concurrency-Safe Seat Allocation:** Prevents overbooking through PostgreSQL transactions, row-level locking, and database constraints.
- **Time-Limited Seat Reservations:** Applicants receive a configurable reservation period through `HOLD_DURATION_SECONDS` (default: 600 seconds).
- **Automatic Hold Expiration:** Expired holds are released through database operations without requiring a background worker or manual intervention.
- **Student Identification:** Uses a 17-digit student birth registration number through the `X-Student-Id` HTTP header.
- **Duplicate Application Prevention:** A student can have at most one active hold or confirmed admission per class.
- **Idempotent Confirmation:** Repeated confirmation requests for an already confirmed admission do not double-count seats.
- **Admission Cancellation:** Active holds can be cancelled, releasing their reserved seats. Confirmed admissions cannot be cancelled.
- **Real-Time Seat Availability:** The frontend periodically refreshes class availability.
- **Live Reservation Countdown:** Applicants can monitor the remaining time on their active reservations.
- **Persistent Storage:** PostgreSQL data survives container restarts through a Docker named volume.

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
| Swagger / OpenAPI | API documentation during development |
| xUnit | Automated integration testing |

### Three-Tier Architecture

The backend follows a three-tier architecture with clear separation of responsibilities.

**1. Presentation Layer — `SchoolAdmission.API`**

- REST API controllers
- HTTP request and response handling
- Dependency injection
- Middleware and exception handling
- CORS configuration

**2. Business Logic Layer — `SchoolAdmission.BLL`**

- Admission business rules
- Service interfaces and implementations
- Request and response DTOs
- Input validation
- Business exceptions and status handling

**3. Data Access Layer — `SchoolAdmission.DAL`**

- Entity Framework Core DbContext
- Database entities and configurations
- Repository implementations
- PostgreSQL queries and transactions
- Database migrations

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

- Docker
- Docker Compose
- Multi-stage Dockerfile for the backend API
- PostgreSQL named volume for persistent storage
- Render for backend hosting
- Vercel for frontend hosting

---

## Project Structure

```text
School_Admission_System/
│
├── .env.example
├── .gitignore
├── Dockerfile
├── docker-compose.yml
├── README.md
├── SchoolAdmissionSystem.sln
│
├── backend/
│   ├── SchoolAdmission.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   │
│   ├── SchoolAdmission.BLL/
│   │   ├── DTOs/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   └── Services/
│   │
│   └── SchoolAdmission.DAL/
│       ├── Data/
│       ├── Entities/
│       ├── Repositories/
│       └── Migrations/
│
├── frontend/
│   └── school-admission-ui/
│       ├── public/
│       ├── src/
│       │   ├── app/
│       │   ├── components/
│       │   ├── features/
│       │   ├── hooks/
│       │   ├── services/
│       │   ├── types/
│       │   └── utils/
│       ├── .env.example
│       ├── package.json
│       └── vercel.json
│
└── tests/
    └── SchoolAdmission.Tests/
        ├── AdmissionIntegrationTests.cs
        └── PostgresTestFixture.cs
```

---

## System Requirements and Business Rules

### System Requirements

For Docker-based execution:

- Docker Desktop
- Docker Compose

For local development:

- .NET 10 SDK
- Node.js and npm
- Docker Desktop (for PostgreSQL)
- Git

### Business Rules

1. **Seat Capacity:** A class cannot have more active reservations and confirmed admissions than its total seat capacity.

2. **Reservation Duration:** A newly created hold remains active for the configured duration. The default is 600 seconds.

3. **Hold Expiration:** An expired hold cannot be confirmed. Its reserved seat becomes available again.

4. **Student Identification:** Student IDs must contain exactly 17 ASCII digits.

5. **Duplicate Applications:** A student cannot create another active hold or confirmed admission for the same class.

6. **Confirmation:** Only the student who owns an active, unexpired hold can confirm it.

7. **Cancellation:** Students can cancel their own active holds. Confirmed admissions cannot be cancelled.

8. **Idempotency:** Repeating a successful confirmation does not increase the confirmed seat count.

9. **Concurrency:** PostgreSQL row-level locking and transactions serialize competing seat-allocation operations for the same class.

10. **Persistence:** Application data remains available after API restarts and Docker container restarts.

### Admission Statuses

| Status | Description |
|---|---|
| `HELD` | Seat temporarily reserved |
| `CONFIRMED` | Admission successfully confirmed |
| `EXPIRED` | Reservation period elapsed |
| `CANCELLED` | Applicant cancelled an active reservation |

---

## Environment Configuration

### 1. Backend and PostgreSQL Configuration

Create a root `.env` file using `.env.example` as a template.

**Windows CMD:**

```cmd
copy .env.example .env
```

**PowerShell / Bash:**

```bash
cp .env.example .env
```

Configure the following variables:

| Variable | Description | Example |
|---|---|---|
| `POSTGRES_DB` | PostgreSQL database name | `school_admission` |
| `POSTGRES_USER` | PostgreSQL username | `school_app` |
| `POSTGRES_PASSWORD` | PostgreSQL password | Set your own password |
| `POSTGRES_PORT` | PostgreSQL host port | `5432` |
| `HOLD_DURATION_SECONDS` | Hold duration in seconds | `600` |

The provided Docker Compose configuration uses `600` seconds as the API hold duration.

**Important:** Keep `.env` private. Never commit database passwords or other secrets to GitHub.

### 2. Frontend Configuration

The frontend uses Vite environment variables.

Create or configure the following files inside `frontend/school-admission-ui/`.

**`.env.development`**

```env
VITE_API_BASE_URL=http://localhost:8080
```

**`.env.production`**

```env
VITE_API_BASE_URL=https://school-admission-system-t609.onrender.com
```

**`.env.example`**

```env
VITE_API_BASE_URL=http://localhost:8080
```

Vite reads these variables at build time. Restart the development server after changing environment files.

---

## Quick Start with Docker

Docker Compose runs the PostgreSQL database and ASP.NET Core API together.

The React frontend is run separately during local development or accessed through the deployed Vercel application.

### 1. Clone the Repository

```bash
git clone https://github.com/maeedanim/School_Admission_System.git
cd School_Admission_System
```

### 2. Configure Environment Variables

**Windows CMD:**

```cmd
copy .env.example .env
```

Open `.env` and configure your PostgreSQL password.

### 3. Build and Start the Services

```bash
docker compose up --build -d
```

This command:

- Builds the backend API Docker image.
- Starts PostgreSQL.
- Waits for PostgreSQL's health check.
- Starts the API.
- Applies pending EF Core migrations automatically.

### 4. Verify the Containers

```bash
docker compose ps
```

Both services should be running, and PostgreSQL should report a healthy status.

### 5. Access the API

- **Backend API:** http://localhost:8080
- **Health Check:** http://localhost:8080/health
- **Classes Endpoint:** http://localhost:8080/classes

The API's Swagger UI is enabled in the Development environment.

### 6. Run the Frontend

Open another terminal:

```bash
cd frontend/school-admission-ui
npm install
npm run dev
```

Open:

http://localhost:5173

### 7. View Container Logs

```bash
docker compose logs -f api
```

For PostgreSQL:

```bash
docker compose logs -f postgres
```

### 8. Stop the Services

```bash
docker compose down
```

**Data Persistence:** Running `docker compose down` preserves the PostgreSQL named volume. Existing classes and admission records remain available when the services are started again.

> Avoid `docker compose down -v` unless you intentionally want to delete the database volume and all stored data.

---

## Running Locally in Development Mode

You can run the backend and frontend outside Docker while using Docker for PostgreSQL.

### 1. Start PostgreSQL

From the repository root:

```bash
docker compose up -d postgres
```

### 2. Configure the Backend Connection String

The backend reads its database connection string from:

`ConnectionStrings:DefaultConnection`

For local development, configure it through .NET User Secrets or an appropriate local environment variable.

Example connection string:

```text
Host=localhost;Port=5434;Database=school_admission;Username=school_app;Password=YOUR_PASSWORD
```

Use the host port configured in your root `.env` file. For example, the port above assumes `POSTGRES_PORT=5434`.

### 3. Run the Backend

From the repository root:

```bash
dotnet run --project backend/SchoolAdmission.API/SchoolAdmission.API.csproj --urls http://localhost:8080
```

The API applies pending database migrations during application startup.

Backend URL:

http://localhost:8080

### 4. Run the Frontend

Open another terminal:

```bash
cd frontend/school-admission-ui
npm install
npm run dev
```

Frontend URL:

http://localhost:5173

---

## Running Automated Tests

The project includes PostgreSQL integration tests covering admission operations, business rules, expiration handling, idempotency, and concurrent seat reservations.

### 1. Prepare the Test Database

Create a separate PostgreSQL database for testing:

```sql
CREATE DATABASE school_admission_test;
```

The tests must use a separate database from the application's development database.

### 2. Configure the Test Connection String

Set the `TEST_DATABASE_CONNECTION_STRING` environment variable to point to the test database.

Example Windows CMD command:

```cmd
set TEST_DATABASE_CONNECTION_STRING=Host=localhost;Port=5434;Database=school_admission_test;Username=school_app;Password=YOUR_PASSWORD
```

Use your actual PostgreSQL host port and credentials.

### 3. Run the Tests

From the repository root:

```bash
dotnet test
```

For detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Integration Test Coverage

The test suite includes 12 PostgreSQL integration tests covering:

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

### Concurrency Test Guarantee

The concurrency integration test sends 10 simultaneous hold requests for a class with only 3 seats.

It verifies that:

- Exactly 3 requests successfully reserve seats.
- The remaining 7 requests are rejected with `409 Conflict`.
- No more than 3 seats are held or confirmed.
- The invariant `heldSeats + confirmedSeats <= totalSeats` is maintained.

---

## API Specification

All endpoints use JSON request and response bodies.

Student-specific endpoints require the applicant's 17-digit birth registration number in the `X-Student-Id` HTTP header, except `GET /holds/{holdId}`, which does not require a student header.

### Base URLs

Local:

```text
http://localhost:8080
```

Production:

```text
https://school-admission-system-t609.onrender.com
```

### Endpoints Overview

| # | Method | Endpoint | Description | Student Header |
|---|---|---|---|---|
| 1 | `POST` | `/classes` | Create a class | No |
| 2 | `GET` | `/classes` | List classes and seat availability | No |
| 3 | `GET` | `/classes/{classId}` | Get class details | No |
| 4 | `POST` | `/classes/{classId}/holds` | Create a seat hold | Yes |
| 5 | `GET` | `/holds/{holdId}` | Get hold status | No |
| 6 | `POST` | `/holds/{holdId}/confirm` | Confirm admission | Yes |
| 7 | `DELETE` | `/holds/{holdId}` | Cancel an active hold | Yes |
| 8 | `GET` | `/me/holds` | Get the current student's applications | Yes |

### Student Header Example

```http
X-Student-Id: 20152691234567890
```

The ID must contain exactly 17 digits.

### 1. Create a Class

**Request**

```http
POST /classes
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

### 2. Get All Classes

**Request**

```http
GET /classes
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

### 3. Get Class Details

**Request**

```http
GET /classes/{classId}
```

Returns the class information and current seat breakdown.

### 4. Create a Seat Hold

**Request**

```http
POST /classes/{classId}/holds
X-Student-Id: 20152691234567890
```

**Response — `201 Created`**

```json
{
  "holdId": "8b2d1be9-801d-4163-a538-977870d6f166",
  "classId": "b30f36d1-3f0c-4ce2-9b2c-23fd8e1324a5",
  "studentId": "20152691234567890",
  "status": "HELD",
  "expiresAt": "2026-11-10T12:00:00Z"
}
```

A `409 Conflict` response is returned if no seats are available or the student already has an active application for that class.

### 5. Get Hold Status

**Request**

```http
GET /holds/{holdId}
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

No student header is required for this endpoint.

### 6. Confirm Admission

**Request**

```http
POST /holds/{holdId}/confirm
X-Student-Id: 20152691234567890
```

**Response — `200 OK`**

```json
{
  "holdId": "8b2d1be9-801d-4163-a538-977870d6f166",
  "status": "CONFIRMED"
}
```

Confirmation is only allowed for the owner of an active, unexpired hold.

Repeated confirmation requests are idempotent.

> This project simulates admission-fee confirmation. It does not integrate with an actual payment gateway.

### 7. Cancel a Hold

**Request**

```http
DELETE /holds/{holdId}
X-Student-Id: 20152691234567890
```

**Response — `204 No Content`**

Cancellation releases the reserved seat. Confirmed admissions cannot be cancelled.

### 8. Get My Applications

**Request**

```http
GET /me/holds
X-Student-Id: 20152691234567890
```

**Response — `200 OK`**

```json
[
  {
    "holdId": "8b2d1be9-801d-4163-a538-977870d6f166",
    "classId": "b30f36d1-3f0c-4ce2-9b2c-23fd8e1324a5",
    "className": "Class 6 - Morning Shift",
    "status": "CONFIRMED",
    "expiresAt": "2026-11-10T12:00:00Z"
  }
]
```

---

## Admission Hold Lifecycle

```text
        Applicant applies
               |
               v
        +-------------+
        |    HELD     |
        +-------------+
          /         \
         /           \
 Payment confirmed   Reservation expires
       |                     |
       v                     v
 +-------------+      +-------------+
 |  CONFIRMED  |      |   EXPIRED   |
 +-------------+      +-------------+

       HELD
         |
         | Applicant cancels
         v
   +-------------+
   |  CANCELLED  |
   +-------------+
```

The system ensures that a seat is never allocated beyond its configured capacity, even when multiple applicants submit requests concurrently.

---

## License

This project was developed as a software engineering assignment demonstrating backend architecture, REST API development, relational database design, concurrency control, automated integration testing, and containerized deployment.