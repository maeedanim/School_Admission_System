# AI Usage Documentation

## Project

School Admission System

## Purpose

This document describes how AI assistance was used during the development process and how the resulting work was reviewed and validated.

## Areas Where AI Assistance Was Used

AI assistance was used as a development support tool in the following areas:

### 1. Architecture and Technical Planning

- Discussing the separation of responsibilities between the API, business logic, and data access layers.
- Reviewing backend design decisions and application workflows.
- Exploring approaches to database consistency and concurrent seat allocation.

### 2. Backend Development and Debugging

- Discussing ASP.NET Core, Entity Framework Core, PostgreSQL, and authentication implementation details.
- Investigating errors and unexpected application behavior.
- Reviewing API endpoint behavior, authorization requirements, and configuration.
- Exploring improvements to maintainability and error handling.

### 3. Database and Concurrency

- Discussing relational data modeling and database constraints.
- Reviewing transaction and row-level locking concepts.
- Planning integration-test scenarios for admission capacity and concurrent requests.

### 4. Frontend and API Integration

- Discussing frontend-to-backend communication.
- Reviewing authentication flows and role-based route protection.
- Investigating API configuration and deployment-related integration issues.

### 5. Testing and Documentation

- Reviewing test coverage and concurrency test expectations.
- Identifying outdated documentation and discrepancies between the documented API and implemented controllers.
- Assisting with project setup instructions, API reference material, and deployment documentation.

## Developer Responsibilities

AI-generated suggestions were treated as assistance rather than authoritative implementation decisions.

The developer remained responsible for:

- Understanding the application requirements and business rules.
- Reviewing suggested code and technical approaches.
- Integrating changes into the project.
- Checking implementation behavior against the actual source code.
- Running builds and tests and investigating failures.
- Validating configuration and deployment behavior.
- Protecting credentials, environment variables, and other sensitive information.

## Verification

Project changes were evaluated using the available development workflow, including source-code inspection, .NET builds, PostgreSQL integration tests, Docker Compose validation, and API health checks where applicable.

A successful build or health check does not independently prove that every application workflow is correct. Feature-level behavior must be assessed through appropriate tests and application verification.

## Limitations

AI assistance can produce incorrect, incomplete, or outdated suggestions. Recommendations were therefore reviewed against the actual codebase and project requirements before being used.

AI assistance does not replace developer understanding, code ownership, testing, or responsibility for the final submission.

## Declaration

AI tools were used to support planning, troubleshooting, technical review, and documentation during development. The developer is responsible for reviewing the submitted implementation and ensuring that it accurately represents the completed project.
