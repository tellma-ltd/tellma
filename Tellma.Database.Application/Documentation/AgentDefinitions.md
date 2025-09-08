# AgentDefinitions Table Documentation

## Overview
The AgentDefinitions table defines various types of agents in the Tellma system. An agent is any entity for which we may generate a statement of account. Common examples include:
- Employees
- Bank accounts
- Tax departments
- Social security departments
- Supplier accounts
- Customer accounts
- Purchase invoices
- Sales invoices
- Production orders
- Warehouses

## Key Concepts

### Agent Types and Their Usage
1. **Primary Agents** (stored in Entries.AgentId)
   - Main entities for which statements are generated
   - Examples: Employees, Bank accounts, Tax departments, Social security departments, Supplier accounts, Customer accounts

2. **Secondary Agents** (stored in Entries.NotedAgentId)
   - Related entities that provide context to the primary agent
   - Examples:
     - For VAT entries: Purchase invoices or sales invoices
     - For Social security tax: Employees

## Table Structure

### Basic Information
```sql
CREATE TABLE [dbo].[AgentDefinitions] (
    [Id] INT CONSTRAINT [PK_AgentDefinitions] PRIMARY KEY IDENTITY,
    [Code] NVARCHAR(255) NOT NULL CONSTRAINT [UQ_AgentDefinitions__Code] UNIQUE,
    [TitleSingular] NVARCHAR(255),
    [TitleSingular2] NVARCHAR(255),
    [TitleSingular3] NVARCHAR(255),
    [TitlePlural] NVARCHAR(255) NOT NULL,
    [TitlePlural2] NVARCHAR(255),
    [TitlePlural3] NVARCHAR(255)
)
```

### Visibility Settings
```sql
[IdentifierVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_RAgentDefinitions__IdentifierVisibility] 
    CHECK ([IdentifierVisibility] IN (N'None', N'Optional', N'Required')),
[CurrencyVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[CenterVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[ImageVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[DescriptionVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[LocationVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[FromDateVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[ToDateVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[DateOfBirthVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[ContactEmailVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[ContactMobileVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[ContactAddressVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None'
```

### Custom Date Fields
```sql
[Date1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_AgentDefinitions__Date1Visibility] 
    CHECK ([Date1Visibility] IN (N'None', N'Optional', N'Required')),
[Date1Label] NVARCHAR(50),
[Date1Label2] NVARCHAR(50),
[Date1Label3] NVARCHAR(50)
```

### Agent Relationships
```sql
[Agent1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_AgentDefinitions__Agent1Visibility] 
    CHECK ([Agent1Visibility] IN (N'None', N'Required', N'Optional')),
[Agent1DefinitionId] INT CONSTRAINT [FK_AgentDefinitions__Agent1DefinitionId] 
    REFERENCES dbo.[AgentDefinitions]([Id]),
[Agent1Label] NVARCHAR(50),
[Agent1Label2] NVARCHAR(50),
[Agent1Label3] NVARCHAR(50)

[Agent2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_AgentDefinitions__Agent2Visibility] 
    CHECK ([Agent2Visibility] IN (N'None', N'Required', N'Optional')),
[Agent2DefinitionId] INT CONSTRAINT [FK_AgentDefinitions__Agent2DefinitionId] 
    REFERENCES dbo.[AgentDefinitions]([Id]),
[Agent2Label] NVARCHAR(50),
[Agent2Label2] NVARCHAR(50),
[Agent2Label3] NVARCHAR(50)
```

### Scripts and Attachments
```sql
[PreprocessScript] NVARCHAR(MAX),
[ValidateScript] NVARCHAR(MAX),
[HasAttachments] BIT NOT NULL DEFAULT 0,
[AttachmentsCategoryDefinitionId] INT 
    CONSTRAINT [FK_AgentDefinitions__AttachmentsCategoryDefinitionId] 
    REFERENCES dbo.LookupDefinitions([Id])
```

## Detailed Field Usage

### Identifier
- Used when the Agent has a well-defined reference in the real world
- Examples:
  - Employee: National ID or Residence ID
  - Bank Account: IBAN number
- Allows searching by:
  - Agent code
  - Name
  - Identifier

### Currency
- Used when the Agent has only one currency
- Examples:
  - Supplier Account
  - Customer Account
  - Purchase Invoice
  - Sales Invoice
- When set to Required:
  - All entries (in primary context) will have the same currency
  - Simplifies accounting transactions

### Center
- Used to specify a specific center for all transactions
- When set:
  - All entries (in primary context) will use the same center
  - Enforces consistent transaction centering

### Date Fields
- **FromDate and ToDate**:
  - Typically indicate the agent's lifetime
  - Examples:
    - Employee: Joining date and termination date
    - Warehouse: Start and end of usage period

- **Custom Date Fields (Date1-4)**:
  - Used for significant dates requiring quick reporting
  - Examples:
    - Employee:
      - Date1: End of probation period
      - Date2: Expiry date for ID/residence card
      - Date3: Not used
      - Date4: Expected termination date
    - ToDate: Stores actual termination date

### Scripts
- **PreprocessScript**:
  - Automatically fills fields based on other values
  - Example: Auto-generate Code from other fields

- **ValidateScript**:
  - Validates data during entry
  - Example: Employee age validation against legal limits

### Agent Relationships
- **Agent1**:
  - Groups agents under a higher category
  - Examples:
    - Employee → Supervisor (Agent1Definition is Employee)
    - Purchase Invoice → Supplier Account
    - Supplier Account → Supplier
    - Supplier → Supplier Group

- **Agent2**:
  - Additional grouping level
  - Used as needed for specific hierarchies

### User Access
- **UserCardinality**:
  - Controls login access
  - Options:
    - None: No login access
    - Single: One login per agent
    - Multiple: Multiple logins per agent
  - Example: Supplier with multiple department logins

- **Attachments**:
  - **HasAttachments**: Enables attachment functionality
  - **AttachmentsCategoryDefinitionId**: Categorizes attachments

## Best Practices

1. **Field Usage**:
   - Follow standard patterns for maintainability
   - Use date fields only for critical reporting needs
   - Store non-critical information in subsidiary tables

2. **Agent Grouping**:
   - Use Agent1 for primary hierarchical relationships
   - Use Agent2 for secondary grouping needs
   - Maintain consistent grouping patterns

3. **Currency and Center**:
   - Use Currency for single-currency agents
   - Use Center for single responsibility center transactions
   - Ensure consistent usage across similar agent types

4. **Date Fields**:
   - Use Date fields for significant events
   - Plan reporting needs before assigning dates
   - Keep Date fields flexible for future requirements

5. **Scripts**:
   - Use PreprocessScript for automation
   - Use ValidateScript for business rules
   - Document script logic for maintainability

6. **Multi-language Support**:
   - Use language fields (2, 3) for localization
   - Maintain consistent translations
   - Consider regional requirements

7. **Attachments**:
   - Enable only when necessary
   - Categorize attachments appropriately
   - Consider storage implications

## Special Notes

1. **Search Functionality**:
   - Agents can be searched by:
     - Code
     - Name
     - Identifier
   - Design identifiers for easy searching

2. **Maintenance**:
   - Following standard patterns simplifies maintenance
   - Document non-standard usage
   - Keep field usage consistent across similar agents

3. **System Versioning**:
   - All changes are tracked
   - History available in AgentDefinitionsHistory
   - Maintain versioning for audit purposes

4. **Multi-language Support**:
   - All title fields support multiple languages
   - Labels for custom fields also support multiple languages
   - Consider regional requirements when setting up agents

5. **Visibility States**:
   - None: Field hidden from UI
   - Optional: Field visible but not required
   - Required: Field visible and must be filled

6. **State Management**:
   - Hidden: Not visible in UI
   - Visible: Normal state
   - Archived: Historical records
   - Testing: Development/test state

7. **MainMenu Fields**:
   - Control menu display
   - Icon: Visual representation
   - Section: Menu organization
   - SortKey: Menu ordering