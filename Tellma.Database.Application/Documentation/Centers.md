# Centers Table

The `dbo.Centers` table represents organizational centers within the system, forming a hierarchical structure. Centers can be either business units or leaves (specific departments/functions).

## Table Structure

### Primary Key
- `Id`: INT, Primary Key, Identity

### Hierarchical Structure
- `ParentId`: INT, Foreign Key referencing Centers(Id)
- `Node`: HIERARCHYID, Unique Clustered Index
- `Level`: Computed property from Node.GetLevel()
- `IsLeaf`: BIT, indicates if the center is a leaf node

### Center Type
- `CenterType`: NVARCHAR(255), NOT NULL
  - Valid values:
    - Abstract
    - BusinessUnit
    - Administration
    - Marketing
    - Service
    - Operation
    - Sale
    - OtherPL
    - ConstructionInProgressExpendituresControl
    - InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl
    - WorkInProgressExpendituresControl
    - CurrentInventoriesInTransitExpendituresControl

### Naming
- `Name`: NVARCHAR(255), NOT NULL
- `Name2`: NVARCHAR(255)
- `Name3`: NVARCHAR(255)
- All names are unique within their CenterType

### Status
- `IsActive`: BIT, NOT NULL, DEFAULT 1
- `Code`: NVARCHAR(50), NOT NULL, UNIQUE

### Audit Fields
- `CreatedAt`: DATETIMEOFFSET(7), NOT NULL, DEFAULT SYSDATETIMEOFFSET()
- `CreatedById`: INT, NOT NULL, Foreign Key to Users(Id)
- `ModifiedAt`: DATETIMEOFFSET(7), NOT NULL, DEFAULT SYSDATETIMEOFFSET()
- `ModifiedById`: INT, NOT NULL, Foreign Key to Users(Id)

## Constraints

### Primary Key
- `PK_Centers`: Primary Key on Id

### Foreign Keys
- `FK_Centers__ParentId`: References Centers(Id)
- `FK_Centers__CreatedById`: References Users(Id)
- `FK_Centers__ModifiedById`: References Users(Id)

### Unique Constraints
- `UQ_Centers__Code`: Unique on Code
- `UQ_Centers__Node`: Unique Clustered on Node
- `UQ_Centers__Name`: Unique on (CenterType, Name)
- `UQ_Centers__Name2`: Unique on (CenterType, Name2) WHERE Name2 IS NOT NULL
- `UQ_Centers__Name3`: Unique on (CenterType, Name3) WHERE Name3 IS NOT NULL

### Check Constraints
- `CK_Centers__CenterType`: Validates CenterType values
- `CK_Centers__CenterType_IsLeaf`: Ensures IsLeaf = 0 only for Abstract or BusinessUnit types

## Indexes
- `IX_Centers__ParentId`: Nonclustered on ParentId
- `UQ_Centers__Name`: Unique Nonclustered on (CenterType, Name)
- `UQ_Centers__Name2`: Unique Nonclustered on (CenterType, Name2) with filter
- `UQ_Centers__Name3`: Unique Nonclustered on (CenterType, Name3) with filter

## Triggers

### trIU_Centers (AFTER INSERT, UPDATE)
- Maintains IsLeaf property when Id or ParentId changes
- Sets IsLeaf = 1 if no children exist
- Sets IsLeaf = 0 if children exist

### trD_Centers (AFTER DELETE)
- Maintains IsLeaf property after deletion
- Recalculates IsLeaf for affected nodes

## Business Rules
1. Centers form a hierarchical structure using HIERARCHYID
2. Only Abstract and BusinessUnit types can have children
3. CenterType must be one of the predefined values
4. Names must be unique within their CenterType
5. Code must be unique across all centers
6. Center hierarchy is maintained through triggers
