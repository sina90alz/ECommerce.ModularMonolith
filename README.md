# ECommerce.ModularMonolith

Production-oriented modular monolith built with ASP.NET Core, Clean Architecture, CQRS, EF Core, SQL Server, RabbitMQ, and Outbox/Inbox messaging.

The project focuses on module boundaries, reliable asynchronous communication, and an architecture that can evolve toward distributed services without starting from scratch.

## Architecture Overview

The API is the composition root. Business capabilities live in separate modules, and each module owns its own domain model, application layer, infrastructure, and database context.

```text
src/
|-- ECommerce.API
|   |-- Controllers
|   |-- Behaviors
|   `-- Middleware
`-- Modules/
    |-- Orders/
    |   |-- Orders.Domain
    |   |-- Orders.Application
    |   |-- Orders.Infrastructure
    |   `-- Orders.Contracts
    `-- Products/
        |-- Products.Domain
        |-- Products.Application
        |-- Products.Infrastructure
        `-- Products.Contracts

tests/
`-- Architecture.Tests
```

Each module follows the same internal structure:

- `Domain`: aggregates, value objects, domain events, and business rules.
- `Application`: use cases, CQRS commands, handlers, validators, and abstractions.
- `Infrastructure`: EF Core, repositories, messaging, background services, and module wiring.
- `Contracts`: public contracts that can be referenced by other modules.

Dependency direction follows Clean Architecture:

```text
Infrastructure -> Application -> Domain
```

Core rules:

- No shared DbContext between modules.
- No cross-module domain references.
- Modules communicate through contracts and integration events.
- RabbitMQ is hidden behind module-local message bus abstractions.
- Architecture tests enforce the main dependency boundaries.

## Runtime Flow

Typical request flow:

```text
HTTP Controller -> MediatR Command -> Application Handler -> Domain Model -> Repository/DbContext
```

Order creation uses `Products.Contracts` to read the product snapshot needed by the Orders module. Order payment and cancellation write integration events to the Orders Outbox in the same database transaction as the order state change. A background publisher sends those events to RabbitMQ. The Products module consumes the events through its Inbox and updates stock idempotently.

```text
OrdersDb -> OutboxPublisher -> RabbitMQ exchange -> Products Inbox -> ProductsDb
```

## Modules

### Orders

Owns order creation, payment, and cancellation.

Key implementation details:

- Order aggregate and domain events.
- CQRS commands handled with MediatR.
- EF Core persistence with `OrdersDbContext`.
- Outbox table, retry tracking, and dead-letter marking.
- RabbitMQ publisher hosted from the API process.

Endpoints:

```http
POST /api/orders
POST /api/orders/{orderId}/pay
POST /api/orders/{orderId}/cancel
```

### Products

Owns products and stock.

Key implementation details:

- Product aggregate with stock rules.
- EF Core persistence with `ProductsDbContext`.
- Product read contract exposed through `Products.Contracts`.
- Inbox table for duplicate-message protection.
- RabbitMQ consumer hosted from the API process.

Endpoint:

```http
POST /api/products
```

## Persistence

The solution uses logical database-per-module ownership:

- Orders owns `OrdersDbContext`, order tables, and outbox messages.
- Products owns `ProductsDbContext`, product tables, and inbox messages.
- EF Core migrations live inside each module's Infrastructure project.

## Messaging

RabbitMQ is used for asynchronous module communication.

- Exchange: `ecommerce.events`
- Products queue: `products.inbox`
- Orders publishes events through the Outbox.
- Products consumes order events through the Inbox.
- Supported routing keys include `orders.order-paid.v1` and `orders.order-cancelled.v1`.

Reliability behavior:

- Orders persists state changes before publishing messages.
- The Outbox publisher retries failed publishes and marks exhausted messages as dead-lettered.
- Products records message IDs in the Inbox before applying stock changes.
- Duplicate messages are acknowledged without applying the same stock change twice.

## Requirements

- .NET 9 SDK
- SQL Server
- Docker, for local RabbitMQ

## Run Locally

Start RabbitMQ:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

RabbitMQ management UI:

```text
http://localhost:15672
```

Configure SQL Server connection strings in `src/ECommerce.API/appsettings.json` if needed:

```json
{
  "ConnectionStrings": {
    "OrdersDb": "Server=localhost;Database=OrdersDb;Trusted_Connection=True;TrustServerCertificate=True",
    "ProductsDb": "Server=localhost;Database=ProductsDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Build and run:

```bash
dotnet build
dotnet run --project src/ECommerce.API
```

Run tests:

```bash
dotnet test
```

## Project Status

Implemented:

- Orders and Products modules
- CQRS command handling
- Module-owned EF Core DbContexts
- RabbitMQ integration events
- Orders Outbox with retries and dead-letter marking
- Products Inbox with idempotency
- Architecture boundary tests
