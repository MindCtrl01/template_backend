-- ================================================================
-- Webhook Processor Database Migration Script
-- Creates 3 tables: WebhookEvents, WebhookProcessingLogs, PaymentEvents
-- Target: SQL Server (Docker Container)
-- ================================================================

USE TemplateBackendDb;
GO

PRINT 'Starting Webhook Processor table creation...';
GO

-- ================================================================
-- 1. WebhookEvents Table
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WebhookEvents' AND xtype='U')
BEGIN
    CREATE TABLE WebhookEvents (
        Id INT PRIMARY KEY IDENTITY(1,1),
        EventId NVARCHAR(255) NOT NULL,           -- Provider's unique event ID
        Provider NVARCHAR(50) NOT NULL,           -- stripe, paypal, square, etc.
        EventType NVARCHAR(100) NOT NULL,         -- payment.succeeded, charge.failed, etc.
        Status NVARCHAR(50) NOT NULL,             -- pending, completed, failed, retrying
        RawPayload NVARCHAR(MAX) NOT NULL,        -- Original webhook JSON payload
        ProcessedPayload NVARCHAR(MAX),           -- Processed/sanitized payload
        Signature NVARCHAR(1000),                 -- Webhook signature for validation
        SourceIp NVARCHAR(45),                    -- Source IP address (IPv4/IPv6)
        UserAgent NVARCHAR(500),                  -- HTTP User-Agent header
        ErrorMessage NVARCHAR(2000),              -- Error details if processing failed
        AttemptCount INT NOT NULL DEFAULT 1,      -- Current processing attempt number
        MaxAttempts INT NOT NULL DEFAULT 3,       -- Maximum retry attempts allowed
        NextRetryAt DATETIME2,                    -- Scheduled time for next retry
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),    -- When webhook was received
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),    -- Last update timestamp
        ProcessedAt DATETIME2                     -- When processing completed successfully
    );
    
    PRINT '✅ WebhookEvents table created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  WebhookEvents table already exists';
END
GO

-- ================================================================
-- 2. WebhookProcessingLogs Table
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WebhookProcessingLogs' AND xtype='U')
BEGIN
    CREATE TABLE WebhookProcessingLogs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        WebhookEventId INT NOT NULL,              -- Foreign key to WebhookEvents
        ProcessingStep NVARCHAR(100) NOT NULL,    -- validation, parsing, persistence, notification
        Status NVARCHAR(50) NOT NULL,             -- success, failed, warning, skipped
        DurationMs BIGINT NOT NULL,               -- Processing time in milliseconds
        ErrorMessage NVARCHAR(2000),              -- Step-specific error details
        AdditionalData NVARCHAR(4000),            -- Extra context data (JSON format)
        ProcessedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),  -- When this step was processed
        
        -- Foreign key constraint
        CONSTRAINT FK_WebhookProcessingLogs_WebhookEvents 
            FOREIGN KEY (WebhookEventId) REFERENCES WebhookEvents(Id)
            ON DELETE CASCADE
    );
    
    PRINT '✅ WebhookProcessingLogs table created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  WebhookProcessingLogs table already exists';
END
GO

-- ================================================================
-- 3. PaymentEvents Table
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PaymentEvents' AND xtype='U')
BEGIN
    CREATE TABLE PaymentEvents (
        Id INT PRIMARY KEY IDENTITY(1,1),
        WebhookEventId INT NOT NULL,              -- Foreign key to WebhookEvents
        PaymentId NVARCHAR(255) NOT NULL,         -- Payment/transaction ID from provider
        Provider NVARCHAR(50) NOT NULL,           -- Payment provider (stripe, paypal, etc.)
        EventType NVARCHAR(100) NOT NULL,         -- Specific event type
        Status NVARCHAR(50) NOT NULL,             -- succeeded, failed, pending, refunded
        Amount DECIMAL(18,2),                     -- Payment amount (e.g., 19.99)
        Currency NVARCHAR(3),                     -- ISO currency code (USD, EUR, GBP)
        CustomerId NVARCHAR(255),                 -- Customer identifier
        OrderId NVARCHAR(255),                    -- Order/invoice identifier
        TransactionId NVARCHAR(255),              -- Transaction identifier
        Metadata NVARCHAR(4000),                  -- Additional payment metadata (JSON)
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),     -- Payment event creation time
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),     -- Last update timestamp
        
        -- Foreign key constraint
        CONSTRAINT FK_PaymentEvents_WebhookEvents 
            FOREIGN KEY (WebhookEventId) REFERENCES WebhookEvents(Id)
            ON DELETE CASCADE
    );
    
    PRINT '✅ PaymentEvents table created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  PaymentEvents table already exists';
END
GO

-- ================================================================
-- 4. Performance Indexes
-- ================================================================
PRINT 'Creating performance indexes...';

-- Index for webhook event queries by provider and type
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookEvents_Provider_EventType' AND object_id = OBJECT_ID('WebhookEvents'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WebhookEvents_Provider_EventType 
        ON WebhookEvents(Provider, EventType)
        INCLUDE (Status, CreatedAt);
    PRINT '✅ Index IX_WebhookEvents_Provider_EventType created';
END

-- Index for webhook status and date filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookEvents_Status_CreatedAt' AND object_id = OBJECT_ID('WebhookEvents'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WebhookEvents_Status_CreatedAt 
        ON WebhookEvents(Status, CreatedAt DESC)
        INCLUDE (Provider, EventType);
    PRINT '✅ Index IX_WebhookEvents_Status_CreatedAt created';
END

-- Index for retry processing
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookEvents_NextRetryAt' AND object_id = OBJECT_ID('WebhookEvents'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WebhookEvents_NextRetryAt 
        ON WebhookEvents(NextRetryAt)
        WHERE NextRetryAt IS NOT NULL AND Status = 'failed';
    PRINT '✅ Index IX_WebhookEvents_NextRetryAt created';
END

-- Index for payment queries by payment ID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentEvents_PaymentId' AND object_id = OBJECT_ID('PaymentEvents'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PaymentEvents_PaymentId 
        ON PaymentEvents(PaymentId)
        INCLUDE (Provider, Status, Amount, Currency);
    PRINT '✅ Index IX_PaymentEvents_PaymentId created';
END

-- Index for payment queries by provider and status
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentEvents_Provider_Status' AND object_id = OBJECT_ID('PaymentEvents'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PaymentEvents_Provider_Status 
        ON PaymentEvents(Provider, Status)
        INCLUDE (CreatedAt, Amount, Currency);
    PRINT '✅ Index IX_PaymentEvents_Provider_Status created';
END

-- Index for processing logs by webhook event
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookProcessingLogs_WebhookEventId' AND object_id = OBJECT_ID('WebhookProcessingLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WebhookProcessingLogs_WebhookEventId 
        ON WebhookProcessingLogs(WebhookEventId, ProcessedAt DESC)
        INCLUDE (ProcessingStep, Status, DurationMs);
    PRINT '✅ Index IX_WebhookProcessingLogs_WebhookEventId created';
END

GO

-- ================================================================
-- 5. Verification & Summary
-- ================================================================
PRINT '';
PRINT '📊 WEBHOOK PROCESSOR DATABASE SETUP COMPLETE';
PRINT '============================================';

-- Table verification
SELECT 
    TABLE_NAME as 'Table Name',
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as 'Columns',
    CASE 
        WHEN TABLE_NAME = 'WebhookEvents' THEN 'Main webhook event storage'
        WHEN TABLE_NAME = 'WebhookProcessingLogs' THEN 'Processing step logs'
        WHEN TABLE_NAME = 'PaymentEvents' THEN 'Payment-specific data'
        ELSE 'Unknown'
    END as 'Description'
FROM INFORMATION_SCHEMA.TABLES t 
WHERE TABLE_NAME IN ('WebhookEvents', 'WebhookProcessingLogs', 'PaymentEvents')
ORDER BY TABLE_NAME;

-- Index verification
SELECT 
    i.name as 'Index Name',
    t.name as 'Table Name',
    CASE i.type_desc 
        WHEN 'CLUSTERED' THEN 'Primary Key'
        WHEN 'NONCLUSTERED' THEN 'Performance Index'
        ELSE i.type_desc
    END as 'Type'
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('WebhookEvents', 'WebhookProcessingLogs', 'PaymentEvents')
    AND i.name IS NOT NULL
ORDER BY t.name, i.name;

PRINT '';
PRINT '🎯 Next Steps:';
PRINT '1. Start the WebhookProcessor service';
PRINT '2. Configure webhook endpoints in your payment providers';
PRINT '3. Test webhook reception and processing';
PRINT '4. Monitor the tables for incoming webhook data';
PRINT '';
PRINT '✅ Database migration completed successfully!';

GO
