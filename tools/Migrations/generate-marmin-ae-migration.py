#!/usr/bin/env python3
"""
Generates tools/Migrations/2026-09-MarminAe.sql.

The migration script is NOT hand-written. Every object body in it is copied verbatim from the
canonical .sql file in Tellma.Database.Application, with only two mechanical rewrites:

  CREATE PROCEDURE  ->  CREATE OR ALTER PROCEDURE
  CREATE VIEW       ->  CREATE OR ALTER VIEW
  CREATE FUNCTION   ->  CREATE OR ALTER FUNCTION

so that re-running the migration is safe. Only the ALTER TABLE guards and the verification tail
are authored here. That keeps the migration and the database project from drifting apart: if a
canonical file changes, re-run this script rather than editing the .sql by hand.

Usage (from the repo root):
    python tools/Migrations/generate-marmin-ae-migration.py
"""

import io
import os
import re

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DB = os.path.join(REPO, "Tellma.Database.Application")
OUT = os.path.join(REPO, "tools", "Migrations", "2026-09-MarminAe.sql")

# The three procedures that reference the dbo.DocumentDefinitionList table type. A table type
# cannot be ALTERed, and SQL Server refuses to DROP one while any procedure references it, so all
# three have to be dropped and recreated around the swap. Verified exhaustive by grepping the
# database project for "DocumentDefinitionList".
TVP_DEPENDENTS = [
    ("api", "DocumentDefinitions__Save", "api/Stored Procedures/api.DocumentDefinitions__Save.sql"),
    ("bll", "DocumentDefinitions_Validate__Save", "bll/Stored Procedures/bll.DocumentDefinitions_Validate__Save.sql"),
    ("dal", "DocumentDefinitions__Save", "dal/Stored Procedures/dal.DocumentDefinitions__Save.sql"),
]

# Everything else the feature adds or changes, in dependency order.
PROGRAMMABLE = [
    "map/map.Documents.sql",
    "dal/Stored Procedures/dal.MarminAe__GetInvoices.sql",
    "dal/Stored Procedures/dal.MarminAe__MarkSubmitting.sql",
    "dal/Stored Procedures/dal.MarminAe__UpdateDocumentInfo.sql",
    "dal/Stored Procedures/dal.MarminAe__SaveSecrets.sql",
    "dal/Stored Procedures/dal.MarminAe__ApplyWebhook.sql",
    "bll/Stored Procedures/bll.Documents_Validate__Open.sql",
    "bll/Stored Procedures/bll.Documents_Validate__Close.sql",
]

# (table, column, the exact type/constraint clause to add)
COLUMNS = [
    ("dbo.Settings", "MarminAeEnvironment", "NVARCHAR(10) NOT NULL CONSTRAINT [DF_Settings__MarminAeEnvironment] DEFAULT N'Sandbox'"),
    ("dbo.Settings", "MarminAeEncryptedClientSecret", "NVARCHAR(MAX) NULL"),
    ("dbo.Settings", "MarminAeEncryptedWebhookSecret", "NVARCHAR(MAX) NULL"),
    ("dbo.Settings", "MarminAeEncryptionKeyIndex", "INT NOT NULL CONSTRAINT [DF_Settings__MarminAeEncryptionKeyIndex] DEFAULT 0"),
    ("dbo.Documents", "MarminAeState", "INT NULL"),
    ("dbo.Documents", "MarminAeDocumentId", "NVARCHAR(50) NULL"),
    ("dbo.Documents", "MarminAeDocumentNumber", "NVARCHAR(50) NULL"),
    ("dbo.Documents", "MarminAeResult", "NVARCHAR(MAX) NULL"),
    ("dbo.Documents", "MarminAeLastEventId", "UNIQUEIDENTIFIER NULL"),
    ("dbo.Documents", "MarminAeLastEventAt", "DATETIMEOFFSET(7) NULL"),
    ("dbo.DocumentDefinitions", "MarminAeDocumentType", "NVARCHAR(20) NULL CONSTRAINT [CK_DocumentDefinitions__MarminAeDocumentType] CHECK ([MarminAeDocumentType] IN (N'SalesInvoice', N'SalesCreditNote'))"),
    ("dbo.DocumentDefinitions", "MarminAeTypeCode", "NVARCHAR(10) NULL"),
]


def read(rel):
    """Reads a canonical file, stripping the BOM and any trailing GO."""
    with io.open(os.path.join(DB, rel), encoding="utf-8-sig") as f:
        body = f.read()

    body = body.replace("\r\n", "\n").strip()
    body = re.sub(r"\nGO\s*$", "", body).strip()
    return body


def creatable(rel):
    """The canonical body, rewritten so it can be applied repeatedly."""
    body = read(rel)
    # map.Documents is a table-valued FUNCTION rather than a view, so all three keywords are
    # handled, and a file that starts with none of them is an error rather than a silent pass:
    # it would land in the migration as a plain CREATE and fail the second time it is run.
    body, n = re.subn(
        r"^CREATE\s+(PROCEDURE|VIEW|FUNCTION)\b",
        lambda m: "CREATE OR ALTER " + m.group(1).upper(),
        body, count=1, flags=re.IGNORECASE)

    if n != 1:
        raise SystemExit(f"{rel}: expected a leading CREATE PROCEDURE / VIEW / FUNCTION")

    return body


def main():
    out = []
    w = out.append

    w("""/*
 * Marmin (UAE) e-invoicing -- schema migration.
 *
 * GENERATED FILE. Do not edit by hand: run tools/Migrations/generate-marmin-ae-migration.py,
 * which copies every object body verbatim out of Tellma.Database.Application so that this script
 * and the database project cannot drift apart.
 *
 * Safe to run more than once: the column additions are guarded, and every programmable object is
 * CREATE OR ALTER.
 *
 * BEFORE RUNNING
 *   1. Take a backup. Section 2 drops and recreates the dbo.DocumentDefinitionList table type,
 *      and a table type cannot be ALTERed, so its three dependent procedures must be dropped and
 *      recreated around it. A single transaction cannot span the GO batches that CREATE PROCEDURE
 *      requires, so a failure part-way through leaves those three procedures missing.
 *   2. Run it in a maintenance window. Between the first and last batch of section 2, saving a
 *      document definition fails.
 */
SET NOCOUNT ON;
GO

-- Required, not cosmetic. sqlcmd defaults QUOTED_IDENTIFIER to OFF, and SQL Server refuses to
-- create a filtered index (section 1) or to compile a stored procedure that later touches one
-- unless it is ON. SSDT sets both of these for the same reason.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '--- 1. Columns and indexes -------------------------------------------------';
GO
""")

    for table, column, clause in COLUMNS:
        w(f"""IF COL_LENGTH('{table}', '{column}') IS NULL
    ALTER TABLE [{table.split('.')[0]}].[{table.split('.')[1]}] ADD [{column}] {clause};
GO
""")

    w("""IF INDEXPROPERTY(OBJECT_ID('dbo.Documents'), 'IX_Documents__MarminAeDocumentId', 'IndexID') IS NULL
    CREATE NONCLUSTERED INDEX [IX_Documents__MarminAeDocumentId]
      ON [dbo].[Documents]([MarminAeDocumentId]) WHERE [MarminAeDocumentId] IS NOT NULL;
GO

PRINT '--- 2. dbo.DocumentDefinitionList table type --------------------------------';
GO
""")

    for schema, name, _ in TVP_DEPENDENTS:
        w(f"DROP PROCEDURE IF EXISTS [{schema}].[{name}];\nGO\n")

    w("DROP TYPE IF EXISTS [dbo].[DocumentDefinitionList];\nGO\n")
    w(read("dbo/User Defined Types/dbo.DocumentDefinitionList.sql") + "\nGO\n")

    for _, _, rel in TVP_DEPENDENTS:
        w("\n" + creatable(rel) + "\nGO\n")

    w("""
PRINT '--- 3. Views, procedures and validation -------------------------------------';
GO
""")

    for rel in PROGRAMMABLE:
        w("\n" + creatable(rel) + "\nGO\n")

    w("""
PRINT '--- 4. Verification ---------------------------------------------------------';
GO

-- Expected: 4 Settings columns, 6 Documents columns, 2 DocumentDefinitions columns,
-- 2 TypeColumns, 1 Index, and 9 Objects.
SELECT 'Column' AS [Kind], OBJECT_NAME(c.[object_id]) AS [Parent], c.[name] AS [Name]
FROM sys.columns c
JOIN sys.tables t ON t.[object_id] = c.[object_id]   -- real tables only: sys.columns also
WHERE c.[name] LIKE 'MarminAe%'                      -- carries table types and function results
UNION ALL
SELECT 'TypeColumn', 'DocumentDefinitionList', c.[name]
FROM sys.table_types tt
JOIN sys.columns c ON c.[object_id] = tt.[type_table_object_id]
WHERE tt.[name] = 'DocumentDefinitionList' AND c.[name] LIKE 'MarminAe%'
UNION ALL
SELECT 'Index', 'Documents', [name]
FROM sys.indexes
WHERE [name] = 'IX_Documents__MarminAeDocumentId'
UNION ALL
-- The parentheses matter: AND binds tighter than OR, so without them the type filter would
-- apply only to the second half and the MarminAe procedures would be reported unfiltered.
-- map.Documents is an inline table-valued function ('IF'), not a view.
SELECT 'Object', SCHEMA_NAME([schema_id]), [name]
FROM sys.objects
WHERE ([name] LIKE 'MarminAe%'
       OR [name] IN ('Documents', 'DocumentDefinitions__Save', 'DocumentDefinitions_Validate__Save',
                     'Documents_Validate__Open', 'Documents_Validate__Close'))
  AND [type] IN ('P', 'V', 'IF', 'TF')
ORDER BY [Kind], [Parent], [Name];
GO
""")

    text = "".join(out)
    with io.open(OUT, "w", encoding="utf-8-sig", newline="\r\n") as f:
        f.write(text)

    print(f"wrote {OUT} ({len(text.splitlines())} lines)")


if __name__ == "__main__":
    main()
