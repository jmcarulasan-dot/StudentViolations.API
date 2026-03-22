
==Student Violation System API==

A RESTful API for managing student violations built with **ASP.NET Core**, **Dapper**, and **SQL Server**. Designed for ACLC College of Mandaue as a team project.


--Team Members--

          Name        |    Responsibility  |
Jeff Marion Carulasan | ASP.NET Core API   |
Hannah MAica Maningo  | MudBlazor Web App  |
Junamie Rivera        | Flutter Mobile App |
Katrina Palag         | Documentaton       |


--Tech Stack--

Backend: ASP.NET Core Web API
ORM: Dapper (Stored Procedures)
Database: SQL Server (SSMS)
Authentication: JWT Bearer Token
Documentation: Swagger / OpenAPI
Mobile: Flutter
Web: MudBlazor



--Project Structure--

StudentViolations.API/
├── Class/                  # Dapper implementations
│   ├── GuardClass.cs
│   ├── LoginClass.cs
│   ├── RegisterClass.cs
│   ├── SAOClass.cs
│   ├── StudentClass.cs
│   └── ViolationClass.cs
├── Controllers/            # API Controllers
│   ├── GuardController.cs
│   ├── GuidanceController.cs
│   ├── LoginController.cs
│   ├── RegistrationController.cs
│   ├── SAOController.cs
│   └── StudentController.cs
├── IRepository/            # Interfaces
│   ├── IGuardRepository.cs
│   ├── ILoginRepository.cs
│   ├── IRegisterRepository.cs
│   ├── ISAORepository.cs
│   ├── IStudentRepository.cs
│   └── IViolationRepository.cs
├── Model/                  # Request/Response models
├── Properties/
│   └── launchSettings.json
├── appsettings.json
└── Program.cs



--Authentication--

JWT Bearer Token authentication with 4 roles:
 
|      Role       |             Access             |
|    `student`    | View own violations            |
|    `guard`      | Record and view violations     |
|    `guidance`   | Manage students and violations |
|    `sao`        | Full admin access              |

All protected endpoints require:

Authorization: Bearer <token>


--API Endpoints--

Public (No token required)

| Method |     Endpoint    |           Description          |
| POST   | `/api/login`    | Login and receive JWT token    |
| POST   | `/api/register` | Register a new student account |

Student — `[Authorize(Roles = "student")]`

| Method |          Endpoint         |        Description        |
|  GET   | `/api/student/violations` | Get own violation records |

Guard — `[Authorize(Roles = "guard")]`
 
| Method |              Endpoint             |          Description           |
|  GET   | `/api/guard/students`             | Get all students               |
|  GET   | `/api/guard/students/{studentNo}` | Search student by student no.  |
|  POST  | `/api/guard/violations`           | Record a new violation         |
|  GET   | `/api/guard/violations`           | View all violations            |

Guidance — `[Authorize(Roles = "guidance")]`

| Method |                 Endpoint               |         Description        |
| GET    | `/api/guidance/students`               | Get all students           |
| GET    | `/api/guidance/students/{studentNo}`   | Get student by student no. |
| GET    | `/api/guidance/violations`             | Get all violations         |
| PUT    | `/api/guidance/violations/{id}/resolve`| Resolve a violation        |
| DELETE | `/api/guidance/violations/{id}`        | Delete a violation         |

SAO — `[Authorize(Roles = "sao")]`

| Method |                 Endpoint                 |                  Description                 |
| GET    | `/api/sao/violations`                    | Get all violations with student info         | 
| GET    | `/api/sao/violations/by-status/{status}` | Filter by status (pending/approved/rejected) |
| GET    | `/api/sao/violations/summary`            | Get violation stats and summary              |
| PUT    | `/api/sao/violations/{id}/approve`       | Approve a violation                          |
| PUT    | `/api/sao/violations/{id}/reject`        | Reject a violation                           |
| DELETE | `/api/sao/violations/{id}`               | Delete a violation                           |
| GET    | `/api/sao/students/{studentNo}/report`   | Full student profile + violation history     |
| GET    | `/api/sao/users`                         | Get all system users                         |
| PUT    | `/api/sao/users/{id}`                    | Update user info                             |
| DELETE | `/api/sao/users/{id}`                    | Delete a user                                |



--Database--

Database`StudentViolations`
ORM: Dapper with Stored Procedures
Pattern: `@statementType` parameter per stored procedure

--Stored Procedures--

|      Stored Procedure     |     Used By     |
| `SP_GUARD`                | GuardClass      |
| `SP_STUDENT_DATA`         | StudentClass    |
| `SP_VIOLATION`            | ViolationClass  |
| `SP_SAO`                  | SAOClass        |
| `SP_STUDENT_GETUSERLOGIN` | LoginClass      |
| `SP_STUDENT_REGISTRATION` | RegisterClass   |



==Getting Started==

--Prerequisites--
- Visual Studio 2022
- .NET 8 SDK
- SQL Server / SSMS
- Postman or Swagger UI

Setup

1. Clone the repository
```bash
git clone https://github.com/jmcarulasan-dot/StudentViolations.API.git
cd StudentViolations.API
```

2. Configure the database connection

Open `appsettings.json` and update:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=StudentViolations;Trusted_Connection=True;"
  }
}
```

3. Run the stored procedures in SSMS

All schema changes are applied manually via raw SQL in SSMS (no EF Core migrations).

4. Update launchSettings.json with your IP
```json
"applicationUrl": "http://YOUR_IP:5277"
```

5. Run the project

Press `F5` in Visual Studio or:
```bash
dotnet run
```

**6. Open Swagger**
```
http://YOUR_IP:5277/swagger
```



--License--

This project is for educational purposes only — ACLC College of Mandaue, 2026.