# Use the official .NET 9 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 7001

# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["src/TemplateBackend.API/TemplateBackend.API.csproj", "src/TemplateBackend.API/"]
RUN dotnet restore "src/TemplateBackend.API/TemplateBackend.API.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/TemplateBackend.API"

# Build the application
RUN dotnet build "TemplateBackend.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TemplateBackend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the final image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "TemplateBackend.API.dll"] 