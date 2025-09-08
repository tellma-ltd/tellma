# AgentAttachments

The `AgentAttachments` table stores file attachments associated with agents in the system. This table maintains a record of all files uploaded and linked to agent records, including metadata about each attachment.

## Table Structure
```sql
CREATE TABLE [dbo].[AgentAttachments] (
    [Id]                    INT                 PRIMARY KEY IDENTITY,
    [AgentId]               INT                 NOT NULL,
    [CategoryId]            INT                 NULL,
    [FileName]              NVARCHAR(255)       NOT NULL,
    [FileExtension]         NVARCHAR(50)        NULL,
    [FileId]                NVARCHAR(50)        NOT NULL,  -- Reference to blob storage
    [Size]                  BIGINT              NOT NULL,
    [CreatedAt]             DATETIMEOFFSET(7)   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CreatedById]           INT                 NOT NULL,
    [ModifiedAt]            DATETIMEOFFSET(7)   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [ModifiedById]          INT                 NOT NULL
)
```

## Key Fields
- **Id**: Unique identifier for each attachment record
- **AgentId**: References the agent this attachment belongs to (foreign key to `Agents` table)
- **CategoryId**: Optional category for the attachment (references `Lookups` table)
- **FileName**: Original name of the attached file
- **FileExtension**: File extension (e.g., .pdf, .jpg)
- **FileId**: Unique identifier used to retrieve the file from blob storage
- **Size**: Size of the file in bytes

## Audit Fields
- **CreatedAt**: Timestamp when the record was created
- **CreatedById**: User who created the record (references `Users` table)
- **ModifiedAt**: Timestamp when the record was last modified
- **ModifiedById**: User who last modified the record (references `Users` table)

## Relationships
- **Agent**: One-to-many relationship with `Agents` table (on `AgentId`)
- **Category**: Optional many-to-one relationship with `Lookups` table (on `CategoryId`)
- **CreatedBy**: Many-to-one relationship with `Users` table (on `CreatedById`)
- **ModifiedBy**: Many-to-one relationship with `Users` table (on `ModifiedById`)

## Constraints
- **PK_AgentAttachments**: Primary key on `Id`
- **FK_AgentAttachments__AgentId**: Foreign key to `Agents` table
- **FK_AgentAttachments__CategoryId**: Optional foreign key to `Lookups` table
- **FK_AgentAttachments__CreatedById**: Foreign key to `Users` table
- **FK_AgentAttachments__ModifiedById**: Foreign key to `Users` table

## Usage Notes
- When an agent is deleted, all associated attachments are automatically deleted (CASCADE DELETE)
- The actual file content is stored in blob storage, with `FileId` used as the reference
- File metadata is stored in the database for efficient querying and display
