# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and project files for dependency restoration
COPY CrewQuiz.sln ./
COPY Backend/Backend.csproj Backend/
COPY CrewQuiz.Tests/CrewQuiz.Tests.csproj CrewQuiz.Tests/

# Restore NuGet packages
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /src/Backend
RUN dotnet build "Backend.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8.0 runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' --shell /bin/bash --uid 1001 appuser

# Set the working directory
WORKDIR /app

# Create logs directory and set permissions
RUN mkdir -p /app/logs && chown -R appuser:appuser /app/logs

# Copy the published application
COPY --from=publish /app/publish .

# Change ownership of the application files to the non-root user
RUN chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Set environment variables for render.com
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:$PORT
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Expose the port (render.com will set the PORT environment variable)
EXPOSE $PORT

# Set the entry point
ENTRYPOINT ["dotnet", "Backend.dll"]