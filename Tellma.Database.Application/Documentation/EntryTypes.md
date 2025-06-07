# EntryTypes Table Documentation

## Overview
The `EntryTypes` table defines the classification and purpose of accounting entries in the system. It implements a hierarchical structure inspired by IFRS (International Financial Reporting Standards) concepts, particularly for income statement by function and cash flow reporting (direct method) and fixed asset register.

## Key Characteristics

1. **Hierarchical Structure**
   - Implements a tree-like structure using HIERARCHYID
   - Each entry type can have parent-child relationships
   - Supports multiple levels of classification

2. **Purpose**
   - Classifies accounting entries by their economic purpose
   - Facilitates smart posting and reporting
   - Enables IFRS-compliant financial reporting
   - Supports income statement by function
   - Supports cash flow reporting (direct method)
   - Supports fixed asset register

## Key Fields

1. **Hierarchy Fields**
```sql
[Id] INT,
[ParentId] INT,
[Node] HIERARCHYID,
[ParentNode] AS Node.GetAncestor(1) PERSISTED,
[Level] AS Node.GetLevel() PERSISTED,
[Path] AS Node.ToString() PERSISTED
```
   - Id: Primary key
   - ParentId: References parent entry type
   - Node: HIERARCHYID for tree structure
   - ParentNode: Computed field for parent node
   - Level: Computed field for hierarchy level
   - Path: Computed field for node path

2. **Classification Fields**
```sql
[Code] NVARCHAR(50),
[Concept] NVARCHAR(255),
[Name] NVARCHAR(255),
[Name2] NVARCHAR(255),
[Name3] NVARCHAR(255),
[Description] NVARCHAR(1024),
[Description2] NVARCHAR(1024),
[Description3] NVARCHAR(1024)
```
   - Code: Unique identifier for entry type
   - Concept: IFRS concept or classification
   - Name/Name2/Name3: Multi-language display names
   - Description/Description2/Description3: Multi-language descriptions

3. **Control Fields**
```sql
[IsAssignable] BIT,
[IsActive] BIT,
[IsSystem] BIT
```
   - IsAssignable: Controls whether entries can be assigned to this type
   - IsActive: Controls whether the entry type is currently active
   - IsSystem: Indicates if this is a system-defined entry type

4. **Audit Fields**
```sql
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT
```
   - CreatedAt/ModifiedAt: Timestamps for record changes
   - CreatedById/ModifiedById: References Users table for accountability

## Related Tables

### Users
- References for CreatedById and ModifiedById
- Tracks accountability for record changes

## Best Practices

### Hierarchy Management
- Use ParentId to establish relationships
- Maintain proper hierarchy levels
- Use Node for efficient tree traversal

### Classification
- Use meaningful Codes that align with IFRS concepts
- Maintain accurate Concepts for reporting
- Keep descriptions clear and consistent

### Usage
- Set IsAssignable = 1 for entry types that can be used in postings
- Set IsActive = 1 for active entry types
- Use IsSystem = 1 for system-defined entry types



## Performance Considerations
- Use proper indexes for hierarchy queries
- Consider partitioning for large entry type sets
- Use Node for efficient tree traversal
- Cache frequently used entry types
- Use proper indexing on Code and Concept fields
