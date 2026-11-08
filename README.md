# 🏍️ eBike Dealership & Distribution Management System (DMS) Backend

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://docs.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/MS%20SQL%20Server-2022-CC292B?style=flat&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue?style=flat)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Swagger / OpenAPI](https://img.shields.io/badge/Swagger-OpenAPI%203.0-85EA2D?style=flat&logo=swagger&logoColor=black)](https://swagger.io/)

A full-fledged, enterprise-grade **Dealership Management System (DMS) & ERP Backend** built for Electric Bike (e-Bike) manufacturers, importers, and multi-branch dealer networks. Engineered with **Clean Architecture**, **ASP.NET Core 8**, **Entity Framework Core**, and **Microsoft SQL Server**.

---

## 🌟 Domain Modules & Key Features

### 1. 🚲 Vehicle, Parts & Warehouse Inventory
- **E-Bike & Spare Parts Catalog:** Full inventory tracking by model, frame/chassis number, motor number, battery specifications, colors, and spare parts numbers.
- **Multi-Warehouse & Godown Management:** Multi-tier inventory tracking across central warehouses, regional godowns, and branch dealer showrooms.
- **Stock Transfers & Rack Locations:** Inter-branch transfers, in-transit tracking, and bin/rack location management.

### 2. 💰 Sales, Distribution & Credit Financing
- **Direct & Credit Sales Orders:** Automated order lifecycle from quotation to invoice delivery.
- **Installment / Credit Sales:** Multi-tenor installment planning, credit interest percentage computation, and penalty handling.
- **Cash Management:** Cash collection, bank deposits, credit verification, and sales returns reconciliation.
- **Promotions & Dealer Commissions:** Sales targets, seasonal offers, and dealer commission percentage calculation.

### 3. 📦 Procurement & Supply Chain
- **Purchase Orders & Delivery Orders (DO):** Comprehensive procurement workflow from supplier quote to DO clearance.
- **Purchase Returns & Replacements:** Warranty claims, defective parts replacements, and vendor credit adjustments.
- **Supplier Ledger:** Real-time accounts payable tracking and purchase reconciliations.

### 4. 🛠️ After-Sales Service & Warranty
- **Vehicle Registration & Licensing:** Tracking customer registration numbers, documentation, and regulatory compliance.
- **Periodic Maintenance & Service Orders:** Service schedules, warranty claims, mechanic job cards, and spare replacement logs.

### 5. 👥 HR, Multi-Company & System Administration
- **Multi-Company & Sister Concerns:** Hierarchical organizational structure supporting parent groups, sister concerns, and regional dealer branches.
- **HR & Employee Setup:** Departmental mapping, designations, employee records, and regional holiday calendars.
- **RBAC & Security:** JWT-based authentication, granular dynamic permissions, role assignments, and menu access control.
- **Financial Accounting Setup:** Bank ledger, daily transactions, operating income/expense categorization, and VAT management.

---

## 🏛️ Clean Architecture Breakdown

The solution follows strict separation of concerns following **Uncle Bob's Clean Architecture**:

```
eBikeDealerShipManagement/
├── src/
│   ├── eBike.Domain/            # Enterprise Entities, Value Objects, Enums & Custom Exceptions
│   ├── eBike.Application/       # Business Logic, CQRS / Service Interfaces, DTOs & Validation
│   ├── eBike.Infrastructure/    # Data Access (EF Core), Migrations, DB Contexts & External Services
│   ├── eBike.API/               # RESTful Controllers, Middlewares, Dependency Injection & Swagger
│   └── dbchange/                # Database migration and seed scripts
│
├── .gitignore                   # Comprehensive ignore rules
├── publish.bat                  # Automated release publishing script
└── README.md                    # Project documentation
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Microsoft SQL Server 2019+](https://www.microsoft.com/sql-server)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/)

### 🛠️ Configuration & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/mostafizur229/eBikeDealerShipManagement.git
   cd eBikeDealerShipManagement
   ```

2. **Configure Database Connection:**
   Update the connection string in `src/eBike.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=eBikeDealershipDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Run Migrations:**
   ```bash
   cd src/eBike.API
   dotnet ef database update --project ../eBike.Infrastructure
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```

5. **Explore API Documentation:**
   Navigate to `https://localhost:5001/swagger` (or your configured port) in your browser to inspect and test all API endpoints with interactive Swagger UI.

---

## 🔒 Security & Optimization
- **JWT Token Authentication:** Stateless, claims-based token auth with role validation.
- **Global Exception Handling:** Custom middleware catching unhandled exceptions and returning standard RFC-compliant error responses.
- **Repository & Unit of Work Pattern:** Transaction-safe database modifications.
- **Optimized SQL Queries:** Compiled EF Core queries with eager/explicit loading to prevent N+1 query overhead.

---

## 👨‍💻 Author
**Mostafizur Rahman**  
*Full-Stack Software Engineer*  
- GitHub: [@mostafizur229](https://github.com/mostafizur229)
