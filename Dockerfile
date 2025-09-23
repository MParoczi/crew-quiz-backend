# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the solution file and project files
COPY CrewQuiz.sln ./
COPY Backend/Backend.csproj ./Backend/
COPY CrewQuiz.Tests/CrewQuiz.Tests.csproj ./CrewQuiz.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application in Release mode
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish Backend/Backend.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 8.0 runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Change ownership of the app directory to the appuser
RUN chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "Backend.dll"]