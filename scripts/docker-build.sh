#!/bin/bash

# CrewQuiz Backend - Docker Build and Test Script
# This script builds the Docker image and optionally tests it locally

set -e  # Exit on any error

echo "🚀 CrewQuiz Backend - Docker Build Script"
echo "=========================================="

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Error: Docker is not installed or not in PATH"
    echo "Please install Docker and try again"
    exit 1
fi

# Set default values
IMAGE_NAME="crew-quiz-backend"
TAG="latest"
BUILD_ONLY=false
PUSH_REGISTRY=""

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --image-name)
            IMAGE_NAME="$2"
            shift 2
            ;;
        --tag)
            TAG="$2"
            shift 2
            ;;
        --build-only)
            BUILD_ONLY=true
            shift
            ;;
        --push-to)
            PUSH_REGISTRY="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --image-name NAME    Set the image name (default: crew-quiz-backend)"
            echo "  --tag TAG            Set the image tag (default: latest)"
            echo "  --build-only         Only build the image, don't run tests"
            echo "  --push-to REGISTRY   Push the image to specified registry"
            echo "  --help, -h           Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                                    # Build and test locally"
            echo "  $0 --build-only                      # Just build the image"
            echo "  $0 --push-to myregistry.com          # Build and push to registry"
            echo "  $0 --image-name my-quiz --tag v1.0   # Custom name and tag"
            exit 0
            ;;
        *)
            echo "❌ Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

FULL_IMAGE_NAME="${IMAGE_NAME}:${TAG}"
if [ ! -z "$PUSH_REGISTRY" ]; then
    FULL_IMAGE_NAME="${PUSH_REGISTRY}/${IMAGE_NAME}:${TAG}"
fi

echo "📋 Configuration:"
echo "   Image Name: $FULL_IMAGE_NAME"
echo "   Build Only: $BUILD_ONLY"
echo "   Push Registry: ${PUSH_REGISTRY:-"none"}"
echo ""

# Build the Docker image
echo "🔨 Building Docker image..."
echo "   Command: docker build -t $FULL_IMAGE_NAME ."
echo ""

if docker build -t "$FULL_IMAGE_NAME" .; then
    echo "✅ Docker image built successfully: $FULL_IMAGE_NAME"
else
    echo "❌ Docker build failed"
    exit 1
fi

# Get image size
IMAGE_SIZE=$(docker images "$FULL_IMAGE_NAME" --format "table {{.Size}}" | tail -n 1)
echo "📦 Image size: $IMAGE_SIZE"

if [ "$BUILD_ONLY" = true ]; then
    echo "🏁 Build completed (build-only mode)"
    exit 0
fi

# Test the image locally (basic health check)
echo ""
echo "🧪 Testing the Docker image..."

# Start container in background
CONTAINER_ID=$(docker run -d -p 8080:8080 \
    -e ConnectionStrings__CrewQuiz="Host=localhost;Database=test;Username=test;Password=test" \
    -e AppSettings__Jwt__Secret="test-secret-key-for-docker-testing-minimum-32-characters" \
    -e AppSettings__Environment="Production" \
    "$FULL_IMAGE_NAME")

echo "   Container started: $CONTAINER_ID"

# Wait for container to be ready
echo "   Waiting for application to start..."
sleep 10

# Check if container is still running
if docker ps -q --filter id="$CONTAINER_ID" | grep -q .; then
    echo "✅ Container is running successfully"
    
    # Test health endpoint (if accessible)
    if command -v curl &> /dev/null; then
        echo "   Testing health endpoint..."
        if curl -f http://localhost:8080/api/health >/dev/null 2>&1; then
            echo "✅ Health endpoint responding"
        else
            echo "⚠️  Health endpoint not accessible (this is expected without a database)"
        fi
    fi
else
    echo "❌ Container failed to start or exited"
    echo "   Container logs:"
    docker logs "$CONTAINER_ID"
fi

# Cleanup
echo "🧹 Cleaning up test container..."
docker stop "$CONTAINER_ID" >/dev/null 2>&1 || true
docker rm "$CONTAINER_ID" >/dev/null 2>&1 || true

# Push to registry if specified
if [ ! -z "$PUSH_REGISTRY" ]; then
    echo ""
    echo "📤 Pushing to registry: $PUSH_REGISTRY"
    if docker push "$FULL_IMAGE_NAME"; then
        echo "✅ Image pushed successfully"
    else
        echo "❌ Failed to push image"
        echo "   Make sure you're logged in: docker login $PUSH_REGISTRY"
        exit 1
    fi
fi

echo ""
echo "🎉 All done!"
echo "   Image: $FULL_IMAGE_NAME"
echo "   Ready for deployment to Render.com or other platforms"
echo ""
echo "💡 Next steps:"
echo "   1. Push your code to GitHub"
echo "   2. Create a new Web Service on Render.com"
echo "   3. Connect your GitHub repository"
echo "   4. Configure environment variables"
echo "   5. Deploy!"