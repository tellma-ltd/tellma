# Agents

The `Agents` table is a fundamental table that represents any entity for which the system needs to track accountability. While it can represent natural and legal persons, its usage extends far beyond that to model various business concepts that require tracking of relationships, responsibilities, or financial transactions.

## Core Concept: What is an Agent?

An Agent in the system represents any entity that can be held accountable or be a party in business transactions. This flexible concept allows the system to model a wide variety of business scenarios:

### Common Agent Types:
- **People & Organizations**
  - Employees
  - Customers (and their specific accounts/projects)
  - Suppliers (and their specific accounts/projects)
  - Creditors and Debtors
  - Shareholders
  - Related Parties

- **Business Entities**
  - Sales Invoices (to track debits/credits)
  - Purchase Invoices (to track payables)
  - Customer Accounts (per project/relationship)
  - Supplier Accounts (per project/relationship)

- **Physical & Logical Locations**
  - Cash Safes (referring to the custodian's accountability)
  - Warehouses (referring to the custodian's responsibility)
  - Duty Stations
  - Departments or Cost Centers

### Key Characteristics:
- Each agent has a `DefinitionId` that determines its type and behavior
- Agents can participate in financial transactions, document workflows, and other business processes
- The same conceptual entity (like a customer) might be represented by multiple agents for different purposes or time periods
- The system maintains a complete audit trail of all changes to agent records

## Table Structure
```sql
CREATE TABLE [dbo].[Agents] (
    [Id]                        INT                 PRIMARY KEY IDENTITY,
    [DefinitionId]              INT                 NOT NULL,
    [Name]                      NVARCHAR(255)       NOT NULL,
    [Name2]                     NVARCHAR(255),
    [Name3]                     NVARCHAR(255),
    [Code]                      NVARCHAR(50),
    [Identifier]                NVARCHAR(50),
    [CurrencyId]                NCHAR(3),
    [CenterId]                  INT,
    [ImageId]                   NVARCHAR(50),
    [Description]               NVARCHAR(2048),
    [Description2]              NVARCHAR(2048),
    [Description3]              NVARCHAR(2048),
    [Location]                  GEOGRAPHY,
    [LocationJson]              NVARCHAR(MAX),
    [FromDate]                  DATE,           -- Joining Date
    [ToDate]                    DATE,           -- Termination Date
    [DateOfBirth]               DATE,
    [ContactEmail]              NVARCHAR(255),
    [ContactMobile]             NVARCHAR(50),
    [NormalizedContactMobile]   NVARCHAR(50),
    [ContactAddress]            NVARCHAR(255),
    -- Custom date fields
    -- The meaning of these fields is defined in the AgentDefinitions table
    -- Common examples include Visa expiry, Passport expiry, Document dates, etc.
    [Date1]                     DATE,
    [Date2]                     DATE,
    [Date3]                     DATE,
    [Date4]                     DATE,
    -- Custom numeric fields
    -- The meaning of these fields is defined in the AgentDefinitions table
    [Decimal1]                  DECIMAL(19,4),
    [Decimal2]                  DECIMAL(19,4),
    [Int1]                      INT,
    [Int2]                      INT,
    -- Lookup references
    -- The meaning of these fields is defined in the AgentDefinitions table
    -- Common examples include Citizenship, Religion, Marital Status, etc.
    [Lookup1Id]                 INT,
    [Lookup2Id]                 INT,
    [Lookup3Id]                 INT,
    [Lookup4Id]                 INT,
    [Lookup5Id]                 INT,
    [Lookup6Id]                 INT,
    [Lookup7Id]                 INT,
    [Lookup8Id]                 INT,
    -- Custom text fields
    -- The meaning of these fields is defined in the AgentDefinitions table
    [Text1]                     NVARCHAR(255),
    [Text2]                     NVARCHAR(255),
    [Text3]                     NVARCHAR(255),
    [Text4]                     NVARCHAR(255),
    -- Audit fields
    [CreatedAt]                 DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CreatedById]               INT             NOT NULL,
    [ModifiedAt]                DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [ModifiedById]              INT             NOT NULL,
    -- Constraints
    CONSTRAINT [PK_Agents] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Agents__Id_DefinitionId] UNIQUE ([Id], [DefinitionId]),
    CONSTRAINT [FK_Agents__DefinitionId] FOREIGN KEY ([DefinitionId]) REFERENCES dbo.[AgentDefinitions]([Id]),
    CONSTRAINT [FK_Agents__CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [dbo].[Currencies]([Id]),
    CONSTRAINT [FK_Agents__CenterId] FOREIGN KEY ([CenterId]) REFERENCES [dbo].[Centers]([Id]),
    -- Lookup constraints
    CONSTRAINT [FK_Agents__Lookup1Id] FOREIGN KEY ([Lookup1Id]) REFERENCES [dbo].[Lookups] ([Id]),
    CONSTRAINT [FK_Agents__Lookup2Id] FOREIGN KEY ([Lookup2Id]) REFERENCES [dbo].[Lookups] ([Id]),
    CONSTRAINT [FK_Agents__Lookup3Id] FOREIGN KEY ([Lookup3Id]) REFERENCES [dbo].[Lookups] ([Id]),
    CONSTRAINT [FK_Agents__Lookup4Id] FOREIGN KEY ([Lookup4Id]) REFERENCES [dbo].[Lookups] ([Id]),
    CONSTRAINT [FK_Agents__Lookup5Id] FOREIGN KEY ([Lookup5Id]) REFERENCES [dbo].[Lookups] ([Id]),
    CONSTRAINT [FK_Agents__Lookup6Id] FOREIGN KEY ([Lookup6Id]) REFERENCES [dbo].[Lookups] ([Id]),
    CONSTRAINT [FK_Agents__Lookup7Id] FOREIGN KEY ([Lookup7Id]) REFERENCES [dbo].[Lookups] ([Id]),
    CONSTRAINT [FK_Agents__Lookup8Id] FOREIGN KEY ([Lookup8Id]) REFERENCES [dbo].[Lookups] ([Id]),
    -- Audit constraints
    CONSTRAINT [FK_Agents__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Agents__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
)
```

## Key Fields

### Identification
- **Id**: Unique identifier for each agent
- **DefinitionId**: References the agent definition that determines the agent's type and properties. This controls:
  - What type of entity this agent represents (e.g., Employee, Customer, Invoice)
  - Which fields are relevant and required
  - What business rules apply to this agent type
- **CurrencyId**: (Optional) Default currency for this agent. When set, any new entries in the `Entries` table that reference this agent will use this currency. This value cannot be overridden at the entry level, ensuring consistent currency usage for all transactions related to this agent.
- **Name/Name2/Name3**: Multi-lingual name fields for the agent. The interpretation depends on the agent type:
  - For people: Personal name
  - For organizations: Company name
  - For documents: Document number or reference
  - For locations: Location name
- **Code**: Business identifier for the agent (e.g., employee number, customer account number, invoice number)
- **Identifier**: A unique external identifier for the agent. This is typically used for integration with external systems and must be unique across all agents. Examples include:
  - National ID or passport number for individuals
  - Tax identification number for businesses
  - Registration number for legal entities
  - External system reference ID

### Contact Information
- **ContactEmail**: Primary email address
- **ContactMobile**: Mobile phone number
- **NormalizedContactMobile**: Standardized format of the mobile number
- **ContactAddress**: Physical address

### Dates
- **FromDate**: Date when the agent became active (e.g., hiring date, start date)
- **ToDate**: Date when the agent became inactive (e.g., termination date)
- **DateOfBirth**: Agent's date of birth
- **Date1-Date4**: Generic date fields whose meaning is defined in the `AgentDefinitions` table. These can represent any date relevant to the agent type, such as:
  - Document expiry dates (Visa, Passport, etc.)
  - Membership dates
  - Contract dates
  - Custom event dates
  The specific meaning for each field is configured per agent definition.

### Location
- **Location**: Geographical coordinates (GEOGRAPHY data type) used primarily for static agents like warehouses, branches, or offices. This enables mapping and distance calculations in features like:
  - Google Maps integration
  - Route planning
  - Location-based services
  - Proximity searches
- **LocationJson**: Structured JSON representation of the location data, which may include:
  - Formatted address components
  - Additional location metadata
  - Custom place information

### Custom Numeric Fields
- **Decimal1/Decimal2**: Generic decimal fields whose meaning is defined in the `AgentDefinitions` table. These can represent any decimal value relevant to the agent type, such as:
  - Financial amounts
  - Measurements
  - Percentages
  - Custom calculations
  
- **Int1/Int2**: Generic integer fields whose meaning is defined in the `AgentDefinitions` table. These can represent any whole number value relevant to the agent type, such as:
  - Quantities
  - Ratings
  - Status codes
  - Custom enumerations
  
  The specific meaning and validation rules for each numeric field are configured per agent definition.

### Lookup References
- **Lookup1Id-Lookup8Id**: Generic lookup fields whose meaning is defined in the `AgentDefinitions` table. These can represent any categorized attribute relevant to the agent type, such as:
  - Personal attributes (Citizenship, Religion, Marital Status, Gender)
  - Professional information (Profession, Educational Status, Job Title)
  - Financial details (Salary Bank, Payment Terms)
  - Custom categorizations
  
  The specific meaning and allowed values for each lookup field are configured per agent definition.

### Custom Text Fields
- **Text1-Text4**: Generic text fields whose meaning is defined in the `AgentDefinitions` table. These can store any text information relevant to the agent type, such as:
  - Address information
  - Descriptions
  - Notes
  - Custom identifiers
  
  The specific purpose, format, and validation rules for each text field are configured per agent definition.

### Audit Fields
- **CreatedAt**: Timestamp when the record was created
- **CreatedById**: User who created the record
- **ModifiedAt**: Timestamp when the record was last modified
- **ModifiedById**: User who last modified the record

## User Representation
Each agent is represented by a user account in the system. This user account is what Tellma recognizes as the agent's identity for authentication and authorization purposes.

### Relationships
- **AgentDefinition**: Many-to-one relationship with `AgentDefinitions` table (on `DefinitionId`)
- **Currency**: Many-to-one relationship with `Currencies` table (on `CurrencyId`)
- **Center**: Many-to-one relationship with `Centers` table (on `CenterId`)
- **Lookups1-8**: Many-to-one relationships with `Lookups` table (on `Lookup1Id`-`Lookup8Id`)
- **CreatedBy/ModifiedBy**: Many-to-one relationships with `Users` table
- **AgentUsers**: One-to-many relationship with `AgentUsers` table, linking to users who can represent this agent

## Currency Behavior
- The `CurrencyId` field is optional for all agent types
- When set on an agent, it serves as the fixed currency for all financial transactions involving that agent
- This currency is automatically applied to all new entries in the `Entries` table where the agent is referenced
- The currency cannot be overridden at the entry level - it is strictly determined by the agent's `CurrencyId`
- When not set, each entry must explicitly specify its currency

## Standardized Field Usages

While the meaning of generic fields is configurable per agent definition, the following standard usages are recommended for common agent types to maintain consistency across implementations:

### Employee Agents

#### Standard Dates
- **FromDate**: Actual joining date
- **ToDate**: Actual termination date (when the employee left)
- **Date1**: End of probation period
- **Date2**: Expiry date for ID/Residence
- **Date4**: Expected termination date (based on contract)

#### Standard Lookups
- **Lookup1Id**: Gender
- **Lookup2Id**: Citizenship
- **Lookup3Id**: Religion
- **Lookup4Id**: Marital Status
- **Lookup5Id**: Contract Marital Status (if different from current)
- **Lookup6Id**: Bank (for salary transfer)
- **Lookup7Id**: Official Job (as per visa)
- **Lookup8Id**: (Available for custom use)

#### Standard Numeric Fields
- **Decimal2**: Remaining leave balance (as of end of current year)
- **Int1**: Fingerprint ID in the fingerprint device

#### Standard Text Fields
- **Text3**: Expected arrival time (e.g., "09:00")
- **Text4**: Expected departure time (e.g., "16:00")

### Notes on Standardization
- These are recommended conventions but can be adapted based on specific implementation needs
- When customizing field usages, document the changes in the `AgentDefinitions` table
- Maintain consistency within each implementation to simplify support and maintenance

## Usage Notes
- The table is designed to be flexible and can represent various types of agents through the `DefinitionId`
- The `Name`, `Name2`, and `Name3` fields support multi-lingual display
- Custom fields (Text1-4, Date1-4, etc.) can be used to extend the agent model without schema changes
- The `FromDate` and `ToDate` fields are used to track the active period of the agent
- The table includes comprehensive audit fields for tracking changes
- Location data can be stored both as GEOGRAPHY for spatial queries and as JSON for flexible representation
