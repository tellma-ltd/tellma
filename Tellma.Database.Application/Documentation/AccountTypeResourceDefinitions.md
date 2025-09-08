# AccountTypeResourceDefinitions Table Documentation

## Overview
The AccountTypeResourceDefinitions table defines the valid resource types that can be associated with specific account types. This table is optional - an account type can have 0 or more resource definitions.

When defined, it ensures that only appropriate resources can be used when creating entries with specific account types.

## Purpose
The primary purpose of this table is to enforce resource type restrictions when using specific accounts in table Entries. For example:
- If an account type is "Current Raw Materials"
- And it is associated with "Raw Materials" resource definition
- Then any entry using this account type must use a raw materials resource

### Optional Nature
- An account type can have 0 resource definitions
- If no definitions are specified, any resource can be used
- This provides flexibility for different accounting needs

### How It Works
1. Entries table uses Accounts (not AccountTypes directly)
2. Each Account has an AccountType
3. AccountTypeResourceDefinitions defines valid resource types for each AccountType
4. When creating an entry:
   - The system checks the Account's AccountType
   - Looks up allowed resource definitions from AccountTypeResourceDefinitions
   - Validates that the selected resource matches one of the allowed types

## Table Structure
```sql
CREATE TABLE [dbo].[AccountTypeResourceDefinitions] (
    [Id] INT CONSTRAINT [PK_AccountTypeResourceDefinitions] PRIMARY KEY IDENTITY,
    [AccountTypeId] INT NOT NULL CONSTRAINT [FK_AccountTypeResourceDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
    [ResourceDefinitionId] INT NOT NULL CONSTRAINT [FK_AccountTypeResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.[ResourceDefinitions]([Id])
)
```

### Key Fields

1. **AccountTypeId**
   - References AccountTypes table
   - Identifies which account type this restriction applies to
   - ON DELETE CASCADE ensures cleanup if account type is deleted

2. **ResourceDefinitionId**
   - References ResourceDefinitions table
   - Defines which resource type is allowed for this account type
   - Must match the resource type used in Entries.ResourceId

## Usage Examples

1. **Inventory Example**
```sql
-- Account Type: Inventory Accounts
-- Resource Definition: Raw Materials
-- Result: Only raw materials resources can be used when using this account type
```

2. **Fixed Assets Example**
```sql
-- Account Type: Fixed Assets
-- Resource Definition: Office Equipment
-- Result: Only office equipment resources can be used when using this account type
```

## Best Practices

1. **Resource Type Consistency**
   - Always ensure the resource definition matches the account type's purpose
   - For example, don't associate "Raw Materials" resource with "Fixed Assets" account type

2. **Multiple Resource Types**
   - An account type can have multiple resource definitions
   - This allows flexibility while maintaining control
   - Example: An account type might allow both "Raw Materials" and "Finished Goods" resources

3. **System Validation**
   - The system automatically validates that entries match these restrictions
   - Attempts to use incorrect resource types will be rejected
   - This prevents accounting errors and maintains data integrity

## System Features

1. **Relationship Enforcement**
   - Ensures entries follow proper accounting rules
   - Prevents incorrect resource assignments
   - Maintains data consistency across the system

2. **Audit Trail**
   - All changes are tracked
   - History available in AccountTypeResourceDefinitionsHistory
   - Maintains versioning for audit purposes

3. **Cascade Deletion**
   - When an account type is deleted, its resource definitions are automatically removed
   - Prevents orphaned records
   - Maintains database integrity

## Special Notes

1. **Entry Validation**
   - When creating an entry:
     1. The system checks the account type
     2. Looks up allowed resource definitions
     3. Validates that the selected resource matches one of the allowed types

2. **Multiple Resource Types**
   - While an account type can have multiple resource definitions:
     - Each entry must use exactly one resource type
     - The resource must match one of the allowed definitions
     - This maintains clear accounting trails

3. **System Integration**
   - Works in conjunction with AccountTypes and ResourceDefinitions tables
   - Forms part of the core accounting validation system
   - Critical for maintaining proper accounting controls
