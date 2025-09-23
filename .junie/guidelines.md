# CrewQuiz Project Guidelines

## Project Overview

**CrewQuiz** is a real-time multiplayer quiz application built with .NET 8.0 Web API. It enables users to create quizzes, organize questions into groups, and participate in live, turn-based quiz games with competitive features like question "robbing" and point scoring.

### Key Features
- **User Management**: Registration, authentication with JWT tokens
- **Quiz Creation**: Users can create quizzes with organized question groups
- **Real-time Gameplay**: Live multiplayer quiz sessions using SignalR
- **Turn-based System**: Players take turns answering questions
- **Scoring System**: Points awarded for correct answers
- **Game Master Role**: Special user role for managing game sessions
- **Question Robbing**: Competitive feature allowing players to steal questions
- **Session Management**: Game sessions with unique identifiers

### Technology Stack
- **.NET 8.0** Web API with nullable reference types
- **Entity Framework Core** with SQL Server and PostgreSQL support
- **SignalR** for real-time communication
- **JWT Bearer Authentication** for security
- **AutoMapper** for object mapping
- **Serilog** for structured logging
- **Swagger** for API documentation

## Project Structure

```
Backend/
├── Attributes/          # Custom attributes
├── Constants/           # Application constants
├── Controllers/         # Web API controllers
├── Data/               # Data access layer
│   ├── Configurations/ # EF Core entity configurations
│   └── Repositories/   # Repository implementations
├── Enums/              # Enumeration types
├── Extensions/         # Extension methods
├── Handlers/           # Custom handlers
├── Hubs/               # SignalR hubs for real-time communication
├── Interfaces/         # Interface definitions
│   ├── Data/          # Data layer interfaces
│   ├── ServiceUtils/  # Service utility interfaces
│   ├── Services/      # Service interfaces
│   └── Utils/         # Utility interfaces
├── Middlewares/        # Custom middleware
├── Migrations/         # EF Core database migrations
├── Models/            # Data models and DTOs
│   ├── Configurations/ # Model configurations
│   ├── DTOs/          # Data Transfer Objects
│   ├── Domains/       # Domain entities
│   ├── Exceptions/    # Custom exceptions
│   └── Handlers/      # Model handlers
├── Profilers/         # AutoMapper profiles
├── ServiceUtils/      # Service utilities (business logic helpers)
├── Services/          # Business logic services
└── Utils/             # Utility classes
```

### Core Domain Models
- **User**: User accounts with authentication
- **Quiz**: Quiz containers with metadata
- **QuestionGroup**: Organizational units within quizzes
- **Question**: Individual quiz questions with points
- **CurrentGame**: Active game sessions
- **CurrentGameUser**: User participation in games
- **CurrentGameQuestion**: Question states during gameplay

## Development Guidelines

### Architecture Patterns
- **Repository Pattern**: Data access through repositories with Unit of Work
- **Service Layer**: Business logic in services and service utilities
- **DTO Pattern**: Data transfer objects for API communication
- **Hub Pattern**: SignalR hubs for real-time features
- **AuditableEntity**: Base class for entities with creation/modification tracking

### Code Style and Conventions
- Use **nullable reference types** (enabled in project)
- Follow **C# naming conventions**
- Implement **async/await** for all I/O operations
- Use **required** keyword for mandatory properties
- Prefer **collection expressions** `[]` over `new List<>()`
- Use **primary constructors** for dependency injection where appropriate

### Error Handling
- Use **BusinessValidationException** for business logic errors
- Implement proper exception handling in controllers and services
- Log errors using **Serilog** with structured logging

### Real-time Communication
- Use **SignalR GameHub** for real-time game events
- Send events to specific groups using SessionId
- Handle **HubException** appropriately in service utilities

## Testing Guidelines

### Running Tests
- Use standard `dotnet test` command
- Tests should be organized by feature/component
- Always run tests before submitting changes
- Ensure both unit tests and integration tests pass

### Test Categories
- **Unit Tests**: Test individual services and utilities
- **Integration Tests**: Test API endpoints and database operations
- **SignalR Hub Tests**: Test real-time communication features

## Build and Deployment

### Building the Project
- Use `dotnet build` to compile the solution
- Ensure no build warnings or errors
- The project targets .NET 8.0 runtime

### Database Migrations
- Use Entity Framework Core migrations for schema changes
- Run `dotnet ef migrations add <MigrationName>` for new migrations
- Update database with `dotnet ef database update`

### Configuration
- **Development**: Use `appsettings.Development.json`
- **Production**: Configure connection strings and JWT settings
- Support both **SQL Server** and **PostgreSQL** databases

## Development Workflow

1. **Before Making Changes**:
   - Understand the business logic in ServiceUtils and Services
   - Check existing domain models and relationships
   - Review SignalR hub implementations for real-time features

2. **Implementation**:
   - Follow the established repository and service patterns
   - Use appropriate DTOs for data transfer
   - Implement proper validation and error handling
   - Add logging where necessary

3. **Testing**:
   - Write unit tests for new business logic
   - Test API endpoints thoroughly
   - Verify SignalR functionality works correctly
   - Run full test suite before submitting

4. **Quality Assurance**:
   - Build the project to ensure compilation
   - Check for any breaking changes in existing functionality
   - Verify database migrations work correctly

## Special Considerations

- **Game Flow**: The GameFlowServiceUtil manages complex turn-based game logic
- **Real-time Updates**: All game events must be broadcast via SignalR
- **Data Consistency**: Use Unit of Work pattern for transactional operations
- **Authentication**: JWT tokens required for authenticated endpoints
- **Session Management**: Games are identified by unique SessionId values
