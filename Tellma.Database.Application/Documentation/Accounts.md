# Accounts Table Documentation

## Overview
The Accounts table represents the actual accounts in the accounting system. Each account is associated with an AccountType, which defines its behavior and restrictions. Accounts are used in the Entries table to record financial transactions.

### Key Concepts
- Account codes (Code field) are purely for reference and reporting
- Business logic should always be based on AccountType, not account codes
- Each tenant can structure their account numbering system as needed
- AccountType determines:
  - Allowed agents and resources
  - Entry type compatibility
  - Field visibility in Entries
  - Validation rules
  - Accounting behavior

## Purpose
The primary purpose of this table is to:
1. Store specific accounts used in entries
2. Define account behavior through AccountType
3. Enforce validation rules
4. Support multi-language, multi-currency, and cost center tracking
5. Enable two entry methods:
   - Manual journal vouchers for accountants
   - Smart screens for business users
6. Control account visibility based on user role and entry method

### Design Philosophy
- Accounts table focuses solely on specific accounts used in entries
- No hierarchy or organization stored in Accounts table
- No balance storage - balances are calculated from Entries
- Organization is managed through:
  - AccountClassifications for accounting hierarchy
  - AccountTypes for behavioral hierarchy
- Benefits:
  - Clear separation of concerns
  - More flexible organization
  - Better data integrity
  - Easier maintenance
  - More efficient queries
  - Cleaner schema design
  - Proper normalization
  - No redundant data
  - Consistent balance calculation
  - Better audit trail

### Key Relationships
- Each account must have exactly one AccountType
- AccountType defines:
  - What information can be stored in the account
  - What information is shown in entries when using the account
  - Which entry types are compatible with the account
  - Which agent types are allowed (via AccountTypeAgentDefinitions)
  - Which noted agent types are allowed (via AccountTypeNotedAgentDefinitions)
  - Which resource types are allowed (via AccountTypeResourceDefinitions)
  - Which noted resource types are allowed (via AccountTypeNotedResourceDefinitions)

## Table Structure
```sql
CREATE TABLE [dbo].[Accounts] (
    [Id] INT CONSTRAINT [PK_Accounts] PRIMARY KEY NONCLUSTERED IDENTITY,
    [AccountTypeId] INT NOT NULL CONSTRAINT [FK_Accounts__AccountTypeId] REFERENCES [dbo].[AccountTypes] ([Id]),
    [CenterId] INT CONSTRAINT [FK_Accounts__CenterId] REFERENCES [dbo].[Centers] ([Id]),
    [Name] NVARCHAR(255) NOT NULL,
    [Name2] NVARCHAR(255),
    [Name3] NVARCHAR(255),
    [Code] NVARCHAR(50),
    [ClassificationId] INT CONSTRAINT [FK_Accounts__ClassificationId] REFERENCES [dbo].[AccountClassifications] ([Id]),
    [AgentDefinitionId] INT CONSTRAINT [FK_Accounts__AgentDefinitionId] REFERENCES [dbo].[AgentDefinitions] ([Id]),
    [AgentId] INT CONSTRAINT [FK_Accounts__AgentId] REFERENCES [dbo].[Agents] ([Id]),
    [ResourceDefinitionId] INT CONSTRAINT [FK_Accounts__ResourceDefinitionId] REFERENCES [dbo].[ResourceDefinitions] ([Id]),
    [ResourceId] INT CONSTRAINT [FK_Accounts__ResourceId] REFERENCES [dbo].[Resources] ([Id]),
    [NotedAgentDefinitionId] INT CONSTRAINT [FK_Accounts__NotedAgentDefinitionId] REFERENCES [dbo].[AgentDefinitions] ([Id]),
    [NotedAgentId] INT CONSTRAINT [FK_Accounts__NotedAgentId] REFERENCES dbo.[Agents] ([Id]),
    [NotedResourceDefinitionId] INT CONSTRAINT [FK_Accounts__NotedResourceDefinitionId] REFERENCES [dbo].[ResourceDefinitions] ([Id]),
    [NotedResourceId] INT CONSTRAINT [FK_Accounts__NotedResourceId] REFERENCES [dbo].[Resources] ([Id]),
    [CurrencyId] NCHAR(3) CONSTRAINT [FK_Accounts__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
    [EntryTypeId] INT CONSTRAINT [FK_Accounts__EntryTypeId] REFERENCES [dbo].[EntryTypes],
    [IsAutoSelected] BIT NOT NULL CONSTRAINT [DF_Accounts__IsAutoSelected] DEFAULT (0),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CreatedById] INT NOT NULL CONSTRAINT [FK_Accounts__CreatedById] REFERENCES [dbo].[Users] ([Id]),
    [ModifiedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [ModifiedById] INT NOT NULL CONSTRAINT [FK_Accounts__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
```

### Key Fields

1. **Id**
   - Primary key of the account
   - Non-clustered index for performance
   - Auto-incrementing identity

2. **AccountTypeId**
   - References AccountTypes table
   - Defines the account's behavior and restrictions
   - Cannot be null
   - Foreign key constraint ensures valid account type

3. **CenterId**
   - References Centers table
   - Used for cost center or department association
   - Can be null if not applicable
   - When not null:
     - The same CenterId is automatically copied to Entries.CenterId
     - Ensures consistent cost center tracking
     - Used for cost center-based reporting and analysis
   - When null:
     - Entries.CenterId must be specified separately
     - Allows flexibility in cost center assignment per entry

4. **Name**
   - Display name of the account
   - Required field
   - Used in reports and UI

5. **Name2**
   - Alternative name (localization support)
   - Optional field
   - Used for additional language support

6. **Name3**
   - Additional alternative name
   - Optional field
   - Used for additional language support

7. **Code**
   - Reference code for the account
   - NVARCHAR(50)
   - Used for:
     - Reference in reports and documents
     - Tenant-specific account numbering
     - External system integration
   - NOT used for:
     - Business logic
     - Validation rules
     - Accounting behavior
   - Clustered index for performance

8. **ClassificationId**
   - References AccountClassifications table
   - Used for flexible account organization
   - Can be null
   - Purpose:
     - Provides a flexible way to categorize accounts
     - Supports different classification systems
     - Examples:
       - OHADA accounting system categories
       - Traditional accounting categories (Assets, Liabilities, etc.)
     - Independent of AccountType
     - Used for:
       - Reporting
       - Analysis
       - User-defined account grouping
     - Allows tenants to:
       - Implement their preferred accounting system
       - Follow local regulations
       - Customize account organization
     - Common classification systems:
       - OHADA system (for African countries)
       - Traditional system (Assets, Liabilities, Equities, Revenues, Expenses)
       - Custom systems based on business needs
     - Note:
       - Classification is purely organizational
       - Does not affect account behavior
       - Works alongside AccountType for different purposes

9. **Agent Fields**
   - **AgentDefinitionId**: Defines allowed agent type
     - Must be one of the agent definitions allowed by the account's AccountType
     - When set:
       - Restricts Entries.AgentId to this specific agent definition
       - Overrides any multiple agent definitions allowed by AccountType
       - Ensures consistent agent type usage
     - When null:
       - Entries.AgentId can use any agent definition allowed by AccountType
       - Provides flexibility in agent selection
   - **AgentId**: References specific agent
     - Optional field
     - When not null:
       - AgentDefinitionId must also be not null
       - Agent must be compatible with the specified AgentDefinitionId
       - The agent is automatically copied to Entries.AgentId
     - When null:
       - Agent selection is determined by Entries.AgentId
       - Follows AccountType's agent definition restrictions

10. **Resource Fields**
    - **ResourceDefinitionId**: Defines allowed resource type
      - Must be one of the resource definitions allowed by the account's AccountType
      - When set:
        - Restricts Entries.ResourceId to this specific resource definition
        - Overrides any multiple resource definitions allowed by AccountType
        - Ensures consistent resource type usage
      - When null:
        - Entries.ResourceId can use any resource definition allowed by AccountType
        - Provides flexibility in resource selection
    - **ResourceId**: References specific resource
      - Optional field
      - When not null:
        - ResourceDefinitionId must also be not null
        - Resource must be compatible with the specified ResourceDefinitionId
        - The resource is automatically copied to Entries.ResourceId
      - When null:
        - Resource selection is determined by Entries.ResourceId
        - Follows AccountType's resource definition restrictions

11. **Noted Agent Fields**
    - **NotedAgentDefinitionId**: Defines allowed noted agent type
      - Must be one of the noted agent definitions allowed by the account's AccountType
      - When set:
        - Restricts Entries.NotedAgentId to this specific noted agent definition
        - Overrides any multiple noted agent definitions allowed by AccountType
        - Ensures consistent noted agent type usage
      - When null:
        - Entries.NotedAgentId can use any noted agent definition allowed by AccountType
        - Provides flexibility in noted agent selection
    - **NotedAgentId**: References specific noted agent
      - Optional field
      - When not null:
        - NotedAgentDefinitionId must also be not null
        - Noted agent must be compatible with the specified NotedAgentDefinitionId
        - The noted agent is automatically copied to Entries.NotedAgentId
      - When null:
        - Noted agent selection is determined by Entries.NotedAgentId
        - Follows AccountType's noted agent definition restrictions

12. **Noted Resource Fields**
    - **NotedResourceDefinitionId**: Defines allowed noted resource type
      - Must be one of the noted resource definitions allowed by the account's AccountType
      - When set:
        - Restricts Entries.NotedResourceId to this specific noted resource definition
        - Overrides any multiple noted resource definitions allowed by AccountType
        - Ensures consistent noted resource type usage
      - When null:
        - Entries.NotedResourceId can use any noted resource definition allowed by AccountType
        - Provides flexibility in noted resource selection
    - **NotedResourceId**: References specific noted resource
      - Optional field
      - When not null:
        - NotedResourceDefinitionId must also be not null
        - Noted resource must be compatible with the specified NotedResourceDefinitionId
        - The noted resource is automatically copied to Entries.NotedResourceId
      - When null:
        - Noted resource selection is determined by Entries.NotedResourceId
        - Follows AccountType's noted resource definition restrictions

13. **CurrencyId**
    - References Currencies table
    - NCHAR(3) for currency code
    - Used for multi-currency support

14. **EntryTypeId**
    - References EntryTypes table
    - Used to restrict entry types
    - Can be null

15. **IsAutoSelected**
    - Flag indicating if the account can be used in smart screens
    - Default value is 0 (false)
    - Purpose:
      - Controls account visibility in different entry methods
      - Defines "smart accounts" that can be used in smart screens
    - Entry Methods:
      1. Manual Journal Voucher:
         - Any account can be selected
         - Requires accounting knowledge
         - Used by accountants
      2. Smart Screens:
         - Only accounts with IsAutoSelected = 1 can be selected
         - User-friendly interface for non-accountants
         - Converts business data to accounting entries
         - Examples:
           - Purchase orders
           - Sales invoices
           - Expense reports
         - Ensures proper accounting without requiring accounting knowledge
    - Best Practices:
      - Set IsAutoSelected = 1 for accounts commonly used in business operations
      - Keep IsAutoSelected = 0 for accounts used only in manual accounting
      - Review and update IsAutoSelected based on business process changes
    - Note:
      - This flag is independent of account type and classification
      - Used for UI/UX purposes only
      - Does not affect account behavior or restrictions

16. **IsActive**
    - Account status flag
    - Default value is 1 (true)
    - Used to deactivate accounts

17. **Audit Fields**
    - **CreatedAt**: Creation timestamp with timezone
    - **CreatedById**: User who created the account
    - **ModifiedAt**: Last modification timestamp with timezone
    - **ModifiedById**: User who last modified the account

### Indexes
1. **Clustered Index**: IX_Accounts__Code
2. **Non-clustered Index**: IX_Accounts__AccountTypeId
3. **Non-clustered Index**: IX_Accounts__AccountClassificationId
4. **Non-clustered Index**: IX_Accounts__AgentId
5. **Composite Index**: IX_Accounts__ResourceDefinitionId_ResourceId

## Usage Examples

1. **Basic Account Structure**
```sql
-- Account Type: Current Value Added Tax Receivables
-- Account:
-- Code: 1010
-- Name: VAT Receivables - Production Supplies
-- AccountTypeId: 100 (VAT Receivables)
-- Note: 
-- Balance is calculated from Entries table, not stored in Accounts
-- Hierarchy is managed through AccountTypes and AccountClassifications
```

### Organization Notes
- Accounts table contains only specific accounts used in entries
- AccountClassifications manages accounting hierarchy
- AccountTypes manages behavioral hierarchy
- Example:
  - AccountType: VAT Receivables
  - Classification: Current Assets
  - Account: VAT Receivables - Production Supplies
- The relationship is:
  - Classification (10) -> Account (1010)
  - Classification provides the organizational context
  - Account is the actual account used in entries
- Note: The classification code (10) is often reflected in the account code (1010) for reference purposes only

### Design Benefits
- Clean separation of concerns:
  - Accounts: Specific accounts used in entries
  - AccountClassifications: Accounting hierarchy
  - AccountTypes: Behavioral rules
- More flexible organization:
  - Multiple classification systems possible
  - Independent of account behavior
  - Easier to modify organization
- Better data integrity:
  - No circular references
  - Clear relationships
  - Simpler validation
  - No redundant balance data
  - Consistent calculations
- More efficient queries:
  - Flat table structure
  - Clear joins
  - Better performance
  - Single source of truth for balances

### Balance Calculation
- Balances are never stored in Accounts table
- Calculated from Entries table
- Benefits:
  - Single source of truth
  - Always up-to-date
  - Consistent calculations
  - Better audit trail
  - No data inconsistencies
  - Proper normalization
- Example query:
```sql
SELECT 
    a.Id, a.Name,
    SUM(e.Debit - e.Credit) as Balance
FROM Accounts a
JOIN Entries e ON a.Id = e.AccountId
GROUP BY a.Id, a.Name
```

### Hierarchy Notes
- Accounts table is flat (no ParentId)
- Hierarchy is managed through:
  - AccountTypes (behavioral hierarchy)
  - AccountClassifications (organizational hierarchy)
- Example:
  - AccountType: VAT Receivables
  - Classification: Current Assets
  - Both tables together define the account's position in the accounting system

## Best Practices

### Chart of Accounts Design
Tellma supports two approaches to designing the chart of accounts:

1. **Summary Approach (Recommended)**
```sql
-- Account Type: Balances with Banks
-- Account:
-- Code: 1000
-- Name: Balances with Banks
-- AgentDefinitionId: NULL
-- AgentId: NULL
```
- Pros:
  - Simpler chart of accounts
  - Less maintenance required
  - Easier to add new bank accounts
  - Trial balance shows consolidated bank balances
  - Bank details are stored in Entries
- Example:
  - When adding a new bank account:
    - Add new agent (e.g., "Citibank")
    - No need to modify chart of accounts
    - Bank details are specified in Entries

2. **Detailed Approach**
```sql
-- Account Type: Balances with Banks
-- Account:
-- Code: 1001
-- Name: Citibank IBAN 1001 - Current
-- AgentDefinitionId: (Bank Account Agent Definition)
-- AgentId: (Citibank IBAN 1001 Agent)

-- Account:
-- Code: 1002
-- Name: HSBC IBAN 2002 - Savings
-- AgentDefinitionId: (Bank Account Agent Definition)
-- AgentId: (HSBC IBAN 2002 Agent)
```

### Agent Hierarchy
- Bank Account is an agent
- Bank is also an agent
- Relationship:
  - Bank Account (agent) is grouped under Bank (agent) using Agent1
  - Example:
    - Agent: Citibank IBAN 1001 (bank account)
    - Agent1: Citibank (bank)
    - Agent: Citibank IBAN 1002 (bank account)
    - Agent1: Citibank (bank)
- This allows:
  - Separate tracking of individual bank accounts
  - Grouping under the parent bank
  - Clear hierarchy in reports
  - Proper consolidation at bank level
  - Example hierarchy:
    - Citibank (bank)
      - Citibank IBAN 1001 (bank account)
      - Citibank IBAN 1002 (bank account)
    - HSBC (bank)
      - HSBC IBAN 2001 (bank account)
      - HSBC IBAN 2002 (bank account)
- Pros:
  - More detailed trial balance
  - Clearer account structure
  - Better visibility in general ledger
- Cons:
  - More maintenance required
  - More complex chart of accounts
  - More work when adding new bank accounts
  - Duplicate information with Entries

### Best Practice Recommendation
- Use the Summary Approach unless detailed trial balance is required
- The Summary Approach:
  - Reduces maintenance overhead
  - Simplifies account management
  - Follows normalization principles
  - Provides flexibility in reporting
  - Reduces data redundancy

1. **Account Code Structure**
   - Use a consistent numbering scheme (though it's purely for reference)
   - Consider using classification codes in the account code
   - Remember that account codes are not used for business logic
   - Make codes meaningful and hierarchical
   - Reserve specific ranges for different account types

2. **Account Organization**
   - Keep Accounts table flat and focused on specific accounts
   - Use AccountClassifications for accounting hierarchy
   - Use AccountTypes for behavioral hierarchy
   - Don't store organizational information in Accounts table

3. **Account Naming**
   - Use clear, descriptive names
   - Include purpose in the name
   - Follow company naming conventions

4. **System Accounts**
   - Mark critical accounts as IsSystem
   - Only modify system accounts if absolutely necessary
   - Document any changes to system accounts

2. **Account Naming**
   - Use clear, descriptive names
   - Include purpose in the name
   - Follow company naming conventions

2. **Agent and Resource Restrictions**
   - Agent and Resource restrictions are defined in AccountType
   - Accounts table only stores specific agent/resource instances
   - Restrictions are enforced through AccountType definitions

3. **Noted Fields**
   - Noted fields (NotedAgent, NotedResource) are secondary associations
   - Used for additional tracking and reporting
   - Not required for basic accounting operations

4. **Smart Accounts**
   - Accounts with IsAutoSelected = 1 are smart accounts
   - Used in smart screens for business operations
   - Not used in manual journal vouchers

## System Features

1. **Audit Trail**
   - All changes are tracked
   - History available in AccountsHistory
   - Maintains versioning for audit purposes

2. **Account Type Deletion**
   - When an AccountType is deleted:
     - Accounts using that type are not automatically removed
     - Must be handled through application logic
     - Prevents accidental deletion of accounts
   - Best Practices:
     - Always check for dependent accounts before deleting AccountType
     - Use application logic to handle account type changes
     - Consider soft deletion of AccountTypes if needed
