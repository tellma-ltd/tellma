# AgentUsers Table

## Overview
This table was designed to establish a many-to-many relationship between `Agents` and `Users`, which would allow multiple users to be associated with a single agent. This design was intended to support scenarios where multiple users need to access the system on behalf of the same agent (e.g., multiple employees of a supplier company).

**Note:** While this table was created in the database schema, it was never actually used in the Tellma application. The functionality it was meant to provide was implemented differently in the actual system.

## Table Structure
```sql
CREATE TABLE [dbo].[AgentUsers] (
    [Id]                INT                 IDENTITY PRIMARY KEY,
    [AgentId]           INT                 NOT NULL,
    [UserId]            INT                 NOT NULL,
    [CreatedAt]         DATETIMEOFFSET(7)   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CreatedById]       INT                 NOT NULL,
    [ModifiedAt]        DATETIMEOFFSET(7)   NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
    [ModifiedById]      INT                 NOT NULL
);
```

## Key Fields
- **Id**: Unique identifier for each agent-user association
- **AgentId**: References the `Agents` table. This is the agent that the user can represent.
- **UserId**: References the `Users` table. This is the user who can represent the agent.

### Audit Fields
- **CreatedAt**: Timestamp when the association was created
- **CreatedById**: User who created the association
- **ModifiedAt**: Timestamp when the association was last modified
- **ModifiedById**: User who last modified the association

## Relationships
- **Agent**: Many-to-one relationship with `Agents` table (on `AgentId`)
- **User**: Many-to-one relationship with `Users` table (on `UserId`)
- **CreatedBy/ModifiedBy**: Many-to-one relationships with `Users` table

## Design Intent
This table was designed to support the following use cases, though these were never implemented:

1. **For Organizations**:
   - Multiple users can be associated with a single agent (e.g., multiple employees of a supplier company)
   - All associated users can log in and represent the same agent

2. **For Individuals**:
   - Individual agents (like employees) typically have their user account directly in the `Agents` table
   - The `AgentUsers` table is not used for individual agents

3. **User Experience**:
   - When users log in, they can select which agent they want to represent from their associated agents
   - The system enforces that users can only access data for agents they are explicitly associated with

## Constraints
- **PK_AgentUsers**: Primary key on `Id`
- **FK_AgentUsers__AgentId**: Foreign key to `Agents(Id)` with CASCADE DELETE
- **FK_AgentUsers__UserId**: Foreign key to `Users(Id)`
- **FK_AgentUsers__CreatedById**: Foreign key to `Users(Id)`
- **FK_AgentUsers__ModifiedById**: Foreign key to `Users(Id)`

## Example Use Cases
1. **Supplier Portal**:
   - A supplier company (Agent) has multiple employees (Users)
   - Each employee has their own login credentials
   - All employees access the same supplier data

2. **Department Access**:
   - A department (Agent) has multiple staff members (Users)
   - All staff can access department-level information
   - Individual staff may have different permissions within the department

3. **Multi-Company Users**:
   - A consultant (User) works with multiple client companies (Agents)
   - The consultant can switch between different client contexts
   - Access is restricted to only the companies they are associated with
