# Payment API Documentation

## Overview

The Payment API provides comprehensive third-party payment integration using Stripe. It supports payment processing, subscriptions, payment method validation, and payment history tracking.

## Features

- ✅ **Payment Processing**: Create and process payments with Stripe
- ✅ **Payment Validation**: Validate payment methods before processing
- ✅ **Payment History**: Track and retrieve payment history
- ✅ **Subscription Management**: Create and manage subscriptions
- ✅ **MongoDB Integration**: Store payment and subscription data
- ✅ **Error Handling**: Comprehensive error handling and logging
- ✅ **FastEndpoints**: High-performance API endpoints

## Configuration

### Stripe Setup

1. **Get Stripe Keys**: Sign up for a Stripe account and get your API keys
2. **Update Configuration**: Update `appsettings.json` with your Stripe keys:

```json
{
  "PaymentSettings": {
    "StripeSecretKey": "sk_test_your_stripe_secret_key_here",
    "StripePublishableKey": "pk_test_your_stripe_publishable_key_here",
    "WebhookSecret": "whsec_your_webhook_secret_here"
  }
}
```

### Environment Variables (Recommended)

For production, use environment variables:

```bash
export STRIPE_SECRET_KEY="sk_live_your_production_key"
export STRIPE_PUBLISHABLE_KEY="pk_live_your_production_key"
export STRIPE_WEBHOOK_SECRET="whsec_your_webhook_secret"
```

## API Endpoints

### 1. Create Payment

**POST** `/api/payments/create`

Creates a new payment intent with Stripe.

**Request Body:**
```json
{
  "amount": 2000,
  "currency": "USD",
  "paymentMethodId": "pm_1234567890",
  "customerId": "cus_1234567890",
  "description": "Payment for order #12345",
  "capture": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "paymentId": "pi_1234567890",
    "status": "requires_confirmation",
    "amount": 2000,
    "currency": "USD",
    "clientSecret": "pi_1234567890_secret_abc123"
  },
  "message": "Payment created successfully"
}
```

### 2. Process Payment

**POST** `/api/payments/process`

Processes a payment intent with the specified payment method.

**Request Body:**
```json
{
  "paymentIntentId": "pi_1234567890",
  "paymentMethodId": "pm_1234567890",
  "customerId": "cus_1234567890"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "paymentId": "pi_1234567890",
    "status": "succeeded",
    "amount": 2000,
    "currency": "USD"
  },
  "message": "Payment processed successfully"
}
```

### 3. Get Payment History

**GET** `/api/payments/history/{userId}?page=1&pageSize=10`

Retrieves payment history for a specific user with pagination.

**Response:**
```json
{
  "success": true,
  "data": {
    "payments": [
      {
        "id": "pi_1234567890",
        "customerId": "user123",
        "amount": 2000,
        "currency": "USD",
        "status": "succeeded",
        "description": "Payment for order #12345",
        "paymentMethod": {
          "id": "pm_1234567890",
          "type": "card",
          "brand": "visa",
          "last4": "4242",
          "expMonth": 12,
          "expYear": 2025
        },
        "createdAt": "2024-01-01T10:00:00Z",
        "updatedAt": "2024-01-01T10:00:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "message": "Payment history retrieved successfully"
}
```

### 4. Validate Payment Method

**POST** `/api/payments/validate`

Validates a payment method (credit card) with Stripe.

**Request Body:**
```json
{
  "cardNumber": "4242424242424242",
  "expMonth": 12,
  "expYear": 2025,
  "cvc": "123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "paymentMethod": {
      "id": "pm_1234567890",
      "type": "card",
      "brand": "visa",
      "last4": "4242",
      "expMonth": 12,
      "expYear": 2025
    }
  },
  "message": "Payment method is valid"
}
```

### 5. Create Subscription

**POST** `/api/payments/subscriptions/create`

Creates a new subscription with Stripe.

**Request Body:**
```json
{
  "customerId": "cus_1234567890",
  "priceId": "price_1234567890",
  "paymentMethodId": "pm_1234567890",
  "trialEnd": "2024-02-01T00:00:00Z"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "subscriptionId": "sub_1234567890",
    "status": "incomplete"
  },
  "message": "Subscription created successfully"
}
```

## Usage Examples

### Frontend Integration (JavaScript)

```javascript
// 1. Create Payment Intent
const createPayment = async (amount, currency, paymentMethodId, customerId) => {
  const response = await fetch('/api/payments/create', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      amount,
      currency,
      paymentMethodId,
      customerId,
      description: 'Payment for order #12345'
    })
  });
  
  return await response.json();
};

// 2. Process Payment
const processPayment = async (paymentIntentId, paymentMethodId, customerId) => {
  const response = await fetch('/api/payments/process', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      paymentIntentId,
      paymentMethodId,
      customerId
    })
  });
  
  return await response.json();
};

// 3. Validate Payment Method
const validatePaymentMethod = async (cardNumber, expMonth, expYear, cvc) => {
  const response = await fetch('/api/payments/validate', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      cardNumber,
      expMonth,
      expYear,
      cvc
    })
  });
  
  return await response.json();
};

// 4. Get Payment History
const getPaymentHistory = async (userId, page = 1, pageSize = 10) => {
  const response = await fetch(`/api/payments/history/${userId}?page=${page}&pageSize=${pageSize}`);
  return await response.json();
};
```

### Complete Payment Flow

```javascript
// Complete payment flow example
const completePayment = async () => {
  try {
    // 1. Validate payment method
    const validation = await validatePaymentMethod(
      '4242424242424242', // Test card number
      12, // Expiry month
      2025, // Expiry year
      '123' // CVC
    );
    
    if (!validation.data.isValid) {
      throw new Error(validation.data.errorMessage);
    }
    
    // 2. Create payment intent
    const paymentIntent = await createPayment(
      2000, // $20.00 in cents
      'USD',
      validation.data.paymentMethod.id,
      'cus_1234567890'
    );
    
    if (!paymentIntent.success) {
      throw new Error(paymentIntent.data.errorMessage);
    }
    
    // 3. Process payment
    const result = await processPayment(
      paymentIntent.data.paymentId,
      validation.data.paymentMethod.id,
      'cus_1234567890'
    );
    
    if (result.success && result.data.status === 'succeeded') {
      console.log('Payment successful!');
      return result.data;
    } else {
      throw new Error(result.data.errorMessage || 'Payment failed');
    }
    
  } catch (error) {
    console.error('Payment error:', error.message);
    throw error;
  }
};
```

## Error Handling

The API provides comprehensive error handling:

### Common Error Responses

```json
{
  "success": false,
  "data": {
    "success": false,
    "errorMessage": "Card declined. Please try a different card."
  },
  "message": "Failed to process payment"
}
```

### Error Types

- **Validation Errors**: Invalid card details, expired cards
- **Stripe Errors**: Declined payments, insufficient funds
- **Network Errors**: Connection issues, timeout
- **Server Errors**: Internal server errors

## Testing

### Test Card Numbers

Use these test card numbers for development:

- **Visa**: `4242424242424242`
- **Visa (debit)**: `4000056655665556`
- **Mastercard**: `5555555555554444`
- **American Express**: `378282246310005`
- **Discover**: `6011111111111117`

### Test CVC Codes

- **Success**: `123`
- **Decline**: `000`

## Security Considerations

1. **HTTPS Only**: Always use HTTPS in production
2. **API Key Security**: Never expose Stripe secret keys in client-side code
3. **Webhook Verification**: Verify webhook signatures
4. **Input Validation**: Validate all input data
5. **Rate Limiting**: Implement rate limiting to prevent abuse

## Monitoring and Logging

The payment service includes comprehensive logging:

- Payment creation and processing events
- Error logging with stack traces
- Performance metrics
- Security events

## MongoDB Collections

The payment service uses MongoDB for data persistence:

- **payments**: Payment documents with Stripe payment intent IDs
- **subscriptions**: Subscription documents with Stripe subscription IDs

## Dependencies

- **Stripe.net**: Official Stripe .NET library
- **MongoDB.Driver**: MongoDB driver for .NET
- **FastEndpoints**: High-performance API framework
- **Serilog**: Structured logging

## Support

For issues or questions:

1. Check the Stripe documentation
2. Review the application logs
3. Test with Stripe's test mode first
4. Contact the development team 