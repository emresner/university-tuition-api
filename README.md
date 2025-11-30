🚀 University Tuition Payment API – README
📌 Overview
This project implements a University Tuition Payment System API as described in the course assignment.

It provides REST endpoints for:

University Mobile App
Query tuition balance (no authentication, rate-limited)

Banking App
Query tuition balance (JWT authentication required)
Pay tuition (no authentication)

University Web Admin
Add tuition
Batch tuition upload (.csv)
List unpaid students (with paging + authentication)

The API supports API Versioning, Swagger, Rate-Limiting, JWT-based authentication, and Gateway integration.


## 🔗 Source Code

**GitHub repository:**  
👉 [https://github.com/emresner/university-tuition-api](https://github.com/emresner/university-tuition-api)

**🌍 Live Swagger (via API Gateway):**  
👉 [https://tuition-api-emre-arbhhdf5hsdygqbv.francecentral-01.azurewebsites.net](https://tuition-api-emre-arbhhdf5hsdygqbv.francecentral-01.azurewebsites.net)


🧩 System Design
✔ Architecture
ASP.NET Core 8 Web API
REST endpoints grouped by consumers: Mobile, Banking, Admin
API Versioning using URL segments (/api/v1/...)
Authentication + Authorization with JWT
Required for Banking App and Admin
Rate-Limiting for Mobile App
3 tuition queries per 24 hours per student

API Version 2.0 is included only for demonstration/testing purposes (e.g., the Ping controller) and does not contain any production tuition endpoints. 
All functional requirements are implemented in Version 1.0.

✔ Logging
The required request-level and response-level logging fields can be tracked through Azure API Management diagnostics in the live environment (via the APIM Test/Trace panel), 
and during local development they are captured by the custom middleware and visible in the application’s debug output.

A custom RequestResponseLoggingMiddleware logs:
HTTP method
Full path
Timestamp (UTC)
Source IP
All headers
Request size & response size
Authentication result
Response latency
Matches assignment’s required logging criteria.


✔ Rate Limiting
Originally implemented via a custom middleware (TuitionRateLimitMiddleware).
Later, as per instructor requirement, rate limiting is enforced at the API Gateway (Azure APIM).
The middleware remains in the codebase for demonstration, but is disabled in production.

✔ Swagger
Swagger UI is exposed through API Gateway.
Server URL is dynamically injected via configuration.

Every version (v1) has its own Swagger document.

✔ Deployment
Local development used SQL Server LocalDB
Production uses Azure SQL
Database was migrated from local to Azure using dotnet ef database update
API hosted on Azure App Service
API protected via Azure API Management (APIM)

🧠 Assumptions
No real payment gateway is required; payment records are simply stored in the database.

CSV batch file structure is assumed as:
StudentNo,Term,Amount

Admin authentication uses a fixed demo user (admin / Passw0rd!) since no user management was required.

Mobile app queries do not require authentication but must be rate-limited.

Banking application uses JWT tokens issued by /auth/token.

Each student + term combination may have multiple tuition charges and multiple payments.

Swagger must show all security schemes, including:
JWT Bearer
APIM Subscription Key
APIM Trace header (optional)


🐞 Issues & Challenges
1. Integrating Swagger with API Gateway URL
Configuring Swagger to use the gateway invoke URL instead of local Kestrel URL required several attempts.
A custom config class (SwaggerConfig.cs) was created to dynamically insert the server URL.

2. Rate-Limiting Interaction with APIM
The assignment required rate-limiting at gateway level, but I initially implemented it as custom middleware.
Later:
APIM rate limit policy was applied,
Middleware was kept in the code as an example but disabled for production.

3. Database Migration to Azure SQL
Data seeding and EF migrations needed adjustments because:
Azure SQL timestamps differ slightly from LocalDB.
Needed TrustServerCertificate=True in the connection string during testing.


🗄️ Data Model
Entities:

1- Student
Id (PK)
StudentNo (unique)
FullName


2- TuitionCharge
Id (PK)
StudentId (FK)
Term
Amount
CreatedAt

3- Payment
Id (PK)
StudentId (FK)
Term
Amount
CreatedAt


Relationships:
Student → TuitionCharge : 1-to-many
Student → Payment : 1-to-many
Tuition balance = Sum(charges) − Sum(payments)


🧪 API Versioning
Default version: v1
Controllers decorated with:
[ApiVersion("1.0")]


✔ Requirements Mapping (Quick)
Requirement	Status
Versioned REST APIs	✔ Implemented
Mobile Query Tuition (No Auth + Rate Limit)	✔
Banking Query Tuition (Auth)	✔
Pay Tuition (No Auth)	✔
Admin Add Tuition	✔
Admin Batch Add Tuition (CSV)	✔
Admin Unpaid List (Paging + Auth)	✔
Logging (request + response)	✔
API Gateway Integration	✔
Swagger pointing to Gateway	✔


📚 Technologies Used
ASP.NET Core 8
Entity Framework Core 8
SQL Server (LocalDB)
Azure SQL
Azure App Service
Azure API Management (APIM)
Swagger / Swashbuckle
API Versioning Library
