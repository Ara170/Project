# Hotel Management API

A RESTful API for hotel management built with ASP.NET Core 8, Entity Framework Core, JWT Authentication, and Swagger.

## Features

- **Authentication & Authorization**: JWT-based authentication with role-based authorization (Admin, Staff, Customer)
- **Room Management**: Create, read, update, and delete hotel rooms and room types
- **Booking System**: Handle room bookings, check-in, and check-out processes
- **Service Management**: Manage hotel services and their pricing
- **Billing System**: Generate bills for bookings with discount support
- **User Management**: Register, update profiles, and manage user accounts
- **Feedback System**: Allow customers to provide feedback

## Technologies

- ASP.NET Core 8
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger/OpenAPI

## API Endpoints

### Authentication
- POST /api/Auth/register - Register a new customer
- POST /api/Auth/login - Login and get JWT token

### Room Management
- GET /api/Room - Get all rooms
- GET /api/Room/{id} - Get room by ID
- GET /api/Room/available - Get available rooms
- POST /api/Room - Create a new room (Admin)
- PUT /api/Room/{id} - Update room (Admin, Staff)
- DELETE /api/Room/{id} - Delete room (Admin)

### Room Types
- GET /api/TypeRoom - Get all room types
- GET /api/TypeRoom/{id} - Get room type by ID
- POST /api/TypeRoom - Create a new room type (Admin)
- PUT /api/TypeRoom/{id} - Update room type (Admin)
- DELETE /api/TypeRoom/{id} - Delete room type (Admin)

### Booking
- GET /api/Booking - Get all bookings (Admin, Staff)
- GET /api/Booking/{id} - Get booking by ID
- GET /api/Booking/customer - Get customer bookings (Customer)
- POST /api/Booking - Create a new booking (Customer)
- DELETE /api/Booking/{id} - Cancel booking (Customer)

### Services
- GET /api/Service - Get all services
- GET /api/Service/{id} - Get service by ID
- POST /api/Service - Create a new service (Admin)
- PUT /api/Service/{id} - Update service (Admin)
- DELETE /api/Service/{id} - Delete service (Admin)

### Discounts
- GET /api/Discount - Get all discounts
- GET /api/Discount/{id} - Get discount by ID
- POST /api/Discount - Create a new discount (Admin)
- PUT /api/Discount/{id} - Update discount (Admin)
- DELETE /api/Discount/{id} - Delete discount (Admin)

### Bills
- GET /api/Bill - Get all bills (Admin, Staff)
- GET /api/Bill/{id} - Get bill by ID
- GET /api/Bill/customer - Get customer bills (Customer)
- POST /api/Bill - Create a new bill (Staff)
- PUT /api/Bill/{id}/check - Mark bill as checked (Staff)

### User Management
- GET /api/User/profile - Get user profile
- PUT /api/User/profile - Update user profile
- PUT /api/User/password - Change password
- GET /api/User - Get all users (Admin)
- GET /api/User/customers - Get all customers (Admin, Staff)
- GET /api/User/staff - Get all staff (Admin)
- POST /api/User/staff - Create a new staff account (Admin)
- PUT /api/User/{id}/state - Enable/disable user account (Admin)

### Feedback
- GET /api/Feedback - Get all feedbacks
- POST /api/Feedback - Create a new feedback (Customer)
- DELETE /api/Feedback - Delete feedback (Customer)

## Getting Started

1. Clone the repository
2. Update the connection string in `appsettings.json` to point to your SQL Server instance
3. Run the database migrations: `dotnet ef database update`
4. Run the application: `dotnet run`
5. Access the Swagger UI at `https://localhost:7240/`

## Database Schema

The database consists of the following tables:
- TypeRoom: Stores room types with prices
- Discounts: Stores discount information
- Rooms: Stores room information
- Services: Stores hotel services with prices
- Roles: Stores user roles (Admin, Staff, Customer)
- Users: Stores user account information
- Customers: Stores customer information
- Staffs: Stores staff information
- Bookings: Stores booking information
- Detail_Bookings: Stores detailed booking information
- Detail_Services: Stores services booked by customers
- Bills: Stores billing information
- Feedbacks: Stores customer feedback
