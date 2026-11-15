# Minutes of Meeting (MoM) Management System

A web-based Minutes of Meeting (MoM) Management application built with **ASP.NET Core (MVC)** and **SQL Server**.

## 📌 Features

- **Authentication & Authorization**: Secure login for staff and administrators.
- **Meeting Management**: Schedule meetings, track agendas, and log meeting discussions.
- **Member Management**: Track meeting attendees, departments, and staff details.
- **Master Setup**: Configure departments, meeting types, and meeting venues.
- **Reports & Export**: Export meeting details and reports to Excel using ClosedXML.

## 🛠️ Tech Stack

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Database**: Microsoft SQL Server
- **Data Access**: ADO.NET / Microsoft.Data.SqlClient
- **Reporting**: ClosedXML
- **Front-end**: Razor Views, Bootstrap, HTML5, CSS3, JavaScript

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Microsoft SQL Server](https://www.microsoft.com/sql-server)
- [Visual Studio 2022 / 2025](https://visualstudio.microsoft.com/) or Visual Studio Code

### Setup Instructions

1. **Clone the repository**:
   ```bash
   git clone https://github.com/JADEJAVEERBHADRASINH/Minutes-of-Meeting-MoM-.git
   cd Minutes-of-Meeting-MoM-
   ```

2. **Configure Database Connection**:
   Update the connection string in `MOM/appsettings.json` to match your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=MOM_Project;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Build and Run**:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project MOM/MOM.csproj
   ```
