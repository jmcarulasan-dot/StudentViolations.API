# Student Violation System API

A RESTful API for managing student violations built with **ASP.NET Core**, **Dapper**, and **SQL Server**.  
Designed for **ACLC College of Mandaue** as a team project.

---

## Team Members

| Name                  | Responsibility       |
|-----------------------|----------------------|
| Jeff Marion Carulasan | ASP.NET Core API     |
| Hannah Maica Maningo  | MudBlazor Web App    |
| Junamie Rivera        | Flutter Mobile App   |
| Katrina Palag         | Ducomentation        |

---

## Tech Stack

| Layer          | Technology                        |
|----------------|-----------------------------------|
| Backend        | ASP.NET Core Web API (.NET 8)     |
| Language       | C#                                |
| Database       | SQL Server LocalDB                |
| ORM            | Dapper (Stored Procedures)        |
| Authentication | JWT Bearer Token                  |
| Documentation  | Swagger / OpenAPI                 |
| QR Code        | QRCoder NuGet Package             |

---

## Project Structure

```
StudentViolations.API/
├── Class/                      # Dapper implementations — all database logic
│   ├── GuardClass.cs           # Calls SP_GUARD
│   ├── LoginClass.cs           # Calls SP_STUDENT_GETUSERLOGIN
│   ├── RegisterClass.cs        # Calls SP_STUDENT_REGISTRATION
│   ├── SAOClass.cs             # Calls SP_SAO
│   ├── StudentClass.cs         # Calls SP_STUDENT_DATA
│   └── ViolationClass.cs       # Calls SP_VIOLATION
├── Controllers/                # API Controllers — receives requests, validates input
│   ├── GuardController.cs
│   ├── GuidanceController.cs
│   ├── LoginController.cs
│   ├── RegistrationController.cs
│   ├── SAOController.cs
│   └── StudentController.cs
├── Helpers/                    # Shared utility code
│   └── ViolationHelper.cs      # GetWarningLevel() — used by all controllers
├── IRepository/                # Interfaces — contracts for each Class
│   ├── IGuardRepository.cs
│   ├── ILoginRepository.cs
│   ├── IRegisterRepository.cs
│   ├── ISAORepository.cs
│   ├── IStudentRepository.cs
│   └── IViolationRepository.cs
├── Model/                      # Request and response models
│   ├── GuardModel.cs           # GetSummaryModel, RecordViolationModel
│   ├── LoginModel.cs
│   ├── RegistrationModel.cs
│   ├── SaoModel.cs             # UpdateUserModel
│   ├── ServiceResponse.cs      # Generic response wrapper
│   └── User.cs                 # User entity
├── Properties/
│   └── launchSettings.json
├── appsettings.json
└── Program.cs
```

---

## Authentication

JWT Bearer Token with role-based access control. Token is valid for **8 hours** (one full school day).

| Role       | What They Can Do                                              |
|------------|---------------------------------------------------------------|
| `guard`    | Scan QR code, record violations, view students                |
| `student`  | View own violations, profile, and QR code                     |
| `guidance` | View all students, reports, violations by status and severity |
| `sao`      | Full admin — approve/reject violations, manage all users      |

All protected endpoints require:
```
Authorization: Bearer <your_token_here>
```

---

## API Endpoints

### Public — No token required

| Method | Endpoint    | Description                    |
|--------|-------------|--------------------------------|
| POST   | `/login`    | Login and receive JWT token    |
| POST   | `/register` | Register a new user account    |

### Guard — `[Authorize(Roles = "guard")]`

| Method | Endpoint                          | Description                                                             |
|--------|-----------------------------------|-------------------------------------------------------------------------|
| GET    | `/api/guard/student/validate`     | Scan QR code — returns student info + warning level + violation history |
| POST   | `/api/guard/student/violation`    | Record a new violation for a student                                    |
| GET    | `/api/guard/violations/summary`   | Get violation summary for a date range                                  |
| GET    | `/api/guard/students`             | Get list of all registered students                                     |
| GET    | `/api/guard/students/exist`       | Check if a student exists by StudentNo                                  |

### Student — `[Authorize(Roles = "student")]`

| Method | Endpoint                  | Description                           |
|--------|---------------------------|---------------------------------------|
| GET    | `/api/student/violations` | View own violations and warning level |
| GET    | `/api/student/profile`    | View own profile information          |
| GET    | `/api/student/qrcode`     | View own QR code                      |

### Guidance — `[Authorize(Roles = "guidance")]`

| Method | Endpoint                                      | Description                                          |
|--------|-----------------------------------------------|------------------------------------------------------|
| GET    | `/api/guidance/students`                      | View all students with violation counts              |
| GET    | `/api/guidance/students/{studentNo}/report`   | View full profile and violation history of a student |
| GET    | `/api/guidance/violations/by-status`          | View violations grouped by status                    |
| GET    | `/api/guidance/violations/by-severity`        | View violations grouped by severity level            |

### SAO (Admin) — `[Authorize(Roles = "sao")]`

| Method | Endpoint                                  | Description                                         |
|--------|-------------------------------------------|-----------------------------------------------------|
| GET    | `/api/sao/violations`                     | View all violations in the system                   |
| GET    | `/api/sao/violations/by-status/{status}`  | Filter violations by Pending, Approved, or Rejected |
| PUT    | `/api/sao/violations/{id}/approve`        | Approve a violation                                 |
| PUT    | `/api/sao/violations/{id}/reject`         | Reject a violation                                  |
| DELETE | `/api/sao/violations/{id}`                | Delete a violation — returns deletion history       |
| GET    | `/api/sao/violations/summary`             | View violation counts by status, severity, and type |
| GET    | `/api/sao/students/{studentNo}/report`    | View full student profile and violation history     |
| GET    | `/api/sao/users`                          | View all registered users                           |
| GET    | `/api/sao/users/{id}`                     | View one user by ID — use before updating           |
| PUT    | `/api/sao/users/{id}`                     | Update a user's information                         |
| DELETE | `/api/sao/users/{id}`                     | Permanently delete a user                           |

---

## Database

**Database name:** `StudentViolations`  
**Engine:** SQL Server LocalDB  
**Pattern:** All queries use Stored Procedures with `@statementType` parameter as a switch

### Tables

| Table      | Description                                                    |
|------------|----------------------------------------------------------------|
| Users      | All users regardless of role — guards, students, guidance, SAO |
| Students   | Student-specific records including QR code                     |
| Violations | All violation records created by guards                        |

### Stored Procedures

| Stored Procedure          | Used By        | Operations                                                                                           |
|---------------------------|----------------|------------------------------------------------------------------------------------------------------|
| `SP_STUDENT_GETUSERLOGIN` | LoginClass     | GETLOGIN, USEREXISTS                                                                                 |
| `SP_STUDENT_REGISTRATION` | RegisterClass  | REGISTER, STUDENTNOEXISTS                                                                            |
| `SP_GUARD`                | GuardClass     | GETBYDATE, GETBYSTUDENT, GETSTUDENTBYQR, RECORDVIOLATION, GETALLSTUDENTS, GETSTUDENTBYNO, GETBYGUARD |
| `SP_VIOLATION`            | ViolationClass | GETALL, GETBYID, GETBYSTUDENT, RECORDVIOLATION, UPDATESTATUS, DELETE                                 |
| `SP_STUDENT_DATA`         | StudentClass   | GETSTUDENT, GETALLSTUDENTS, UPDATESTUDENT                                                            |
| `SP_SAO`                  | SAOClass       | GETALLUSERS, GETUSERBYID, UPDATEUSER, DELETEUSER                                                     |

---

## Warning Level System

| Violations | Level    | Color  |
|------------|----------|--------|
| 0          | Safe     | Green  |
| 1          | Warning  | Yellow |
| 2          | Danger   | Orange |
| 3 or more  | Critical | Red    |

---

## Getting Started

### Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server / SSMS
- Swagger UI (built-in)

### Setup

**1. Clone the repository**
```bash
git clone https://github.com/jmcarulasan-dot/StudentViolations.API.git
cd StudentViolations.API
```

**2. Configure the database connection**

Open `appsettings.json` and update the connection string:
```json
{
  "ConnectionStrings": {
    "StudentViolationsdb": "Server=(localdb)\\MSSQLLocalDB;Database=StudentViolations;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**3. Run all stored procedures in SSMS**

All database schema and stored procedures are applied manually via raw SQL in SSMS.  
No Entity Framework migrations are used in this project.

**4. Update launchSettings.json with your IP**
```json
"applicationUrl": "http://YOUR_IP:5277"
```

**5. Run the project**

Press `F5` in Visual Studio or:
```bash
dotnet run
```

**6. Open Swagger**
```
http://YOUR_IP:5277/swagger
```

---

## Test Accounts

| Name   | Username | Password  | Role     |
|--------|----------|-----------|----------|
| Jeff   | jeff     | jeff123!  | guard    |
| Hannah | hannah   | hannah123!| student  |
| Juna   | juna     | juna123!  | guidance |
| Kath   | kath     | kath123!  | sao      |

---

## License

This project is for educational purposes only — ACLC College of Mandaue, 2026.