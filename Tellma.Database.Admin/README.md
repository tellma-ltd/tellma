# Tellma.Database.Admin

SQL Server database project (SSDT) for the Tellma admin database.

## Generating a Migration Script

To generate a diff script between this project and an existing admin database on your local:

1. Build the project in Visual Studio in `Release` mode to produce the `.dacpac` file.

2. Run the following command:

```bash
sqlpackage /Action:Script /SourceFile:"Tellma.Database.Admin\bin\Release\Tellma.Database.Admin.dacpac" /TargetServerName:"." /TargetDatabaseName:"Tellma" /OutputPath:"admin-diff.sql" /TargetTrustServerCertificate:True /p:DropIndexesNotInSource=True
```

3. Review the generated `diff-admin.sql` script, then run it on the target database.

### Useful Flags

- `/p:DropObjectsNotInSource=False` -- prevent dropping objects that exist in the DB but not in the DACPAC
- `/p:BlockOnPossibleDataLoss=True` -- abort if changes would cause data loss
- `/p:ExcludeObjectTypes=Users;Logins;RoleMembership` -- skip security objects

### Installing SqlPackage

If `sqlpackage` is not on your PATH:

```bash
dotnet tool install -g microsoft.sqlpackage
```

# Common Errors
## Blocking Index

> *** Verification of the deployment plan failed.
> Error SQL72031: This deployment may encounter errors during execution because changes to [dbo].[DirectoryUsers].[EmailOrClientId] are blocked by [dbo].[DirectoryUsers].[IX_DirectoryUsers_Email]'s dependency in the target database.

Fix: Must manually drop the index on the target before the running `sqlpackage` command.
```
IF EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_DirectoryUsers_Email'
      AND object_id = OBJECT_ID('dbo.DirectoryUsers')
)
BEGIN
    DROP INDEX IX_DirectoryUsers_Email ON dbo.DirectoryUsers;
END
```