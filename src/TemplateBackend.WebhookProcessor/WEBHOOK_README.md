# Webhook Processor Service

A dedicated .NET service for processing webhooks from multiple payment providers and persisting them to SQL Server.

## 🚀 Overview

The Webhook Processor Service is a standalone application that:

- **Receives Webhooks**: Handles webhooks from Stripe, PayPal, and other payment providers
- **Validates Signatures**: Ensures webhook authenticity and security
- **Processes Events**: Extracts payment information and business logic
- **Persists Data**: Stores all webhook events and processing logs in SQL Server
- **Retry Logic**: Automatically retries failed webhook processing
- **Monitoring**: Provides statistics and monitoring endpoints
- **Background Processing**: Runs as a background service for continuous processing

## 📋 Features

### **1. Multi-Provider Support**
- **Stripe**: Complete webhook handling for all Stripe payment events
- **PayPal**: Full PayPal webhook processing
- **Extensible**: Easy to add new payment providers

### **2. Database Persistence**
- **Webhook Events**: All incoming webhooks are stored
- **Processing Logs**: Step-by-step processing logs
- **Payment Events**: Extracted payment information
- **Retry Tracking**: Failed webhooks with retry logic

### **3. Security Features**
- **Signature Validation**: Validates webhook signatures
- **IP Tracking**: Records source IP addresses
- **User Agent Logging**: Tracks request origins
- **Error Handling**: Comprehensive error logging

### **4. Monitoring & Analytics**
- **Real-time Statistics**: Success rates, processing times
- **Event Filtering**: Filter by provider, status, date range
- **Performance Metrics**: Average processing times
- **Retry Monitoring**: Failed webhook tracking

## 🏗️ Architecture

```
Payment Provider → Webhook → API → Processor → Database
                                    ↓
                              Background Service
                                    ↓
                              Retry Logic
```

### **Components**

#### **1. Webhook Controller**
- Receives webhooks from payment providers
- Extracts event information
- Delegates to processor service

#### **2. Webhook Processor Service**
- Core processing logic
- Database persistence
- Error handling and retries

#### **3. Webhook Handlers**
- Provider-specific logic
- Event type processing
- Payment data extraction

#### **4. Background Service**
- Continuous retry processing
- Statistics monitoring
- Health checks

## 📊 Database Schema

### **WebhookEvent Table**
```sql
CREATE TABLE WebhookEvents (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EventId NVARCHAR(255) NOT NULL,
    Provider NVARCHAR(50) NOT NULL,
    EventType NVARCHAR(100) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    RawPayload NVARCHAR(MAX) NOT NULL,
    ProcessedPayload NVARCHAR(MAX),
    Signature NVARCHAR(1000),
    SourceIp NVARCHAR(45),
    UserAgent NVARCHAR(500),
    ErrorMessage NVARCHAR(2000),
    AttemptCount INT NOT NULL DEFAULT 1,
    MaxAttempts INT NOT NULL DEFAULT 3,
    NextRetryAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt DATETIME2
);
```

### **WebhookProcessingLog Table**
```sql
CREATE TABLE WebhookProcessingLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    WebhookEventId INT NOT NULL,
    ProcessingStep NVARCHAR(100) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    DurationMs BIGINT NOT NULL,
    ErrorMessage NVARCHAR(2000),
    AdditionalData NVARCHAR(4000),
    ProcessedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WebhookEventId) REFERENCES WebhookEvents(Id)
);
```

### **PaymentEvent Table**
```sql
CREATE TABLE PaymentEvents (
    Id INT PRIMARY KEY IDENTITY(1,1),
    WebhookEventId INT NOT NULL,
    PaymentId NVARCHAR(255) NOT NULL,
    Provider NVARCHAR(50) NOT NULL,
    EventType NVARCHAR(100) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2),
    Currency NVARCHAR(3),
    CustomerId NVARCHAR(255),
    OrderId NVARCHAR(255),
    TransactionId NVARCHAR(255),
    Metadata NVARCHAR(4000),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WebhookEventId) REFERENCES WebhookEvents(Id)
);
```

## 🔧 Configuration

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TemplateBackendDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
  },
  "WebhookProcessorSettings": {
    "ProcessingInterval": 30000,
    "ErrorRetryInterval": 5000,
    "MaxRetries": 10,
    "EnableDetailedLogging": true
  }
}
```

### **Environment Variables**
```bash
# Database
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TemplateBackendDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;

# Webhook Processor Settings
WebhookProcessorSettings__ProcessingInterval=30000
WebhookProcessorSettings__ErrorRetryInterval=5000
WebhookProcessorSettings__MaxRetries=10
WebhookProcessorSettings__EnableDetailedLogging=true
```

## 📚 API Endpoints

### **1. Receive Webhooks**

#### **Stripe Webhook**
```http
POST /api/webhooks/stripe
Headers:
  Stripe-Signature: t=1234567890,v1=abc123...
  User-Agent: Stripe/v1
```

#### **PayPal Webhook**
```http
POST /api/webhooks/paypal
Headers:
  Paypal-Signature: t=1234567890,v1=abc123...
  User-Agent: PayPal/v1
```

### **2. Monitoring Endpoints**

#### **Get Statistics**
```http
GET /api/webhooks/statistics?provider=stripe&fromDate=2024-01-01&toDate=2024-01-31
```

**Response:**
```json
{
  "totalEvents": 150,
  "successfulEvents": 145,
  "failedEvents": 5,
  "pendingEvents": 0,
  "eventsByProvider": {
    "stripe": 100,
    "paypal": 50
  },
  "eventsByType": {
    "payment_intent.succeeded": 80,
    "charge.succeeded": 20
  },
  "averageProcessingTimeMs": 125.5,
  "successRate": 96.67
}
```

#### **Get Webhook Events**
```http
GET /api/webhooks/events?provider=stripe&status=completed&page=1&pageSize=20
```

**Response:**
```json
{
  "events": [
    {
      "id": 1,
      "eventId": "evt_123456789",
      "provider": "stripe",
      "eventType": "payment_intent.succeeded",
      "status": "completed",
      "createdAt": "2024-01-15T10:30:00Z",
      "processedAt": "2024-01-15T10:30:05Z"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

#### **Retry Failed Webhooks**
```http
POST /api/webhooks/retry?maxRetries=10
```

**Response:**
```json
{
  "message": "Retried 5 failed webhooks",
  "retryCount": 5
}
```

## 🧪 Testing

### **1. Local Development**
```bash
# Start the service
cd src/TemplateBackend.WebhookProcessor
dotnet run

# Test Stripe webhook
curl -X POST http://localhost:7002/api/webhooks/stripe \
  -H "Content-Type: application/json" \
  -H "Stripe-Signature: t=1234567890,v1=abc123" \
  -d '{
    "id": "evt_123456789",
    "type": "payment_intent.succeeded",
    "data": {
      "object": {
        "id": "pi_123456789",
        "amount": 2000,
        "currency": "usd",
        "status": "succeeded"
      }
    }
  }'

# Test PayPal webhook
curl -X POST http://localhost:7002/api/webhooks/paypal \
  -H "Content-Type: application/json" \
  -H "Paypal-Signature: t=1234567890,v1=abc123" \
  -d '{
    "id": "WH-123456789",
    "event_type": "PAYMENT.CAPTURE.COMPLETED",
    "resource": {
      "id": "2GG903537H481924B",
      "status": "COMPLETED",
      "amount": {
        "currency_code": "USD",
        "value": "29.99"
      }
    }
  }'
```

### **2. Docker Testing**
```bash
# Start all services
docker-compose up -d

# Test webhook endpoints
curl -X POST http://localhost:7002/api/webhooks/stripe \
  -H "Content-Type: application/json" \
  -d '{"id": "test", "type": "test.event"}'

# Check statistics
curl http://localhost:7002/api/webhooks/statistics
```

## 🔄 Webhook Processing Flow

### **1. Webhook Reception**
```
Payment Provider → HTTP POST → Webhook Controller
```

### **2. Validation**
```
Extract Headers → Validate Signature → Log Request
```

### **3. Processing**
```
Parse Payload → Extract Payment Data → Save to Database
```

### **4. Business Logic**
```
Provider Handler → Event Type Processing → Payment Event Creation
```

### **5. Persistence**
```
Webhook Event → Processing Logs → Payment Events
```

### **6. Retry Logic**
```
Failed Events → Background Service → Exponential Backoff → Retry
```

## 📈 Monitoring & Observability

### **1. Logging**
- **Structured Logging**: Serilog with JSON formatting
- **Performance Tracking**: Processing time measurements
- **Error Logging**: Detailed error information
- **Audit Trail**: Complete webhook processing history

### **2. Metrics**
- **Success Rate**: Percentage of successful webhook processing
- **Processing Time**: Average time to process webhooks
- **Error Rates**: Failed webhook statistics
- **Retry Statistics**: Retry success rates

### **3. Health Checks**
- **Database Connectivity**: SQL Server connection status
- **Processing Queue**: Pending webhook count
- **Error Rate**: Current error percentage
- **Service Status**: Overall service health

## 🛡️ Security

### **1. Signature Validation**
- **Stripe**: HMAC SHA256 validation
- **PayPal**: Webhook signature verification
- **IP Whitelisting**: Optional IP address filtering
- **Rate Limiting**: Prevent abuse

### **2. Data Protection**
- **Encrypted Storage**: Sensitive data encryption
- **Audit Logging**: Complete request/response logging
- **Error Handling**: Secure error messages
- **Input Validation**: Payload validation

## 🚀 Deployment

### **1. Docker Deployment**
```bash
# Build and run
docker build -f docker/Dockerfile.webhook-processor -t webhook-processor .
docker run -p 7002:80 webhook-processor

# Using docker-compose
docker-compose up webhook-processor
```

### **2. Kubernetes Deployment**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: webhook-processor
spec:
  replicas: 2
  selector:
    matchLabels:
      app: webhook-processor
  template:
    metadata:
      labels:
        app: webhook-processor
    spec:
      containers:
      - name: webhook-processor
        image: webhook-processor:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
```

### **3. Environment Variables**
```bash
# Production
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=prod-sql;Database=WebhookDb;...
WebhookProcessorSettings__ProcessingInterval=60000
WebhookProcessorSettings__MaxRetries=5
```

## 🔧 Development

### **1. Adding New Providers**
```csharp
// 1. Create handler
public class NewProviderWebhookHandler : IWebhookHandler
{
    public string Provider => "newprovider";
    
    public async Task<List<PaymentEvent>> HandleWebhookAsync(string eventType, string payload)
    {
        // Implementation
    }
}

// 2. Register in Program.cs
builder.Services.AddScoped<IWebhookHandler, NewProviderWebhookHandler>();

// 3. Add controller endpoint
[HttpPost("newprovider")]
public async Task<IActionResult> NewProviderWebhook()
{
    // Implementation
}
```

### **2. Custom Event Processing**
```csharp
// Extend PaymentEvent model
public class PaymentEvent
{
    // Add custom fields
    public string? CustomField { get; set; }
}

// Update handler
private PaymentEvent CreatePaymentEvent(WebhookEvent webhookEvent, string eventType, string status)
{
    return new PaymentEvent
    {
        // Extract custom data
        CustomField = ExtractCustomField(webhookEvent),
        // ... other fields
    };
}
```

## 🆘 Troubleshooting

### **Common Issues**

#### **1. Database Connection**
```bash
# Check connection string
# Verify SQL Server is running
# Check firewall settings
```

#### **2. Webhook Signature Validation**
```bash
# Verify webhook secret configuration
# Check signature format
# Validate timestamp
```

#### **3. Processing Failures**
```bash
# Check logs for errors
# Verify payload format
# Check database constraints
```

### **Debug Commands**
```bash
# View logs
docker-compose logs webhook-processor

# Check database
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd

# Test webhook endpoint
curl -X POST http://localhost:7002/api/webhooks/stripe \
  -H "Content-Type: application/json" \
  -d '{"test": "data"}'
```

## 📚 Resources

- [Stripe Webhooks Documentation](https://stripe.com/docs/webhooks)
- [PayPal Webhooks Documentation](https://developer.paypal.com/docs/api-basics/notifications/webhooks/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Background Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)

---

**Built with ❤️ using .NET 9 and Entity Framework Core** 🚀✨ 