# SQL Server Connection Guide

This guide provides instructions for connecting to SQL Server in different environments using various tools.

## 🗄️ Environment Overview

### Local Development (LocalDB)
- **Server**: `(localdb)\mssqllocaldb`
- **Database**: `TemplateBackendDb`
- **Authentication**: Windows Authentication (Trusted Connection)
- **Port**: Default (no port needed)

### Docker Environment
- **Server**: `localhost` or `sqlserver` (from container)
- **Database**: `TemplateBackendDb`
- **Authentication**: SQL Server Authentication
- **Username**: `sa`
- **Password**: `YourStrong@Passw0rd`
- **Port**: `1433`

## 🔧 Connection Methods

## 1. Using MSSQL Command Line (sqlcmd)

### Local Development (LocalDB)
```bash
# Connect to LocalDB
sqlcmd -S "(localdb)\mssqllocaldb" -E

# Connect to specific database
sqlcmd -S "(localdb)\mssqllocaldb" -E -d TemplateBackendDb

# Execute query directly
sqlcmd -S "(localdb)\mssqllocaldb" -E -Q "SELECT name FROM sys.databases"
```

### Docker Environment
```bash
# Connect to Docker SQL Server from host
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd"

# Connect to specific database
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -d TemplateBackendDb

# Execute query directly
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -Q "SELECT name FROM sys.databases"

# Connect from inside Docker container
docker exec -it template_backend_sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"
```

## 2. Using PowerShell with SqlServer Module

### Install SqlServer Module
```powershell
# Install SqlServer PowerShell module
Install-Module -Name SqlServer -Force -AllowClobber

# Import the module
Import-Module SqlServer
```

### Local Development (LocalDB)
```powershell
# Connect and execute query
Invoke-Sqlcmd -ServerInstance "(localdb)\mssqllocaldb" -Database "TemplateBackendDb" -Query "SELECT name FROM sys.tables"

# Connect with integrated security
$connection = "Server=(localdb)\mssqllocaldb;Database=TemplateBackendDb;Trusted_Connection=true;"
Invoke-Sqlcmd -ConnectionString $connection -Query "SELECT @@VERSION"
```

### Docker Environment
```powershell
# Connect to Docker SQL Server
Invoke-Sqlcmd -ServerInstance "localhost,1433" -Username "sa" -Password "YourStrong@Passw0rd" -Database "TemplateBackendDb" -Query "SELECT name FROM sys.tables"

# Using connection string
$connection = "Server=localhost,1433;Database=TemplateBackendDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
Invoke-Sqlcmd -ConnectionString $connection -Query "SELECT @@VERSION"
```

## 3. Using SQL Server Management Studio (SSMS)

### Local Development (LocalDB)
1. **Server name**: `(localdb)\mssqllocaldb`
2. **Authentication**: Windows Authentication
3. **Click Connect**

### Docker Environment
1. **Server name**: `localhost,1433`
2. **Authentication**: SQL Server Authentication
3. **Login**: `sa`
4. **Password**: `YourStrong@Passw0rd`
5. **Click Connect**

## 4. Using Azure Data Studio

### Local Development (LocalDB)
1. **Connection type**: Microsoft SQL Server
2. **Server**: `(localdb)\mssqllocaldb`
3. **Authentication type**: Windows Authentication
4. **Database**: `TemplateBackendDb` (optional)

### Docker Environment
1. **Connection type**: Microsoft SQL Server
2. **Server**: `localhost,1433`
3. **Authentication type**: SQL Login
4. **User name**: `sa`
5. **Password**: `YourStrong@Passw0rd`
6. **Database**: `TemplateBackendDb` (optional)

## 5. Using VS Code with mssql Extension

### Install Extension
```bash
# Install mssql extension for VS Code
code --install-extension ms-mssql.mssql
```

### Connection Profiles

#### Local Development Profile
```json
{
  "server": "(localdb)\\mssqllocaldb",
  "database": "TemplateBackendDb",
  "authenticationType": "Integrated",
  "profileName": "LocalDB-TemplateBackend",
  "password": ""
}
```

#### Docker Environment Profile
```json
{
  "server": "localhost,1433",
  "database": "TemplateBackendDb",
  "authenticationType": "SqlLogin",
  "user": "sa",
  "profileName": "Docker-TemplateBackend",
  "password": "YourStrong@Passw0rd"
}
```

## 6. Using .NET Entity Framework Tools

### Package Manager Console (Visual Studio)
```powershell
# Update database (applies migrations)
Update-Database

# Add new migration
Add-Migration "MigrationName"

# Script migrations
Script-Migration

# List migrations
Get-Migration
```

### .NET CLI
```bash
# Navigate to API project
cd src/TemplateBackend.API

# Update database
dotnet ef database update

# Add migration
dotnet ef migrations add "MigrationName"

# Generate SQL script
dotnet ef migrations script

# List migrations
dotnet ef migrations list

# Drop database (careful!)
dotnet ef database drop
```

## 🔍 Common SQL Queries

### Database Information
```sql
-- List all databases
SELECT name FROM sys.databases;

-- Get current database
SELECT DB_NAME();

-- Get SQL Server version
SELECT @@VERSION;

-- List tables in current database
SELECT name FROM sys.tables;

-- Get table row counts
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCounts
FROM sys.tables t
INNER JOIN sys.dm_db_partition_stats p ON t.object_id = p.object_id
WHERE p.index_id < 2
ORDER BY p.rows DESC;
```

### Application-Specific Queries
```sql
-- Check if TemplateBackendDb exists
SELECT name FROM sys.databases WHERE name = 'TemplateBackendDb';

-- List all tables in TemplateBackendDb
USE TemplateBackendDb;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Check migration history
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;

-- Check AspNetUsers (Identity tables)
SELECT COUNT(*) as UserCount FROM AspNetUsers;

-- Check Products table
SELECT COUNT(*) as ProductCount FROM Products;
```

## 🐳 Docker-Specific Commands

### Container Management
```bash
# Check if SQL Server container is running
docker ps | grep sqlserver

# View SQL Server container logs
docker logs template_backend_sqlserver

# Start SQL Server container
docker-compose up -d sqlserver

# Stop SQL Server container
docker-compose stop sqlserver

# Restart SQL Server container
docker-compose restart sqlserver
```

### Execute Commands in Container
```bash
# Execute sqlcmd inside container
docker exec -it template_backend_sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"

# Execute specific query
docker exec template_backend_sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION"

# Access container bash
docker exec -it template_backend_sqlserver bash
```

### Backup and Restore
```bash
# Backup database from Docker container
docker exec template_backend_sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "BACKUP DATABASE TemplateBackendDb TO DISK = '/var/opt/mssql/backup/TemplateBackendDb.bak'"

# Copy backup file from container to host
docker cp template_backend_sqlserver:/var/opt/mssql/backup/TemplateBackendDb.bak ./TemplateBackendDb.bak

# Copy backup file from host to container
docker cp ./TemplateBackendDb.bak template_backend_sqlserver:/var/opt/mssql/backup/TemplateBackendDb.bak

# Restore database in Docker container
docker exec template_backend_sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "RESTORE DATABASE TemplateBackendDb FROM DISK = '/var/opt/mssql/backup/TemplateBackendDb.bak' WITH REPLACE"
```

## 🔧 Troubleshooting

### Common Connection Issues

#### LocalDB Not Started
```bash
# Start LocalDB instance
sqllocaldb start mssqllocaldb

# Create LocalDB instance if not exists
sqllocaldb create mssqllocaldb

# Get LocalDB info
sqllocaldb info mssqllocaldb
```

#### Docker SQL Server Issues
```bash
# Check if container is healthy
docker inspect template_backend_sqlserver | grep Health

# View container resource usage
docker stats template_backend_sqlserver

# Check SQL Server error logs
docker exec template_backend_sqlserver cat /var/opt/mssql/log/errorlog
```

#### Connection String Issues
```csharp
// Local Development - Working Examples
"Server=(localdb)\\mssqllocaldb;Database=TemplateBackendDb;Trusted_Connection=true;MultipleActiveResultSets=true"

// Docker - Working Examples
"Server=localhost,1433;Database=TemplateBackendDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
"Server=sqlserver;Database=TemplateBackendDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"

// Common Mistakes to Avoid
"Server=localhost\\mssqllocaldb"  // ❌ Wrong for Docker
"Server=(localdb)\\mssqllocaldb;User Id=sa"  // ❌ LocalDB doesn't use SQL auth
"Server=sqlserver,1433"  // ❌ Port not needed inside Docker network
```

### Firewall and Network Issues
```bash
# Test connectivity to Docker SQL Server
telnet localhost 1433

# Check if port is listening
netstat -an | findstr 1433

# Test with PowerShell
Test-NetConnection -ComputerName localhost -Port 1433
```

## 📚 Additional Resources

### Download Links
- **SQL Server Management Studio (SSMS)**: https://aka.ms/ssmsfullsetup
- **Azure Data Studio**: https://aka.ms/azuredatastudio
- **SQL Server Command Line Tools**: https://aka.ms/sqlcmd

### Documentation
- **SqlServer PowerShell Module**: https://docs.microsoft.com/powershell/module/sqlserver/
- **Entity Framework Core Tools**: https://docs.microsoft.com/ef/core/cli/
- **SQL Server on Docker**: https://hub.docker.com/_/microsoft-mssql-server

### Quick Reference Card
```
┌─────────────────────────────────────────────────────────────┐
│                    Quick Connection Reference                │
├─────────────────────────────────────────────────────────────┤
│ LOCAL DEVELOPMENT                                           │
│ Server: (localdb)\mssqllocaldb                             │
│ Auth: Windows Authentication                                │
│ Database: TemplateBackendDb                                 │
│                                                             │
│ DOCKER ENVIRONMENT                                          │
│ Server: localhost,1433                                      │
│ Auth: sa / YourStrong@Passw0rd                             │
│ Database: TemplateBackendDb                                 │
│                                                             │
│ QUICK COMMANDS                                              │
│ sqlcmd -S "(localdb)\mssqllocaldb" -E                     │
│ sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd"   │
│ dotnet ef database update                                   │
│ docker-compose up -d sqlserver                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚀 Quick Start Checklist

For Local Development:
- [ ] Ensure LocalDB is installed (comes with Visual Studio/SQL Server Express)
- [ ] Use connection string: `Server=(localdb)\mssqllocaldb;Database=TemplateBackendDb;Trusted_Connection=true;MultipleActiveResultSets=true`
- [ ] Run `dotnet ef database update` from `src/TemplateBackend.API`

For Docker Development:
- [ ] Start SQL Server container: `docker-compose up -d sqlserver`
- [ ] Wait for health check to pass
- [ ] Connect using: `sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd"`
- [ ] Applications will auto-migrate database on startup

**Happy Coding! 🎯**
