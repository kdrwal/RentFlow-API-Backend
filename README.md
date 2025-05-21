# RentflowAPI
## General Information
Backend API for the RentFlow bike rental system. Provides RESTful services for managing users, bikes, reservations, and admin functionalities. Built with ASP.NET Core Web API and uses SQL Server as the database.

## Table of Contents
* [General Information](#general-information)
* [Technologies Used](#technologies-used)
* [Features](#features)
* [Setup](#setup)

## Technologies Used
- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL Server
- AutoMapper
- JWT Authentication
- Swagger / Swashbuckle
- FluentValidation
- CORS
- LINQ
- Identity Roles

## Features
- User registration, login, and JWT-based authentication
- Role-based access control (Admin, User)
- CRUD operations for:
  - Bikes
  - Categories
  - Reservations
- Many-to-many relationship between reservations and bikes
- Quantity and date validation for reservations
- Admin-only endpoints for managing users and system data
- Swagger UI for API testing and documentation
- Data seeding for roles and sample data
- DTOs for secure data transfer
- CORS setup for Angular frontend integration

## Setup

### Prerequisites
- [.NET SDK 8+](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/) or Swagger for testing

### Installation

1. Clone the repository:
git clone https://github.com/your-username/rentflow-backend.git
cd rentflow-backend

2. Update the connection string in `appsettings.json`:
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=RentflowDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

3. Apply migartions and seed the database:
dotnet ef database update

4. Run the API
