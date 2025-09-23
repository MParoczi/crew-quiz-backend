# CrewQuiz

A real-time multiplayer quiz application built with .NET 8.0 Web API, featuring live gameplay, turn-based question answering, competitive question "robbing", and comprehensive game session management.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [API Documentation](#api-documentation)
- [Real-time Communication](#real-time-communication)
- [Database Schema](#database-schema)
- [Testing](#testing)
- [Development Guidelines](#development-guidelines)
- [Contributing](#contributing)
- [License](#license)

## Overview

**CrewQuiz** is a sophisticated real-time multiplayer quiz application that enables users to create custom quizzes, organize questions into groups, and participate in competitive live quiz sessions. The application features a turn-based system where players can answer questions, "rob" questions from other players, and compete for points in real-time game sessions.

### Key Highlights

- **Real-time Multiplayer**: Live quiz sessions using SignalR with up to multiple concurrent players
- **Turn-based Gameplay**: Structured game flow with player turns and question selection
- **Question Robbing**: Unique competitive feature allowing players to steal questions from others
- **Session Management**: Secure game sessions with unique identifiers and state management
- **Comprehensive Testing**: Extensive test suite covering integration, unit, and SignalR testing
- **Production Ready**: Built with enterprise-grade patterns and practices

## Features

### Core Features

- **User Management**
  - User registration and authentication
  - JWT-based security with token management
  - User profile management
  - Password hashing and security

- **Quiz Creation & Management**
  - Create custom quizzes with metadata
  - Organize questions into logical groups
  - Question management with point values
  - Quiz sharing and reuse across games

- **Real-time Multiplayer Gameplay**
  - Live game sessions with SignalR
  - Turn-based question answering
  - Real-time score tracking and updates
  - Session-based player management

- **Advanced Game Mechanics**
  - **Question Robbing**: Players can steal questions from others
  - **Game Master Role**: Special privileges for session management
  - **Next Player Selection**: Strategic turn management
  - **Game Completion**: Automatic scoring and session closure

- **Session & Data Management**
  - Automatic session cleanup (configurable intervals)
  - Game state persistence
  - Previous game history tracking
  - Audit trails with creation/modification tracking

## Technology Stack

### Backend Framework
- **.NET 8.0** - Web API with nullable reference types
- **C# 12** - Latest language features and syntax

### Database & ORM
- **Entity Framework Core 8.0** - ORM with Code First approach
- **PostgreSQL** - Primary database (production)
- **SQL Server** - Alternative database support
- **Database Migrations** - Schema versioning and updates

### Real-time Communication
- **SignalR** - WebSocket-based real-time communication
- **Connection Groups** - Session-based message broadcasting

### Security & Authentication
- **JWT Bearer Authentication** - Stateless token-based security
- **ASP.NET Core Authorization** - Role and policy-based access control
- **Password Hashing** - Secure credential storage

### Additional Libraries
- **AutoMapper** - Object-to-object mapping
- **Serilog** - Structured logging with enrichers
- **Swagger/OpenAPI** - API documentation and testing
- **Moq** - Mocking framework for testing

### Development Tools
- **Entity Framework Tools** - Migration and database management
- **Visual Studio/Rider** - Development environment
- **PowerShell** - Build and deployment scripts

## Architecture

### Design Patterns

- **Repository Pattern** - Data access abstraction with Unit of Work
- **Service Layer Pattern** - Business logic separation
- **DTO Pattern** - Data transfer object mapping
- **Dependency Injection** - Loose coupling and testability
- **Hub Pattern** - SignalR real-time communication

### Project Structure

```
Backend/
├── Controllers/         # Web API controllers
├── Hubs/               # SignalR hubs for real-time communication
├── Services/           # Business logic services
├── ServiceUtils/       # Business logic utilities and helpers
├── Data/               # Data access layer
│   ├── Repositories/   # Repository implementations
│   └── Configurations/ # EF Core entity configurations
├── Models/             # Data models and DTOs
│   ├── Domains/        # Domain entities
│   ├── DTOs/           # Data Transfer Objects
│   └── Exceptions/     # Custom exceptions
├── Interfaces/         # Interface definitions
├── Middlewares/        # Custom middleware components
├── Extensions/         # Extension methods
├── Utils/              # Utility classes
├── Profilers/          # AutoMapper profiles
├── Attributes/         # Custom attributes
├── Constants/          # Application constants
└── Enums/              # Enumeration types

CrewQuiz.Tests/
├── Integration/        # End-to-end integration tests
├── Foundation/         # Core functionality tests
├── GameSession/        # Game flow and session tests
├── SignalRTesting/     # Real-time communication tests
├── DataManagement/     # Data operations tests
├── EnvironmentSetup/   # Test infrastructure
└── ContentCreation/    # Quiz and content tests
```

### Core Domain Models

- **User** - User accounts with authentication and relationships
- **Quiz** - Quiz containers with metadata
- **QuestionGroup** - Organizational units within quizzes
- **Question** - Individual quiz questions with points and content
- **CurrentGame** - Active game sessions with state management
- **CurrentGameUser** - User participation in active games
- **CurrentGameQuestion** - Question states during gameplay
- **PreviousGame** - Completed game history
- **AuditableEntity** - Base class with creation/modification tracking

## Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 12+](https://www.postgresql.org/download/) (or SQL Server)
- [Git](https://git-scm.com/downloads)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CrewQuiz/Backend/CrewQuiz
   ```

2. **Setup Database**
   ```bash
   # Create PostgreSQL database
   createdb crew_quiz
   
   # Run migrations
   dotnet ef database update
   ```

3. **Configure Application**
   ```bash
   # Copy and configure settings
   cp Backend/appsettings.json Backend/appsettings.Development.json
   # Edit appsettings.Development.json with your settings
   ```

4. **Install Dependencies**
   ```bash
   dotnet restore
   ```

5. **Run Application**
   ```bash
   dotnet run --project Backend
   ```

6. **Access Application**
   - API: `https://localhost:5001` (or configured port)
   - Swagger UI: `https://localhost:5001/swagger`
   - SignalR Hub: `https://localhost:5001/crew-quiz`

## Configuration

### Database Connection

```json
{
  "ConnectionStrings": {
    "CrewQuiz": "Host=localhost:5432; Database=crew_quiz; Username=your_user; Password=your_password; Include Error Detail=true"
  }
}
```

### JWT Authentication

```json
{
  "AppSettings": {
    "Jwt": {
      "Secret": "your-secret-key-here",
      "ExpirationInMinutes": 60,
      "Issuer": "https://localhost:5001",
      "Audience": "http://localhost:3000"
    }
  }
}
```

### CORS Configuration

```json
{
  "AppSettings": {
    "Cors": {
      "AllowedOrigins": "http://localhost:3000,http://localhost:5173"
    }
  }
}
```

### Session Management

```json
{
  "AppSettings": {
    "SessionCleanup": {
      "IntervalHours": 6,
      "SessionTimeoutHours": 24
    }
  }
}
```

### Logging Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/crewquiz-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

## Deployment

### Docker Deployment

The application is containerized using Docker and optimized for deployment on cloud platforms like Render.com, Heroku, or any Docker-compatible hosting service.

#### Building the Docker Image

```bash
# Build the Docker image
docker build -t crew-quiz-backend .

# Run the container locally (optional)
docker run -p 8080:8080 \
  -e ConnectionStrings__CrewQuiz="your-postgres-connection-string" \
  -e AppSettings__Jwt__Secret="your-secret-key-minimum-32-characters" \
  crew-quiz-backend
```

#### Dockerfile Features

- **Multi-stage build**: Optimized for smaller production images
- **Security**: Runs as non-root user
- **Production-ready**: Uses official .NET 8.0 runtime
- **Health checks**: Exposed health endpoint at `/api/health`

### Render.com Deployment

The application includes a `render.yaml` configuration file for easy deployment to Render.com.

#### Prerequisites

1. **PostgreSQL Database**: Create a PostgreSQL database on Render.com
2. **GitHub Repository**: Your code must be in a GitHub repository
3. **Environment Variables**: Configure the required environment variables

#### Deployment Steps

1. **Fork or Clone** this repository to your GitHub account

2. **Create a PostgreSQL Database** on Render.com:
   - Database Name: `crew_quiz`
   - User: `crew_quiz_user`
   - Plan: Choose appropriate plan (starter for development)

3. **Create a Web Service** on Render.com:
   - Connect your GitHub repository
   - Use the provided `render.yaml` configuration
   - Or manually configure:
     - **Environment**: Docker
     - **Dockerfile Path**: `./Dockerfile`
     - **Health Check Path**: `/api/health`

4. **Configure Environment Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   ConnectionStrings__CrewQuiz=[Auto-filled from database]
   AppSettings__Environment=Production
   AppSettings__Cors__AllowedOrigins=https://your-frontend-domain.com
   AppSettings__Jwt__Secret=[Generate a secure secret]
   AppSettings__Jwt__ExpirationInMinutes=60
   AppSettings__Jwt__Issuer=https://your-app.onrender.com
   AppSettings__Jwt__Audience=https://your-frontend-domain.com
   AppSettings__SessionCleanup__IntervalHours=6
   AppSettings__SessionCleanup__SessionTimeoutHours=24
   ```

5. **Deploy**: Render.com will automatically build and deploy your application

#### render.yaml Configuration

The included `render.yaml` file provides Infrastructure-as-Code deployment configuration:

```yaml
services:
  - type: web
    name: crew-quiz-backend
    env: docker
    dockerfilePath: ./Dockerfile
    healthCheckPath: /api/health
    # Database connection and environment variables are configured
```

### Other Cloud Providers

#### Heroku

```bash
# Install Heroku CLI and login
heroku login

# Create a new Heroku app
heroku create your-app-name

# Add PostgreSQL addon
heroku addons:create heroku-postgresql:hobby-dev

# Set environment variables
heroku config:set ASPNETCORE_ENVIRONMENT=Production
heroku config:set AppSettings__Jwt__Secret="your-secure-secret-key"

# Deploy using Docker
heroku container:push web
heroku container:release web
```

#### Azure Container Instances

```bash
# Create resource group
az group create --name crew-quiz-rg --location eastus

# Create container instance
az container create \
  --resource-group crew-quiz-rg \
  --name crew-quiz-backend \
  --image your-registry/crew-quiz-backend:latest \
  --dns-name-label crew-quiz-unique \
  --ports 8080 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__CrewQuiz="your-connection-string"
```

### Database Migrations

For production deployment, ensure database migrations are applied:

```bash
# Using Entity Framework CLI
dotnet ef database update --project Backend

# Or via code (automatically on startup)
# The application will apply pending migrations on startup
```

### Health Monitoring

The application provides a comprehensive health check endpoint at `/api/health` that monitors:

- **Database connectivity**
- **System resources**
- **Application components**

Health check response example:
```json
{
  "status": "Healthy",
  "version": "1.0.0",
  "environment": "Production",
  "database": {
    "status": "Healthy",
    "connectionCount": 5
  }
}
```

### Security Considerations

- **JWT Secret**: Generate a secure random secret (minimum 32 characters)
- **CORS**: Configure allowed origins for your frontend domains
- **Database**: Use connection strings with restricted user permissions
- **HTTPS**: Ensure HTTPS is enforced in production
- **Environment Variables**: Never commit secrets to source control

## API Documentation

### Authentication Endpoints

#### Register User
```http
POST /api/User/Register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "password": "SecurePassword123"
}
```

#### Login
```http
POST /api/User/Login
Content-Type: application/json

{
  "username": "johndoe",
  "password": "SecurePassword123"
}
```

### Game Flow Endpoints

All game flow endpoints require JWT authentication via `Authorization: Bearer <token>` header.

#### Join Game Session
```http
POST /api/GameFlow/AddUserToCurrentGame
Content-Type: application/json
Authorization: Bearer <jwt-token>

{
  "sessionId": "unique-session-id",
  "userId": 123
}
```

#### Start Game
```http
POST /api/GameFlow/StartGame
Content-Type: application/json
Authorization: Bearer <jwt-token>

{
  "sessionId": "unique-session-id"
}
```

#### Select Question
```http
POST /api/GameFlow/SelectQuestion
Content-Type: application/json
Authorization: Bearer <jwt-token>

{
  "sessionId": "unique-session-id",
  "questionId": 456
}
```

#### Submit Answer
```http
POST /api/GameFlow/SubmitAnswer
Content-Type: application/json
Authorization: Bearer <jwt-token>

{
  "sessionId": "unique-session-id",
  "questionId": 456,
  "answer": "The correct answer"
}
```

#### Rob Question
```http
POST /api/GameFlow/RobQuestion
Content-Type: application/json
Authorization: Bearer <jwt-token>

{
  "sessionId": "unique-session-id",
  "questionId": 456,
  "targetUserId": 789
}
```

### Quiz Management Endpoints

#### Create Quiz
```http
POST /api/Quiz/Create
Content-Type: application/json
Authorization: Bearer <jwt-token>

{
  "name": "My Awesome Quiz",
  "questionGroups": [
    {
      "name": "General Knowledge",
      "questions": [
        {
          "text": "What is the capital of France?",
          "answer": "Paris",
          "points": 10
        }
      ]
    }
  ]
}
```

## Real-time Communication

### SignalR Hub Connection

The application uses SignalR for real-time communication. Connect to the hub at `/crew-quiz` endpoint.

#### JavaScript Client Example

```javascript
// Establish connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/crew-quiz", {
        accessTokenFactory: () => localStorage.getItem("jwt-token")
    })
    .build();

// Join game session
await connection.start();
await connection.invoke("JoinGame", "session-id", userId);

// Listen for game events
connection.on("GameStarted", (data) => {
    console.log("Game started:", data);
});

connection.on("QuestionSelected", (data) => {
    console.log("Question selected:", data);
});

connection.on("AnswerSubmitted", (data) => {
    console.log("Answer submitted:", data);
});

connection.on("QuestionRobbed", (data) => {
    console.log("Question robbed:", data);
});

// Leave game session
await connection.invoke("LeaveGame", "session-id", userId);
```

#### Hub Methods

- `JoinGame(sessionId, userId)` - Join a game session
- `LeaveGame(sessionId, userId)` - Leave a game session

#### Hub Events

The server sends these events to connected clients:

- `GameStarted` - Game session has started
- `PlayerJoined` - New player joined the session
- `PlayerLeft` - Player left the session
- `QuestionSelected` - Question was selected for answering
- `AnswerSubmitted` - Answer was submitted by a player
- `QuestionRobbed` - Question was robbed by another player
- `NextPlayerSelected` - Next player's turn was determined
- `GameCompleted` - Game session completed with final scores

## Database Schema

### Key Relationships

```
User (1) ←→ (N) Quiz
User (1) ←→ (N) CurrentGameUser
Quiz (1) ←→ (N) QuestionGroup
QuestionGroup (1) ←→ (N) Question
CurrentGame (1) ←→ (N) CurrentGameUser
CurrentGame (1) ←→ (N) CurrentGameQuestion
```

### Entity Framework Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Generate SQL script
dotnet ef migrations script
```

## Testing

The project includes comprehensive test coverage across multiple categories:

### Test Categories

- **Integration Tests** - Full workflow and end-to-end scenarios
- **Foundation Tests** - User management and core functionality
- **Game Session Tests** - Game flow, player interactions, and state management
- **SignalR Tests** - Real-time communication and hub functionality
- **Data Management Tests** - Repository operations and data consistency
- **Environment Setup Tests** - Configuration and infrastructure

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Integration"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CompleteGameFlowTests"
```

### Test Infrastructure

- **TestBase** - Common test infrastructure and setup
- **Mock Services** - Comprehensive mocking for isolated testing
- **SignalR Testing** - Custom testing framework for real-time features
- **Database Testing** - In-memory and test database support
- **Connection Simulation** - Multi-client testing scenarios

### Writing Tests

```csharp
public class MyFeatureTests : TestBase
{
    [Test]
    public async Task Should_PerformAction_WithValidInput()
    {
        // Arrange
        var testUser = await CreateTestUserAsync();
        var gameSession = await CreateTestGameAsync();

        // Act
        var result = await _service.PerformAction(gameSession.SessionId);

        // Assert
        Assert.That(result, Is.Not.Null);
        // Add your assertions
    }
}
```

## Development Guidelines

### Code Style

- Follow **C# naming conventions**
- Use **nullable reference types** (enabled project-wide)
- Implement **async/await** for all I/O operations
- Use **required** keyword for mandatory properties
- Prefer **collection expressions** `[]` over `new List<>()`
- Use **primary constructors** for dependency injection

### Architecture Patterns

- **Repository Pattern** for data access
- **Service Layer** for business logic
- **DTO Pattern** for API communication
- **Unit of Work** for transactional operations
- **Dependency Injection** throughout the application

### Error Handling

```csharp
// Use BusinessValidationException for business logic errors
throw new BusinessValidationException("Game has already started");

// Log errors with structured logging
_logger.LogError("Failed to process game action for session {SessionId}", sessionId);
```

### SignalR Guidelines

```csharp
// Send events to specific game session
await _hubContext.Clients.Group(sessionId).SendAsync("EventName", eventData);

// Handle hub exceptions appropriately
try 
{
    await _hubContext.Clients.Group(sessionId).SendAsync("EventName", data);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to broadcast event to session {SessionId}", sessionId);
    throw new HubException("Failed to send real-time update");
}
```

## Contributing

### Getting Started

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes following the development guidelines
4. Write tests for your changes
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

### Development Workflow

1. **Before Making Changes**
   - Understand the business logic in ServiceUtils and Services
   - Check existing domain models and relationships
   - Review SignalR hub implementations for real-time features

2. **Implementation**
   - Follow established repository and service patterns
   - Use appropriate DTOs for data transfer
   - Implement proper validation and error handling
   - Add structured logging where necessary

3. **Testing**
   - Write unit tests for new business logic
   - Test API endpoints thoroughly
   - Verify SignalR functionality works correctly
   - Run full test suite before submitting

4. **Quality Assurance**
   - Build the project to ensure compilation
   - Check for breaking changes in existing functionality
   - Verify database migrations work correctly

### Pull Request Guidelines

- Provide clear description of changes
- Include test coverage for new features
- Update documentation if needed
- Ensure CI/CD pipeline passes
- Request review from maintainers

## License

This project is private and proprietary. All rights reserved.

---

## Support

For questions, issues, or contributions:

- Create an issue for bugs or feature requests
- Join discussions for general questions
- Check documentation for setup and usage help

**Built with ❤️ using .NET 8.0 and modern development practices**