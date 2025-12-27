# ECommerce.ModularMonolith

A **production-oriented modular monolith** built with **ASP.NET Core**, **Clean Architecture**, and **CQRS**.  
This project demonstrates how to design a system that is **modular, maintainable, and ready to evolve into microservices**.

---

## 🎯 Purpose

This repository is part of my hands-on learning journey to master:

- Modular Monolith architecture
- Clean Architecture principles
- CQRS with MediatR
- Database-per-module strategy
- Real-world ASP.NET Core application wiring

The focus is on **correct architecture and boundaries**, not feature quantity.

---

## 🧱 Architecture Overview

The solution follows a **vertical modular structure**:

```
src/
 ├─ ECommerce.API                # Composition Root (HTTP, DI, configuration)
 ├─ Modules/
 │   ├─ Orders/
 │   │   ├─ Orders.Domain        # Domain entities & business rules
 │   │   ├─ Orders.Application   # Use cases (CQRS, MediatR)
 │   │   ├─ Orders.Infrastructure# EF Core, persistence, repositories
 │   │   └─ Orders.Contracts     # Public contracts (DTOs / future events)
```

### Core Principles

- Each module owns its **domain, application, and persistence**
- No shared `DbContext`
- No cross-module domain references
- Infrastructure depends on Application & Domain (never the opposite)
- API acts as the **composition root**

---

## 🧩 Current Module: Orders

### Implemented features

- Create Order use case
- CQRS with MediatR
- EF Core persistence
- SQL Server database
- Module-owned migrations

### Example endpoint

```
POST /api/orders
```

Returns:

```
<Guid>
```

---

## 🗄️ Database Strategy

- One database per module (logical isolation)
- Orders module uses `OrdersDb`
- Migrations are stored inside the module:

```
Orders.Infrastructure/Persistence/Migrations
```

This makes the module **ready to be extracted as a microservice** later with minimal refactoring.

---

## 🚀 Running the project

### Prerequisites

- .NET SDK 9.x
- SQL Server (LocalDB is sufficient)

### Run locally

```bash
dotnet build
dotnet run --project src/ECommerce.API
```

The API starts on:

```
http://localhost:5240
```

### Test the endpoint (PowerShell)

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5240/api/orders
```

---

## 🧠 Why a Modular Monolith?

A modular monolith allows you to:

- Keep deployment simple
- Enforce strong internal boundaries
- Avoid distributed-system complexity too early
- Transition to microservices **only when it makes sense**

This project shows how to do that **properly**.

---

## 🔮 Planned Improvements

- Enforce strict module boundaries
- Add Products and Customers modules
- Validation & pipeline behaviors
- Asynchronous communication (events)
- Microservice extraction readiness

---
