# Docker Setup with Kafka & Zookeeper

## 🚀 **Overview**

This Docker Compose setup provides a complete development environment for the Template Backend with pub-sub payment processing using Apache Kafka and Zookeeper.

## 🏗️ **Services**

### **1. Core Infrastructure**

#### **SQL Server**
- **Image**: `mcr.microsoft.com/mssql/server:2022-latest`
- **Port**: `1433`
- **Purpose**: Primary database for user management and application data
- **Credentials**: 
  - Username: `sa`
  - Password: `YourStrong@Passw0rd`

#### **Redis**
- **Image**: `redis:7-alpine`
- **Port**: `6379`
- **Purpose**: Caching and session storage
- **Features**: In-memory data structure store

#### **MongoDB**
- **Image**: `mongo:7.0`
- **Port**: `27017`
- **Purpose**: NoSQL database for email logs, OTPs, and payment errors
- **Credentials**:
  - Username: `admin`
  - Password: `YourStrong@Passw0rd`

### **2. Message Queue Infrastructure**

#### **Zookeeper**
- **Image**: `confluentinc/cp-zookeeper:7.4.0`
- **Port**: `2181`
- **Purpose**: Distributed coordination service for Kafka
- **Features**:
  - Service discovery
  - Configuration management
  - Leader election
  - Distributed synchronization

#### **Apache Kafka**
- **Image**: `confluentinc/cp-kafka:7.4.0`
- **Ports**: 
  - `9092`: External access
  - `9101`: JMX monitoring
- **Purpose**: Distributed streaming platform for pub-sub messaging
- **Features**:
  - High-throughput, fault-tolerant messaging
  - Horizontal scalability
  - Message persistence
  - Topic partitioning

#### **Kafka UI**
- **Image**: `provectuslabs/kafka-ui:latest`
- **Port**: `8080`
- **Purpose**: Web-based Kafka management interface
- **Features**:
  - Topic management
  - Message browsing
  - Consumer group monitoring
  - Cluster health monitoring

### **3. Application Services**

#### **API Service**
- **Port**: `7001`
- **Purpose**: Main backend API with FastEndpoints
- **Features**:
  - User authentication and authorization
  - Payment event publishing
  - Email notifications
  - Health checks

#### **Payment Processor Service**
- **Port**: `80` (internal)
- **Purpose**: Background service for processing payment events
- **Features**:
  - Kafka message consumption
  - Stripe payment processing
  - Error handling and retry logic
  - MongoDB error persistence

## 🔧 **Configuration**

### **Environment Variables**

#### **API Service**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ASPNETCORE_URLS=http://+:7001
  - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TemplateBackend;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
  - ConnectionStrings__Redis=redis:6379
  - ConnectionStrings__MongoDB=mongodb://admin:YourStrong@Passw0rd@mongodb:27017
  - KafkaSettings__BootstrapServers=kafka:29092
  - KafkaSettings__ClientId=template-backend
  - KafkaSettings__ConsumerGroupId=template-backend-group
  - KafkaSettings__PaymentTopic=payment-events
  - KafkaSettings__PaymentResultTopic=payment-results
  - KafkaSettings__SubscriptionTopic=subscription-events
  - KafkaSettings__RetryTopic=payment-retry
```

#### **Payment Processor Service**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__MongoDB=mongodb://admin:YourStrong@Passw0rd@mongodb:27017
  - PaymentSettings__StripeSecretKey=sk_test_your_stripe_secret_key_here
  - PaymentSettings__StripePublishableKey=pk_test_your_stripe_publishable_key_here
  - PaymentSettings__WebhookSecret=whsec_your_webhook_secret_here
  - KafkaSettings__BootstrapServers=kafka:29092
  - KafkaSettings__ClientId=payment-processor
  - KafkaSettings__ConsumerGroupId=payment-processor-group
```

### **Kafka Configuration**

#### **Kafka Settings**
```yaml
environment:
  KAFKA_BROKER_ID: 1
  KAFKA_ZOOKEEPER_CONNECT: 'zookeeper:2181'
  KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
  KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,PLAINTEXT_HOST://localhost:9092
  KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
  KAFKA_TRANSACTION_STATE_LOG_MIN_ISR: 1
  KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR: 1
  KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS: 0
  KAFKA_JMX_PORT: 9101
  KAFKA_JMX_HOSTNAME: localhost
  KAFKA_AUTO_CREATE_TOPICS_ENABLE: 'true'
  KAFKA_DELETE_TOPIC_ENABLE: 'true'
```

## 🚀 **Getting Started**

### **1. Prerequisites**
- Docker Desktop installed
- At least 4GB RAM available
- Ports 1433, 6379, 27017, 2181, 9092, 8080, 7001 available

### **2. Start Services**
```bash
# Start all services
docker-compose up -d

# Start specific services
docker-compose up -d kafka zookeeper mongodb redis sqlserver
docker-compose up -d api payment-processor
```

### **3. Verify Services**
```bash
# Check service status
docker-compose ps

# View logs
docker-compose logs -f kafka
docker-compose logs -f api
docker-compose logs -f payment-processor
```

### **4. Access Services**

#### **Web Interfaces**
- **API Documentation**: http://localhost:7001/swagger
- **Kafka UI**: http://localhost:8080
- **Health Check**: http://localhost:7001/health

#### **Database Connections**
- **SQL Server**: `localhost:1433`
- **Redis**: `localhost:6379`
- **MongoDB**: `localhost:27017`
- **Kafka**: `localhost:9092`
- **Zookeeper**: `localhost:2181`

## 📊 **Monitoring & Management**

### **1. Kafka UI Dashboard**
Access http://localhost:8080 to:
- **View Topics**: Monitor payment-events, payment-results, etc.
- **Browse Messages**: Inspect message content and metadata
- **Monitor Consumers**: Track consumer group progress
- **Cluster Health**: Check Kafka cluster status

### **2. Service Health Checks**
```bash
# API Health
curl http://localhost:7001/health

# Kafka Topics
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092

# MongoDB Status
docker exec mongodb mongosh --eval "db.adminCommand('ping')"
```

### **3. Log Monitoring**
```bash
# API Logs
docker-compose logs -f api

# Payment Processor Logs
docker-compose logs -f payment-processor

# Kafka Logs
docker-compose logs -f kafka

# MongoDB Logs
docker-compose logs -f mongodb
```

## 🔧 **Development Workflow**

### **1. Local Development**
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

### **2. Testing Payment Flow**
```bash
# Create a payment
curl -X POST http://localhost:7001/api/payments/create \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2000,
    "currency": "USD",
    "paymentMethodId": "pm_test_123",
    "customerId": "cus_test_123",
    "description": "Test payment"
  }'

# Monitor Kafka messages
docker exec kafka kafka-console-consumer \
  --topic payment-events \
  --bootstrap-server localhost:9092 \
  --from-beginning
```

### **3. Database Management**
```bash
# SQL Server
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd

# MongoDB
docker exec -it mongodb mongosh -u admin -p YourStrong@Passw0rd

# Redis
docker exec -it redis redis-cli
```

## 🛠️ **Troubleshooting**

### **1. Common Issues**

#### **Kafka Connection Issues**
```bash
# Check Kafka status
docker-compose logs kafka

# Verify Zookeeper connection
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092

# Check Kafka UI
curl http://localhost:8080
```

#### **Database Connection Issues**
```bash
# Check SQL Server
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"

# Check MongoDB
docker exec mongodb mongosh --eval "db.adminCommand('ping')"

# Check Redis
docker exec redis redis-cli ping
```

#### **Service Startup Issues**
```bash
# Check all services
docker-compose ps

# View startup logs
docker-compose logs --tail=100

# Restart specific service
docker-compose restart api
```

### **2. Performance Tuning**

#### **Kafka Performance**
```yaml
# Add to kafka service environment
KAFKA_NUM_NETWORK_THREADS: 3
KAFKA_NUM_IO_THREADS: 8
KAFKA_SOCKET_SEND_BUFFER_BYTES: 102400
KAFKA_SOCKET_RECEIVE_BUFFER_BYTES: 102400
KAFKA_SOCKET_REQUEST_MAX_BYTES: 104857600
```

#### **MongoDB Performance**
```yaml
# Add to mongodb service environment
MONGO_INITDB_DATABASE: TemplateBackend
```

### **3. Data Persistence**
```bash
# Backup volumes
docker run --rm -v template_backend_sqlserver_data:/data -v $(pwd):/backup alpine tar czf /backup/sqlserver_backup.tar.gz -C /data .
docker run --rm -v template_backend_mongodb_data:/data -v $(pwd):/backup alpine tar czf /backup/mongodb_backup.tar.gz -C /data .
docker run --rm -v template_backend_kafka_data:/data -v $(pwd):/backup alpine tar czf /backup/kafka_backup.tar.gz -C /data .
```

## 🔒 **Security Considerations**

### **1. Production Hardening**
- Change default passwords
- Use secrets management
- Enable SSL/TLS
- Configure firewall rules
- Implement network segmentation

### **2. Environment Variables**
```bash
# Use .env file for sensitive data
echo "STRIPE_SECRET_KEY=sk_live_your_key" > .env
echo "JWT_SECRET_KEY=your-super-secret-key" >> .env
```

## 📚 **Useful Commands**

### **1. Service Management**
```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Restart specific service
docker-compose restart api

# View service logs
docker-compose logs -f [service-name]

# Scale services
docker-compose up -d --scale payment-processor=3
```

### **2. Kafka Management**
```bash
# List topics
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092

# Create topic
docker exec kafka kafka-topics --create --topic payment-events --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1

# Monitor messages
docker exec kafka kafka-console-consumer --topic payment-events --bootstrap-server localhost:9092 --from-beginning

# Check consumer groups
docker exec kafka kafka-consumer-groups --list --bootstrap-server localhost:9092
```

### **3. Database Management**
```bash
# SQL Server backup
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "BACKUP DATABASE TemplateBackend TO DISK = '/var/opt/mssql/backup.bak'"

# MongoDB backup
docker exec mongodb mongodump --out /data/backup --db TemplateBackend

# Redis backup
docker exec redis redis-cli BGSAVE
```

This Docker setup provides a complete development environment for the pub-sub payment system with comprehensive monitoring and management capabilities! 🚀✨ 