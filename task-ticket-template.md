# Respondr — Task Ticket Template and Sample Tickets

## Task Ticket Template

### Title

Use a short action-based title.

Example: Create Login Page

### Module

Example: Auth Module

### Story

As a user, I want to log in so that I can access Respondr.

### Requirements

Describe what the task must include.

### Objectives

Describe what this task is trying to achieve.

### Acceptance Criteria

Use clear pass/fail conditions.

### Test Strategy

Use Given, When, Then.

---

# Sample Task Tickets

## AUTH-001 — Create Login Page

### Module

Auth Module

### Story

As a Dispatcher or Operations Lead, I want to log in to Respondr so that I can access the application.

### Requirements

* The login page must allow the user to enter email and password.
* The login page must show validation when required fields are missing.
* The login page must show an error message when login fails.
* The login page must redirect the user to the dashboard after successful login.

### Objectives

* Provide a clear entry point to Respondr.
* Prevent unauthenticated users from accessing the application.
* Prepare the application for role-based access.

### Acceptance Criteria

* The user can enter an email and password.
* The user cannot submit the form when required fields are empty.
* The user sees an error message when credentials are invalid.
* The user is redirected to the dashboard after successful login.

### Test Strategy

Given the user opens the login page, when the page loads, then email and password fields are visible.

Given the user submits the form with empty fields, when validation runs, then required field messages are shown.

Given the user enters valid credentials, when they submit the form, then they are redirected to the dashboard.

Given the user enters invalid credentials, when they submit the form, then an error message is displayed.