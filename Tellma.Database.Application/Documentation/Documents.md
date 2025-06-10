# Documents Table Documentation

## Overview
The `Documents` table represents business documents in the system, each defined by a document definition that specifies its structure and behavior. Documents are composed of multiple line definitions, each defining a grid of lines with specific columns and entry mappings.

## Document Structure
1. **Document Definition**
   - Defines the overall document structure
   - Consists of multiple line definitions
   - Each line definition defines a grid of lines

2. **Line Definitions**
   - Each line definition defines a separate grid
   - Lines of the same definition are grouped together
   - Each grid has its own set of columns
   - Entries are mapped differently based on LineDefinitionEntries

3. **Tab-Based Layout**
   - Document is organized into multiple tabs
   - Each tab represents a grid of lines
   - Tabs correspond to different line definitions
   - Each tab has its own column structure

## Key Characteristics

1. **Document States**
   - 1: Closed
   - 0: Open
   - -1: Canceled
   - State changes are tracked in DocumentStatesHistory

2. **Clearance Levels**
   - 0: Public
   - 1: Secret
   - 2: Top secret

   - Note: This field alone does not impose security access control
   - Can be used with roles and permissions to control access
   - Example: 
     - Role A: Access to Public documents only
     - Role B: Access to Secret documents
     - Role C: Access to Top Secret documents

3. **Common vs Line-Specific Properties**
   - Most properties have an IsCommon flag
   - Common properties are copied to all lines
   - Line-specific properties can vary per line

## Purpose
The Documents table serves as the central repository for all business documents in the system, storing:

### Key Relationships
- Each document must have a DocumentDefinitionId
- Links to various reference tables:
  - Agents
  - Resources
  - Centers
  - Currencies
  - Units
  - Users (for CreatedBy/ModifiedBy)
- Supports system-versioning through ValidFrom/ValidTo columns

### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[DefinitionId] INT,
[SerialNumber] INT,
[State] SMALLINT,
[StateAt] DATETIMEOFFSET(7),
[Clearance] TINYINT
```
   - Id: Primary key
   - DefinitionId: References the document definition
   - SerialNumber: Document serial number
     - Auto-generated if document definition has isOriginalDocument = 1
     - Must be manually entered if document definition has isOriginalDocument = 0
     - Unique per document definition
   - State: Current state of the document (-1, 0, or 1)
   - StateAt: Timestamp of the last state change
   - Clearance: Document clearance level (0-2)

2. **Common Properties and Inheritance**
```sql
[PostingDate] DATE,
[Memo] NVARCHAR(255),
[CurrencyId] NCHAR(3),
[CenterId] INT,
[AgentId] INT,
[NotedAgentId] INT,
[ResourceId] INT,
[NotedResourceId] INT,
[Quantity] DECIMAL(19,4),
[UnitId] INT,
[Time1] DATETIME2(2),
[Duration] DECIMAL(19,4),
[DurationUnitId] INT,
[Time2] DATETIME2(2),
[NotedDate] DATE,
[ExternalReference] NVARCHAR(50),
[ReferenceSourceId] INT,
[InternalReference] NVARCHAR(50),
[Lookup1Id] INT,
[Lookup2Id] INT
```
   - Properties can be configured to inherit from document header
   - Inheritance rules:
     - For each line definition LD:
       - If AgentId of entries with index 0 is set to inherit:
         - Setting AgentId in document header →
         - Copies to AgentId of all entries with index 0 in lines with definition LD
       - If AgentId of entries with index 1 is set to inherit:
         - Setting AgentId in document header →
         - Copies to AgentId of all entries with index 1 in lines with definition LD
     - Applies to multiple properties:
       - AgentId
       - NotedAgentId
       - ResourceId
       - NotedResourceId
       - Quantity
       - UnitId
       - Time1, Time2
       - NotedDate
       - ExternalReference
       - ReferenceSourceId
       - InternalReference
   - Example:
     - Document has two line definitions: LD1 and LD2
     - If LD1's entries of index 0 are set to inherit AgentId:
       - Setting AgentId in document header →
       - Copies to AgentId of all entries with index 0 in lines with LD1
     - If LD2's entries of index 1 are set to inherit AgentId:
       - Setting AgentId in document header →
       - Copies to AgentId of all entries with index 1 in lines with LD2

3. **ZATCA Integration**
   - ZATCA (Zakat, Tax, and Customs Authority of Saudi Arabia) integration fields
   - Used for posting business documents to ZATCA server
   - Supported document types:
     - Sales invoice voucher
     - Sales debit memo
     - Sales credit memo

```sql
[ZatcaState] INT,            -- 1=Pending, 10=Reported, -10=Error
[ZatcaResult] NVARCHAR(MAX), -- Validation result from ZATCA
[ZatcaSerialNumber] INT,     -- Invoice serial number for next invoice
[ZatcaHash] NVARCHAR(MAX),   -- Invoice hash for next invoice
[ZatcaUuid] UNIQUEIDENTIFIER -- Invoice UUID linking to XML invoice file
```

   - ZatcaState values:
     - 1: Document is pending submission to ZATCA
     - 10: Document has been successfully reported to ZATCA
     - -10: Error occurred during ZATCA submission

   - ZatcaResult: Contains detailed validation results from ZATCA
   - ZatcaSerialNumber: Serial number assigned by ZATCA for next invoice
   - ZatcaHash: Cryptographic hash used for invoice verification
   - ZatcaUuid: Unique identifier linking to the XML invoice file submitted to ZATCA

4. **Audit Fields**
```sql
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT
```
   - Tracks creation and modification history
   - Links to Users table for audit information

### Implementation Notes
1. **State Management**
   - Document state (-1) is determined by all lines having negative states
   - State changes are tracked in DocumentStatesHistory
   - StateAt is automatically updated on state changes

2. **Workflow Integration**
   - Documents follow workflow rules based on their line types
   - Workflow lines go through full workflow states
   - Non-workflow lines have simplified state progression

3. **Time Zone Considerations**
   - Uses DATETIMEOFFSET for proper time zone handling
   - Accounts for server-hosting location differences
   - Allows 1 day future posting dates for timezone differences

## Best Practices
1. **Document State Management**
   - Always track state changes in DocumentStatesHistory
   - Ensure proper state progression based on line types
   - Update StateAt when changing state

2. **Common Properties**
   - Use IsCommon flags appropriately
   - Copy common properties to all lines
   - Use line-specific properties when values need to vary

3. **Zatca Integration**
   - Properly handle Zatca states and results
   - Maintain Zatca serial numbers and hashes
   - Link invoices correctly using Zatca UUID

4. **Audit Trail**
   - Maintain proper audit information
   - Track all changes to important fields
   - Preserve creation and modification history

## Performance Considerations
1. **Indexing**
   - Primary key is non-clustered
   - ([PostingDate], [Id]) is clustered index
   - Unique constraint on ([DefinitionId], [SerialNumber])

2. **Data Volume**
   - Use appropriate data types for each field
   - Handle large text fields (NVARCHAR(MAX)) carefully
   - System handles large volumes through proper indexing and partitioning

3. **Time Handling**
   - Use DATETIMEOFFSET for proper time zone handling
   - Account for server-hosting location time differences
   - Allow for timezone-aware date handling

4. **Multi-Tenant Environment**
   - Each tenant has its own dedicated database
   - All tenant databases share the same schema at all times
   - Complete data isolation between tenants through separate databases
   - Performance optimized for individual tenant databases
   - Schema updates are synchronized across all tenant databases

5. **Database Management**
   - Database administration handled by Tellma
   - Performance optimization managed centrally
   - Tenants do not need to manage database configuration
