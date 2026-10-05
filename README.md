# AFCODE – Afcon Staff Attendance Register System

A digital staff attendance management system developed by **AFCODE** for **Afcon Industrial Equipment** as part of the **INSY7315 Work Integrated Learning (WIL)** project.

The system replaces Afcon's paper-based attendance register with a centralised web application that manages employee attendance, biometric device identities, attendance requests, monthly reports, employee records, departments, and employee documents.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Problem Statement](#problem-statement)
- [Project Objectives](#project-objectives)
- [Key Features](#key-features)
- [User Roles](#user-roles)
- [Technology Stack](#technology-stack)
- [System Architecture](#system-architecture)
- [Project Structure](#project-structure)
- [Database](#database)
- [Security](#security)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Biometric Device Integration](#biometric-device-integration)
- [Attendance Reports](#attendance-reports)
- [Team Members](#team-members)
- [Version Control](#version-control)
- [License](#license)

---

## Project Overview

Afcon Industrial Equipment previously relied on a paper-based staff attendance register. This created problems such as forgotten entries, inaccurate or illegible times, proxy attendance, manual calculations, and additional administrative work when preparing attendance information.

The **Afcon Staff Attendance Register System** digitises this process. Employees can be linked to a biometric device identity and attendance records can be managed through the web application. Administrators are provided with tools for employee management, attendance monitoring, requests, reports, departments, and employee documents.

The system follows an **ASP.NET Core MVC** architecture with a service and repository layer and uses **MongoDB** for persistent data storage.

---

## Problem Statement

The original paper attendance process presented several challenges:

- Employees could forget to record their attendance.
- Attendance could be recorded on behalf of another employee.
- Handwritten entries could be inaccurate or illegible.
- Clock-in, clock-out and break times required manual processing.
- Monthly attendance calculations were time-consuming.
- Attendance information was not centrally available to employees and administrators.
- Supporting reasons for absence, lateness or leave were difficult to connect to attendance records.

The AFCODE system was developed to provide a more accurate, secure and efficient digital alternative.

---

## Project Objectives

The project aims to:

1. Digitise Afcon's employee attendance process.
2. Improve the accuracy and reliability of attendance records.
3. Support biometric attendance identification.
4. Allow employees to view their own attendance information.
5. Allow employees to submit attendance-related requests.
6. Give administrators centralised control over employees, attendance and departments.
7. Generate monthly attendance reports for administrative use.
8. Allow employee documents such as payslips and IRP5 documents to be securely uploaded and accessed.
9. Reduce the manual administration required by the existing paper-based process.

---

## Key Features

### Employee Features

- Secure employee login.
- Personal employee dashboard.
- View personal profile information.
- View personal attendance history.
- View monthly attendance reports.
- Download personal attendance reports as CSV files.
- Submit attendance requests for matters such as lateness, absence or leave.
- Add supporting information and documentation to attendance requests.
- View the status and details of submitted requests.
- Access personal documents uploaded by an administrator.
- Biometric registration status linked through a device user ID.
- Attendance workflow supporting clock in, start break, end break and clock out.

### Administrator Features

- Secure administrator login and dashboard.
- Create, view, edit and manage employee records.
- Activate/deactivate employee records.
- Assign employees to departments.
- Link employees to biometric device user IDs.
- Manage departments.
- View attendance records across employees.
- Manually perform attendance actions as a backup when the biometric device is unavailable.
- Edit attendance information where authorised.
- View employee attendance requests.
- Accept or reject requests and add administrator comments.
- Generate employee attendance reports by month and year.
- Export attendance reports to CSV for use in applications such as Microsoft Excel.
- Upload payslips and IRP5 documents for individual employees.

---

## User Roles

The application uses role-based access control with two primary roles:

### Admin

Administrators manage the organisation's attendance information and have access to administrative functionality including employees, departments, attendance records, requests, reports and document uploads.

### Employee

Employees have access only to functionality relevant to their own account, including their attendance, requests, profile, reports and documents.

Role restrictions are enforced server-side using ASP.NET Core authorization attributes such as:

```csharp
[Authorize(Roles = "Admin")]
```

and:

```csharp
[Authorize(Roles = "Employee")]
```

---

## Technology Stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core MVC / C# |
| Framework | .NET 10 |
| Database | MongoDB |
| MongoDB Driver | MongoDB.Driver |
| Authentication | ASP.NET Core Identity with AspNetCore.Identity.Mongo |
| Front End | Razor Views, HTML, CSS and JavaScript |
| Styling | Tailwind CSS and DaisyUI |
| API Documentation | Swagger / Swashbuckle |
| File Storage | MongoDB GridFS |
| Version Control | Git and GitHub |
| Development Environment | Visual Studio / Visual Studio Code |

### Main NuGet Packages

```text
AspNetCore.Identity.Mongo 10.1.0
MongoDB.Driver 3.11.0
Swashbuckle.AspNetCore 10.2.3
```

### Front-End Packages

```text
Tailwind CSS 4.3.3
DaisyUI 5.7.46
@tailwindcss/cli 4.3.3
```

---

## System Architecture

The application separates responsibilities into multiple layers:

```text
User / Biometric Device
        |
        v
ASP.NET Core MVC Application
        |
        +-------------------+
        |                   |
        v                   v
   Controllers         Razor Views
        |
        v
     Services
        |
        v
   Repositories
        |
        v
 MongoDB / GridFS
```

### Controllers

Controllers handle HTTP requests, authorization and navigation between the user interface and application services.

### Services

The service layer contains the main business logic for employees, attendance, attendance requests, reporting, departments and documents.

### Repositories

Repositories provide the data-access layer between application services and MongoDB.

### Views

Razor views provide separate interfaces for administrative and employee functionality.

---

## Project Structure

```text
Development/
|
+-- README.md
+-- LICENSE
|
+-- WIL_Afcon/
    |
    +-- WIL_Afcon.slnx
    |
    +-- WIL_Afcon/
        |
        +-- Controllers/
        |   +-- AccountController.cs
        |   +-- AdminController.cs
        |   +-- AttendanceController.cs
        |   +-- AttendanceReportController.cs
        |   +-- AttendanceRequestController.cs
        |   +-- DepartmentController.cs
        |   +-- DocumentController.cs
        |   +-- EmployeeController.cs
        |   +-- HomeController.cs
        |
        +-- Data/
        |   +-- IdentitySetup.cs
        |   +-- MongoDbContext.cs
        |   +-- MongoDbSettings.cs
        |
        +-- Models/
        |   +-- Attendance.cs
        |   +-- AttendanceRequest.cs
        |   +-- BreakRecord.cs
        |   +-- Department.cs
        |   +-- Document.cs
        |   +-- Employee.cs
        |   +-- Enums/
        |   +-- ViewModels/
        |
        +-- Repositories/
        |   +-- Interfaces/
        |   +-- AttendanceRepository.cs
        |   +-- DepartmentRepository.cs
        |   +-- DocumentRepository.cs
        |   +-- EmployeeRepository.cs
        |
        +-- Services/
        |   +-- Interfaces/
        |   +-- AttendanceReportService.cs
        |   +-- AttendanceRequestService.cs
        |   +-- AttendanceService.cs
        |   +-- DepartmentService.cs
        |   +-- DocumentService.cs
        |   +-- EmployeeService.cs
        |
        +-- Views/
        |   +-- Account/
        |   +-- Admin/
        |   +-- Attendance/
        |   +-- AttendanceReport/
        |   +-- AttendanceRequest/
        |   +-- Department/
        |   +-- Document/
        |   +-- Employee/
        |   +-- Home/
        |   +-- Shared/
        |
        +-- wwwroot/
        +-- Program.cs
        +-- appsettings.json
        +-- package.json
        +-- WIL_Afcon.csproj
```

---

## Database

The system uses **MongoDB** as its primary database.

Application data includes:

- Employees
- Departments
- Attendance records
- Break records
- Attendance requests
- ASP.NET Identity users
- ASP.NET Identity roles
- Employee documents

Employee documents are associated with employee records and file content is stored using **MongoDB GridFS**.

The employee model also contains a `DeviceUserId`, which links an employee in the web application to the corresponding identity used by the biometric attendance device.

---

## Security

Security measures implemented in the application include:

- ASP.NET Core Identity authentication.
- Role-based access control for Admin and Employee functionality.
- Unique email requirements for application users.
- Minimum password requirements.
- `[Authorize]` and `[Authorize(Roles = "...")]` protection on restricted controllers/actions.
- Anti-forgery validation using `[ValidateAntiForgeryToken]` on state-changing form actions.
- Razor's standard output encoding to reduce cross-site scripting risks.
- Server-side validation and business-rule checks.
- Separation of employee and administrator functionality.
- Production exception handling and HSTS configuration.

> **Important:** Never commit real database passwords, API keys or other secrets to source control. Use environment variables, .NET User Secrets or secure deployment configuration instead.

---

## Getting Started

### Prerequisites

Install the following before running the project:

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js and npm](https://nodejs.org/)
- Access to a MongoDB database or MongoDB Atlas cluster
- Visual Studio, Visual Studio Code, or another compatible IDE
- Git

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Development/WIL_Afcon/WIL_Afcon
```

### 2. Restore .NET Dependencies

```bash
dotnet restore
```

### 3. Install Front-End Dependencies

```bash
npm install
```

### 4. Configure MongoDB

Do **not** place production credentials directly in source-controlled configuration files.

For local development, .NET User Secrets can be used:

```bash
dotnet user-secrets set "MongoDbSettings:ConnectionString" "<your-mongodb-connection-string>"
dotnet user-secrets set "MongoDbSettings:DatabaseName" "Afcon_Attendance_DB"
```

Alternatively, configure the same values through secure environment variables or the deployment platform's secret management system.

---

## Configuration

The application expects MongoDB settings with the following structure:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "<your-mongodb-connection-string>",
    "DatabaseName": "Afcon_Attendance_DB"
  }
}
```

The project also communicates with the local biometric attendance integration during development. The current integration expects the local service on port `5199`.

Ensure the required local attendance/device service is running when testing functionality that depends on the biometric integration.

---

## Running the Application

From the application directory, run:

```bash
dotnet run
```

The development launch profile currently uses:

```text
http://localhost:5084
https://localhost:7073
```

Open the address shown in the terminal after the application starts.

Swagger is enabled when the application is running in the Development environment.

---

## Biometric Device Integration

The system is designed to work with Afcon's biometric attendance process.

Each employee can be assigned a unique `DeviceUserId`. This creates a link between the employee stored in MongoDB and the identity registered on the biometric device/integration service.

The user interface instructs employees to use a **fingerprint or facial scan** on the biometric device to record attendance.

The attendance workflow supports:

```text
Employee identified
        |
        v
Clock In
        |
        v
Start Break
        |
        v
End Break
        |
        v
Clock Out
        |
        v
Attendance record stored and included in reporting
```

The web application also provides administrator attendance actions as a backup for situations where the biometric device is unavailable.

---

## Attendance Requests

Employees can create attendance-related requests where their normal attendance record requires additional context or approval.

The request workflow is:

```text
Employee creates request
        |
        v
Request stored as pending
        |
        v
Administrator reviews request
       / \
      /   \
 Accepted  Rejected
      |
      v
Attendance/reporting information can reflect the approved reason
```

Requests can include information such as the request type, relevant dates, a description and supporting documentation. Administrators can review the request and provide a comment when making a decision.

---

## Attendance Reports

The reporting functionality generates monthly employee attendance information using stored attendance records and relevant request information.

Reports can contain information such as:

- Employee details
- Department
- Attendance dates
- Starting and finishing times
- Break information
- Hours worked
- Attendance status
- Remarks
- Days present

Administrators can generate reports for employees, while employees can access their own reports.

Reports can also be exported as **CSV files**, allowing the information to be opened in spreadsheet software such as Microsoft Excel and used as part of Afcon's administrative process.

---

## Employee Documents

Administrators can upload employee-specific documents through the system, including:

- Payslips
- IRP5 documents

Document metadata is stored in MongoDB and the uploaded file data is stored using GridFS. Employees can access their own documents through the employee interface.

---

## Team Members

The project was developed by **AFCODE**.

| Team Member | Primary Responsibility |
|---|---|
| **Victoria (Tori) Golten** | Project Manager, Front-End Development & Documentation |
| **Kiaan Naidoo** | Lead Back-End Development |
| **Anjor (AJ) Misser** | Back-End Development |
| **Megan Jackson** | Front-End Development / UI |
| **Kiara Naidoo** | Front-End Development |
| **Diya Lakha** | All-Round Development Support |

The team collaborated across development, integration, testing, documentation and final project delivery.

---

## Version Control

Git and GitHub were used throughout the project for version control and team collaboration.

The development workflow included:

- Feature branches for individual areas of development.
- Regular commits to track progress.
- Pulling and merging work completed by different team members.
- Integration of front-end and back-end functionality.
- A shared development branch for testing combined functionality.

Example workflow:

```bash
git checkout <branch-name>
git pull
git add .
git commit -m "Describe the change"
git push origin <branch-name>
```

---

## PowerPoint Presentation

[Task 2 Final PowerPoint Presentation.pdf](https://github.com/user-attachments/files/33051345/Task.2.Final.PowerPoint.Presentation.pdf)

---

## Future Improvements

Possible future improvements include:

- Further automation between the biometric terminal and the web application.
- Expanded payroll integration.
- Additional report formats and analytics.
- Improved audit-history visualisation for administrative changes.
- Additional automated testing.
- Expanded notification functionality.
- Further deployment and monitoring improvements.

---

## Academic Project

This application was developed as part of the **INSY7315 Work Integrated Learning** module and was created for the Afcon Industrial Equipment attendance-management use case.

---

## License

This repository contains a `LICENSE` file. Refer to that file for the licensing terms applicable to the project.

---

**AFCODE – Afcon Staff Attendance Register System**  
*Developed for Afcon Industrial Equipment as part of INSY7315.*
