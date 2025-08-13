# .NET 9 Backend Template - FastEndpoints with Vertical Slice Architecture

A modern backend template built with .NET 9.0.101 using FastEndpoints and vertical slice architecture for better maintainability and feature organization. Includes a pub-sub payment system with Apache Kafka for scalable, reliable payment processing.

## 🚀 Features

- **.NET 9.0.101** - Latest framework with performance improvements
- **FastEndpoints** - High-performance, minimal API framework
- **Vertical Slice Architecture** - Feature-based organization
- **Entity Framework Core** - Code-first approach with migrations
- **JWT Authentication** - Secure token-based authentication
- **Email Notifications** - Login alerts and registration confirmations
- **Swagger/OpenAPI** - Interactive API documentation
- **Global Exception Handling** - Centralized error management
- **Logging** - Structured logging with Serilog
- **Validation** - FluentValidation for request validation
- **Health Checks** - Application health monitoring
- **CORS** - Cross-origin resource sharing
- **Rate Limiting** - API rate limiting protection
- **Caching** - In-memory and distributed caching
- **Background Services** - Long-running background tasks
- **Unit Testing** - xUnit with Moq for testing
- **Docker Support** - Containerization ready
- **Pub-Sub Payment System** - Apache Kafka with retry policies
- **Payment Processing** - Stripe integration with error handling
- **MongoDB Integration** - NoSQL for logs and error persistence

## 📁 Project Structure

```
template_backend/
├── src/
│   ├── TemplateBackend.API/           # Main API project
│   │   ├── Features/                   # Vertical slices by feature
│   │   │   ├── Auth/                  # Authentication feature
│   │   │   │   ├── Login/             # Login endpoint
│   │   │   │   └── Register/          # Registration endpoint
│   │   │   ├── Products/              # Product management feature
│   │   │   ├── Payments/              # Payment processing feature
│   │   │   └── Health/                # Health checks feature
│   │   ├── Common/                    # Shared components
│   │   │   ├── Extensions/            # Extension methods
│   │   │   ├── Middleware/            # Custom middleware
│   │   │   ├── Models/                # Shared models
│   │   │   ├── Interfaces/            # Message queue interfaces
│   │   │   ├── Implementations/       # Kafka implementation
│   │   │   └── Services/              # Application services
│   │   ├── Infrastructure/            # Data access & external services
│   │   └── Program.cs                 # Application entry point
│   └── TemplateBackend.PaymentProcessor/  # Payment processing service
│       ├── PaymentProcessorService.cs # Background payment processor
│       └── Program.cs                 # Service entry point
├── tests/
│   └── TemplateBackend.API.Tests/     # Integration tests
├── docker/
│   ├── Dockerfile                     # API Docker configuration
│   ├── Dockerfile.payment-processor   # Payment processor Docker
│   └── README.md                      # Docker documentation
├── docs/                              # Documentation
└── scripts/                           # Build and deployment scripts
```

## 🛠️ Prerequisites

- .NET 9.0.101 SDK
- Docker Desktop (for full environment)
- SQL Server / PostgreSQL / SQLite
- Visual Studio 2022 / VS Code
- At least 4GB RAM (for Docker services)

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

#### 1. Clone and Setup
```bash
git clone <repository-url>
cd template_backend
```

#### 2. Start All Services
```bash
# Start all services (API, Payment Processor, Kafka, Databases)
docker-compose up -d
```

#### 3. Verify Services
```bash
# Check service status
docker-compose ps

# View logs
docker-compose logs -f api
docker-compose logs -f payment-processor
docker-compose logs -f kafka
```

#### 4. Access Services
- **API Documentation**: http://localhost:7001/swagger
- **Kafka UI**: http://localhost:8080
- **Health Check**: http://localhost:7001/health

### Option 2: Local Development

#### 1. Start Infrastructure Only
```bash
# Start databases and Kafka
docker-compose up -d sqlserver redis mongodb kafka zookeeper
```

#### 2. Configure Local Development
Update `src/TemplateBackend.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TemplateBackend;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;",
    "Redis": "localhost:6379",
    "MongoDB": "mongodb://admin:YourStrong@Passw0rd@localhost:27017"
  },
  "KafkaSettings": {
    "BootstrapServers": "localhost:9092",
    "ClientId": "template-backend",
    "ConsumerGroupId": "template-backend-group",
    "PaymentTopic": "payment-events",
    "PaymentResultTopic": "payment-results",
    "SubscriptionTopic": "subscription-events",
    "RetryTopic": "payment-retry"
  }
}
```

#### 3. Run Migrations
```bash
cd src/TemplateBackend.API
dotnet ef database update
```

#### 4. Start API Service
```bash
cd src/TemplateBackend.API
dotnet run
```

#### 5. Start Payment Processor Service
```bash
cd src/TemplateBackend.PaymentProcessor
dotnet run
```

## 🔧 Configuration

### Environment Variables

#### API Service
```bash
# Database
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TemplateBackend;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;

# Redis
ConnectionStrings__Redis=redis:6379

# MongoDB
ConnectionStrings__MongoDB=mongodb://admin:YourStrong@Passw0rd@mongodb:27017

# Kafka
KafkaSettings__BootstrapServers=kafka:29092
KafkaSettings__ClientId=template-backend
KafkaSettings__ConsumerGroupId=template-backend-group

# JWT
JwtSettings__SecretKey=your-super-secret-key-with-at-least-32-characters
JwtSettings__Issuer=TemplateBackend
JwtSettings__Audience=TemplateBackend

# Email
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__Username=your-email@gmail.com
EmailSettings__Password=your-app-password
```

#### Payment Processor Service
```bash
# MongoDB
ConnectionStrings__MongoDB=mongodb://admin:YourStrong@Passw0rd@mongodb:27017

# Stripe
PaymentSettings__StripeSecretKey=sk_test_your_stripe_secret_key_here
PaymentSettings__StripePublishableKey=pk_test_your_stripe_publishable_key_here
PaymentSettings__WebhookSecret=whsec_your_webhook_secret_here

# Kafka
KafkaSettings__BootstrapServers=kafka:29092
KafkaSettings__ClientId=payment-processor
KafkaSettings__ConsumerGroupId=payment-processor-group
```

## 📊 Pub-Sub Payment System

### Architecture Overview

The payment system uses a **Publisher-Subscriber (Pub-Sub)** pattern with Apache Kafka:

```
Client → API → Kafka → Payment Processor → Stripe → MongoDB
```

### Key Components

#### 1. Message Queue (Kafka)
- **Payment Events Topic**: Receives payment processing requests
- **Payment Results Topic**: Publishes processing results
- **Retry Topic**: Handles failed payments for retry

#### 2. Payment Queue Service
- Publishes payment events to Kafka
- Returns immediate processing status
- Handles retry policies with exponential backoff

#### 3. Payment Processor Service
- Background service consuming Kafka messages
- Processes Stripe payments with retry logic
- Saves errors to MongoDB for persistence

#### 4. Error Handling
- **Polly Retry Policies**: Exponential backoff (1s, 2s, 4s, 8s, 16s, 30s max)
- **MongoDB Error Storage**: Persistent error logs
- **Error Classification**: Different strategies per error type

### Testing Payment Flow

#### 1. Create Payment
```bash
curl -X POST http://localhost:7001/api/payments/create \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2000,
    "currency": "USD",
    "paymentMethodId": "pm_test_123",
    "customerId": "cus_test_123",
    "description": "Test payment"
  }'
```

#### 2. Monitor Kafka Messages
```bash
# Monitor payment events
docker exec kafka kafka-console-consumer \
  --topic payment-events \
  --bootstrap-server localhost:9092 \
  --from-beginning

# Monitor payment results
docker exec kafka kafka-console-consumer \
  --topic payment-results \
  --bootstrap-server localhost:9092 \
  --from-beginning
```

#### 3. Check Kafka UI
Visit http://localhost:8080 to:
- View topics and messages
- Monitor consumer groups
- Check cluster health

## 🧪 Testing

### Unit Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TemplateBackend.API.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Tests
```bash
# Test payment flow end-to-end
dotnet test --filter "Category=Integration"
```

### Load Testing
```bash
# Test concurrent payment processing
dotnet test --filter "Category=Load"
```

## 📚 API Documentation

### Available Endpoints

#### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/verify-otp` - OTP verification
- `POST /api/auth/verify-google-authenticator` - 2FA verification

#### Payments
- `POST /api/payments/create` - Create payment
- `POST /api/payments/process` - Process payment
- `GET /api/payments/history/{userId}` - Payment history
- `POST /api/payments/validate` - Validate payment method
- `POST /api/payments/subscriptions/create` - Create subscription

#### Products
- `GET /api/products` - Get products
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

#### Health
- `GET /api/health` - Health check

### Swagger Documentation
Visit http://localhost:7001/swagger for interactive API documentation.

## 🔐 Authentication

### JWT Authentication
- User registration and login
- Role-based authorization
- Token refresh mechanism
- Password hashing with BCrypt

### Two-Factor Authentication (2FA)
- Email OTP verification
- Google Authenticator TOTP
- Device-based verification
- Trusted device management

## 📊 Monitoring & Health Checks

### Health Endpoints
- `/api/health` - Overall application health
- Database connectivity
- Redis connectivity
- MongoDB connectivity
- Kafka connectivity

### Logging
- **Structured Logging**: Serilog integration
- **Error Context**: Detailed error information
- **Performance Tracking**: Processing time logs

### Metrics
- Payment processing time
- Error rates and types
- Kafka message throughput
- Consumer group lag

## 🚀 Deployment

### Docker Compose (Recommended)
```bash
# Start all services
docker-compose up -d

# Scale payment processors
docker-compose up -d --scale payment-processor=3

# Stop services
docker-compose down
```

### Azure
```bash
az webapp up --name template-backend --runtime "DOTNET|9.0"
```

### Kubernetes
```bash
kubectl apply -f k8s/
```

## 🛠️ Development Workflow

### 1. Local Development
```bash
# Start infrastructure only
docker-compose up -d sqlserver redis mongodb kafka zookeeper

# Run API locally
cd src/TemplateBackend.API
dotnet run

# Run Payment Processor locally
cd src/TemplateBackend.PaymentProcessor
dotnet run
```

### 2. Database Management
```bash
# SQL Server
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd

# MongoDB
docker exec -it mongodb mongosh -u admin -p YourStrong@Passw0rd

# Redis
docker exec -it redis redis-cli
```

### 3. Kafka Management
```bash
# List topics
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092

# Create topic
docker exec kafka kafka-topics --create --topic payment-events --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1

# Monitor messages
docker exec kafka kafka-console-consumer --topic payment-events --bootstrap-server localhost:9092 --from-beginning
```

## 🔒 Security Considerations

### Production Hardening
- Change default passwords
- Use secrets management
- Enable SSL/TLS
- Configure firewall rules
- Implement network segmentation

### Environment Variables
```bash
# Use .env file for sensitive data
echo "STRIPE_SECRET_KEY=sk_live_your_key" > .env
echo "JWT_SECRET_KEY=your-super-secret-key" >> .env
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:

- Create an issue in the repository
- Check the documentation in the `/docs` folder
- Review the example implementations
- Check the Docker documentation in `/docker/README.md`

---

**Built with ❤️ using .NET 9.0.101 with FastEndpoints, Vertical Slice Architecture, and Apache Kafka for scalable payment processing** 🚀✨ 