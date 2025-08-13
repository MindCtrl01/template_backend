# PayPal Payment Integration

This document provides comprehensive information about the PayPal payment integration in the Template Backend API.

## 🚀 Overview

The PayPal integration provides a complete payment processing solution with the following features:

- **Order Creation**: Create PayPal orders for payment processing
- **Payment Capture**: Capture payments from approved orders
- **Payment Refunds**: Process full or partial refunds
- **Webhook Handling**: Handle PayPal webhook events
- **Signature Validation**: Validate webhook signatures for security
- **Error Handling**: Comprehensive error handling and logging

## 📋 Prerequisites

### PayPal Developer Account
1. Create a PayPal Developer account at [developer.paypal.com](https://developer.paypal.com)
2. Create a new app to get your Client ID and Client Secret
3. Configure webhook endpoints in your PayPal app

### Environment Setup
1. **Sandbox Environment**: Use for testing and development
2. **Production Environment**: Use for live transactions
3. **Webhook Configuration**: Set up webhook endpoints for payment notifications

## 🔧 Configuration

### PayPal Settings
Update your `appsettings.json` with your PayPal credentials:

```json
{
  "PayPalSettings": {
    "ClientId": "your_paypal_client_id_here",
    "ClientSecret": "your_paypal_client_secret_here",
    "IsSandbox": true,
    "WebhookSecret": "your_paypal_webhook_secret_here",
    "WebhookUrl": "https://your-domain.com/api/payments/paypal/webhook"
  }
}
```

### Environment Variables
```bash
# PayPal Configuration
PayPalSettings__ClientId=your_paypal_client_id_here
PayPalSettings__ClientSecret=your_paypal_client_secret_here
PayPalSettings__IsSandbox=true
PayPalSettings__WebhookSecret=your_paypal_webhook_secret_here
PayPalSettings__WebhookUrl=https://your-domain.com/api/payments/paypal/webhook
```

## 📚 API Endpoints

### 1. Create PayPal Order
**Endpoint**: `POST /api/payments/paypal/create-order`

**Request**:
```json
{
  "amount": 29.99,
  "currency": "USD",
  "description": "Premium subscription",
  "customerId": "customer_123",
  "returnUrl": "https://example.com/success",
  "cancelUrl": "https://example.com/cancel",
  "customId": "order_123",
  "invoiceId": "INV-12345"
}
```

**Response**:
```json
{
  "orderId": "EC-123456789",
  "status": "CREATED",
  "checkoutUrl": "https://www.sandbox.paypal.com/checkoutnow?token=EC-123456789",
  "createTime": "2024-01-15T10:30:00Z",
  "amount": 29.99,
  "currency": "USD",
  "description": "Premium subscription",
  "customerId": "customer_123",
  "success": true
}
```

### 2. Capture PayPal Payment
**Endpoint**: `POST /api/payments/paypal/capture`

**Request**:
```json
{
  "orderId": "EC-123456789",
  "noteToPayer": "Thank you for your payment!",
  "paymentInstruction": {
    "disbursementMode": "INSTANT",
    "platformFees": [
      {
        "amount": {
          "currencyCode": "USD",
          "value": "1.00"
        },
        "payee": {
          "emailAddress": "platform@example.com"
        }
      }
    ]
  }
}
```

**Response**:
```json
{
  "captureId": "2GG903537H481924B",
  "orderId": "EC-123456789",
  "status": "COMPLETED",
  "amount": 29.99,
  "currency": "USD",
  "captureTime": "2024-01-15T10:35:00Z",
  "finalCapture": true,
  "sellerProtectionStatus": "ELIGIBLE",
  "grossAmount": 29.99,
  "payPalFee": 1.35,
  "netAmount": 28.64,
  "success": true
}
```

### 3. Refund PayPal Payment
**Endpoint**: `POST /api/payments/paypal/refund`

**Request**:
```json
{
  "captureId": "2GG903537H481924B",
  "amount": 29.99,
  "currency": "USD",
  "noteToPayer": "Refund for cancelled order",
  "reason": "Customer requested cancellation"
}
```

**Response**:
```json
{
  "refundId": "5O190127TN364715V",
  "captureId": "2GG903537H481924B",
  "status": "COMPLETED",
  "amount": 29.99,
  "currency": "USD",
  "refundTime": "2024-01-15T11:00:00Z",
  "invoiceId": "INV-12345",
  "noteToPayer": "Refund for cancelled order",
  "grossAmount": 29.99,
  "payPalFee": 0.00,
  "netAmount": 29.99,
  "totalRefundedAmount": 29.99,
  "success": true
}
```

### 4. PayPal Webhook
**Endpoint**: `POST /api/payments/paypal/webhook`

**Webhook Events Handled**:
- `PAYMENT.CAPTURE.COMPLETED` - Payment captured successfully
- `PAYMENT.CAPTURE.DENIED` - Payment capture denied
- `PAYMENT.CAPTURE.REFUNDED` - Payment refunded
- `CHECKOUT.ORDER.APPROVED` - Order approved
- `CHECKOUT.ORDER.CANCELLED` - Order cancelled

## 🔄 Payment Flow

### 1. Create Order Flow
```
Client → API → PayPal → User → PayPal → API → Client
```

1. **Client Request**: Send payment details to API
2. **API Processing**: Create PayPal order request
3. **PayPal Response**: Return order ID and checkout URL
4. **User Redirect**: Redirect user to PayPal checkout
5. **Payment Completion**: User completes payment on PayPal
6. **Webhook Notification**: PayPal sends webhook event
7. **Order Update**: API processes webhook and updates order status

### 2. Capture Payment Flow
```
Client → API → PayPal → API → Client
```

1. **Client Request**: Send capture request with order ID
2. **API Processing**: Call PayPal capture API
3. **PayPal Response**: Return capture details
4. **API Response**: Return capture information to client

### 3. Refund Flow
```
Client → API → PayPal → API → Client
```

1. **Client Request**: Send refund request with capture ID
2. **API Processing**: Call PayPal refund API
3. **PayPal Response**: Return refund details
4. **API Response**: Return refund information to client

## 🛡️ Security Features

### 1. Webhook Signature Validation
- Validates PayPal webhook signatures
- Prevents unauthorized webhook calls
- Ensures data integrity

### 2. Access Token Management
- Automatic token refresh
- Secure token storage
- Token expiration handling

### 3. Error Handling
- Comprehensive error logging
- Graceful error responses
- Retry mechanisms

## 🧪 Testing

### Sandbox Environment
```bash
# Test order creation
curl -X POST http://localhost:7001/api/payments/paypal/create-order \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 29.99,
    "currency": "USD",
    "description": "Test payment",
    "customerId": "test_customer_123"
  }'

# Test payment capture
curl -X POST http://localhost:7001/api/payments/paypal/capture \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "EC-123456789"
  }'

# Test payment refund
curl -X POST http://localhost:7001/api/payments/paypal/refund \
  -H "Content-Type: application/json" \
  -d '{
    "captureId": "2GG903537H481924B",
    "amount": 29.99,
    "currency": "USD"
  }'
```

### Webhook Testing
```bash
# Test webhook endpoint
curl -X POST http://localhost:7001/api/payments/paypal/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "id": "WH-123456789",
    "event_type": "PAYMENT.CAPTURE.COMPLETED",
    "create_time": "2024-01-15T10:30:00Z",
    "resource_type": "capture",
    "resource": {
      "id": "2GG903537H481924B",
      "status": "COMPLETED"
    }
  }'
```

## 📊 Monitoring

### Logging
- Payment creation logs
- Capture success/failure logs
- Refund processing logs
- Webhook event logs
- Error logs with stack traces

### Metrics
- Payment success rate
- Average processing time
- Error rates by type
- Webhook processing statistics

## 🔧 Integration Examples

### Frontend Integration
```javascript
// Create PayPal order
const createOrder = async (paymentData) => {
  const response = await fetch('/api/payments/paypal/create-order', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(paymentData)
  });
  
  const order = await response.json();
  
  // Redirect to PayPal checkout
  if (order.success && order.checkoutUrl) {
    window.location.href = order.checkoutUrl;
  }
};

// Handle PayPal return
const handlePayPalReturn = async (orderId) => {
  const response = await fetch('/api/payments/paypal/capture', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ orderId })
  });
  
  const capture = await response.json();
  
  if (capture.success) {
    // Payment successful
    console.log('Payment captured:', capture);
  }
};
```

### Backend Integration
```csharp
// Inject PayPal service
private readonly IPayPalService _payPalService;

// Create order
var orderRequest = PayPalService.CreateSimpleOrder(29.99m, "USD", "Premium subscription");
var orderResponse = await _payPalService.CreateOrderAsync(orderRequest);

// Capture payment
var captureRequest = new PayPalCaptureRequest { NoteToPayer = "Thank you!" };
var captureResponse = await _payPalService.CapturePaymentAsync(orderId, captureRequest);

// Refund payment
var refundRequest = new PayPalRefundRequest 
{ 
    NoteToPayer = "Refund for cancellation",
    Reason = "Customer requested cancellation"
};
var refundResponse = await _payPalService.RefundPaymentAsync(captureId, refundRequest);
```

## 🚨 Error Handling

### Common Errors
1. **Invalid Credentials**: Check PayPal Client ID and Secret
2. **Invalid Order ID**: Ensure order exists and is in correct state
3. **Invalid Capture ID**: Ensure capture exists and can be refunded
4. **Webhook Signature Invalid**: Check webhook secret configuration
5. **Network Errors**: Implement retry logic for transient failures

### Error Response Format
```json
{
  "success": false,
  "errorMessage": "PayPal API error: 400 - Invalid order ID",
  "orderId": "",
  "status": "FAILED"
}
```

## 📈 Best Practices

### 1. Security
- Always validate webhook signatures
- Use HTTPS for all API calls
- Store sensitive data securely
- Implement proper error handling

### 2. Performance
- Implement caching for access tokens
- Use async/await for all PayPal API calls
- Implement retry logic for failed requests
- Monitor API response times

### 3. User Experience
- Provide clear error messages
- Implement proper loading states
- Handle edge cases gracefully
- Provide payment status updates

### 4. Monitoring
- Log all payment events
- Monitor success/failure rates
- Track processing times
- Set up alerts for failures

## 🔗 Resources

- [PayPal Developer Documentation](https://developer.paypal.com/docs/)
- [PayPal REST API Reference](https://developer.paypal.com/docs/api/)
- [PayPal Webhooks Guide](https://developer.paypal.com/docs/api-basics/notifications/webhooks/)
- [PayPal Sandbox Testing](https://developer.paypal.com/docs/api-basics/sandbox/)

## 🆘 Support

For issues and questions:
1. Check PayPal Developer documentation
2. Review application logs
3. Test with PayPal sandbox
4. Contact PayPal Developer Support

---

**Built with ❤️ using PayPal REST API and .NET 9** 🚀✨ 