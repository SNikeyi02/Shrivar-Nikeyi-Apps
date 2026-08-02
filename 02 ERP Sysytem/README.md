# # TechMove Logistics — Enterprise Resource Planning (ERP) System

**Enterprise-Grade Multi-Modal Logistics Platform & Contract Management Ecosystem**

TechMove Logistics ERP is a decoupled enterprise ecosystem designed to streamline the lifecycle of commercial logistics contracts. Built with an independent **ASP.NET Core MVC Frontend Client Application** interacting securely via JWT with a **Core backend RESTful Data API**, the system automates client onboarding, dynamic Service Level Agreement (SLA) generation, and multi-modal freight transaction processing (Road, Sea, Air).

---

## 🏛️ System Architecture & Design Patterns

The solution is split into two primary containerized micro-ecosystems working in tandem:

1. **`glms-frontend-web` (ERP):** An interactive MVC interface using Razor templates, Bootstrap 5 UI structures, and cookie-based application state routing.
2. **`glms-backend-api` (ERP.Api):** A headless database-facing transaction service secured via Json Web Tokens (JWT) that manages domain validation rules and handles physical SQL operations.

```text
┌────────────────────────┐      JWT Bearer      ┌────────────────────────┐      EF Core      ┌───────────────────┐
│   glms-frontend-web    │─────────────────────>│    glms-backend-api    │──────────────────>│ SQL Server / DB   │
│   (ASP.NET Core MVC)   │<─────────────────────│   (RESTful Web API)    │<──────────────────│ (Ecosystem Matrix)│
└────────────────────────┘     JSON Streams     └────────────────────────┘                   └───────────────────┘

```

The codebase uses specific software design patterns to enforce compliance with clean-code parameters:

### 🔹 Creational Patterns

* **Factory Method Pattern:** Implemented via a specialized polymorphic service factory configuration. It abstracts the structural creation steps for `Road`, `Sea`, and `Air` cargo request variations, allowing team developers to easily introduce alternative transport options (such as *Rail* or *Drone* freight metrics) without modifying the controllers.

### 🔹 Behavioral Patterns

* **Observer Pattern:** Implemented via contract modification states. When a logistics contract status transitions (e.g., from `Pending` to `Active`), subscribers automatically execute status notification operations across target system endpoints.
* **Strategy Pattern:** Utilized within the external currency synchronization engine to encapsulate exchange rate handling algorithms. The service seamlessly swaps between active external API integrations and local JSON data structure fallbacks.

### 🔹 Structural Patterns

* **Dependency Injection (DI):** Fully leveraged within application startups to decouple controllers from business layer abstractions like `ICurrencyService`, `IAuthService`, and individual database client definitions.

---

## 🛠️ Tech Stack

* **Core Framework Engine:** Microsoft .NET 10.0 (C# 14 syntax integrations)
* **Identity & Access Management:** ASP.NET Core Identity with cross-application JWT tokenization and dual-layer authorization validation (Cookie Auth ↔️ Bearer Token).
* **Persistence Layer Data Mapping:** Entity Framework Core (EF Core 10) utilizing Eager Loading tracking metrics (`.Include()` & `.ThenInclude()`).
* **Database Platform:** Microsoft SQL Server (Transact-SQL optimization).
* **Containerization Engine:** Docker Desktop using Docker Compose specifications.
* **External Integrations:** [ExchangeRate-API Engine](https://www.exchangerate-api.com/) for live currency rate calculation between USD and South African Rand (ZAR).

---

## 📂 Project Repository Structure

```text
📦 prog7311-g2-2026-poe-SNikeyi02
 ┣ 📂 ERP                   # Frontend Web App Project Root
 ┃ ┣ 📂 Controllers          # UI Flow Controllers (Admin, ServiceRequest, Account)
 ┃ ┣ 📂 Views                # Razor View Templates (AdminDashboard, Layout Components)
 ┃ ┗ 📜 Dockerfile           # Multi-Stage Build Script for Web Frontend
 ┣ 📂 ERP.Api               # Backend Web API Project Root
 ┃ ┣ 📂 Data                 # Core DbContext, Migrations, ApplicationUser Claims
 ┃ ┣ 📂 Models               # Entities & Data Transfer Objects (Client, Contract, DTOs)
 ┃ ┣ 📂 Services             # CurrencyService, SLA Management Layers
 ┃ ┗ 📜 Dockerfile           # Multi-Stage Build Script for Backend API
 ┣ 📂 TechMoveTests         # xUnit / Moq Automated Testing Framework
 ┣ 📜 docker-compose.yml    # Main Orchestration Environment Blueprint
 ┗ 📜 README.md             # Systems Engineering Documentation

```

---

## 🚦 Getting Started & Environment Initialization

### 1️⃣ Core Prerequisites

Ensure the following development software versions are pre-installed before beginning system environment allocation paths:

* **Docker Desktop** (v4.20+ with WSL 2 backend integration enabled)
* **.NET 10 SDK** (v10.0.300+ compiler tooling parameters)
* **SQL Server Management Studio (SSMS)** or Azure Data Studio (for structural ledger review paths)

### 2️⃣ Database Schema Architecture

If manual environment initialization or deep testing overrides require a structural relational schema injection without running EF Core automated code-first generation tracking parameters, run the following script within an open Query Editor targeting your SQL instance node:

```sql
USE TechmoveERP;
GO

-- 1. Create Client Table Infrastructure
CREATE TABLE Clients (
    ClientID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Region NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL
);

-- 2. Create Contract Storage Entities
CREATE TABLE Contracts (
    ContractId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClientID UNIQUEIDENTIFIER NOT NULL,
    StartDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    EndDate DATETIME2 NOT NULL,
    Status INT NOT NULL, -- Enum Definition Mappings: 0=OnHold, 1=Active, 2=Expired, 3=Draft
    ServiceLevel NVARCHAR(MAX) NULL,
    AgreementFilePath NVARCHAR(500) NULL,
    CONSTRAINT FK_Contracts_Clients_ClientID FOREIGN KEY (ClientID) REFERENCES Clients(ClientID) ON DELETE CASCADE
);

-- 3. Create Multi-Modal Service Request Records
CREATE TABLE ServiceRequests (
    ServiceRequestId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ContractId UNIQUEIDENTIFIER NOT NULL,
    Description NVARCHAR(MAX) NOT NULL DEFAULT '',
    CostUSD DECIMAL(18,2) NOT NULL,
    CostZAR DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL, -- Enum Tracking Matrix: 0=Pending, 1=Approved, 2=Completed
    Type INT NOT NULL,   -- Poly-Freight Enums: 0=Road, 1=Sea, 2=Air
    Weight FLOAT NOT NULL,
    CONSTRAINT FK_ServiceRequests_Contracts_ContractId FOREIGN KEY (ContractId) REFERENCES Contracts(ContractId) ON DELETE CASCADE
);

-- 4. Systems Optimization Indexes for Rapid Search Orchestration
CREATE INDEX IX_Contracts_ClientID ON Contracts(ClientID);
CREATE INDEX IX_Contracts_Status ON Contracts(Status);
CREATE INDEX IX_ServiceRequests_ContractId ON ServiceRequests(ContractId);
CREATE INDEX IX_ServiceRequests_Type ON ServiceRequests(Type);
CREATE INDEX IX_ServiceRequests_ContractId_Status ON ServiceRequests(ContractId, Status);
GO

```

### 3️⃣ Configuration Parameters (`appsettings.json`)

The application is pre-configured to utilize the ExchangeRate API. Ensure that the core `CurrencyApi` segment mirrors your live development access keys as configured inside `ERP.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db;Database=TechmoveERP;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "CurrencyApi": {
    "BaseUrl": "https://v6.exchangerate-api.com/v6/",
    "ApiKey": "265f49aae3f3392715d495d9"
  },
  "AllowedHosts": "*"
}

```

---

## 🐳 Docker Deployment Guide (The Micro-Ecosystem)

The platform is designed to compile cleanly via Docker Compose, which automatically builds both application layers, sets the internal environment network flags, and overrides local publishing constraints.

### 🔹 Step 1: Clean and Drop Active Containers

If you have active containers listening on local environment ports, safely spin down the active infrastructure using:

```powershell
docker-compose down

```

### 🔹 Step 2: Build and Launch the Ecosystem Cluster

To compile both the MVC Client and the Data API layer from scratch while ignoring duplicate asset publisher compilation paths, invoke:

```powershell
docker-compose up --build -d

```

*Note: The system contains deep file caching workarounds. If you edit explicit environment endpoints, attach the `--no-cache` variable modifier to clear intermediate image dependencies (`docker-compose build --no-cache`).*

---

## 🛡️ Identity Administration & Security Matrices

### **Role-Based Authorization Architecture**

| Managed Security Group | Access Tier Mapping Boundaries |
| --- | --- |
| 👑 **System Admin** | Comprehensive operational command capability. Provisions client identities, edits ledger instances, updates contract agreements, and enforces system states. |
| 🚛 **Logistics Client Account** | Constrained boundary tracking scope. Views own operational SLA details and spawns specific multi-modal freight distribution requests. |

### **Global Enterprise Middleware Fault Processing**

Uncaught backend execution interruptions are securely caught by the `ExceptionHandlingMiddleware` layer before leaking standard system details:

1. **Intercepts** runtime fault exceptions, preventing stack traces from leaking to public browser UI layouts.
2. **Registers** the diagnostic issue across system logs for development tracking.
3. **Packages** the error context into client-safe text notifications through `TempData` storage.
4. **Redirects** the browser session safely back to the originating page view, presenting user-friendly diagnostic alert templates.

---

## 🧪 Enterprise Verification & Test Coverage Matrix

The automated validation suite uses `xUnit` execution steps and `Moq` mocking libraries to verify business logic isolation boundaries without relying on external system connections.

```powershell
# Execute the test suite from root
dotnet test TechMoveTests/

```

### 📸 Database System Engineering Assertions (SSMS Execution Verification)

#### Client Accounts Master Ledger Sync (`SELECT * FROM Clients`)

#### Corporate Logistics Agreements Registry (`SELECT * FROM Contracts`)

#### Multi-Modal Cargo Shipping Ledger (`SELECT * FROM ServiceRequests`)

---

### 📸 Automated Business Logic Testing Pipelines

#### 1. Identity Management Engine Validation (`AccountControllerTests`)

#### 2. System Operations and Tenant Routing Framework (`AdminControllerTests`)

#### 3. Client Onboarding and Validation Processing (`ClientControllerTests`)

#### 4. Contract SLA Dynamic Parser Pipeline (`ContractControllerTests`)

#### 5. Factory Pattern Freight Instantiation Assertions (`ServiceRequestControllerTests`)

---

### 🌍 External Dependency Declarations

* Real-time global monetary transactions handled via the [ExchangeRate-API Platform](https://www.exchangerate-api.com/).
* Platform architecture guidelines followed according to the official [.NET Documentation Hub](https://learn.microsoft.com/en-us/dotnet/).
