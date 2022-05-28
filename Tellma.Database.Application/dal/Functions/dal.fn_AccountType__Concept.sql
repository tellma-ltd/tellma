CREATE FUNCTION [dal].[fn_AccountType__Concept] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN (SELECT [Concept] FROM dbo.AccountTypes WHERE [Id] = @Id)
END