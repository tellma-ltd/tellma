/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/

DECLARE @CustomClassifications dbo.CustomClassificationList;
Declare @Assets_AC INT, @CurrentAssets_AC INT, @BankAndCash_AC INT, @Debtors_AC INT, @Inventories_AC INT, @NonCurrentAssets_AC INT,
	@Liabilities_AC INT, @Equity_AC INT, @Revenue_AC INT, @Expenses_AC INT;
	   
INSERT INTO @CustomClassifications([Index],
[Name],						[Code], [ParentIndex]) VALUES
(0,	N'Assets',				N'1',	NULL),
(1, N'Current Assets',		N'11',	0),
(2, N'Bank and Cash',		N'111',	1),
(3, N'Debtors',				N'112',	1),
(4, N'Inventory',			N'113', 1),
(5, N'Non-current Assets',	N'12',	0),
(6, N'Liabilities',			N'2',	NULL),
(7, N'Equity',				N'3',	NULL),
(8, N'Revenue',				N'4',	NULL),
(9, N'Expenses',			N'5',	NULL);
;
EXEC [api].[CustomClassifications__Save] --  N'cash-and-cash-equivalents',
	@Entities = @CustomClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting CustomClassifications: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

SELECT @Assets_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'1';
SELECT @CurrentAssets_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'11';
SELECT @BankAndCash_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'111';
SELECT @Debtors_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'112';
SELECT @Inventories_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'113';
SELECT @NonCurrentAssets_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'12';
SELECT @Liabilities_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'2';
SELECT @Equity_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'3';
SELECT @Revenue_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'4';
SELECT @Expenses_AC = [Id] FROM dbo.[CustomClassifications] WHERE Code = N'5';

IF @DebugCustomClassifications = 1
	SELECT
		AC.Id,
		SPACE(5 * (AC.[Node].GetLevel() - 1)) +  AC.[Name] As [Name],
		[Code],
		AC.[Node].ToString() As [Node],
		(SELECT COUNT(*) FROM [CustomClassifications] WHERE [ParentNode] = AC.[Node]) AS [ChildCount]
	FROM dbo.[CustomClassifications] AC