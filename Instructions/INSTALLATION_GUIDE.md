# 🚀 Installation Guide

This guide explains how to install and run the project on a new machine.

## 🐳 Docker Setup (Recommended)

Make sure Docker Desktop is installed.

Run the entire system with:

```bash
docker-compose up --build

This will start:

ASP.NET Core API → http://localhost:5000
SQL Server → port 1433
🗄️ Manual Database Setup

Open SQL Server Management Studio or any SQL tool and run:

Database/setup_complete_database.sql

This will create the database, tables, and seed data.

Then update your connection string in appsettings.json:

{
"ConnectionStrings": {
"DefaultConnection": "Server=localhost;Database=RaffleDB;Trusted_Connection=True;TrustServerCertificate=True"
}
}

▶️ Run the Project Manually

Restore dependencies:

dotnet restore

Run the project:

dotnet run

🌐 Access

API: http://localhost:5000
Swagger: http://localhost:5000/swagger

🔑 Default Users

Admin:
Email: admin@example.com
Password: Admin123!

🧪 Test the System

Open Swagger → /api/auth/login → login with admin → get token → test endpoints

📁 Project Structure

Database/
setup_complete_database.sql

WebApplication2/
Controllers/
Models/
Services/
appsettings.json

docker-compose.yml
Dockerfile

✅ Done

If everything is correct:

API runs
Database connected
Swagger works
Authentication works