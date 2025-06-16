# ApiMES System

## 🧭 Architectural Overview

The `ApiMES` system is designed following a pragmatic **Application Service + DAO** architecture. This architecture balances **clarity**, **modularity**, and **ease of maintenance**, while avoiding unnecessary complexity often seen in over-engineered solutions.

---

## 🧱 Project Structure

```plaintext
├── Api/                         → Web API Entry point (Controllers, Filters, Attributes)
│   └── Controllers/
│
├── Application/                → Xử lý logic nghiệp vụ (use-case cụ thể)
│   ├── Services/
│   │   ├── Auth/
│   │   ├── Users/
│   │   └── VG/
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Users/
│   │   └── VG/
│   └── Configurations/
│
├── Domain/                     → Entities thuần (User, Token, Log,...)
│   └── Entities/
│       ├── Auth/
│       ├── Users/
│       └── Menu/
│
├── Infrastructure/            → Truy cập cơ sở dữ liệu, MongoDB, external services
│   ├── DAOs/
│   │   ├── Auth/
│   │   ├── Users/
│   │   ├── VG/
│   │   ├── Mongo/
│   │   └── Common/
│   ├── Database/              → DbContext, migrations, seeders
│   └── Services/              → Các service kỹ thuật như Email, File, FTP, v.v.
│
├── Shared/                    → Thành phần tái sử dụng toàn hệ thống
│   ├── Results/
│   └── Utilities/
│
└── README.md                  → Tài liệu mô tả kiến trúc

---

## 🔍 Architectural Philosophy

- **Use-Case First**: Business logic lives in Application Services, one class per use-case or logical group.
- **Data Access Simplified**: DAO classes directly query the database. No Repository or UnitOfWork pattern unless absolutely needed.
- **No Interface Explosion**: Interfaces are only introduced where multiple implementations or unit testing requires it.
- **Vertical Modularity**: Code grouped by feature/module, not just by type.

---

## 📌 Layer Responsibilities

| Layer | Responsibility |
|-------|----------------|
| **Api** | Routes HTTP requests to appropriate Application Services. |
| **Application** | Implements business use-cases, coordinates DAOs and logic. |
| **Domain** | Pure data structures (entities), without dependencies. |
| **Infrastructure** | Talks to database, file system, external services (MongoDB, FTP, etc.). |
| **Shared** | Contains cross-cutting concerns like Result types, string helpers, etc. |

---

## ✅ Principles & Conventions

- ✅ **Controllers** are thin; they only translate HTTP to service calls.
- ✅ **ApplicationService** contains all business logic — it's the brain.
- ✅ **DAOs** are the only classes accessing the DB (via EF Core or Mongo).
- ✅ **No use of MediatR, CQRS, or DDD Aggregates** unless justified by complexity.
- ✅ **Folder structure follows feature-based boundaries**, not layered separation.

---

## ⚙️ Tech Stack

- .NET 8 Web API
- Entity Framework Core (SQL Server)
- MongoDB .NET Driver
- Cookie-based Authentication (AccessToken + RefreshToken)
- Clean separation between concerns without unnecessary abstraction

---

## 🛠 Development Guidelines

1. **Create a new ApplicationService** when adding business logic.
2. **Inject DAO directly** in ApplicationService; no interface unless needed.
3. **Keep DAO focused** on persistence logic; do not place business logic in DAOs.
4. **DTOs** must be isolated from domain entities.
5. **Avoid utility sprawl** — use `Shared/Utilities` only for truly cross-cutting tools.

---

## 📐 Example Flow: Validate User Login