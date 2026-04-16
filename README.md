
# Student Violation System API & Mobile App

A full-stack student violation tracking system built for **ACLC College of Mandaue**. 

The system features a **C# ASP.NET Core REST API** paired with a **Flutter mobile app** that communicates via JWT Bearer Tokens. All business logic, password hashing, and data aggregation happen on the backend; the mobile app strictly handles UI and displays the JSON responses.

---

## Team Members

| Name                  | Responsibility              | Tech Stack                     |
|-----------------------|-----------------------------|-------------------------------|
| Jeff Marion Carulasan | ASP.NET Core API & Flutter App | C#, Dapper, SQL Server,  |
| Hannah Maica Maningo  | Flutter Mobile App (Original)   | MudBlazor   |
| Junamie Rivera        | MudBlazor Web App           | Flutter                |
| Katrina Palag         | Documentation               | Markdown                       |

---

## Tech Stack

| Layer          | Technology                    |
|----------------|-------------------------------|
| Backend API     | ASP.NET Core Web API (.NET 8) |
| Backend Lang    | C#                            |
| Database       | SQL Server LocalDB            |
| ORM            | Dapper (Stored Procedures)    |
| Security       | JWT Bearer Token              |
| Mobile App      | Flutter (Dart)                |
| State Mgmt      | Provider package               |
| QR Codes       | QRCoder (C#) & Base64 decode (Flutter) |
| Password Hash  | PBKDF2 (HMACSHA256)           |
| API Docs       | Swagger / OpenAPI             |

---

## Backend Project Structure

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
│   └── ViolationHelper.cs      # GetWarningLevel() and GetRecommendedAction()
├── IRepository/                # Interfaces — contracts for each Class
│   ├── IGuardRepository.cs
│   ├── ILoginRepository.cs
│   ├── IRegisterRepository.cs
│   ├── ISAORepository.cs
│   ├── IStudentRepository.cs
│   └── IViolationRepository.cs
├── Model/                      # Request and response models
│   ├── LoginModel.cs
│   ├── RegistrationModel.cs
│   ├── ServiceResponse.cs      # Generic wrapper: { status, message, data, token }
│   ├── StudentModel.cs         # Student data: QR Code, WarningLevel, Status
│   ├── UserModel.cs           # User data: PasswordHash, Salt, Role
│   └── ViolationModel.cs       # Violations: AppealText, AppealStatus, Severity
├── Properties/
│   └── launchSettings.json     # Network IP configuration
└── Program.cs                   # CORS, JWT setup, DI for repositories
```

---

## Authentication

JWT Bearer Token with role-based access control. Token is valid for **8 hours** (one full school day).

| Role       | What They Can Do                                                                 |
|------------|---------------------------------------------------------------------------------|
| `Guard`    | Scan QR code, record violations, view students, view date-range summaries        |
| `Student`  | View own violations, profile, QR code, and submit appeals                       |
| `Guidance` | View all students, reports, violations, warn students, recommend dismissal, review appeals |
| `Sao`      | Full admin — approve/reject violations, manage users, confirm/cancel dismissal, review appeals |

All protected endpoints require: `Authorization: Bearer <token>`

Token claims included: `sub`, `jti`, `nameidentifier` (StudentID), `name` (Full Name), `role`, `studentNo`.

> **Dismissal Block:** If a student account has been **Dismissed**, login is blocked and returns a 400 error with the message: `Your account has been dismissed. Please contact the SAO office.`

---

## API Endpoints

### Public — No token required

| Method | Endpoint              | Description                 |
|--------|-----------------------|-----------------------------|
| POST   | `/api/auth/login`     | Login and receive JWT token |
| POST   | `/api/auth/register`  | Register a new user account |

### Guard — `[Authorize(Roles = "guard,Guard")]`

| Method | Endpoint                        | Description                                                             |
|--------|---------------------------------|-------------------------------------------------------------------------|
| GET    | `/api/guard/student/validate`   | Scan QR code — returns student info, warning level, and violation history |
| POST   | `/api/guard/student/violation`  | Record a new violation for a student (Backend extracts Guard ID from JWT, not request body) |
| GET    | `/api/guard/violations/summary` | Get violation summary for a date range                                  |
| GET    | `/api/guard/students`           | Get list of all registered students                                     |
| GET    | `/api/guard/students/exist`     | Check if a student exists by StudentNo                                  |

### Student — `[Authorize(Roles = "Student,student")]`

| Method | Endpoint                              | Description                                                        |
|--------|---------------------------------------|--------------------------------------------------------------------|
| GET    | `/api/student/violations`             | View own violations including appeal status and remarks            |
| GET    | `/api/student/profile`                | View own profile information                                       |
| GET    | `/api/student/qrcode`                 | View own QR code (Base64) — Flutter decodes and displays it    |
| POST   | `/api/student/violations/{id}/appeal` | Submit an appeal for a specific violation                          |

### Guidance — `[Authorize(Roles = "guidance,Guidance")]`

| Method | Endpoint                                       | Description                                          |
|--------|------------------------------------------------|------------------------------------------------------|
| GET    | `/api/guidance/students`                       | View all students with violation counts and recommended actions |
| GET    | `/api/guidance/students/{studentNo}/report`    | View full profile and violation history of a student |
| GET    | `/api/guidance/violations/by-status`           | View violations grouped by Pending/Approved/Rejected |
| GET    | `/api/guidance/violations/by-severity`         | View violations grouped by severity level            |
| PUT    | `/api/guidance/students/{studentNo}/warn`      | Set student status to Warned                         |
| PUT    | `/api/guidance/students/{studentNo}/recommend-dismiss` | Recommend student for dismissal (sets status to PendingDismissal) — requires 3+ violations |
| POST   | `/api/guidance/violations/{id}/appeal/review`  | Review a student appeal with remarks                 |

### SAO (Admin) — `[Authorize(Roles = "sao,Sao")]`

| Method | Endpoint                                    | Description                                         |
|--------|--------------------------------------------|-----------------------------------------------------|
| GET    | `/api/sao/violations`                       | View all violations in the system                   |
| GET    | `/api/sao/violations/by-status/{status}`    | Filter by Pending, Approved, or Rejected            |
| PUT    | `/api/sao/violations/{id}/approve`          | Approve a violation                                 |
| PUT    | `/api/sao/violations/{id}/reject`           | Reject a violation                                  |
| DELETE | `/api/sao/violations/{id}`                  | Delete a violation                                  |
| GET    | `/api/sao/violations/summary`               | View violation counts by status/severity/type       |
| PUT    | `/api/sao/violations/{id}/appeal/review`    | Review a student appeal with remarks                |
| GET    | `/api/sao/students/{studentNo}/report`      | View full student profile and violation history     |
| PUT    | `/api/sao/students/{studentNo}/dismiss`     | Confirm student dismissal (student must be PendingDismissal) |
| PUT    | `/api/sao/students/{studentNo}/cancel-dismiss` | Cancel dismissal — sets student back to Active   |
| GET    | `/api/sao/users`                            | View all registered users                           |
| GET    | `/api/sao/users/{id}`                       | View one user by ID                                 |
| PUT    | `/api/sao/users/{id}`                       | Update a user's information (cannot change Course, Year, or Role) |
| DELETE | `/api/sao/users/{id}`                       | Permanently delete a user                           |

---

## Input Validation Rules (Enforced by C# Backend)

| Field          | Valid Values                              |
|----------------|-------------------------------------------|
| `role`         | guard, student, guidance, sao             |
| `gender`       | male, female                              |
| `severity`     | minor, moderate, major, critical          |
| `status`       | Pending, Approved, Rejected               |
| `appealStatus` | None, Pending, Approved, Rejected         |
| `studentStatus`| Active, Warned, PendingDismissal, Dismissed |
| `course`       | BSIT, BSHM, BSBA                         |
| `year`         | 1, 2, 3, 4                               |
| `studentNo`    | Format: `C26-01-0001-MAN121`             |
| `contactNumber`| Must start with `09` and be exactly 11 digits |
| `password`     | Minimum 8 characters                     |
| `username`     | Minimum 2 characters                     |
| `age`          | Must be at least 15 years old            |

---

## Database Architecture

**Database name:** `StudentViolations`  
**Engine:** SQL Server LocalDB  
**Pattern:** All queries use Stored Procedures with `@statementType` as a switch.

### Tables

| Table      | Description                                                                          |
|------------|--------------------------------------------------------------------------------------|
| Users      | All users regardless of role — guards, students, guidance, SAO. Includes `StudentNo`. |
| Students   | Student-specific records including QR code (Base64), `StudentNo`, and `Status` (Active, Warned, PendingDismissal, Dismissed). |
| Violations | All violation records including `AppealText`, `AppealStatus`, and `AppealRemarks`. |

> When a student registers, their data is saved to **both** the Users table and the Students table. The QR code is generated using `QRCoder` and stored in the Students table. `StudentNo` is saved to both tables so the JWT token can read it at login.

### Stored Procedures

| Stored Procedure          | Used By        | Statement Types                                  |
|---------------------------|----------------|-------------------------------------------------|
| `SP_STUDENT_GETUSERLOGIN` | LoginClass     | GETLOGIN, USEREXISTS                               |
| `SP_STUDENT_REGISTRATION` | RegisterClass  | REGISTER, STUDENTNOEXISTS                               |
| `SP_GUARD`                | GuardClass     | GETBYDATE, GETBYSTUDENT, GETSTUDENTBYQR, RECORDVIOLATION, GETALLSTUDENTS, GETSTUDENTBYNO |
| `SP_VIOLATION`            | ViolationClass | GETALL, GETBYID, GETBYSTUDENT, RECORDVIOLATION, UPDATESTATUS, DELETE, SUBMITAPPEAL, UPDATEAPPEALSTATUS |
| `SP_STUDENT_DATA`         | StudentClass   | GETSTUDENT, GETALLSTUDENTS, UPDATESTUDENT, UPDATESTATUS                                |
| `SP_SAO`                  | SAOClass       | GETALLUSERS, GETUSERBYID, UPDATEUSER, DELETEUSER                                       |

---

## Warning Level System

Calculated by `ViolationHelper.GetWarningLevel(int count)` and `ViolationHelper.GetRecommendedAction(int count)` — used by all controllers.

| Violations | Warning Level | Color  | Recommended Action              |
|------------|---------------|--------|---------------------------------|
| 0          | Safe          | Green  | No action needed                |
| 1          | Warning       | Yellow | Issue written warning           |
| 2          | Danger        | Orange | Call parents / schedule counseling |
| 3 or more  | Critical      | Red    | Recommend for dismissal         |

---

## Student Status and Dismissal Flow

| Status             | Meaning                                                   |
|--------------------|-----------------------------------------------------------|
| `Active`           | Normal — student can log in and use all endpoints         |
| `Warned`           | Guidance has issued a formal warning                      |
| `PendingDismissal` | Guidance has recommended dismissal — awaiting SAO decision |
| `Dismissed`        | SAO has confirmed dismissal — student cannot log in       |

**Dismissal Flow:**
1. Guard records 3+ violations
2. Guidance calls `PUT /api/guidance/students/{studentNo}/recommend-dismiss` → status becomes `PendingDismissal`
3. SAO reviews and contacts the student
   - If reason is **valid** → `PUT /api/sao/students/{studentNo}/dismiss` → status becomes `Dismissed`
   - If reason is **not valid** → `PUT /api/sao/students/{studentNo}/cancel-dismiss` → status goes back to `Active`

---

## Student Appeal Flow

1. Student views their violations → `GET /api/student/violations` — note the violation `id`
2. Student submits appeal → `POST /api/student/violations/{id}/appeal`
3. Guidance or SAO reviews the appeal:
   - `POST /api/guidance/violations/{id}/appeal/review` or `PUT /api/sao/violations/{id}/appeal/review`
   - Body: `{ "appealStatus": "Approved", "appealRemarks": "Witness confirmed student was absent" }`
4. Student views violations again — can see `appeal_text`, `appeal_status`, and `appeal_remarks`

---

## ServiceResponse

All Class methods return `ServiceResponse<T>` with these fields:

| Field     | Type     | Description                          |
|-----------|----------|--------------------------------------|
| `Status`  | int      | HTTP status code (200, 400, 404, 500)|
| `Message` | string?  | Description of what happened         |
| `Data`    | T?       | The actual returned data             |
| `Token`   | string?  | JWT token (login only)               |

---

## Mobile App (Flutter)

The Flutter app connects to this API using the `http` package and manages state with the `provider` package. 

**Key Architecture:**
* **No local math is done on the mobile app.** It strictly displays the data your C# backend sends.
* **Dynamic Data:** The app reads raw violation strings (e.g., "No ID", "No Uniform") directly from the API and displays them, meaning if you add a new violation type in the database, the app will show it without needing an app update.
* **JWT Decoding:** The app decodes the JWT token on the client side to get the user's Name and ID to display on the dashboard.
* **State Caching:** Uses `SharedPreferences` to save the JWT token and user session so the user stays logged in when they close the app.

---

## Getting Started

### Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server / SSMS
- Flutter SDK (Latest stable)
- Android Studio / VS Code (for Flutter)
- Swagger UI (built-in)

### Backend Setup

**1. Clone the repository**
```bash
git clone https://github.com/jmcarulasan-dot/StudentViolations.API.git
cd StudentViolations.API
```

**2. Configure the database connection**

Open `appsettings.json` and update the connection string if needed:
```json
{
  "ConnectionStrings": {
    "StudentViolationsdb": "Server=(localdb)\\MSSQLLocalDB;Database=StudentViolations;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
```

**3. Run all stored procedures in SSMS**

All schema and stored procedures are applied manually via raw SQL in SSMS.  
No Entity Framework migrations are used in this project.

**4. Apply database column additions**
```sql
-- Add Status to Students table
ALTER TABLE [dbo].[Students]
ADD [Status] VARCHAR(20) NOT NULL DEFAULT 'Active'

-- Add Appeal columns to Violations table
ALTER TABLE [dbo].[Violations]
ADD [AppealText] VARCHAR(MAX) NULL,
    [AppealStatus] VARCHAR(20) NOT NULL DEFAULT 'None',
    [AppealRemarks] VARCHAR(MAX) NULL
```

**5. Update launchSettings.json with your network IP**
```json
"applicationUrl": "http://192.168.254.148:5277"
```

**6. Run the API**
Press `F5` in Visual Studio. Make sure you select **"StudentViolations.API (Network)"** from the play button dropdown so it runs on the network IP, not localhost.

**7. Open Swagger**
```
http://192.168.254.148:5277/swagger
```

### Mobile App Setup

**1. Clone her repository**
Ask Hannah for the Flutter repository link and clone it to your machine.

**2. Update the Base URL**
Open `lib/services/database_service.dart` and make sure the IP matches your backend:
```dart
static const String _baseUrl = 'http://192.168.254.148:5277';
```

**3. Install dependencies**
In the Flutter terminal:
```bash
flutter pub get
```

**4. Run on a physical device**
```bash
flutter run
```

---

## Test Accounts

| Name   | Username | Password   | Role     | StudentNo (if student) |
|--------|----------|------------|----------|---------------------|
| Jeff   | jeff     | jeff123!   | Guard    | N/A                 |
| Hannah | hannah   | hannah123! | Student  | C26-01-0001-MAN121  |
| Juna   | juna     | juna123!   | Guidance | N/A                 |
| Kath   | kath     | kath123!   | SAO      | N/A                 |

---

## License

This project is for educational purposes only — ACLC College of Mandaue, 2026.
```

