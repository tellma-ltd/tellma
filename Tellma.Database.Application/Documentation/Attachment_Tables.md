# Attachment Tables

## Overview
The system includes three similar attachment tables that handle file attachments for different types of entities:
1. `Attachments` - For document attachments
2. `AgentAttachments` - For agent-related attachments
3. `ResourceAttachments` - For resource-related attachments

## Common Structure
All three tables share a similar structure with minor variations:

### Common Fields
- **Id**: Unique identifier
- **[Entity]Id**: References the parent entity (Document/Agent/Resource)
- **FileName**: Original name of the file
- **FileExtension**: File extension (e.g., .pdf, .docx, .jpg)
- **FileId**: Reference to the file in blob storage
- **Size**: File size in bytes
- **CreatedAt**: Timestamp of creation
- **CreatedById**: User who created the attachment
- **ModifiedAt**: Timestamp of last modification
- **ModifiedById**: User who last modified the attachment

### Common Relationships
- **CreatedBy/ModifiedBy**: Links to `Users` table
- **CASCADE DELETE**: All attachments are automatically deleted when their parent entity is deleted

## Table-Specific Details

### 1. Attachments
**Parent Table**: `Documents`
**Key Differences**:
- References `DocumentId`
- No category field

### 2. AgentAttachments
**Parent Table**: `Agents`
**Additional Fields**:
- **CategoryId**: Optional classification of the attachment (references `Lookups` table)

### 3. ResourceAttachments
**Parent Table**: `Resources`
**Additional Fields**:
- **CategoryId**: Optional classification of the attachment (references `Lookups` table)

## Usage Patterns

### When to Use Each Table
1. **Attachments**: For files directly related to documents
2. **AgentAttachments**: For files related to agents (e.g., employee documents, supplier contracts)
3. **ResourceAttachments**: For files related to resources (e.g., product images, specification sheets)

### Common Use Cases
- Storing supporting documents
- Attaching images or files to entities
- Maintaining file-based records

## Implementation Notes
- All files are stored in external blob storage
- The `FileId` field contains the blob storage reference
- File metadata is stored in the database for efficient querying
- The `Size` field allows for size-based validations

## Constraints and Indexes
All tables include:
- Primary key on `Id`
- Foreign key to parent table with CASCADE DELETE
- Foreign keys to `Users` table for audit fields
- Index on the parent entity ID for better query performance
