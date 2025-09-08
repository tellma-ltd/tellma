# Blobs Table

## Overview
The `Blobs` table is used in **on-premises deployments** of Tellma to store binary data such as file attachments, images, and other binary content. In Azure deployments, this functionality is handled by Azure Blob Storage instead.

## Table Structure
```sql
CREATE TABLE [dbo].[Blobs]
(
    [Id]            NVARCHAR(450) NOT NULL PRIMARY KEY, 
    [Content]       VARBINARY(MAX) NOT NULL
);
```

## Key Fields
- **Id**: A unique identifier for the blob, typically a GUID or a string that follows a specific naming convention
- **Content**: The actual binary data stored as a variable-length binary large object (VARBINARY(MAX))

## Deployment-Specific Behavior

### On-Premises Deployment
- The `Blobs` table is actively used to store all binary data
- Files are stored directly in the SQL Server database
- Provides a simple deployment model without requiring additional storage services
- Performance characteristics are determined by the SQL Server instance

### Azure Deployment
- The `Blobs` table is **not used**
- Azure Blob Storage is used instead for storing binary data
- Offers better scalability and cost-effectiveness for large amounts of binary data
- Provides additional features like geo-redundancy and CDN integration

## Usage in the Application
- The application uses a consistent API to access blobs, abstracting away the underlying storage mechanism
- The storage implementation (database vs. Azure Blob Storage) is determined by the deployment configuration

## Migration Considerations
When migrating between deployment types:
1. **To Azure**: Blobs must be migrated from the database to Azure Blob Storage
2. **From Azure to On-Premises**: Blobs must be moved from Azure Blob Storage back to the database

## Performance Considerations for On-Premises
- Large binary objects can significantly increase database size
- May impact backup/restore times
- Consider SQL Server's FILESTREAM feature as an alternative for very large files

## Security
- Access to blob data is controlled by the application's authentication and authorization mechanisms
- In on-premises deployments, database-level security also applies to blob data
- In Azure, blob storage access policies and SAS tokens can be used for fine-grained access control

## Maintenance
For on-premises deployments:
- Regular database maintenance (backups, integrity checks) includes blob data
- Consider implementing a cleanup process for orphaned blobs
- Monitor database size growth due to blob storage
