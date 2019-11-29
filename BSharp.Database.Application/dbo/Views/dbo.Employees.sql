CREATE VIEW [dbo].[Employees]
AS
SELECT 
--TitleId, 
[Name] As [Full Name], 
-- CONVERT(NVARCHAR (255), BirthDate, 104) As DOB, 
IsActive As [Active ?], TaxIdentificationNumber As TIN
--, Gender
FROM [dbo].[Agents]
WHERE [DefinitionId] = N'employees'
AND [IsActive] = 1;