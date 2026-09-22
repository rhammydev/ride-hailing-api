RIDE-HAILING API
Student Backend Project - User Stories & Requirements
ASP.NET Core Web API | C# | Microsoft SQL Server (MSSQL) | EF Core | JWT | Email | SMS

1. Project Overview
   Build a full-fledged Ride-Hailing REST API where passengers can create accounts, verify their identity, request rides and receive notifications; drivers can manage availability and assigned rides; and administrators can manage users, drivers and rides. The project must demonstrate authentication, authorization, resource ownership, database design, email and SMS integration.
2. Mandatory Technology Stack
   ASP.NET Core Web API with C#.
   Microsoft SQL Server (MSSQL) as the primary database.
   Entity Framework Core with Code-First migrations.
   JWT Bearer Authentication.
   BCrypt password hashing.
   Repository Pattern and Service Layer.
   Dependency Injection.
   Swagger / OpenAPI with JWT authorization support.
   Email integration using SMTP or another approved email provider.
   SMS integration using an approved SMS provider or a mock provider for development.
   Git and GitHub for source control.
3. Actors / Roles
   Passenger - registers, manages their profile, requests rides and views only their own rides.
   Driver - manages driver/vehicle details, availability and rides assigned to them.
   Admin - manages users, driver approvals, rides and audit activities.
4. Authentication User Stories
   Registration: As a Passenger or Driver, I want to register with my full name, email, phone number and password so that I can create an account.
   Email Verification: As a newly registered user, I want to receive a 6-digit OTP by email so that I can verify my email address.
   Phone Verification: As a user, I want to receive a verification OTP by SMS so that I can verify my phone number.
   Login: As a verified user, I want to log in using my email and password so that I can receive a JWT and access protected endpoints.
   Forgot Password: As a user who forgot my password, I want to request a password-reset OTP by email so that I can recover my account.
   Reset Password: As a user with a valid reset OTP, I want to create a new password so that I can regain access to my account.
   Change Password: As an authenticated user, I want to change my password after providing my current password.
   OTP rules:
   OTP must expire after 5 minutes.
   OTP must be single-use.
   Resending an OTP must invalidate the previous OTP.
   Password-reset responses must not expose whether an email address exists.
5. Ride-Hailing User Stories
   5.1 Passenger
   As a Passenger, I want to view and update my profile.
   As a Passenger, I want to request a ride by supplying pickup location and destination.
   As a Passenger, I want every ride to have a unique ride reference.
   As a Passenger, I want to view my current ride and ride history.
   As a Passenger, I want to cancel a ride when the business rules allow cancellation.
   As a Passenger, I must not be able to view or modify another passenger's rides.
   5.2 Driver
   As a Driver, I want to submit my driver and vehicle information.
   As a Driver, I want an Admin to approve my account before I can accept rides.
   As a Driver, I want to mark myself Available or Unavailable.
   As an approved and available Driver, I want to view available ride requests.
   As a Driver, I want to accept or reject a ride request.
   As a Driver, I want to update the status of a ride assigned to me.
   As a Driver, I must not be able to update a ride assigned to another driver.
   5.3 Admin
   As an Admin, I want to view all users and drivers.
   As an Admin, I want to approve or reject driver applications.
   As an Admin, I want to activate or deactivate user accounts.
   As an Admin, I want to view all rides in the system.
   As an Admin, I want to view audit logs for important system activities.
6. Ride Lifecycle & Business Rules
   A ride should follow a controlled lifecycle:
   Requested
   Accepted
   DriverArriving
   DriverArrived
   InProgress
   Completed
   Cancelled
   Invalid transitions must be rejected. For example, a Completed ride cannot return to Requested. Only the assigned driver can move an accepted ride through driver-controlled statuses.
7. Email & SMS Notification Stories
   Send an email OTP after registration.
   Send an SMS OTP for phone verification.
   Send password-reset OTP by email.
   Notify the Passenger when a Driver accepts the ride.
   Notify the Passenger when the Driver arrives.
   Notify relevant users when a ride is cancelled.
   Notify the Passenger when the ride is completed.
   Send security notification after a successful password change.
8. Authorization Requirements
   Use role-based authorization for Passenger, Driver and Admin endpoints.
   Use resource-ownership checks in addition to role checks.
   A Passenger cannot access another Passenger's ride.
   A Driver cannot update a ride that is not assigned to them.
   Only Admin can approve/reject Drivers and manage other users.
   Demonstrate 401 Unauthorized for missing/invalid authentication.
   Demonstrate 403 Forbidden when an authenticated user lacks permission.
9. JWT Requirements
   JWT tokens must contain enough claims to identify and authorize the user, including:
   User ID / NameIdentifier
   Full Name
   Email
   Role
   Phone Number
   Issuer
   Audience
   Expiration
10. Microsoft SQL Server (MSSQL) Database Requirements
    Microsoft SQL Server (MSSQL) is mandatory. Students must use Entity Framework Core migrations to create and update the database schema. Appropriate primary keys, foreign keys, indexes, unique constraints and relationships must be configured.
    Minimum suggested tables/entities:
    Users
    DriverProfiles
    Vehicles
    EmailOtps
    PhoneOtps
    PasswordResetOtps
    Rides
    RideStatusHistories
    Notifications
    AuditLogs
11. Suggested API Endpoints
    Module
    Method
    Endpoint
    Auth
    POST
    /api/auth/register
    Auth
    POST
    /api/auth/verify-email
    Auth
    POST
    /api/auth/login
    Auth
    POST
    /api/auth/forgot-password
    Auth
    POST
    /api/auth/reset-password
    Auth
    POST
    /api/auth/verify-phone
    Passenger
    GET
    /api/passenger/profile
    Passenger
    PUT
    /api/passenger/profile
    Ride
    POST
    /api/rides
    Ride
    GET
    /api/rides/my-rides
    Ride
    GET
    /api/rides/{id}
    Ride
    PUT
    /api/rides/{id}/cancel
    Driver
    PUT
    /api/driver/availability
    Driver
    GET
    /api/driver/rides/available
    Ride
    PUT
    /api/rides/{id}/accept
    Ride
    PUT
    /api/rides/{id}/status
    Admin
    GET
    /api/admin/users
    Admin
    GET
    /api/admin/drivers/pending
    Admin
    PUT
    /api/admin/drivers/{id}/approve
    Admin
    PUT
    /api/admin/drivers/{id}/reject
    Admin
    GET
    /api/admin/rides
    Admin
    GET
    /api/admin/audit-logs

12. Required Architecture
    Controller -> Service -> Repository -> Entity Framework Core -> MSSQL
    Controllers handle HTTP requests/responses.
    Services contain application and business logic.
    Repositories contain database-access logic.
    DTOs are used for request and response models.
    Entities must not be returned directly from controllers.
    Interfaces and dependency injection must be used for services and repositories.
13. Audit Logging
    Audit at least the following events:
    Registration and verification
    Successful and failed login attempts
    Password reset/change
    Driver approval/rejection
    Ride creation
    Ride acceptance
    Ride status changes
    Ride cancellation
    Ride completion
    Important Admin actions
14. Required Demonstration Scenarios
15. Register a Passenger and verify email/phone using OTP.
16. Log in and obtain a valid JWT.
17. Use a Passenger JWT on an Admin endpoint and demonstrate 403 Forbidden.
18. Register a Driver and complete verification/onboarding.
19. Approve the Driver using an Admin account.
20. Set the Driver to Available.
21. Request a ride as the Passenger.
22. Accept the ride as the approved Driver.
23. Move the ride through the valid lifecycle.
24. Demonstrate email and SMS notifications.
25. Attempt to access another Passenger's ride and show that access is denied.
26. Attempt to update another Driver's assigned ride and show that access is denied.
27. Complete forgot-password/reset-password flow.
28. Show audit records for important actions.
29. Deliverables
    Complete ASP.NET Core Web API source code.
    Microsoft SQL Server (MSSQL) database configured through EF Core migrations.
    Swagger/OpenAPI documentation with JWT support.
    Evidence that email and SMS integrations work.
    Project presentation and live API demonstration.
30. Assessment Focus
    Authentication and JWT implementation.
    Role-based and resource-based authorization.
    MSSQL database design and EF Core usage.
    Repository/Service architecture.
    Ride business rules and status lifecycle.
    Email and SMS integration.
    Validation, security and error handling.
    API design and Swagger documentation.
    Code quality, Git practices and ability to explain implementation decisions.
31. Important Instruction to Students
    The project is not assessed only by the number of endpoints created. Business rules and security rules must work correctly. You should be able to explain who can access each endpoint, why they can access it, what resource they own, what happens when access is denied, and how the database relationships support the application.
