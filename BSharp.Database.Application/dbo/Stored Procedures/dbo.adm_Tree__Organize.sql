CREATE PROCEDURE [dbo].[adm_Tree__Organize]
AS
/*
	The following code takes a hierarchy table and reassigns the value to the nodes so that it looks
	as if it was filled from scratch. It is similar to organizing an index that has undergone lots of 
	fragmentation due to deletions and insertions.
	It may be useful to run before inserting a huge number of nodes in an already fragmented tree, 
	assuming we have an optimal recusrive code for this insertion.
*/
-- @Entities is only used for illustration. Usually, it is replaced with the actual db table name.
	DECLARE @Entities TABLE (
		[Id] INT DEFAULT 0,
		[Node] HIERARCHYID,
		[Name] NVARCHAR(255),
		[Code] NVARCHAR(255)
	);
	INSERT INTO @Entities (
	[Name],						[Node],			[Code]) VALUES
	(N'Hollow Section Product',		'/1/',			N'1'),
	(N'Circular Hollow Section',		'/1/1/',		N'11'),
	(N'Rectangular Hollow Section',	'/1/2/',		N'12'),
	(N'Square Hollow Section',		'/1/3/',		N'13'),
	(N'LTZ Products',				'/3/',			N'2'),
	(N'L Bars',						'/3/1/',		N'21'),
	(N'T Bars',						'/3/2/',		N'22'),
	(N'Z Bars',						'/3/3/',		N'23'),
	(N'Sheet Metals',				'/4/',			N'3');

	--SELECT  *, [Node].ToString() As [Path] FROM @Entities;-- ORDER BY [Node].GetLevel(), [Node];

	WITH Children ([Id], [ParentId], [Num]) AS (
		-- Use the commented line if you want the tree natural sorting to follow the Code structure
		--SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id], E1.[Code])   
SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id])   
		FROM @Entities E
		LEFT JOIN @Entities E2 ON E.[Node].GetAncestor(1) = E2.[Node]
	),
	Paths ([Node], [Id]) AS (  
		-- This section provides the value for the roots of the hierarchy  
		SELECT CAST(('/'  + CAST(C.Num AS VARCHAR(30)) + '/') AS HIERARCHYID) AS [Node], [Id]
		FROM Children AS C   
		WHERE [ParentId] IS NULL
		UNION ALL   
		-- This section provides values for all nodes except the root  
		SELECT CAST(P.[Node].ToString() + CAST(C.Num AS VARCHAR(30)) + '/' AS HIERARCHYID), C.[Id]
		FROM Children C
		JOIN Paths P ON C.[ParentId] = P.[Id]
	)
	MERGE INTO @Entities As t
	USING Paths As s ON (t.[Id] = s.[Id] AND t.[Node] <> s.[Node])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];
	
	--SELECT  *, [Node].ToString() As [Path] FROM @Entities;-- ORDER BY [Node].GetLevel(), [Node];