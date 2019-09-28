CREATE VIEW [dbo].[Employees]
AS
SELECT TitleId, [Name] As [Full Name], CONVERT(NVARCHAR (255), BirthDate, 104) As DOB, IsActive As [Active ?], TaxIdentificationNumber As TIN, Gender
FROM [dbo].[Agents]
WHERE [AgentType] = N'Individual'
AND [Id] IN (
	SELECT [AgentId]
	FROM dbo.[AgentRelations]
	WHERE [AgentRelationDefinitionId] = N'employee'
	AND [IsActive] = 1
);