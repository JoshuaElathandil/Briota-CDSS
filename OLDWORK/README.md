PatientPortalApp/
├── Program.cs
├── appsettings.json
├── Data/
│   ├── AppDbContext.cs
│   └── AppDbContextFactory.cs   (optional, but useful for migrations)
├── Models/
│   ├── User.cs
│   └── Patient.cs
├── Pages/
│   ├── Login.cshtml
│   ├── Login.cshtml.cs
│   ├── Dashboard.cshtml
│   ├── Dashboard.cshtml.cs
│   ├── Report.cshtml
│   ├── Report.cshtml.cs
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
└── wwwroot/
    └── images/
        ├── logo.png
        └── background.png


Database shape you need

This is the table shape the OLDWORK code expects.

Users
Id INT IDENTITY(1,1) PRIMARY KEY
Email NVARCHAR(100) NOT NULL
Password NVARCHAR(100) NOT NULL
Patients
Id INT IDENTITY(1,1) PRIMARY KEY
Name NVARCHAR(100) NOT NULL
DateOfEntry DATETIME NOT NULL
ReportText NVARCHAR(MAX) NOT NULL