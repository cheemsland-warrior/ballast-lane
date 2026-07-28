This is a test project to showcase my skills as a full stack developer. The purpose of the platform is to provide a service that allows the user to register potholes in the city. 
This is something that can be useful for government agencies or for people to be careful where they are driving. It is a toy project but it has potential
The project is composed of a Web API project in .net core which uses entity framework and PostgreSQL as the database
and angular for the project front end. When running the .net project and angular front end be sure to check that the .net project is ran in https://localhost:7296/ it should be that by default
but if by any chance the .net project is ran in a different port be sure to change that in base.service.ts in the angular code.

You can clone the repository with Git and then be sure to find the folder where the docker file is and do docker compose up that will set up the image with the database. After that you do need to run
dotnet ef database update to run the migrations.
After that you can do npm i --legacy-peer-deps  and run the angular project and register an account, log in add a pothole and see the list of potholes. Pothole list is public and does not need login but creating is guarded under
an authorize attribute on the controller.


The following is the prompt to generate this project:

Configuration & Environment
•	Connection String: PostgreSQL (default: localhost:5432, database: yourappdb)
•	JWT Secret: Configurable via appsettings.json (with fallback for dev)
•	Environment-aware: Support for Development and Production configs
•	User Secrets: Support local secrets storage during development
Dependencies
•	AutoMapper 16.2.0
•	Entity Framework Core 8.0 with PostgreSQL provider (Npgsql)
•	ASP.NET Core Authentication (JWT Bearer)
•	Serilog for logging
•	Swashbuckle for Swagger/OpenAPI
•	Microsoft.AspNetCore.Authorization
Key Features
 User registration with password hashing
 JWT-based authentication and authorization
 Geolocation pothole tracking (latitude/longitude validation)
 Status workflow (Reported → In Progress → Fixed)
 PostgreSQL with EF Core 8.0
 Structured logging with Serilog
 AutoMapper for clean DTO mapping
 Swagger API documentation
 CORS configuration for frontend integration
 Clean layered architecture (Controllers → Services → Data)

 Structure:
├── Controllers/
│   ├── PotholesController.cs
│   ├── UsersController.cs
│   └── FallbackController.cs
├── Services/
│   ├── IUserService.cs & UserService.cs
│   ├── IPotholeService.cs & PotholeService.cs
│   ├── IAuthService.cs & AuthService.cs
├── Models/
│   ├── Pothole.cs
│   ├── User.cs
│   ├── Dtos.cs (UserDto, PotholeDto)
│   ├── AuthDtos.cs (RegisterRequest, LoginRequest, AuthResponse)
│   └── PotholeToCreateDto.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── ApplicationDbContextFactory.cs
├── Program.cs (Startup configuration)
├── MappingProfiles.cs (AutoMapper setup)
├── appsettings.json
└── appsettings.Development.json


The project is just a toy test but it works and has it's corresponding tests on back and front.
I hope you like it.
Regards,
Cesar
