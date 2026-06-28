# Respondr Frontend Design System

## Purpose

This document defines the visual language, interaction patterns, and reusable UI standards for Respondr. It should guide frontend implementation so the application feels consistent, professional, and appropriate for emergency response operations.

Respondr should feel like an operational command dashboard: calm, structured, fast to scan, and optimized for repeated daily use by Dispatchers and Operations Leads.

## Design Principles

- Operational clarity comes before decoration.
- Critical information must be visible, scannable, and hard to miss.
- Layouts should be dense enough for operations work but not visually crowded.
- Status, priority, connection health, and validation must be communicated clearly.
- Controls should use familiar dashboard patterns: navigation, tables, filters, drawers, modals, badges, forms, and toasts.
- The UI should remain usable when realtime updates are temporarily unavailable.
- Design decisions should map cleanly to reusable Angular components.

## Visual Style

Use a light enterprise dashboard style:

- Soft app background.
- White content surfaces.
- Blue primary actions.
- Red critical/danger states.
- Green success/available states.
- Amber warning/pending states.
- Neutral gray for closed, disabled, unavailable, or secondary information.
- Restrained borders and shadows.
- Minimal decorative elements.

The UI should not use a landing-page style, heavy hero sections, large decorative gradients, or overly illustrative layouts.

## Design Tokens

Recommended base tokens:

```text
Primary:        #238be6
Primary dark:   #176fc0
Background:     #f8fafc
Surface:        #ffffff
Surface soft:   #f2f6fb
Border:         #e4e8ef
Text:           #20242b
Muted text:     #667085
Danger:         #f04444
Danger bg:      #fff1f1
Success:        #25bf7a
Warning:        #c56a12
Font:           Inter, system-ui, -apple-system, BlinkMacSystemFont, Segoe UI, sans-serif
Sidebar width:  292px expanded, 88px collapsed
Topbar height:  72px
Card radius:    8px
Control radius: 8px
```

Spacing should use a consistent scale:

```text
4px, 8px, 12px, 16px, 20px, 24px, 32px
```

## Typography

- Use clear, compact dashboard typography.
- Page titles should be prominent but not oversized.
- Card headings should be smaller and tighter than page headings.
- Body text should prioritize readability.
- Labels, table headers, badges, and metadata should use stronger weight for scanning.
- Do not use negative letter spacing.
- Do not scale font size directly with viewport width.

Suggested hierarchy:

```text
Page title:      28-36px
Section title:   20-24px
Card title:      16-18px
Body:            14-16px
Metadata:        12-14px
Badge:           12-13px
```

## Layout

The authenticated app uses a persistent operational shell:

```text
Sidebar
Topbar
Main content area
```

Layout rules:

- Keep the sidebar persistent on desktop.
- Allow the sidebar to collapse on desktop.
- Use an off-canvas sidebar on mobile.
- Keep the topbar sticky.
- Keep page headings close to page content.
- Use multi-column dashboards on desktop and stacked layouts on mobile.
- Avoid nesting cards inside cards.
- Use stable dimensions for toolbars, icon buttons, table rows, stat cards, and status badges to prevent layout shifts.

## Components

Core reusable components:

- App shell
- Sidebar navigation
- Topbar
- Page heading
- Stat card
- Badge/status pill
- Data table
- Pagination
- Search control
- Filter drawer
- Modal
- Toast
- Empty state
- Loading state
- Error alert
- Timeline
- Feed item
- Unit card
- Resource meter
- Segmented control
- Form field wrapper

Components should be accessible, keyboard-operable, and reusable across feature modules.

## Buttons And Actions

Use button hierarchy consistently:

- Primary: main page or form action.
- Outline: secondary action.
- Ghost: low-emphasis action.
- Icon button: compact utility action.
- Destructive: dangerous or irreversible action.

Button rules:

- Use icons for common actions where helpful.
- Icon-only buttons must have accessible labels.
- Avoid vague labels such as Submit when a specific action is available.
- Primary actions should be limited to one or two per page region.
- Loading buttons should preserve width and avoid layout shift.

## Forms

Form rules:

- Labels are always visible.
- Required fields are clearly marked.
- Validation errors appear inline near the field.
- Form-level errors appear above the affected section.
- Inputs, selects, textareas, segmented controls, and upload zones should share visual rhythm.
- Long forms should be grouped into logical sections.
- Do not put long operational forms inside modals.
- Sticky mobile form actions are allowed for long forms.

Use:

- Text inputs for freeform values.
- Selects for controlled backend enum values.
- Segmented controls for short exclusive choices such as priority.
- Checkboxes for independent binary settings.
- Radio groups for mutually exclusive choices where all options should remain visible.

## Tables

Tables are a primary operational surface.

Table rules:

- Support search and filters for operational lists.
- Keep important status and priority columns visible.
- Keep row actions accessible on desktop and mobile.
- Use pagination or virtual scrolling for long lists.
- On mobile, convert rows into stacked key-value layouts.
- Do not rely on color alone to communicate priority or status.

## Badges And Status

Use badges for compact state display.

Priority:

- Critical: red filled badge.
- High: red or amber outline badge.
- Medium: blue or neutral badge.
- Low: neutral badge.

Incident status:

- New: neutral or blue.
- Assigned: blue.
- En Route: amber.
- In Progress or On Scene: primary blue.
- Resolved: green.
- Closed: gray.
- Cancelled: gray or red, depending on reason.

Responder availability:

- Available: green.
- Assigned: blue.
- En Route: amber.
- On Scene: primary blue.
- Unavailable: gray or red.

Resource request status:

- Pending: amber.
- Approved: blue or green.
- Rejected: red.
- Allocated: blue.
- Fulfilled: green.
- Cancelled: gray.

## Drawers, Modals, And Toasts

Use drawers for:

- Advanced filters.
- Temporary side panels.
- Contextual operational details.

Use modals for:

- Short focused tasks.
- Confirmation prompts.
- Small create/edit tasks when the form is brief.

Use toasts for:

- Save confirmations.
- Assignment updates.
- Live connection restored.
- Notification marked as read.
- Critical realtime alerts when appropriate.

Do not use toasts for field validation. Validation belongs near the field.

## Realtime Feedback

Realtime UI should improve awareness without interrupting work.

Rules:

- Show live connection state.
- Show degraded state when SignalR is disconnected but REST still works.
- Highlight updated rows briefly.
- Append important events to feeds and timelines.
- Do not overwrite unsaved form input due to incoming events.
- Use toasts selectively for high-value events.

## Accessibility

Accessibility requirements:

- Every icon-only button has an accessible label.
- Form fields have visible labels.
- Modals and drawers manage focus.
- Dropdowns expose expanded/collapsed state.
- Toast region uses polite live announcements.
- Color is not the only indicator of meaning.
- Keyboard users can operate all controls.
- Tables remain understandable with screen readers and mobile stacked labels.

## Responsive Behavior

Desktop:

- Expanded sidebar.
- Sticky topbar.
- Multi-column dashboards and detail pages.
- Full tables.

Tablet:

- Collapsible sidebar.
- Two-column dashboards where space allows.
- Detail side panels stack below primary content.

Mobile:

- Off-canvas sidebar.
- Compressed topbar search.
- Stacked page headings and actions.
- Stacked table rows.
- Full-width drawers.
- Sticky form actions where useful.

Recommended breakpoints:

```text
1180px: reduce large grids
900px: switch shell/sidebar behavior
680px: stack forms, tables, and page actions
```

## Definition Of Done

UI work is acceptable when:

- Layout works on desktop, tablet, and mobile.
- Text does not overflow or overlap.
- Loading, empty, error, and degraded-live states exist.
- Important controls are keyboard accessible.
- Status and priority are visually consistent.
- Components follow the shared design tokens.
- The implementation remains appropriate for a professional emergency operations product.
