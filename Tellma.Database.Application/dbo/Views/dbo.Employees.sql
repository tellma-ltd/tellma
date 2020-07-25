CREATE VIEW [dbo].[Employees]
AS
SELECT 
--TitleId, 
[Name] As [Full Name], 
-- CONVERT(NVARCHAR (255), BirthDate, 104) As DOB, 
IsActive As [Active ?], TaxIdentificationNumber As TIN
--, Gender
FROM [dbo].[Relations]
WHERE [DefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'employees')
AND [IsActive] = 1;