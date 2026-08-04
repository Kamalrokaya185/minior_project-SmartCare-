# SmartCare Development Roadmap

## Overview

Develop the system feature-by-feature (vertical slices), not
table-by-table.

## Phase 1 -- Foundation & Authentication

**Goal:** Build the project backbone.

### Features

-   Project setup (.NET Clean Architecture)
-   Entity Framework Core
-   Dependency Injection
-   JWT Authentication
-   Login / Logout
-   Refresh Token
-   User Session Management
-   Password Hashing
-   Forgot / Reset Password
-   Email Verification

### Tables

-   Users
-   Roles
-   UserRoles
-   RefreshTokens
-   UserSessions
-   LoginHistories

### Done When

-   User registration works
-   Login/logout works
-   JWT & refresh token work
-   Sessions are tracked

------------------------------------------------------------------------

## Phase 2 -- Super Admin

### Features

-   Dashboard
-   Create / Approve / Suspend Hospital
-   Manage Subscription Plans

### Tables

-   Hospitals
-   SubscriptionPlans
-   HospitalSubscriptions
-   AuditLogs

------------------------------------------------------------------------

## Phase 3 -- Hospital Management

### Features

-   Hospital Profile
-   Departments
-   Staff Management

### Tables

-   Hospitals
-   Departments
-   HospitalMemberships

------------------------------------------------------------------------

## Phase 4 -- Doctor Management

### Features

-   Doctor Profile
-   Doctor Schedule
-   Doctor Leave
-   Availability

### Tables

-   DoctorProfiles
-   DoctorSchedules
-   DoctorLeaves
-   HospitalMemberships

------------------------------------------------------------------------

## Phase 5 -- Patient Module

### Features

-   Register Patient
-   Patient Profile
-   Attendance Score

### Tables

-   PatientProfiles
-   AttendanceScores

------------------------------------------------------------------------

## Phase 6 -- Hospital Policies

### Features

-   Payment Policy
-   Cancellation Policy
-   Booking Rules
-   Doctor Login Setting
-   Advance Payment
-   Booking Window

### Tables

-   HospitalPolicies (or BookingPolicies + PaymentPolicies +
    CancellationPolicies)

------------------------------------------------------------------------

## Phase 7 -- Appointment System (Core)

### Workflow

Patient → Search Hospital → Select Doctor → View Schedule → Book
Appointment → Receptionist Confirms → Appointment Completed / Cancelled
/ No Show

### Tables

-   Appointments
-   AppointmentStatusHistory
-   DoctorSchedules
-   AttendanceScores

------------------------------------------------------------------------

## Phase 8 -- Payment

### Features

-   Appointment Payment
-   Invoice
-   Refund Request

### Tables

-   Payments
-   Invoices
-   RefundRequests

------------------------------------------------------------------------

## Phase 9 -- Notifications

### Features

-   Booking Confirmation
-   Appointment Reminder
-   Cancellation Notice
-   Payment Notifications

### Tables

-   Notifications
-   NotificationTemplates

------------------------------------------------------------------------

## Phase 10 -- Reviews

### Features

-   Doctor Reviews
-   Hospital Reviews

### Tables

-   Reviews

------------------------------------------------------------------------

## Phase 11 -- Security

### Features

-   Audit Logs
-   Active Sessions
-   Login History
-   Force Logout

### Tables

-   AuditLogs
-   UserSessions
-   RefreshTokens

------------------------------------------------------------------------

## Phase 12 -- Reports & Dashboards

### Super Admin

-   Revenue
-   Hospitals
-   Users
-   Subscriptions

### Hospital

-   Daily Appointments
-   Revenue
-   Doctor Performance

### Doctor

-   Today's Appointments
-   Completed Appointments
-   Ratings

### Patient

-   My Appointments
-   Payments

------------------------------------------------------------------------

# Development Workflow (Repeat for Every Phase)

1.  Create Entity Models
2.  Configure EF Core Relationships
3.  Repository Interfaces
4.  Repository Implementations
5.  DTOs
6.  Commands & Queries (CQRS)
7.  Validation
8.  Business Logic
9.  API Controllers
10. Test with Swagger/Postman
11. Unit Tests (optional initially)
12. Connect Frontend

------------------------------------------------------------------------

# Recommended Priority

1.  Authentication
2.  Super Admin
3.  Hospital Management
4.  Doctor Management
5.  Patient Module
6.  Hospital Policies
7.  Appointment Booking (Core)
8.  Payment
9.  Notifications
10. Reviews
11. Security
12. Reports

The core business flow is:

Authentication → Hospital → Doctor → Schedule → Patient → Appointment
