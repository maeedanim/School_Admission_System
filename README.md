# School Admission System

A robust, high-concurrency online school admission system built with **.NET 10 (C#)**, **PostgreSQL**, and **React**. 

The system operates strictly on a **First-Come, First-Served (FCFS)** basis without a lottery. When admission opens and high volumes of parents apply concurrently, applicants can temporarily hold a seat in a specific class and shift. Upon timely payment of the admission fee, the seat is confirmed; otherwise, expired holds are automatically made available to other applicants.

---

## 📋 Table of Contents

- [Features](#features)
- [Architecture & Tech Stack](#architecture--tech-stack)
- [Project Structure](#project-structure)
- [System Requirements & Business Rules](#system-requirements--business-rules)
- [Environment Configuration](#environment-configuration)
- [Quick Start with Docker](#quick-start-with-docker)
- [Running Locally (Development Mode)](#running-locally-development-mode)
- [Running Automated Tests](#running-automated-tests)
- [API Specification](#api-specification)

---

## ✨ Features

- **Class & Shift Management:** Admin can configure classes with dedicated shift capacities (e.g., `Class 6 - Morning Shift`, `Class 6 - Day Shift`).
- **High-Concurrency Seat Holds:** Strict prevention of overbooking under heavy parallel bursts via database transactions and row-level locking.
- **Time-Limited Seat Reservation:** Seats remain on hold for a configurable window (`HOLD_DURATION_SECONDS`, default `600s`). Expired seats are released automatically without requiring manual triggers.
- **Student Identification:** Uses a 17-digit National Birth Registration Number (`X-Student-Id` header) without requiring complex authentication.
- **One Active Seat Rule:** Enforces at most one active application (`HELD` or `CONFIRMED`) per student per class.
- **Idempotent Confirmations:** Re-submitting payment confirmation for an already confirmed hold does not double-count seats.
- **Persistent Storage:** Fully persistent PostgreSQL volume across container restarts (`docker compose down` & `docker compose up`).

---

## 🏗️ Architecture & Tech Stack

### Backend
- **Framework:** .NET 10 / ASP.NET Core Web API
- **Data Access:** Entity Framework Core with Npgsql (PostgreSQL provider)
- **Database:** PostgreSQL 17
- **Architecture:** N-Layer / Clean separation:
  - `SchoolAdmission.API`: Controllers, middlewares, dependency injection, and HTTP pipeline.
  - `SchoolAdmission.BLL`: Business logic, services, DTOs, and domain validation.
  - `SchoolAdmission.DAL`: EF Core `DbContext`, entities, configurations, and database migrations.

### Frontend
- **Framework:** React 18+ (Vite)
- **Language:** TypeScript / JavaScript
- **Features:** Auto-polling seat status, countdown timer for active holds, streamlined payment confirmation simulation, and persistent student session.

### Infrastructure & Orchestration
- **Containerization:** Docker & Multi-stage `Dockerfile`
- **Orchestration:** `docker-compose.yml` for unified one-command execution.

---

## 📁 Project Structure

```text
School_Admission_System/
├── .env.example                     # Sample environment configuration
├── docker-compose.yml               # Multi-container orchestration (DB, API, Frontend)
├── README.md                        # Documentation and run instructions
├── SchoolAdmissionSystem.sln        # .NET Solution file
│
├── backend/
│   ├── SchoolAdmission.API/         # API Layer (Controllers, Config, Program.cs)
│   ├── SchoolAdmission.BLL/         # Business Logic Layer (Services, DTOs, Validation)
│   └── SchoolAdmission.DAL/         # Data Access Layer (EF Core, Entities, Migrations)
│
├── frontend/                        # React (Vite) application
│   ├── src/
│   ├── package.json
│   └── Dockerfile
│
└── tests/
    └── SchoolAdmission.Tests/       # Unit & Concurrency Integration Tests
```

---

## ⚙️ Environment Configuration

Copy `.env.example` to `.env` in the repository root:

```bash
cp .env.example .env
```

| Variable | Description | Default |
|---|---|---|
| `POSTGRES_DB` | PostgreSQL database name | `school_admission` |
| `POSTGRES_USER` | PostgreSQL username | `school_app` |
| `POSTGRES_PASSWORD` | PostgreSQL password | `replace_with_a_local_password` |
| `POSTGRES_PORT` | Exposed port for PostgreSQL on host | `5432` |
| `HOLD_DURATION_SECONDS` | Duration in seconds before a hold expires | `600` |
| `ASPNETCORE_ENVIRONMENT` | Backend environment (`Development` / `Production`) | `Development` |
| `VITE_API_URL` | Base URL of the backend API used by frontend | `http://localhost:8080` |

---

## 🚀 Quick Start with Docker

The entire stack (PostgreSQL, Backend API, and Frontend UI) is configured to run with a single Docker Compose command. No local SDKs are required.

### 1. Start All Services

```bash
docker compose up --build
```

To run in detached (background) mode:
```bash
docker compose up --build -d
```

### 2. Access the Application

- **Frontend UI:** [http://localhost:3000](http://localhost:3000) (or `http://localhost:5173`)
- **Backend API:** [http://localhost:8080](http://localhost:8080)
- **API Health Check:** [http://localhost:8080/health](http://localhost:8080/health)

### 3. Stop Services

```bash
docker compose down
```

> **Data Persistence:** Data is retained inside the `postgres_data` Docker volume. Re-running `docker compose up` will preserve all existing classes and admission records.

---

## 💻 Running Locally (Development Mode)

If you wish to run the backend and frontend locally outside Docker:

### 1. Start PostgreSQL
You can start only the database using Docker:
```bash
docker compose up -d postgres
```

### 2. Run Backend (.NET)
Apply database migrations and start the API:
```bash
cd backend/SchoolAdmission.API
dotnet run
```
The backend will be available at `http://localhost:8080`.

### 3. Run Frontend (React)
```bash
cd frontend
npm install
npm run dev
```

---

## 🧪 Running Automated Tests

The solution includes automated unit tests and high-concurrency race condition tests to guarantee seat reservation invariants under load.

Run tests using the .NET CLI:

```bash
dotnet test
```

To run with detailed test logs:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Concurrency Test Guarantee
The test suite includes a **parallel concurrency test**:
- Dispatches a burst of concurrent seat-hold requests exceeding the total capacity of a class.
- Verifies that strictly `totalSeats` are held and subsequent requests receive `409 Conflict`.
- Verifies that `heldSeats + confirmedSeats <= totalSeats` is never violated.

---

## 🔌 API Specification

All endpoints communicate via JSON. For student endpoints, the applicant's 17-digit birth registration number must be passed in the `X-Student-Id` header.

### Endpoints Overview

| # | Method | Path | Description | Required Header |
|---|---|---|---|---|
| 1 | `POST` | `/classes` | Create a new class with total seats | — |
| 2 | `GET` | `/classes` | List all classes with seat counts | — |
| 3 | `GET` | `/classes/{classId}` | Get details and seat breakdown of a class | — |
| 4 | `POST` | `/classes/{classId}/holds` | Apply / Hold a seat | `X-Student-Id: <17-digit-id>` |
| 5 | `GET` | `/holds/{holdId}` | Get current status of a hold | — |
| 6 | `POST` | `/holds/{holdId}/confirm` | Confirm admission (submit admission fee) | `X-Student-Id: <17-digit-id>` |
| 7 | `DELETE` | `/holds/{holdId}` | Cancel hold / admission | `X-Student-Id: <17-digit-id>` |
| 8 | `GET` | `/me/holds` | Get all applications of current student | `X-Student-Id: <17-digit-id>` |

### Sample Payloads & Responses

#### 1. Create Class
`POST /classes`
```json
{
  "name": "Class 6 - Morning Shift",
  "totalSeats": 120
}
```
**Response (201 Created):**
```json
{
  "id": "c_123",
  "name": "Class 6 - Morning Shift",
  "totalSeats": 120
}
```

#### 2. Get Classes
`GET /classes`
**Response (200 OK):**
```json
[
  {
    "id": "c_123",
    "name": "Class 6 - Morning Shift",
    "totalSeats": 120,
    "heldSeats": 15,
    "confirmedSeats": 80,
    "availableSeats": 25
  }
]
```

#### 3. Seat Hold Request
`POST /classes/{classId}/holds`  
**Header:** `X-Student-Id: 20152691234567890`  
**Response (201 Created):**
```json
{
  "holdId": "h_789",
  "classId": "c_123",
  "studentId": "20152691234567890",
  "status": "HELD",
  "expiresAt": "2026-11-01T04:10:00Z"
}
```
*(Returns `409 Conflict` if no seats are available or if the student already has an active hold/confirmed seat).*

#### 4. Confirm Admission
`POST /holds/{holdId}/confirm`  
**Header:** `X-Student-Id: 20152691234567890`  
**Response (200 OK):**
```json
{
  "holdId": "h_789",
  "status": "CONFIRMED"
}
```
