# CrewQuiz Backend - Deployment Guide

This guide provides step-by-step instructions for deploying the CrewQuiz backend to various cloud platforms.

## 🐳 Docker Deployment

### Quick Start

1. **Build the Docker image:**
   ```bash
   ./scripts/docker-build.sh
   ```

2. **Or build manually:**
   ```bash
   docker build -t crew-quiz-backend .
   ```

### Docker Configuration

The application is configured to:
- Run on port 8080
- Use environment variables for configuration
- Include health checks at `/api/health`
- Run as non-root user for security

## ☁️ Render.com Deployment

### Prerequisites

- GitHub account with your forked repository
- Render.com account

### Step-by-Step Deployment

1. **Create a Database:**
   - Go to [Render Dashboard](https://dashboard.render.com)
   - Click "New +" → "PostgreSQL"
   - Name: `crew-quiz-db`
   - Region: Choose your preferred region
   - Plan: Starter (for development) or Professional (for production)
   - Click "Create Database"

2. **Create Web Service:**
   - Click "New +" → "Web Service"
   - Connect your GitHub account
   - Select your repository
   - Configure:
     - **Name:** `crew-quiz-backend`
     - **Environment:** Docker
     - **Region:** Same as your database
     - **Branch:** `main`
     - **Dockerfile Path:** `./Dockerfile`

3. **Environment Variables:**
   Configure these in the Render dashboard:

   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   ConnectionStrings__CrewQuiz=[Copy from your database info]
   AppSettings__Environment=Production
   AppSettings__Cors__AllowedOrigins=https://your-frontend-domain.com
   AppSettings__Jwt__Secret=[Generate a secure 32+ character secret]
   AppSettings__Jwt__ExpirationInMinutes=60
   AppSettings__Jwt__Issuer=https://your-app-name.onrender.com
   AppSettings__Jwt__Audience=https://your-frontend-domain.com
   AppSettings__SessionCleanup__IntervalHours=6
   AppSettings__SessionCleanup__SessionTimeoutHours=24
   ```

4. **Advanced Settings:**
   - **Health Check Path:** `/api/health`
   - **Auto-Deploy:** Yes (recommended)

5. **Deploy:**
   - Click "Create Web Service"
   - Render will automatically build and deploy your application

### Using render.yaml (Infrastructure as Code)

Alternative method using the included `render.yaml`:

1. In your Render dashboard, click "New +" → "Blueprint"
2. Connect your repository
3. Render will automatically detect and use the `render.yaml` configuration
4. Update the environment variables as needed

## 🚀 Other Cloud Platforms

### Heroku

```bash
# Install Heroku CLI
# Create app
heroku create your-app-name

# Add PostgreSQL
heroku addons:create heroku-postgresql:hobby-dev

# Set environment variables
heroku config:set ASPNETCORE_ENVIRONMENT=Production
heroku config:set AppSettings__Jwt__Secret="your-secure-secret"

# Deploy
heroku container:push web
heroku container:release web
```

### Railway

1. Connect your GitHub repository to Railway
2. Add PostgreSQL database
3. Configure environment variables
4. Deploy automatically

### Azure Container Instances

```bash
az container create \
  --resource-group crew-quiz-rg \
  --name crew-quiz-backend \
  --image your-registry/crew-quiz-backend:latest \
  --ports 8080 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__CrewQuiz="your-connection-string"
```

## 🔧 Configuration

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ASPNETCORE_URLS` | Binding URLs | `http://+:8080` |
| `ConnectionStrings__CrewQuiz` | Database connection | `Host=...;Database=...` |
| `AppSettings__Jwt__Secret` | JWT signing secret | Min 32 characters |
| `AppSettings__Cors__AllowedOrigins` | Frontend domains | `https://yourapp.com` |

### Optional Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `AppSettings__Jwt__ExpirationInMinutes` | `60` | Token expiration time |
| `AppSettings__SessionCleanup__IntervalHours` | `6` | Cleanup interval |
| `AppSettings__SessionCleanup__SessionTimeoutHours` | `24` | Session timeout |

## 🔒 Security Considerations

### JWT Secret Generation

Generate a secure secret:

```bash
# Using OpenSSL
openssl rand -base64 32

# Using PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# Online generator (ensure it's from a trusted source)
# Use a password manager to generate a 32+ character secret
```

### Database Security

- Use restricted database user permissions
- Enable SSL/TLS for database connections
- Regularly update database credentials
- Monitor database access logs

### CORS Configuration

```json
{
  "AppSettings": {
    "Cors": {
      "AllowedOrigins": "https://myapp.com,https://www.myapp.com"
    }
  }
}
```

## 📊 Monitoring and Health Checks

### Health Endpoint

The application provides a comprehensive health check at `/api/health`:

```bash
curl https://your-app.onrender.com/api/health
```

Response example:
```json
{
  "status": "Healthy",
  "version": "1.0.0",
  "environment": "Production",
  "database": {
    "status": "Healthy",
    "connectionCount": 5
  },
  "system": {
    "status": "Healthy",
    "memoryUsageMB": 128
  }
}
```

### Logging

Logs are written to:
- Console (for container platforms)
- File system (if persistent storage is available)

Configure log levels in production:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

## 🐛 Troubleshooting

### Common Issues

1. **Container fails to start:**
   - Check environment variables
   - Verify database connection string
   - Review application logs

2. **Health check fails:**
   - Ensure database is accessible
   - Check JWT secret configuration
   - Verify port binding (8080)

3. **CORS errors:**
   - Update `AllowedOrigins` configuration
   - Ensure HTTPS is used in production

4. **Database connection errors:**
   - Verify connection string format
   - Check database firewall settings
   - Ensure database is running

### Debug Commands

```bash
# View container logs
docker logs <container-id>

# Test health endpoint
curl -v http://localhost:8080/api/health

# Check environment variables
docker exec <container-id> env

# Connect to database (PostgreSQL)
psql "your-connection-string"
```

## 📚 Additional Resources

- [.NET 8 Deployment Guide](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [Docker Best Practices](https://docs.docker.com/develop/best-practices/)
- [Render.com Documentation](https://render.com/docs)
- [Entity Framework Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

## 💬 Support

If you encounter issues:

1. Check the [troubleshooting section](#troubleshooting)
2. Review application logs
3. Verify environment variables
4. Open an issue in the GitHub repository with:
   - Deployment platform
   - Error messages
   - Configuration details (without secrets)