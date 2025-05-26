# Users Table

## Purpose
The `Users` table stores information about all users in the system, including both human users and service accounts. It manages user authentication, preferences, and notification settings. The table is used by the UsersService to handle user management operations, including invitations and profile updates.

## Key Features

### 1. User Types
- Supports two types of users:
  - Human users (when `IsService = 0`)
  - Service accounts (when `IsService = 1`)

### 2. User States
- Automatically calculated state based on user properties:
  - `New`: No `ExternalId` and no `InvitedAt`
  - `Invited`: Has `InvitedAt` but no `ExternalId`
  - `Member`: Has `ExternalId`
- State is persisted and automatically updated based on user properties

### 3. Multi-Language Support
- Multiple language support for user names:
  - `Name`
  - `Name2`
  - `Name3`
- Personalized language preferences:
  - `PreferredLanguage`: Must be one of the languages supported by the company
  - `PreferredCalendar`: Must be one of the calendars supported by the company
- Language preferences are enforced to match company settings

### 4. Authentication
- `ExternalId`: Subject identifier for human users
- `ClientId`: Identifier for service accounts
- Both fields are required based on user type
- `ImageId`: Reference to user's profile image

### 5. Notifications and Communication
- Multiple contact channels:
  - `Email`: Primary email address used for user login and authentication
  - `ContactEmail`: Separate email address where notifications are sent when documents arrive in the user's Tellma inbox
  - Mobile (`ContactMobile` with normalized format)
  - Push notifications (with endpoint and encryption keys)

- Notification preferences:
  - Preferred channel (Email, Sms, or Push)
  - New inbox item notifications:
    - `EmailNewInboxItem`
    - `SmsNewInboxItem`
    - `PushNewInboxItem`

### 6. Activity Tracking
- `LastAccess`: Tracks when the user last accessed the system
- `LastInboxCheck`: Tracks when the user last checked their inbox
- `LastNotificationsCheck`: Tracks when the user last checked notifications
- `SortKey`: Decimal value for sorting users

### 7. Security and Status
- `IsActive`: Controls whether the user account is active
- Version tracking:
  - `PermissionsVersion`: Tracks permission changes
  - `UserSettingsVersion`: Tracks user settings changes
- Audit tracking:
  - `CreatedAt`
  - `CreatedById`
  - `ModifiedAt`
  - `ModifiedById`

## Special Features

### 1. Push Notifications
- Comprehensive push notification support:
  - `PushEndpoint`: URL for sending push notifications
  - `PushP256dh`: Public key for encryption
  - `PushAuth`: Authentication secret

### 2. Mobile Support
- Mobile number normalization:
  - `ContactMobile`: Raw mobile number
  - `NormalizedContactMobile`: Standardized format

### 3. Activity Monitoring
- Tracks last access times for:
  - System access
  - Inbox checks
  - Notification checks

## Usage

1. **User Management**
   - Create and manage both human users and service accounts
   - Track user states from invitation to active membership
   - Manage user preferences and settings
   - Handle user invitations with email notifications
   - Update user profiles with validation

2. **User Settings**
   - Personalized language and calendar preferences
   - Notification preferences (email, SMS, push)
   - Contact information management
   - Profile image management

3. **Security**
   - Email validation
   - Phone number validation
   - Permission-based access control
   - Versioned settings tracking
   - Company-wide language and calendar constraints

2. **Communication**
   - Configure multiple communication channels
   - Set up notification preferences
   - Track user engagement through activity monitoring

3. **Security**
   - Track permission changes
   - Monitor user activity
   - Control account status

## Note
- The table is designed to support both human users and service accounts through the `IsService` flag
- All timestamps are stored with timezone information (DATETIMEOFFSET)
- The table includes comprehensive audit tracking for all modifications
