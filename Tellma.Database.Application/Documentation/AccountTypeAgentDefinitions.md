# AccountTypeAgentDefinitions Table Documentation

## Overview
The AccountTypeAgentDefinitions table defines the valid agent types that can be used with specific account types. This table establishes a relationship between account types and agent definitions, ensuring that only appropriate agents can be used in accounting entries.

## Purpose
The primary purpose of this table is to enforce agent type restrictions when using specific accounts in table Entries. For example:
- If an account has type "Balances with Banks"
- And this account type is associated with "Bank Account" agent definition
- Then any entry using this account must use a bank account as its agent

### How It Works
1. Entries table uses Accounts (not AccountTypes directly)
2. Each Account has an AccountType
3. AccountTypeAgentDefinitions defines valid agent types for each AccountType
4. When creating an entry:
   - The system checks the Account's AccountType
   - Looks up allowed agent definitions from AccountTypeAgentDefinitions
   - Validates that the selected agent matches one of the allowed types

## Table Structure
```sql
CREATE TABLE [dbo].[AccountTypeAgentDefinitions] (
    [Id] INT CONSTRAINT [PK_AccountTypeAgentDefinitions] PRIMARY KEY IDENTITY,
    [AccountTypeId] INT NOT NULL CONSTRAINT [FK_AccountTypeAgentDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
    [AgentDefinitionId] INT NOT NULL CONSTRAINT [FK_AccountTypeAgentDefinitions__AgentDefinitionId] REFERENCES dbo.[AgentDefinitions]([Id])
)
```

### Key Fields

1. **AccountTypeId**
   - References AccountTypes table
   - Identifies which account type this restriction applies to
   - ON DELETE CASCADE ensures cleanup if account type is deleted

2. **AgentDefinitionId**
   - References AgentDefinitions table
   - Defines which agent type is allowed for this account type
   - Must match the agent type used in Entries.AgentId

## Usage Examples

1. **Bank Accounts Example**
```sql
-- Account Type: Balances with Banks
-- Agent Definition: Bank Account
-- Result: Only bank accounts can be used as agents when using this account type
```

2. **Employee Accounts Example**
```sql
-- Account Type: Employee Salaries Payable
-- Agent Definition: Employee
-- Result: Only employee records can be used as agents when using this account type
```

## Best Practices

1. **Agent Type Consistency**
   - Always ensure the agent definition matches the account type's purpose
   - For example, don't associate a "Bank Account" agent with "Employee Expenses" account type

2. **Multiple Agent Types**
   - An account type can have multiple agent definitions
   - This allows flexibility while maintaining control
   - Example: An account type might allow both "Customer" and "Supplier" agents

3. **System Validation**
   - The system automatically validates that entries match these restrictions
   - Attempts to use incorrect agent types will be rejected
   - This prevents accounting errors and maintains data integrity

## System Features

1. **Relationship Enforcement**
   - Ensures entries follow proper accounting rules
   - Prevents incorrect agent assignments
   - Maintains data consistency across the system

2. **Audit Trail**
   - All changes are tracked
   - History available in AccountTypeAgentDefinitionsHistory
   - Maintains versioning for audit purposes

3. **Cascade Deletion**
   - When an account type is deleted, its agent definitions are automatically removed
   - Prevents orphaned records
   - Maintains database integrity

## Special Notes

1. **Entry Validation**
   - When creating an entry:
     1. The system checks the account type
     2. Looks up allowed agent definitions
     3. Validates that the selected agent matches one of the allowed types

2. **Multiple Agent Types**
   - While an account type can have multiple agent definitions:
     - Each entry must use exactly one agent type
     - The agent must match one of the allowed definitions
     - This maintains clear accounting trails

3. **System Integration**
   - Works in conjunction with AccountTypes and AgentDefinitions tables
   - Forms part of the core accounting validation system
   - Critical for maintaining proper accounting controls
