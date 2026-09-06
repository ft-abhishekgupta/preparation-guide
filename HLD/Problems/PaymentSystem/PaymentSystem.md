# Payment System

Payment processing systems like Stripe allow business (referred to throughout this breakdown as merchants) to accept payment from customers, without having to build their own payment processing infrastructure. Customer input their payment details on the merchant's website, and the merchant sends the payment details to Stripe. Stripe then processes the payment and returns the result to the merchant.

## Requirements

![alt text](image.png)

## Core Entities

![alt text](image-1.png)

## API Design

```
POST /payment-intents -> paymentIntentId
{
  "amountInCents": 2499,
  "currency": "usd",
  "description": "Order #1234",
}

POST /payment-intents/{paymentIntentId}/transactions
{
  "type": "charge",
  "card": {
    "number": "4242424242424242",
    "exp_month": 12,
    "exp_year": 2025,
    "cvc": "123"
  }
}

GET /payment-intents/{paymentIntentId} -> PaymentIntent

POST {merchant_webhook_url}
{
  "type": "payment.succeeded",
  "data": {
    "paymentId": "pay_123",
    "amountInCents": 2499,
    "currency": "usd",
    "status": "succeeded"
  }
}
```

## HLD

### Merchants should be able to initiate payment requests

![alt text](image-2.png)

### Users should be able to pay for products with credit/debit cards

![alt text](image-3.png)

- External Payment Network are connected via highly secure line, using different security protocols, VPNs, certificates

## Deep Dives

### The system should be highly secure

**Is the merchant correct**

- Basic API Key Auth: Simple but vulnerable
- Enhanced API Key Management with Request Signing
  - Validated the signature via private key
  - Validate Nonce if it has not been used earliar

  - ```

    // Example request with signature
    {
    "method": "POST",
    "path": "/payment-intents/{paymentIntentId}/transactions",
    "body": {
        // body here
    },
    "headers": {
        "Authorization": "Bearer pk_live_51NzQRtGswQnXYZ8o", // API Key
        "X-Request-Timestamp": "2023-10-15T14:22:31Z", // Timestamp
        "X-Request-Nonce": "a1b2c3d4-e5f6-7890-abcd-ef1234567890", // Nonce
        "X-Signature": "sha256=7f83b1657ff1fc53b92dc18148a1d65dfc2d4b1fa3d677284addd200126d9069" // Hash of body
    }
    }
    ```

  ```

  ```

**Protecting customers sensitive info**

- iFrame and Encryption
  - iFrame domain is of Payment Service
  - Data encrypted inside iFrame via RSA

![alt text](image-4.png)

### The system should guarantee durability and auditability with no transaction data ever being lost, even in case of failures

- Audit Table: Write Ahead Log, Overhead
- BETTER: Database + CDC (Change Data Capture) + Event Stream

![alt text](image-5.png)

### The system should guarantee transaction safety and financial integrity despite the inherently asynchronous nature of external payment networks

- Pending States with Manual Reconcillation Cron Job
  - Good but bad experience for user
- BETTER: Event driven Safety with Reconcilliation
  - Worker will verify for timedout transaction via Event Stream
  - ![alt text](image-6.png)

### The system should be scalable to handle high transaction volume (10,000+ TPS)

- Servers can scale horizontally
- Kafka can handle millions of messages per second, each partition can handle 5-10k mps
  - Partition by PayementIntentId, for ordered transaction
  - Replicate by 3, for high durability
- Database
  - Move old transactions to S3
  - Read replicas

### How can we expand the design to support Webhooks?

- Webhook service listens to kafka event stream, checks DB for what events merchant care about, and calls to merchant endpoint to deliver the data
  ![alt text](image-7.png)

## FINAL

![alt text](image-8.png)
