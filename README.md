
---

# WorkFlow API

WorkFlow is a backend system for task and project management, inspired by tools like Trello and Jira.
The system follows a hierarchical structure:

Workspace → Board → List → Card → Task / SubTask

The project is built with ASP.NET Core 8 and applies Clean Architecture, CQRS, and role-based access control with real-time collaboration support.

---

## Architecture Overview

The solution follows Clean Architecture with clear separation of concerns:

```
Directory structure:
└── nguyentheanh1424-workflow/
    ├── README.md
    ├── WorkFlow.API/
    │   ├── Program.cs
    │   ├── Controllers/
    │   ├── Hubs/
    │   ├── Middleware/
    │   └── Services/
    ├── WorkFlow.Application/
    │   ├── Common/
    │   │   ├── Behaviors/
    │   │   ├── Cache/
    │   │   ├── Constants/
    │   │   │   └── EventNames/
    │   │   ├── Exceptions/
    │   │   ├── Interfaces/
    │   │   │   ├── Auth/
    │   │   │   ├── Repositories/
    │   │   │   └── Services/
    │   │   ├── Mappings/
    │   │   ├── Models/
    │   │   └── Services/
    │   └── Features/
    │       ├── Attachments/
    │       │   ├── Commands/
    │       │   ├── Dtos/
    │       │   └── Queries/
    │       ├── Authentication/
    │       ├── BoardMembers/
    │       ├── Boards/
    │       ├── CardAssignees/
    │       ├── Cards/
    │       ├── Comments/
    │       ├── InviteLinks/
    │       ├── Lists/
    │       ├── SubTasks/
    │       ├── Tasks/
    │       ├── Users/
    │       ├── WorkSpaceMembers/
    │       └── WorkSpaces/
    ├── WorkFlow.Domain/
    │   ├── Common/
    │   │   └── Helpers/
    │   ├── Entities/
    │   ├── Enums/
    │   ├── Events/
    │   ├── Exceptions/
    │   └── ValueObjects/
    └── WorkFlow.Infrastructure/
        ├── DependencyInjection.cs
        ├── Auth/
        ├── Persistence/
        ├── Repositories/
        └── Services/
```

Request flow:

```
Controller → MediatR Command / Query → Handler → Domain / Infrastructure
```

---

## Features

### Authentication and Authorization

* User registration with email OTP verification
* Login, refresh token, logout
* Forgot password and reset password via OTP
* JWT authentication
* Role-based authorization

Workspace roles:

* Owner
* Admin
* Member

Board roles:

* Owner
* Editor
* Viewer

---

### Workspace

* Create, update, delete workspace
* View workspace details
* Manage workspace members and roles
* Invite links to join workspace or board

---

### Board

* Create, update, duplicate, archive, restore board
* Manage board members and permissions
* Board visibility (Public, Private, Protected)
* Labels, background, pinned boards
* Real-time updates via SignalR

---

### List

* Create list in board
* Rename and reorder lists (drag and drop)
* Archive and unarchive lists
* Clone list
* Move lists and cards between boards

---

### Card

* Create, update, move, delete (soft delete)
* Restore deleted cards
* Assign and remove users
* Labels, status, dates, reminders
* Comments
* Attachments (upload and delete)

---

### Task and SubTask

* Tasks act as checklist groups inside a card
* SubTasks with status, due date, reminder
* Reorder tasks and subtasks

---

### Real-time Communication (SignalR)

SignalR hubs:

* BoardHub
* WorkspaceHub
* UserHub

Clients can join groups:

```
board:{boardId}
ws:{workspaceId}
user:{userId}
```

Used for real-time updates such as card movement, comments, assignments, and notifications.

---
## Real-time Event Examples

* Board.Created
* Board.Updated
* Card.Created
* Card.Moved
* Comment.Added
* Comment.Deleted
* Workspace.Member.Added
* Workspace.Member.Removed

---

## Technology Stack

* .NET 8
* ASP.NET Core Web API
* MediatR (CQRS)
* FluentValidation
* AutoMapper
* SignalR
* JWT Bearer Authentication
* Swagger / OpenAPI
* Background Services
* Docker
---

## Run Locally

### Requirements

* .NET SDK 8.0 or higher
* Visual Studio 2022 or VS Code

### Run the application

```
dotnet restore
dotnet run --project WorkFlow.API
```

The API will be available at:

```
http://localhost:5174
```

Swagger UI:

```
http://localhost:5174/swagger
```

---

## Run with Docker

Build image:

```
docker build -t workflow-api .
```

Run container:

```
docker run -p 8080:8080 workflow-api
```

---

## Deployment

The project includes a render.yaml file for Render.com deployment.

Basic configuration:

```
runtime: dotnet
buildCommand: dotnet restore && dotnet publish
startCommand: dotnet WorkFlow.API.dll
```

Deployment is triggered automatically on push.


## Coding Conventions

* CQRS with separate Command and Query objects
* No business logic in controllers
* Validation handled by FluentValidation
* Permission checks in Application layer
* Centralized exception handling via middleware
* Clear separation between layers

---

## License

This project is intended for learning, demonstration, and internal use.

