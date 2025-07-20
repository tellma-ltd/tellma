# DashboardDefinitionRoles Table

## Overview
The `DashboardDefinitionRoles` table establishes a many-to-many relationship between dashboards and roles, controlling which user roles have access to specific dashboards. This table implements role-based access control (RBAC) for dashboards in the Tellma application.

## Table Structure
```sql
CREATE TABLE [dbo].[DashboardDefinitionRoles]
(
    [Id]                    INT     IDENTITY PRIMARY KEY,
    [DashboardDefinitionId] INT     NOT NULL,
    [RoleId]               INT     NOT NULL
);
```

## Key Fields
- **Id**: Unique identifier for the role assignment
- **DashboardDefinitionId**: References the dashboard in `DashboardDefinitions`
- **RoleId**: References the role in `Roles` that is granted access to the dashboard

## Relationships
- **DashboardDefinition**: Many-to-one relationship with `DashboardDefinitions` table (on `DashboardDefinitionId`)
  - CASCADE DELETE: When a dashboard is deleted, all its role assignments are automatically deleted
- **Role**: Many-to-one relationship with `Roles` table (on `RoleId`)

## Access Control Mechanism
1. **Role-Based Access**:
   - Only users with roles listed in this table can access the associated dashboard
   - Users must have at least one role that has been granted access

2. **Inheritance**:
   - If a user has multiple roles, they will have access to all dashboards accessible by any of their roles
   - Role hierarchies or permissions are not managed in this table

3. **Default Access**:
   - If no roles are assigned to a dashboard, it is effectively hidden from all users
   - System administrators typically have a special role that grants access to all dashboards

## Usage Examples
1. **Sales Dashboard Access**:
   - Dashboard: 'Sales Performance'
   - Roles: 'Sales Manager', 'Sales Representative', 'Regional Director'

2. **HR Dashboard Access**:
   - Dashboard: 'Employee Performance'
   - Roles: 'HR Manager', 'Department Head', 'CEO'

3. **Finance Dashboard Access**:
   - Dashboard: 'Financial Overview'
   - Roles: 'CFO', 'Finance Manager', 'Accountant'

## Constraints
- **PK_DashboardDefinitionRoles**: Primary key on `Id`
- **FK_DashboardDefinitionRoles_DashboardDefinitionId**: Foreign key to `DashboardDefinitions(Id)` with CASCADE DELETE
- **FK_DashboardDefinitionRoles_RoleId**: Foreign key to `Roles(Id)`
- **UQ_DashboardDefinitionRoles_DashboardId_RoleId**: Unique constraint on the combination of `DashboardDefinitionId` and `RoleId` to prevent duplicate role assignments

## Best Practices
1. **Least Privilege**:
   - Only grant dashboard access to roles that truly need it
   - Regularly review and clean up unused role assignments

2. **Role Design**:
   - Create roles that align with organizational structure and responsibilities
   - Consider creating composite roles for common permission sets

3. **Documentation**:
   - Maintain clear documentation of which roles have access to which dashboards
   - Include access control information in dashboard documentation

## Related Tables
- `DashboardDefinitions`: Contains the dashboard definitions
- `Roles`: Contains the role definitions
- `RoleMemberships`: Defines which users belong to which roles

## Security Considerations
- Changes to role assignments should be audited
- Consider implementing row-level security if more granular control is needed
- Regular security reviews should verify that access controls are correctly configured
