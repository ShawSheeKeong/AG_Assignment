# BFF Design (Quotation + Payment)

# Assumptions

This design assumes:

- Quotation and Payment microservices already exist
- Payment gateway integration is handled by Payment service
- Authentication handled by centralized identity provider
- Infrastructure deployed in AWS

---

# Architecture Overview

## High-Level Architecture

```
                    Internet
                       │
                       ▼
                  API Gateway
                       │
           ┌───────────┴─────────────┐
           ▼                         ▼
      Customer BFF              Agent BFF
           │                         │
           ├───────────────┬─────────┤
           ▼               ▼         ▼
      Quotation MS     Payment MS  Policy MS
           │               │
           ▼               ▼
        Database    External System
                           │
                           ▼
                    Payment Gateway
```

### Key Components

| Component              | Responsibility                                           |
| ---------------------- | -------------------------------------------------------- |
| API Gateway            | Entry point for external traffic, routing, rate limiting |
| Customer BFF           | Aggregates APIs optimized for customer experience        |
| Agent BFF.             | Provides extended APIs for agent workflows               |
| Quotation Microservice | Handles quotation generation and pricing                 |
| Payment Microservice   | Processes payments through payment gateway               |
| Policy Service         | Manages policy lifecycle                                 |
| Payment Gateway        | External system for payment processing                   |

---

### Customer Portal Needs

- Generate quotation
- Purchase policy
- View policies
- Simplified responses
- Minimal steps

### Agent Portal Needs

- Create quotation for customer
- Override quotation fields
- Manage multiple customers
- Submit policy purchase on behalf of customers

Using BFF allows:

- Tailored API responses
- Reduced frontend complexity
- Aggregation of multiple services
- Improved performance via optimized endpoints

---

# API Design

## Customer BFF APIs

| Endpoint                | Description            |
| ----------------------- | ---------------------- |
| GET /customer/quote     | Generate quotation     |
| POST /customer/purchase | Purchase policy        |
| GET /customer/policies  | Retrieve user policies |

Example Flow

```
Customer Portal
     │
     ▼
Customer BFF
     │
     ├── Quotation Service
     └── Payment Service
```

---

## Agent BFF APIs

| Endpoint                   | Description               |
| -------------------------- | ------------------------- |
| GET /agent/quotes          | Retrieve quotes           |
| POST /agent/quote          | Create quote for customer |
| POST /agent/quote/override | Modify premium            |
| POST /agent/purchase       | Submit purchase           |

Agents require **more capabilities and validation logic**, which is implemented in the Agent BFF.

---

# Data Consistency Strategy

Because policy purchase involves multiple services, **distributed transaction management is required**.

This design uses the **Saga Pattern**.

## Policy Purchase Saga

```
1. Create Quote
2. Create Policy (Pending)
3. Initiate Payment
4. Payment Success
5. Activate Policy
```

If payment fails:

```
Cancel Policy
Rollback Transaction
```

### Implementation Options

- Event-driven architecture
- Message broker

Recommended technologies:

- AWS SNS/SQS

---

# Security Strategy

Because the platform handles **PII and payments**, strong security practices are required.

## Authentication

OAuth 2.0

Clients obtain **JWT tokens** which are validated by the API Gateway and BFF services.

---

## Authorization

Role-based access control (RBAC)

Roles:

- Customer
- Agent
- Admin

Example:

```
Customer → Customer APIs only
Agent → Agent APIs
```

---

## Data Protection

### Data in Transit

All communication secured with:

```
TLS 1.2+
HTTPS
```

### Data at Rest

Sensitive data encrypted using:

```
AES-256
```

Examples of protected data:

- Personal identification information (PII)
- Payment tokens
- Policyholder details

### Payment Security

Credit card information should never be stored.

Instead use:

```
Tokenization from payment gateway
```

---

# Observability Strategy

Observability ensures visibility into system health and performance.

The system implements **three pillars of observability**.

---

## Logging

Structured logging using:

```
Serilog (Splunk)
```

Log attributes:

- RequestId
- UserId
- ServiceName
- ResponseTime
- ErrorCode

---

## Metrics

Metrics collected using:

```
Grafana
```

Important metrics:

- API latency
- Payment success rate
- Quote generation time
- Error rate

---

## Distributed Tracing

Tracing across services using:

```
OpenTelemetry
```

This enables tracking a request across:

```
Gateway → BFF → Quotation → Payment
```

---

# Resiliency and Fault Handling

External systems such as payment gateways may fail.

Strategies include:

### Retry Policies

Implemented using Polly.

Example:

```
Retry 3 times
Exponential backoff
```

### Circuit Breaker

Prevents cascading failures when downstream services are unavailable.

### Idempotency

Prevent duplicate payments.

Example:

```
POST /payment
Header: Idempotency-Key (Unique client-generated UUID, so request will be processed only once)
```

---

# Deployment Strategy

The system is containerized and deployed using modern cloud infrastructure.

## Containerization

```
Docker
```

Each service runs independently.

---

## Orchestration

```
Kubernetes
```

Benefits:

- auto-scaling
- rolling updates
- self-healing

---

## CI/CD Pipeline

Example pipeline using GitHub Actions:

1. Build
2. Run unit tests
3. Static code analysis (SonarQube scan)
4. Security vulnerabilities analysis (Nexus scan)
5. Build Docker image
6. Push to container registry
7. Deploy to cloud environment (DEV, QA, PROD)

---

# Scalability Strategy

The system scales horizontally.

```
BFF Services → Stateless → Easily Scalable
```

Auto-scaling triggers:

- CPU utilization
- Request throughput

---

# Key Design Principles

The system follows these architectural principles:

- **Microservices architecture**
- **API-first design**
- **Loose coupling**
- **High cohesion**
- **Event-driven communication**
- **Security by design**
- **Observability-first approach**

---

# Conclusion

The proposed BFF architecture provides:

- Tailored APIs for different frontend clients
- Improved frontend performance
- Strong security for PII and payment data
- High observability and operational visibility
- Scalable microservice architecture

This design ensures the platform can support both **customer self-service purchases** and **agent-assisted policy purchases** efficiently.
