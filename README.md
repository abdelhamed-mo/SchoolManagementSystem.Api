# School Management System API

A comprehensive **School Management System** built with **ASP.NET Core Web API**, designed to manage academic and administrative operations through a layered architecture.

The system provides centralized management of students, teachers, parents, classes, subjects, academic years, attendance, examinations, grades, and dynamic school timetables.

---

## Overview

The School Management System aims to simplify school administration by providing a structured backend platform for managing educational data and daily academic activities.

It follows a layered architecture that separates business logic, data access, presentation, and shared contracts, making the application easier to maintain, extend, and test.

One of its key features is an advanced timetable management module that supports reusable base timetables, independent weekly schedules, lesson modifications, holiday handling, and change history.

## Features

### 1. User and Role Management
- User management using ASP.NET Core Identity.
- User registration and login.
- Role-based user organization.
- JWT token generation.

### 2. Student Management
- Manage student information.
- Associate students with grades and classes.
- Manage student enrollments.
- Track student attendance and academic results.

### 3. Teacher Management
- Manage teacher information.
- Associate teachers with subjects.
- Associate teachers with classes.
- Track teaching assignments and subject qualifications.

### 4. Academic Structure
- Manage academic years.
- Manage semesters within academic years.
- Manage grades and classes.
- Define subjects assigned to classes.
- Manage student enrollments across academic years.

### 5. Attendance Management
- Record student attendance.
- Associate attendance records with students and classes.
- Retrieve attendance information.

### 6. Examination and Grade Management
- Manage examinations.
- Associate exams with subjects, classes, grades, and semesters.
- Store student examination results.
- Retrieve academic performance information.

### 7. Timetable Management

The timetable module provides a structured approach to creating and managing school schedules.

**Base Timetable**
- Configure the school day starting time.
- Define lesson durations.
- Configure break periods and their positions.
- Assign subjects and qualified teachers to specific days and periods.
- Maintain a reusable timetable for each class.

**Weekly Schedule**
- Generate weekly schedules from the base timetable.
- Store independent weekly snapshots.
- Preserve existing schedules when the base timetable changes.
- Represent daily periods and breaks.
- Retrieve weekly schedules by class and week.

**Schedule Modifications**
- Add new lessons.
- Update scheduled lessons.
- Cancel lessons.
- Validate teacher qualifications and class-subject assignments.
- Prevent overlapping teacher schedules.
- Restrict modifications to the current week's schedule.

**Holiday Management**
- Define holiday date ranges.
- Handle scheduled lessons affected by holidays.
- Record automatic lesson cancellation events.

**Schedule Change History**
- Maintain a history of lesson changes.
- Store previous and updated lesson information.
- Record the operation type, source, reason, and timestamp.

---

## Architecture

The solution follows a **Layered Architecture** with clear separation of responsibilities.

```text
SchoolManagementSystem
│
├── Core
│   ├── Domain
│   │   ├── Entities
│   │   ├── Contracts
│   │   └── Exceptions
│   │
│   ├── ServiceAbstraction
│   │   └── Service Interfaces
│   │
│   └── Service
│       ├── Services
│       ├── ConcreteSpecifications
│       └── MappingProfiles
│
├── Infrastructure
│   ├── Persistence
│   │   ├── MainContext
│   │   ├── Contracts
│   │   ├── Migrations
│   │   └── Data Seeding
│   │
│   └── Presentation
│       └── Controllers
│
├── Shared
│   └── DTOs and Shared Models
│
└── SchoolSystem.Api
    ├── Program.cs
    ├── Middlewares
    └── Application Configuration
```

### Application Flow

```text
HTTP Request
     |
     v
Controller
     |
     v
IServiceManager
     |
     v
Service Layer
     |
     v
Unit of Work
     |
     v
Generic Repository
     |
     v
Entity Framework Core
     |
     v
SQL Server
```

### Design Patterns

The project uses the following architectural patterns:

- **Generic Repository Pattern:** Provides reusable data access operations.
- **Unit of Work Pattern:** Coordinates database operations and transactions.
- **Specification Pattern:** Encapsulates query conditions and reusable filtering logic.
- **Service Layer Pattern:** Keeps business logic separate from controllers.
- **Dependency Injection:** Manages application dependencies.
- **DTO Pattern:** Separates API request/response models from domain entities.

---

## Technology Stack

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| .NET 10 | Application platform |
| ASP.NET Core Web API | REST API development |
| Entity Framework Core | Object-relational mapping |
| SQL Server | Relational database |
| ASP.NET Core Identity | User management |
| JWT | Token generation |
| AutoMapper | Object mapping |
| Swagger / OpenAPI | API documentation |

---

## Getting Started

### Prerequisites

Make sure you have the following installed:

- .NET 10 SDK
- SQL Server
- Visual Studio 2022 or a compatible IDE
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/abdelhamed-mo/SchoolManagementSystem.Api.git
```

```bash
cd SchoolManagementSystem.Api
```

### 2. Configure the Database

Update the connection string in `SchoolSystem.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultSqlConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  }
}
```

For production environments, use environment variables or a secure secrets provider instead of committing credentials to source control.

### 3. Configure JWT Settings

Configure the JWT options required by the authentication service:

```json
{
  "JwtOptions": {
    "SecretKey": "YOUR_SECRET_KEY",
    "Issure": "YOUR_ISSUER",
    "Audience": "YOUR_AUDIENCE",
    "DurationInDays": 50
  }
}
```

Keep production signing keys outside the repository.

### 4. Apply Database Migrations

From the solution directory, run:

```bash
dotnet ef database update
```

If the startup project or migrations assembly requires explicit configuration, specify the appropriate project paths.

### 5. Run the Application

```bash
dotnet run --project SchoolSystem.Api
```

### 6. Explore the API

Once the application is running, open the Swagger UI:

```text
https://localhost:<port>/swagger
```

Use Swagger to explore and test the available API endpoints.

---

## Data Access and Business Logic

The application uses a centralized service manager to expose application services.

Controllers delegate requests to the service layer, while services interact with repositories through the Unit of Work abstraction.

This approach helps maintain separation of concerns and keeps data access logic out of controllers.

## Background Processing

The application includes a hosted service for weekly schedule generation.

The generation process checks the active academic year and semester, identifies eligible classes, and generates the required weekly schedules through the weekly scheduling service.

---

## Project Goals

- Centralize school administrative operations.
- Organize academic data and relationships.
- Simplify student and teacher management.
- Improve attendance and examination tracking.
- Automate weekly timetable generation.
- Maintain historical records of schedule changes.
- Provide a maintainable and extensible backend architecture.

---

## Repository

**GitHub:** [abdelhamed-mo/SchoolManagementSystem.Api](https://github.com/abdelhamed-mo/SchoolManagementSystem.Api)

---

## Author

**Abdelhamed Mohamed**

Full Stack .NET Developer

Email:
abdelhamed.dev@gmail.com

LinkedIn:
https://www.linkedin.com/in/abdelhamed-mo

GitHub:
https://github.com/abdelhamed-mo

---

*This project is continuously evolving as new features and improvements are introduced.*
