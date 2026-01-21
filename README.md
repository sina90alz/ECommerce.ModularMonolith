# ECommerce.ModularMonolith

A **production-oriented modular monolith** built with **ASP.NET Core**, **Clean Architecture**, **CQRS**, and **asynchronous messaging**.  
This project demonstrates how to design a system that is **modular, reliable, and ready to evolve into a distributed architecture**.

---

## 🎯 Purpose

This repository is a hands-on architectural project focused on mastering:

- Modular Monolith architecture
- Clean Architecture principles
- CQRS with MediatR
- Database-per-module strategy
- Reliable asynchronous messaging
- Outbox & Inbox patterns
- RabbitMQ fundamentals
- Distributed-system safety inside a monolith

The focus is on **correct boundaries, reliability, and evolvability**, not feature quantity.

---

## 🧱 Architecture Overview

The solution follows a **vertical modular structure** with **strong internal boundaries**:

```
src/
 ├─ ECommerce.API                  # Composition Root (HTTP, DI, hosted services)
 │
 ├─ Modules/
 │   ├─ Orders/
 │   │   ├─ Orders.Domain          # Order aggregate, business rules
 │   │   ├─ Orders.Application     # CQRS commands, handlers, validation
 │   │   ├─ Orders.Infrastructure  # EF Core, DbContext, Outbox, RabbitMQ publisher
 │   │   └─ Orders.Contracts       # Integration events / public contracts
 │   │
 │   ├─ Products/
 │   │   ├─ Products.Domain        # Product aggregate (stock, pricing)
 │   │   ├─ Products.Application   # Use cases & abstractions
 │   │   ├─ Products.Infrastructure# EF Core, Inbox, RabbitMQ consumer
 │   │   └─ Products.Contracts     # Public read contracts
 │
 └─ tests/
     └─ Architecture.Tests         # Enforced architecture & dependency rules
```

---

## 🧭 Core Principles

- Each module owns its **Domain, Application, and Infrastructure**
- **No shared `DbContext`**
- **No cross-module domain references**
- Modules communicate **only via integration events or contracts**
- Clean Architecture dependency flow:
  - Infrastructure → Application → Domain
- API acts as the **Composition Root**
- CQRS by default:
  - Commands mutate state
  - Queries are isolated
- **Reliability over immediacy**
  - State changes are persisted first
  - Events are published asynchronously
- Infrastructure is **replaceable**
  - RabbitMQ is abstracted behind `IMessageBus`
  - Business logic is transport-agnostic

---

## 🧩 Modules

### Orders Module

**Responsibilities**
- Owns the Order aggregate
- Handles order lifecycle:
  - Create
  - Pay
  - Cancel

**Key concepts**
- CQRS with MediatR
- EF Core with module-owned DbContext
- Domain invariants enforced inside aggregate
- Integration events emitted via Outbox

**Endpoints**
```
POST /api/orders
POST /api/orders/{id}/pay
POST /api/orders/{id}/cancel
```

---

### Products Module

**Responsibilities**
- Owns the Product aggregate
- Manages product stock
- Reacts to Orders integration events

**Key concepts**
- Inbox pattern for idempotency
- Asynchronous event consumption
- Stock updates driven by Orders events
- Safe reprocessing & duplicate protection

---

## 🗄️ Database Strategy

- **Database per module** (logical isolation)
- Orders and Products each own their schema
- EF Core migrations live inside the module

---

## 🔄 Asynchronous Messaging

- RabbitMQ as message broker
- Topic exchange: `ecommerce.events`
- Integration-event-based communication
- Outbox (Orders) + Inbox (Products)

---

## 🧠 Distributed-System Readiness

The system already supports:
- At-least-once delivery
- Idempotency
- Explicit retries
- Poison-message isolation
- Transport abstraction

---

## 🚀 Running the Project

### Start RabbitMQ

```bash
docker run -d --name rabbitmq   -p 5672:5672   -p 15672:15672   rabbitmq:3-management
```

RabbitMQ UI:
```
http://localhost:15672
```

---

### Run the API

```bash
dotnet build
dotnet run --project src/ECommerce.API
```

---

## 🔮 Next Steps

- Message versioning
- Delayed retries
- Kafka support
- Observability & tracing
