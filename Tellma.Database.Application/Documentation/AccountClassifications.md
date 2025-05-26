# AccountClassifications Table

## Purpose
The `AccountClassifications` table is used to create a hierarchical classification system for accounts. This classification system is completely customizable by the user and provides a flexible way to organize accounts according to their needs.

## Key Features

### 1. Hierarchical Structure
- Uses `HIERARCHYID` to maintain the tree structure
- Each row has a `Node` column that represents its position in the hierarchy
- The `ParentId` column references the parent classification
- The `IsLeaf` column indicates whether a classification is a leaf node (no children)

### 2. Naming and Identification
- Each classification has a `Code` (required, unique, max 50 chars)
- Multiple language support through `Name`, `Name2`, and `Name3` columns
- The `Code` serves as the basis of the tree structure

### 3. Activity Control
- `IsActive` column controls visibility in the account classification UI
- Inactive classifications don't appear when classifying accounts
- Default value is 1 (active)

### 4. Audit Tracking
- Complete audit trail with `CreatedAt`, `CreatedById`, `ModifiedAt`, and `ModifiedById`
- All timestamps are stored with timezone information (DATETIMEOFFSET)

## Important Notes

1. **AccountTypeParentId**
   - This column is present but not used in the current implementation
   - It was likely intended for future use but remains unused

2. **Tree Structure Maintenance**
   - The table includes triggers to maintain the tree structure:
     - `trIU_AccountClassifications`: Updates leaf status on insert/update
     - `trD_AccountClassifications`: Updates leaf status on delete

3. **Unique Constraints**
   - The `Code` column has a unique constraint
   - The `Node` column has a unique constraint
   - The table uses a non-clustered primary key on `Id`
   - The clustered index is on the `Code` column

## Usage
The AccountClassifications table provides a flexible way to organize accounts according to local financial standards and requirements. Here are some practical examples:

1. **Non-IFRS Countries**
   - **Senegal**: Follows OHADA accounting standards
   - **Lebanon**: Uses a customized version of OHADA
   - **Ethiopia**: Requires specific financial statement formats for the Ministry of Finance (even though it follows IFRS)
   
   In these cases, the AccountClassifications table allows organizations to:
   - Create account categories that align with local accounting standards
   - Generate financial statements in formats required by local authorities
   - Maintain compliance with regional accounting practices
   - Organize accounts in ways that are meaningful for local reporting requirements

2. **General Usage**
   - Create custom account categories for internal reporting
   - Organize accounts by department, function, or other business-specific criteria
   - Control which classifications are visible to users
   - Track who created and modified classifications

## Schema
```sql
CREATE TABLE [dbo].[AccountClassifications] (
    [Id]                      INT             CONSTRAINT [PK_AccountClassifications] PRIMARY KEY NONCLUSTERED IDENTITY,
    [ParentId]                INT             CONSTRAINT [FK_AccountClassifications__ParentId] REFERENCES [dbo].[AccountClassifications] ([Id]),
    [Name]                    NVARCHAR (255),
    [Name2]                   NVARCHAR (255),
    [Name3]                   NVARCHAR (255),
    [Code]                    NVARCHAR (50)   NOT NULL CONSTRAINT [UQ_AccountClassifications__Code] UNIQUE CLUSTERED,
    [AccountTypeParentId]     INT             CONSTRAINT [FK_AccountClassifications__AccountTypeParentId] REFERENCES dbo.AccountTypes([Id]),
    [IsActive]                BIT             NOT NULL DEFAULT 1,
    [CreatedAt]               DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CreatedById]             INT             NOT NULL CONSTRAINT [FK_AccountClassifications__CreatedById] REFERENCES [dbo].[Users] ([Id]),
    [ModifiedAt]              DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [ModifiedById]            INT             NOT NULL CONSTRAINT [FK_AccountClassifications__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
    [Node]                    HIERARCHYID     NOT NULL CONSTRAINT [UQ_AccountClassifications__Node] UNIQUE,
    [ParentNode]              AS [Node].GetAncestor(1),
    [IsLeaf]                  BIT             DEFAULT 0
);
```

## Indexes
- Non-clustered index on `ParentId`
- Unique clustered index on `Code`
- Unique non-clustered index on `Node`
- Primary key index on `Id`
