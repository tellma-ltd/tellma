CREATE FUNCTION [map].[Users] ()
RETURNS TABLE
AS
RETURN (
	SELECT A.[Name], A.[Name2], A.[Name3], A.[ImageId], U.*, IIF(ExternalId IS NULL, 'New', 'Confirmed') As [State] FROM [dbo].[Users] AS U
	JOIN [dbo].[Agents] AS A ON U.[Id] = A.[Id]
)
