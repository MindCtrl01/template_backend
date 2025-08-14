# Webhook Processor Database Setup Script
# Creates all necessary tables in SQL Server Docker container

param(
    [string]$ServerName = "localhost,1433",
    [string]$DatabaseName = "TemplateBackendDb",
    [string]$Username = "sa",
    [string]$Password = "YourStrong@Passw0rd"
)

Write-Host "🗄️  Webhook Processor Database Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Check if SQL Server container is running
Write-Host "📋 Checking SQL Server container status..." -ForegroundColor Yellow
$containerStatus = docker ps --filter "name=template_backend_sqlserver" --format "{{.Status}}"
if ([string]::IsNullOrEmpty($containerStatus)) {
    Write-Host "❌ SQL Server container is not running!" -ForegroundColor Red
    Write-Host "💡 Please start it with: docker-compose up -d sqlserver" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✅ SQL Server container is running: $containerStatus" -ForegroundColor Green
}

# Connection string
$connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=true;"

Write-Host ""
Write-Host "📊 Executing migration script..." -ForegroundColor Yellow
Write-Host "Server: $ServerName" -ForegroundColor Gray
Write-Host "Database: $DatabaseName" -ForegroundColor Gray

try {
    # Check if SqlServer module is available
    if (!(Get-Module -ListAvailable -Name SqlServer)) {
        Write-Host "⚠️  SqlServer PowerShell module not found. Installing..." -ForegroundColor Yellow
        Install-Module -Name SqlServer -Force -Scope CurrentUser -AllowClobber
        Import-Module SqlServer
    }

    # Execute the SQL script
    $scriptPath = Join-Path $PSScriptRoot "create-webhook-tables.sql"
    if (Test-Path $scriptPath) {
        Write-Host "📄 Executing SQL script: $scriptPath" -ForegroundColor Yellow
        Invoke-Sqlcmd -ConnectionString $connectionString -InputFile $scriptPath -Verbose
        Write-Host "✅ Database migration completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ SQL script not found: $scriptPath" -ForegroundColor Red
        exit 1
    }

} catch {
    Write-Host "❌ Error during migration: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Troubleshooting tips:" -ForegroundColor Yellow
    Write-Host "   - Ensure SQL Server container is healthy" -ForegroundColor Gray
    Write-Host "   - Check connection string parameters" -ForegroundColor Gray
    Write-Host "   - Verify database exists" -ForegroundColor Gray
    exit 1
}

# Verification
Write-Host ""
Write-Host "🔍 Verifying table creation..." -ForegroundColor Yellow

try {
    $verificationQuery = @"
SELECT 
    TABLE_NAME as TableName,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as ColumnCount
FROM INFORMATION_SCHEMA.TABLES t 
WHERE TABLE_NAME IN ('WebhookEvents', 'WebhookProcessingLogs', 'PaymentEvents')
ORDER BY TABLE_NAME;
"@

    $tables = Invoke-Sqlcmd -ConnectionString $connectionString -Query $verificationQuery
    
    Write-Host "📊 Created Tables:" -ForegroundColor Green
    foreach ($table in $tables) {
        Write-Host "   ✅ $($table.TableName) ($($table.ColumnCount) columns)" -ForegroundColor Green
    }

    if ($tables.Count -eq 3) {
        Write-Host ""
        Write-Host "🎉 SUCCESS! All webhook processor tables created!" -ForegroundColor Green
        Write-Host ""
        Write-Host "🚀 Next Steps:" -ForegroundColor Cyan
        Write-Host "   1. Start WebhookProcessor service: docker-compose up -d webhook-processor" -ForegroundColor White
        Write-Host "   2. Configure payment provider webhooks" -ForegroundColor White
        Write-Host "   3. Test webhook endpoints" -ForegroundColor White
        Write-Host "   4. Monitor webhook processing in the database" -ForegroundColor White
    } else {
        Write-Host "⚠️  Expected 3 tables, but found $($tables.Count)" -ForegroundColor Yellow
    }

} catch {
    Write-Host "⚠️  Could not verify table creation: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📝 Connection Details:" -ForegroundColor Cyan
Write-Host "   Server: $ServerName" -ForegroundColor Gray
Write-Host "   Database: $DatabaseName" -ForegroundColor Gray
Write-Host "   Tables: WebhookEvents, WebhookProcessingLogs, PaymentEvents" -ForegroundColor Gray

Write-Host ""
Write-Host "✨ Webhook Processor database setup complete!" -ForegroundColor Green
