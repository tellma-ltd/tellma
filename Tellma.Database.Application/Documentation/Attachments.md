# Attachments Table

## Overview
The `Attachments` table stores metadata about files attached to documents in the system. It serves as a reference to files stored in external blob storage, allowing the system to manage document attachments efficiently.

## Table Structure
```sql
CREATE TABLE [dbo].[Attachments] (
    [Id]                INT                 IDENTITY PRIMARY KEY,
    [DocumentId]        INT                 NOT NULL,
    [FileName]          NVARCHAR(255)       NOT NULL,
    [FileExtension]     NVARCHAR(50)        NULL,
    [FileId]            NVARCHAR(50)        NOT NULL,  -- Reference to blob storage
    [Size]              BIGINT              NOT NULL,
    [CreatedAt]         DATETIMEOFFSET(7)   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CreatedById]       INT                 NOT NULL,
    [ModifiedAt]        DATETIMEOFFSET(7)   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [ModifiedById]      INT                 NOT NULL
);

-- Index for better query performance
CREATE INDEX IX_Attachments__DocumentId ON dbo.Attachments([DocumentId]);
```

## Key Fields
- **Id**: Unique identifier for each attachment
- **DocumentId**: References the `Documents` table. This is the document that the attachment is associated with.
- **FileName**: Original name of the attached file
- **FileExtension**: File extension (e.g., .pdf, .docx, .jpg)
- **FileId**: Unique identifier for the file in the external blob storage system
- **Size**: Size of the file in bytes

### Audit Fields
- **CreatedAt**: Timestamp when the attachment was created
- **CreatedById**: User who uploaded the attachment
- **ModifiedAt**: Timestamp when the attachment was last modified
- **ModifiedById**: User who last modified the attachment

## Relationships
- **Document**: Many-to-one relationship with `Documents` table (on `DocumentId`)
  - CASCADE DELETE: When a document is deleted, all its attachments are automatically deleted
- **CreatedBy/ModifiedBy**: Many-to-one relationships with `Users` table

## Usage Notes
1. **File Storage**:
   - The actual file content is stored in an external blob storage system
   - The `FileId` field contains the reference to the file in blob storage
   - The `FileExtension` helps determine the file type for proper handling and display

2. **File Management**:
   - The system enforces referential integrity with the `Documents` table
   - Attachments are automatically cleaned up when their parent document is deleted

3. **Performance**:
   - An index on `DocumentId` ensures efficient querying of all attachments for a document
   - The `Size` field allows for size-based validations and reporting

## Example Use Cases
1. **Document Attachments**:
   - Attach supporting documents like receipts, contracts, or images to a main document
   - Example: Attach a scanned receipt to an expense report

2. **File Versioning**:
   - Store multiple versions of a document as separate attachments
   - Track changes to documents over time

3. **Multi-file Documents**:
   - Associate multiple related files with a single document
   - Example: A project document with multiple supporting files

## Constraints
- **PK_Attachments**: Primary key on `Id`
- **FK_Attachments__DocumentId**: Foreign key to `Documents(Id)` with CASCADE DELETE
- **FK_Attachments__CreatedById**: Foreign key to `Users(Id)`
- **FK_Attachments__ModifiedById**: Foreign key to `Users(Id)`

## Indexes
- **IX_Attachments__DocumentId**: Improves performance when querying attachments for a specific document
