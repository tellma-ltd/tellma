# AccountTypeNotedAgentDefinitions Table Documentation

## Overview
The AccountTypeNotedAgentDefinitions table defines the valid agent types that can be used as noted agents (Entries.NotedAgentId) when using specific account types. This table is optional - an account type can have 0 or more noted agent definitions.

When defined, it ensures that only appropriate secondary agents can be associated with entries using specific account types.

## Purpose
The primary purpose of this table is to enforce noted agent type restrictions when using specific accounts in table Entries. For example:
- If an account type is "Employee Benefits Expense"
- And it is associated with "Employee" noted agent definition
- Then any entry using this account type must use an employee as its noted agent

### Optional Nature
- An account type can have 0 noted agent definitions
- If no definitions are specified, any agent can be used as noted agent
- This provides flexibility for different accounting needs

### How It Works
1. Entries table uses Accounts (not AccountTypes directly)
2. Each Account has an AccountType
3. AccountTypeNotedAgentDefinitions defines valid noted agent types for each AccountType
4. When creating an entry:
   - The system checks the Account's AccountType
   - Looks up allowed noted agent definitions from AccountTypeNotedAgentDefinitions
   - Validates that the selected noted agent matches one of the allowed types

## Table Structure
```sql
CREATE TABLE [dbo].[AccountTypeNotedAgentDefinitions] (
    [Id] INT CONSTRAINT [PK_AccountTypeNotedAgentDefinitions] PRIMARY KEY IDENTITY,
    [AccountTypeId] INT NOT NULL CONSTRAINT [FK_AccountTypeNotedAgentDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
    [AgentDefinitionId] INT NOT NULL CONSTRAINT [FK_AccountTypeNotedAgentDefinitions__AgentDefinitionId] REFERENCES dbo.[AgentDefinitions]([Id])
)
```

### Key Fields

1. **AccountTypeId**
   - References AccountTypes table
   - Identifies which account type this restriction applies to
   - ON DELETE CASCADE ensures cleanup if account type is deleted

2. **AgentDefinitionId**
   - References AgentDefinitions table
   - Defines which agent type is allowed as noted agent for this account type
   - Must match the agent type used in Entries.NotedAgentId

## Usage Examples

1. **Employee Expenses Example**
```sql
-- Account Type: Employee Expenses
-- Noted Agent Definition: Employee
-- Result: When using this account type, the noted agent must be an employee
```

2. **Fuel Expenses Example**
```sql
-- Account Type: Fuel Expenses
-- Noted Agent Definition: Employee
-- Result: When recording fuel expenses, the noted agent must be the employee who incurred the expense
-- Example: Employee Tom drove company vehicle and incurred fuel expenses
```

2. **Purchase Orders Example**
```sql
-- Account Type: Purchase Orders
-- Noted Agent Definition: Purchase Invoice
-- Result: When using this account type, the noted agent must be a purchase invoice
```

## Best Practices

1. **Noted Agent Type Consistency**
   - Always ensure the noted agent definition matches the account type's purpose
   - For example, don't associate "Supplier" noted agent with "Employee Expenses" account type

2. **Multiple Noted Agent Types**
   - An account type can have multiple noted agent definitions
   - This allows flexibility while maintaining control
   - Example: An account type might allow both "Employee" and "Department" noted agents

3. **System Validation**
   - The system automatically validates that entries match these restrictions
   - Attempts to use incorrect noted agent types will be rejected
   - This prevents accounting errors and maintains data integrity

## System Features

1. **Relationship Enforcement**
   - Ensures entries follow proper accounting rules
   - Prevents incorrect noted agent assignments
   - Maintains data consistency across the system

2. **Audit Trail**
   - All changes are tracked
   - History available in AccountTypeNotedAgentDefinitionsHistory
   - Maintains versioning for audit purposes

3. **Cascade Deletion**
   - When an account type is deleted, its noted agent definitions are automatically removed
   - Prevents orphaned records
   - Maintains database integrity

## Special Notes

1. **Entry Validation**
   - When creating an entry:
     1. The system checks the account type
     2. Looks up allowed noted agent definitions
     3. Validates that the selected noted agent matches one of the allowed types

2. **Multiple Noted Agent Types**
   - While an account type can have multiple noted agent definitions:
     - Each entry must use exactly one noted agent type
     - The noted agent must match one of the allowed definitions
     - This maintains clear accounting trails

3. **System Integration**
   - Works in conjunction with AccountTypes and AgentDefinitions tables
   - Forms part of the core accounting validation system
   - Critical for maintaining proper accounting controls
