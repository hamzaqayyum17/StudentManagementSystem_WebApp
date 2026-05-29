# EduTrack — Student Management System

A full-featured web-based Student Management System built with **ASP.NET Core MVC (.NET 8)**.
Designed to manage students, teachers, courses, attendance via Google Sheets, and fee records — with role-based dashboards for Admin, Teacher, and Student.

---

## 🚀 Features

### 👨‍💼 Admin
- Full dashboard with stats (students, teachers, courses, fees)
- Student & Teacher CRUD (Add, Edit, Delete, View)
- Class & Section management
- Course & Enrollment management
- Attendance sync from Google Sheets
- Fee management — individual & bulk generation
- Unpaid fee tracking & email reminders
- PDF Result Card generation per student
- Student Progress Reports

### 👨‍🏫 Teacher
- Teacher dashboard with today's attendance count
- Mark attendance directly via Google Sheets
- Sync attendance from Sheet to database

### 👨‍🎓 Student
- Personal dashboard — attendance %, pending fees, enrolled courses
- View attendance history with progress bar
- View fee records (paid/unpaid)
- Download PDF Result Card
- View Progress Report

---

## 🛠️ Technologies Used

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core MVC (.NET 8) |
| Database | Microsoft SQL Server (ADO.NET) |
| Frontend | Razor Views, Bootstrap 5, JavaScript |
| Auth | Session-based Role Authentication |
| Attendance | Google Sheets API v4 |
| Email | Gmail SMTP (MailKit) |
| PDF | iTextSharp |

---

## ⚙️ How to Run

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/StudentManagementSystem_WebApp.git
cd StudentManagementSystem_WebApp
```

### 2. Configure Database
- Open **SQL Server Management Studio**
- Run the SQL script in `/Database/StudentDB.sql` to create and populate the database

### 3. Update appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GoogleSheets": {
    "SpreadsheetId": "1D0n-YaOS-JPHUaOKBQqNgIvABU2p4gKaZwbFa_I2CFk",
    "CredentialsPath": "Credentials/studentms-496507-6bfa18f8bdbb.json",
    "SheetName": "Sheet1"
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Email": "kalilinux9t9@gmail.com",
    "Password": "vfxz llcf rxjz rumn",
    "DisplayName": "EduTrack SMS"
  }
}
```

### 4. Google Sheets Setup
- Create a Google Cloud project and enable **Google Sheets API**
- Download credentials JSON and place in `/Credentials/` folder
- Share your attendance Google Sheet with the service account email

### 5. Run the Project
- Open in **Visual Studio 2022**
- Build and run (`F5`)
- Default login page opens at `/Student/SignIn`

---

## 👥 Default Login Credentials

| Role | ID | Password |
|------|----|----------|
| Admin | admin1 | 123 |
| Student | std1 | 123 |
| Teacher | t1 | 123 |

> ⚠️ Change these credentials before deploying to production.

---

## 📁 Project Structure

```
StudentManagementSystem/
├── Controllers/
│   ├── StudentController.cs
│   ├── TeacherController.cs
│   ├── AttendanceController.cs
│   ├── FeeController.cs
│   ├── CourseController.cs
│   ├── EnrollmentController.cs
│   ├── ClassController.cs
│   ├── ReportController.cs
│   └── BaseController.cs
├── Models/
│   ├── Student.cs
│   ├── Teacher.cs
│   ├── Attendance.cs
│   ├── Fee.cs
│   ├── Course.cs
│   ├── Enrollment.cs
│   ├── Class.cs
│   ├── Section.cs
│   └── DBAccess.cs
├── Views/
│   ├── Student/
│   ├── Teacher/
│   ├── Attendance/
│   ├── Fee/
│   ├── Course/
│   ├── Enrollment/
│   ├── Class/
│   ├── Report/
│   └── Shared/
├── Services/
│   ├── GoogleSheetsService.cs
│   └── EmailService.cs
├── Credentials/
│   └── (Google API credentials — not included in repo)
└── appsettings.json
```

---

## 📧 Contact

**Developed by Hamza Qayyum**
Email: hamzaqayyum909@gmail.com
