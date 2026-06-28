# Respondr — Product Requirements Document

## 1. Product Name

Respondr

## 2. Product Summary

Respondr is a lightweight disaster response operations application for tracking emergency incidents, assigning response units, and keeping users updated in real time.

The application is designed for a small operations team with two main users: a Dispatcher and an Operations Lead.

## 3. Problem Statement

During an emergency, response teams need a simple way to record incidents, see available response units, assign help, and track progress. Without a shared operational view, users may rely on manual updates, repeated communication, or outdated information.

Respondr solves this by providing one shared workspace where incidents and response units can be managed clearly.

## 4. Goals

The goal of Respondr is to allow users to:

* Log in securely.
* View the current emergency situation.
* Create and manage incidents.
* View and manage response units.
* Assign available units to incidents.
* Receive real-time updates when important changes happen.

## 5. Users

### Dispatcher

The Dispatcher records new emergency reports and monitors incident updates.

### Operations Lead

The Operations Lead reviews incidents, assigns response units, and tracks progress until an incident is resolved or closed.

## 6. Scope

Respondr will include the following modules:

1. Auth Module
2. Dashboard Module
3. Incident Module
4. Response Unit Module
5. Assignment Module
6. Real-Time Updates Module

## 7. Out of Scope

The first version will not include:

* GPS tracking
* Maps
* SMS notifications
* Email notifications
* Mobile app
* File uploads
* Advanced reports
* AI assistant
* Offline mode
* Multi-agency workflows

## 8. Main User Flow

A Dispatcher receives a report about a flood emergency in Tampa, Florida. The Dispatcher logs in to Respondr, opens the dashboard, and creates a new critical flood incident. The Operations Lead receives a real-time notification, reviews the incident, checks the available response units, and assigns Rescue Team A to the incident. Respondr updates the incident status and response unit status. Both users see the latest changes immediately. As the response progresses, the incident moves from New, to Assigned, to In Progress, to Resolved, and then Closed.

## 9. Success Criteria

Respondr is successful when:

* A user can log in and log out.
* A Dispatcher can create an incident.
* An Operations Lead can assign a response unit to an incident.
* Users can update incident status.
* Users can view response unit availability.
* Real-time updates appear without refreshing the page.
* Basic tests are included for key flows.
