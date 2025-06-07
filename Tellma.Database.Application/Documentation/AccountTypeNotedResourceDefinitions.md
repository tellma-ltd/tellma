# AccountTypeNotedResourceDefinitions Table Documentation

## Overview
The AccountTypeNotedResourceDefinitions table defines the valid resource types that can be used as noted resources when using specific account types. This table is optional - an account type can have 0 or more noted resource definitions.

When defined, it ensures that only appropriate resources can be used as noted resources in entries with specific account types.

## Purpose
The primary purpose of this table is to enforce noted resource type restrictions when using specific accounts in table Entries. For example:
- If an account type is "Current value added tax receivables"
- And it is associated with "Production Supplies" as one of the noted resource definitions
- Then any entry using this account type must use one of the defined noted resources

### Optional Nature
- An account type can have 0 noted resource definitions
- If no definitions are specified, any resource can be used as noted resource
- This provides flexibility for different accounting needs

### How It Works
1. Entries table uses Accounts (not AccountTypes directly)
2. Each Account has an AccountType
3. AccountTypeNotedResourceDefinitions defines valid noted resource types for each AccountType
4. When creating an entry:
   - The system checks the Account's AccountType
   - Looks up allowed noted resource definitions from AccountTypeNotedResourceDefinitions
   - Validates that the selected noted resource matches one of the allowed types

## Table Structure
```sql
CREATE TABLE [dbo].[AccountTypeNotedResourceDefinitions] (
    [Id] INT CONSTRAINT [PK_AccountTypeNotedResourceDefinitions] PRIMARY KEY IDENTITY,
    [AccountTypeId] INT NOT NULL CONSTRAINT [FK_AccountTypeNotedResourceDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
    [ResourceDefinitionId] INT NOT NULL CONSTRAINT [FK_AccountTypeNotedResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.[ResourceDefinitions]([Id])
)
```

### Key Fields

1. **AccountTypeId**
   - References AccountTypes table
   - Identifies which account type this restriction applies to
   - ON DELETE CASCADE ensures cleanup if account type is deleted

2. **ResourceDefinitionId**
   - References ResourceDefinitions table
   - Defines which resource type is allowed as notes resource for this account type
   - Must match the resource type used in Entries.NotesResourceId

## Usage Examples

1. **Current Value Added Tax Receivables Example**
```sql
-- Account Type: Current Value Added Tax Receivables
-- Noted Resource Definitions:
-- 1. Production Supplies
-- 2. Merchandise
-- 3. Computer Equipment
-- 4. Non-current Non-financial Assets
-- Result: When recording VAT receivables, the noted resource must be one of these types
-- Example: VAT receivable for computer equipment purchase
```

2. **Fuel Expenses Example**
```sql
-- Account Type: Fuel Expenses
-- Noted Resource Definition: Machinery or Motor Vehicles
-- Result: When recording fuel expenses, the noted resource must be the fixed asset that used the fuel
-- Example: Fuel expense for Machine Mixer 1 or Company Vehicle
```

2. **Project Costs Example**
```sql
-- Account Type: Project Costs
-- Notes Resource Definition: Project Task
-- Result: When using this account type, the notes resource must be a project task
```

## Best Practices

1. **Notes Resource Type Consistency**
   - Always ensure the notes resource definition matches the account type's purpose
   - For example, don't associate "Raw Materials" notes resource with "Project Costs" account type

2. **Multiple Notes Resource Types**
   - An account type can have multiple notes resource definitions
   - This allows flexibility while maintaining control
   - Example: An account type might allow both "Project Task" and "Project Milestone" notes resources

3. **System Validation**
   - The system automatically validates that entries match these restrictions
   - Attempts to use incorrect notes resource types will be rejected
   - This prevents accounting errors and maintains data integrity

## System Features

1. **Relationship Enforcement**
   - Ensures entries follow proper accounting rules
   - Prevents incorrect notes resource assignments
   - Maintains data consistency across the system

2. **Audit Trail**
   - All changes are tracked
   - History available in AccountTypeNotedResourceDefinitionsHistory
   - Maintains versioning for audit purposes

3. **Cascade Deletion**
   - When an account type is deleted, its notes resource definitions are automatically removed
   - Prevents orphaned records
   - Maintains database integrity

## Special Notes

1. **Entry Validation**
   - When creating an entry:
     1. The system checks the account type
     2. Looks up allowed notes resource definitions
     3. Validates that the selected notes resource matches one of the allowed types

2. **Multiple Notes Resource Types**
   - While an account type can have multiple notes resource definitions:
     - Each entry must use exactly one notes resource type
     - The notes resource must match one of the allowed definitions
     - This maintains clear accounting trails

3. **System Integration**
   - Works in conjunction with AccountTypes and ResourceDefinitions tables
   - Forms part of the core accounting validation system
   - Critical for maintaining proper accounting controls
