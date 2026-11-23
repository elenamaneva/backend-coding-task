# Marine Insurance Claims API

A multi-tier .NET 10 Web API for managing marine insurance claims and covers. Built following SOLID principles with proper layering, validation, and asynchronous auditing.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Business Rules](#business-rules)
- [Project Structure](#project-structure)
- [Design Decisions](#design-decisions)
- [Running Tests](#running-tests)
- [Configuration](#configuration)

---

## Features

- **RESTful API** for Claims and Covers management
- **Layered Architecture** (Controller → Service → Data)
- **SOLID Principles** throughout the codebase
- **Business Validation** with dedicated validator classes
- **Asynchronous Auditing** using in-memory Channel + BackgroundService
- **Dual Database** setup (SQL Server for audits, MongoDB for business data)
- **Swagger/OpenAPI** documentation with enum support
- **Unit Tests** with xUnit and Moq

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Layer                                 │
│  ┌─────────────────┐           ┌─────────────────┐              │
│  │ ClaimsController │           │ CoversController │              │
│  └────────┬────────┘           └────────┬────────┘              │
└───────────┼─────────────────────────────┼───────────────────────┘
            │                             │
            ▼                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Service Layer                               │
│  ┌─────────────────┐           ┌─────────────────┐              │
│  │  IClaimsService  │           │  ICoversService  │              │
│  │  ClaimsService   │           │  CoversService   │              │
│  └────────┬────────┘           └────────┬────────┘              │
│           │                             │                        │
│  ┌────────┴────────┐           ┌────────┴────────┐              │
│  │ IClaimValidator  │           │ ICoverValidator  │              │
│  └─────────────────┘           └─────────────────┘              │
└───────────┼─────────────────────────────┼───────────────────────┘
            │                             │
            ▼                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Data Layer                                 │
│  ┌─────────────────┐           ┌─────────────────┐              │
│  │  ClaimsContext   │           │   AuditContext   │              │
│  │    (MongoDB)     │           │   (SQL Server)   │              │
│  └─────────────────┘           └─────────────────┘              │
└─────────────────────────────────────────────────────────────────┘

                    Async Auditing Flow
┌─────────────────────────────────────────────────────────────────┐
│  Service ──► IAuditer ──► IAuditQueue ──► Channel (in-memory)   │
│                                               │                  │
│                     AuditBackgroundService ◄──┘                  │
│                              │                                   │
│                              ▼                                   │
│                        AuditContext                              │
└─────────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

- **.NET 10 SDK** or later
- **Visual Studio 2022** (or VS Code with C# extension)
- **SQL Server LocalDB** (included with Visual Studio)
- **MongoDB Atlas** account (free tier) or local MongoDB

---

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd KResourcing
```

### 2. Configure MongoDB Atlas

1. Create a free account at [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
2. Create a free M0 cluster
3. Create a database user (note the username and password)
4. Add your IP to Network Access (or `0.0.0.0/0` for development)
5. Get your connection string

### 3. Update Configuration

Edit `Claims/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AuditDb": "Server=(localdb)\\MSSQLLocalDB;Database=ClaimsAudit;Trusted_Connection=True;TrustServerCertificate=True;",
    "MongoDb": "mongodb+srv://<username>:<password>@<cluster>.mongodb.net/?retryWrites=true&w=majority"
  },
  "MongoDb": {
    "DatabaseName": "MarineInsurance"
  }
}
```

### 4. Verify LocalDB

```powershell
sqllocaldb info
sqllocaldb start MSSQLLocalDB
```

### 5. Run the Application

```powershell
dotnet run --project Claims
```

### 6. Access Swagger UI

Open: https://localhost:7052/swagger

---

## API Endpoints

### Covers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Covers` | Get all covers |
| GET | `/Covers/{id}` | Get cover by ID |
| POST | `/Covers` | Create a new cover |
| DELETE | `/Covers/{id}` | Delete a cover |
| POST | `/Covers/compute` | Calculate premium |

### Claims

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Claims` | Get all claims |
| GET | `/Claims/{id}` | Get claim by ID |
| POST | `/Claims` | Create a new claim |
| DELETE | `/Claims/{id}` | Delete a claim |

### Example Requests

**Create Cover:**
```json
POST /Covers
{
  "startDate": "2026-12-01T00:00:00Z",
  "endDate": "2027-06-01T00:00:00Z",
  "type": "Yacht"
}
```

**Create Claim:**
```json
POST /Claims
{
  "coverId": "<cover-id-from-above>",
  "created": "2027-01-15T00:00:00Z",
  "name": "Storm damage to hull",
  "type": "BadWeather",
  "damageCost": 25000
}
```

---

## Business Rules

### Cover Validation

| Rule | Description |
|------|-------------|
| StartDate | Cannot be in the past |
| Period | Cannot exceed 365 days (1 year) |
| EndDate | Must be after StartDate |

### Claim Validation

| Rule | Description |
|------|-------------|
| DamageCost | Cannot exceed 100,000 |
| CoverId | Must reference an existing Cover |
| Created | Must be within the Cover's StartDate and EndDate |

### Premium Calculation

**Base Rate:** 1,250 per day

**Type Multipliers:**
| Cover Type | Multiplier | Premium/Day |
|------------|------------|-------------|
| Yacht | 1.1 (+10%) | 1,375 |
| PassengerShip | 1.2 (+20%) | 1,500 |
| ContainerShip | 1.3 (+30%) | 1,625 |
| BulkCarrier | 1.3 (+30%) | 1,625 |
| Tanker | 1.5 (+50%) | 1,875 |

**Progressive Discounts:**
| Period | Yacht Discount | Other Types Discount |
|--------|----------------|---------------------|
| Days 1-30 | 0% | 0% |
| Days 31-180 | 5% | 2% |
| Days 181-365 | 8% | 3% |

---

## Project Structure

```
KResourcing/
├── Claims/                          # Main API project
│   ├── Controllers/
│   │   ├── ClaimsController.cs      # Claims HTTP endpoints
│   │   └── CoversController.cs      # Covers HTTP endpoints
│   ├── Services/
│   │   ├── IClaimsService.cs        # Claims service interface
│   │   ├── ClaimsService.cs         # Claims business logic
│   │   ├── ICoversService.cs        # Covers service interface
│   │   └── CoversService.cs         # Covers business logic + ComputePremium
│   ├── Validators/
│   │   ├── IClaimValidator.cs       # Claim validation interface
│   │   ├── ClaimValidator.cs        # Claim validation rules
│   │   ├── ICoverValidator.cs       # Cover validation interface
│   │   └── CoverValidator.cs        # Cover validation rules
│   ├── Data/
│   │   └── ClaimsContext.cs         # MongoDB DbContext
│   ├── Auditing/
│   │   ├── AuditContext.cs          # SQL Server DbContext for audits
│   │   ├── IAuditer.cs              # Auditer interface
│   │   ├── Auditer.cs               # Queues audit messages
│   │   ├── IAuditQueue.cs           # Queue interface
│   │   ├── InMemoryAuditQueue.cs    # Channel-based queue
│   │   ├── AuditBackgroundService.cs # Processes audit queue
│   │   ├── AuditMessage.cs          # Audit message DTO
│   │   ├── ClaimAudit.cs            # Claim audit entity
│   │   └── CoverAudit.cs            # Cover audit entity
│   ├── Claim.cs                     # Claim entity
│   ├── Cover.cs                     # Cover entity
│   ├── Program.cs                   # Application entry point & DI
│   └── appsettings.json             # Configuration
│
├── Claims.Tests/                    # Unit tests
│   ├── ComputePremiumTests.cs       # Premium calculation tests
│   ├── ClaimValidatorTests.cs       # Claim validation tests
│   ├── CoverValidatorTests.cs       # Cover validation tests
│   └── ClaimsControllerTests.cs     # Integration tests
│
└── README.md                        # This file
```

---

## Design Decisions

### 1. Layered Architecture

**Decision:** Controller → Service → Data layer separation

**Rationale:** 
- Controllers handle HTTP concerns only
- Services contain business logic
- Clear boundaries enable testing and maintenance

### 2. Dual Database (SQL Server + MongoDB)

**Decision:** SQL Server for audits, MongoDB for business data

**Rationale:**
- **Audit data** requires ACID compliance and immutable records → SQL Server
- **Business data** (Claims/Covers) benefits from flexible document schema → MongoDB
- Demonstrates polyglot persistence understanding

**Trade-off:** Adds operational complexity. For simpler systems, a single database would suffice.

### 3. Asynchronous Auditing with In-Memory Queue

**Decision:** Use `System.Threading.Channels` with `BackgroundService`

**Rationale:**
- HTTP responses are not blocked by audit DB writes
- Pure .NET solution, no external dependencies
- Production-ready pattern

**Trade-off:** Messages lost on crash. For production, consider Azure Service Bus.

### 4. Dedicated Validator Classes

**Decision:** Separate `IClaimValidator` and `ICoverValidator` classes

**Rationale:**
- **Single Responsibility:** Validators only validate
- **Dependency Inversion:** Services depend on interfaces
- **Testability:** Validators can be unit tested in isolation

### 5. Interface-Based Design (SOLID)

| Principle | Implementation |
|-----------|----------------|
| **S**ingle Responsibility | Each class has one job |
| **O**pen/Closed | New validators/services don't modify existing code |
| **L**iskov Substitution | Interfaces allow swapping implementations |
| **I**nterface Segregation | Small, focused interfaces |
| **D**ependency Inversion | Depend on abstractions, not concretions |

---

## Running Tests

### Run All Tests

```powershell
cd KResourcing
dotnet test
```

### Run with Verbose Output

```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Class

```powershell
dotnet test --filter "FullyQualifiedName~ComputePremiumTests"
```

### Test Coverage

| Test Class | Tests | Coverage |
|------------|-------|----------|
| ComputePremiumTests | 10 | Premium calculation |
| CoverValidatorTests | 8 | Cover validation rules |
| ClaimValidatorTests | 9 | Claim validation rules |

---

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "AuditDb": "Server=(localdb)\\MSSQLLocalDB;Database=ClaimsAudit;...",
    "MongoDb": "mongodb+srv://user:pass@cluster.mongodb.net/..."
  },
  "MongoDb": {
    "DatabaseName": "MarineInsurance"
  }
}
```

### Environment Variables (Alternative)

```powershell
$env:ConnectionStrings__MongoDb = "mongodb+srv://..."
$env:ConnectionStrings__AuditDb = "Server=..."
```

---

## Enums Reference

### ClaimType

| Value | Description |
|-------|-------------|
| `Collision` | Ship crashed into another vessel, dock, or object |
| `Grounding` | Ship ran aground (hit sea floor or beach) |
| `BadWeather` | Damage caused by storms, waves, or wind |
| `Fire` | Fire broke out on the vessel |

### CoverType

| Value | Description | Premium Multiplier |
|-------|-------------|-------------------|
| `Yacht` | Recreational yacht | +10% |
| `PassengerShip` | Passenger ship | +20% |
| `ContainerShip` | Container cargo ship | +30% |
| `BulkCarrier` | Bulk carrier vessel | +30% |
| `Tanker` | Oil/fuel tanker | +50% |

---

