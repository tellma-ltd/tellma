# Entries Table Documentation

## Overview
The `Entries` table represents the basic unit of data in line definitions. Each entry captures specific information within a line, forming the fundamental building block of data in the system.

## Key Characteristics

1. **Entry Types**
   - Two main types of entries:
     - Transaction Entries:
       - Use regular accounts
       - Have non-zero MonetaryValue
       - Track financial movements
     - Statistical Entries:
       - Use statistical accounts
       - Have zero MonetaryValue
       - Used for requests, inquiries, and other non-financial data
   - Each entry must be associated with a Line
   - Entries must have a unique combination of LineId and Index

2. **Direction and Value**
   - Direction: -1 (Debit) or 1 (Credit)
   - Multiple value types:
     - MonetaryValue: Transaction amount in currency
     - Value: Equivalent in functional currency
     - RValue: Re-instated value in functional currency
     - PValue: Equivalent in presentation currency

3. **Multi-Currency Support**
   - Each entry has a CurrencyId
   - Values are tracked in both transaction and functional currencies
   - Presentation currency support through PValue

## Hyper-Inflationary Economies
   - RValue is specifically designed for hyper-inflationary economies
   - Used to adjust values for inflation over time
   - Maintains accurate financial records in rapidly changing economic conditions
   - Provides a way to restate historical values in current terms

## Purpose
The Entries table serves as the core data structure for:

1. **General Entries**
   - Recording accounting transactions
   - Capturing request entries
   - Processing inquiry entries
   - Handling other non-transaction entries

2. **Financial Movements**
   - Maintaining double-entry bookkeeping for transaction entries
   - Tracking transaction amounts and directions for transaction entries
   - Recording monetary values only for transaction entries (0 for statistical entries)

3. **Measurements and Quantities**
   - Tracking physical quantities
   - Recording time-based measurements
   - Capturing resource movements

4. **Audit Trail**
   - Maintaining creation and modification history
   - Tracking user actions
   - Providing traceability for all entries

## Key Fields

1. **Core Fields**
```sql
[Id] INT,
[LineId] INT,
[Index] INT,
[Direction] SMALLINT,
[AccountId] INT,
[CurrencyId] NCHAR(3),
[CenterId] INT
```
   - Id: Primary key
   - LineId: References Lines table (ON DELETE CASCADE)
   - Index: Position within the line
   - Direction: -1 (Debit) or 1 (Credit)
   - AccountId: Account affected by the entry
   - CurrencyId: Transaction currency
   - CenterId: Responsibility center for the entry

2. **Value Fields**
```sql
[MonetaryValue] DECIMAL(19,4),
[Value] DECIMAL(19,4),
[RValue] DECIMAL(19,4),
[PValue] DECIMAL(19,4)
```
   - MonetaryValue:
     - Transaction entries: Actual transaction amount
     - Statistical entries: Always 0
   - Value: 
     - Transaction entries: Equivalent in functional currency
     - Statistical entries: Always 0
   - RValue: 
     - Transaction entries: Re-instated value in functional currency
     - Used in hyper-inflationary economies to adjust values for inflation
     - Statistical entries: Always 0
   - PValue: 
     - Transaction entries: Equivalent in presentation currency
     - Statistical entries: Always 0

3. **Quantity and Time Fields**
```sql
[Quantity] DECIMAL(19,4),
[UnitId] INT,
[Time1] DATETIME2(2),
[Time2] DATETIME2(2)
```
   - Quantity: Physical quantity of the transaction
   - UnitId: Unit of measurement
   - Time1: Start time of the transaction
   - Time2: End time of the transaction

4. **Reference Fields**
```sql
[AgentId] INT,
[NotedAgentId] INT,
[ResourceId] INT,
[NotedResourceId] INT,
[EntryTypeId] INT,
[ExternalReference] NVARCHAR(50),
[ReferenceSourceId] INT,
[InternalReference] NVARCHAR(50),
[NotedAgentName] NVARCHAR(255),
[NotedAmount] DECIMAL(19,4),
[NotedDate] DATE
```
   - AgentId: Primary agent involved
   - NotedAgentId: Additional agent reference
   - ResourceId: Resource involved
   - NotedResourceId: Additional resource reference
   - EntryTypeId: Type of entry (if required by account type)
   - ReferenceSourceId: Source of the reference (e.g., bank statement)

   - ExternalReference: Reference from external systems
     - Used when the same entry exists in another system
     - Example: Bank transaction reference number
     - Maintains traceability between internal and external systems

   - InternalReference: Reference from internal manual systems
     - Used for entries tracked in company's manual systems
     - Example: Internal voucher number
     - Maintains traceability within company's manual records

   - NotedAgentName: Name of related agent without full profile
     - Used when recording agent names without creating full profiles
     - Example: Name of warehouse picker
     - Allows recording of temporary or one-time agent names

   - NotedAmount: Related amount for specific purposes
     - Residual value for fixed asset acquisitions
     - Taxable amount for tax entries
     - Salary subject to social security for SS entries
     - Any other related monetary amounts

   - NotedDate: Related dates for specific purposes
     - Due date for payables and receivables
     - Contract dates
     - Document dates
     - Any other significant dates related to the entry

5. **Audit Fields**
```sql
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT
```
   - Creation and modification timestamps
   - User IDs for tracking actions
   - Maintains full audit trail

## Constraints and Rules

1. **Key Constraints**
   - Primary key: Id
   - Unique constraint: (LineId, Index)
   - Direction must be -1 or 1
   - CurrencyId is required
   - CenterId is required

2. **Foreign Key Constraints**
   - LineId references Lines table
   - AccountId references Accounts table
   - CurrencyId references Currencies table
   - AgentId references Agents table
   - ResourceId references Resources table
   - CenterId references Centers table
   - EntryTypeId references EntryTypes table
   - CreatedById and ModifiedById reference Users table

## Best Practices

1. **Transaction Recording**
   - Always specify correct Direction (-1 or 1)
   - Ensure proper currency conversion
   - Maintain proper account references

2. **Quantity Tracking**
   - Use appropriate units for measurements
   - Record accurate quantities
   - Track time-based measurements when relevant

3. **Reference Management**
   - Use reference fields appropriately for reporting
   - Maintain accurate agent and resource references
   - Keep noted information up to date

4. **Audit Trail**
   - Never modify creation timestamps
   - Ensure proper user tracking
   - Maintain proper modification history

## Multi-Tenant Considerations
- Each tenant has its own database
- Entries are isolated per tenant
- Currency and account references are tenant-specific
- Audit trail is maintained per tenant

## Performance Considerations
- Indexes on frequently queried fields
- Proper foreign key relationships
- Efficient value calculations
- Optimized currency conversions

## Common Queries

1. **Get Entry Details**
```sql
SELECT e.*, a.AccountName, c.CurrencyName, ag.AgentName
FROM Entries e
JOIN Accounts a ON e.AccountId = a.Id
JOIN Currencies c ON e.CurrencyId = c.Id
LEFT JOIN Agents ag ON e.AgentId = ag.Id
WHERE e.Id = @EntryId
```

2. **Get Line Entries**
```sql
SELECT e.*, l.*
FROM Entries e
JOIN Lines l ON e.LineId = l.Id
WHERE e.LineId = @LineId
ORDER BY e.Index
```

3. **Get Account Movements**
```sql
SELECT e.*, l.*, d.*
FROM Entries e
JOIN Lines l ON e.LineId = l.Id
JOIN Documents d ON l.DocumentId = d.Id
WHERE e.AccountId = @AccountId
ORDER BY e.CreatedAt
```
