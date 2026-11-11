# Assumptions

This document describes the assumptions and implementation decisions made while developing the School Admission System.

## 1. Student Identification

- Each student is identified by a unique 17-digit Birth Registration Number (BRN).
- The BRN is collected during student registration.
- The current implementation uses JWT authentication to identify students and authorize student operations.
- Student identity is derived from the authenticated account rather than trusting a client-provided student identifier.

**Note:** The assignment specifies using the `X-Student-Id` header without requiring login. The current implementation uses JWT authentication instead. This is an implementation difference that should be considered when evaluating assignment compliance.

## 2. Class Management

- Each admission class has a name and a fixed number of available seats.
- Only administrators can create admission classes.
- Class availability is calculated using total seats, currently held seats, and confirmed seats.
- A class cannot have more held and confirmed seats combined than its total capacity.

## 3. Temporary Seat Reservations

- A student can temporarily hold a seat before confirming an application.
- The default hold duration is 600 seconds (10 minutes).
- The hold duration can be configured through the `HOLD_DURATION_SECONDS` environment variable.
- An expired hold releases its reserved seat.
- A student cannot have multiple active or confirmed applications for the same class.

## 4. Application Confirmation

- Confirmation uses a simulated payment process.
- No real payment gateway or financial transaction is involved.
- A successfully confirmed application retains its seat allocation.
- Repeated confirmation requests should not create duplicate applications or reserve additional seats.

## 5. Cancellation

- Students can cancel their temporary holds.
- Cancelling a hold releases the temporarily reserved seat.
- A cancelled or expired hold cannot be used to confirm an application.

## 6. Concurrency and Seat Availability

- Multiple students may attempt to reserve seats simultaneously.
- The backend and database are responsible for enforcing seat capacity.
- Seat allocation must prevent overbooking, including during concurrent requests.

## 7. Data Persistence

- PostgreSQL is used as the primary database.
- Application data is expected to persist across application and container restarts.
- Docker Compose uses a named PostgreSQL volume for persistent storage.

## 8. Authentication and Authorization

- Student and administrator accounts use separate application roles.
- Passwords are stored as hashes rather than plain text.
- Administrative operations are restricted to users with the administrator role.
- Student operations are restricted to authenticated student accounts.

## 9. Deployment Configuration

- Environment variables are used for database credentials, JWT configuration, administrator bootstrap settings, and hold duration.
- Sensitive configuration values are not committed to the public repository.
- The application is designed to run locally through Docker Compose.
