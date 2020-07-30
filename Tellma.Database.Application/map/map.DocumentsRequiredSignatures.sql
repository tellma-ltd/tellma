CREATE FUNCTION [map].[DocumentsRequiredSignatures] (
	@DocumentIds IdList READONLY
)
RETURNS @DocumentSignatures TABLE (
	[LineId]			INT,
	[ToState]			SMALLINT,
	[RuleType]			NVARCHAR(50),
	[RoleId]			INT,
	[UserId]			INT,
	[CustodyId]		INT,
	[LineSignatureId]	INT,
	[SignedById]		INT,
	[SignedAt]			DATETIMEOFFSET(7),
	[OnBehalfOfUserId]	INT,
	[LastUnsignedState]	SMALLINT,
	[LastNegativeState]	SMALLINT,
	[CanSign]			BIT,
	[ProxyRoleId]		INT,
	[CanSignOnBehalf]	BIT,
	[ReasonId]			INT,
	[ReasonDetails]		NVARCHAR (1024)
)
AS BEGIN
	DECLARE @LineIds IdList;
	
	INSERT INTO @LineIds([Id])
	SELECT [Id] FROM [dbo].[Lines] WHERE [DocumentId] IN (SELECT [Id] FROM @DocumentIds);

	INSERT INTO @DocumentSignatures(
		[LineId], [ToState], [RuleType], [RoleId], [UserId], [CustodyId], [LineSignatureId], [SignedById], [SignedAt], [OnBehalfOfUserId], [LastUnsignedState], [LastNegativeState], [CanSign], [ProxyRoleId], [CanSignOnBehalf], [ReasonId], [ReasonDetails])
	SELECT
		[LineId], [ToState], [RuleType], [RoleId], [UserId], [CustodyId], [LineSignatureId], [SignedById], [SignedAt], [OnBehalfOfUserId], [LastUnsignedState], [LastNegativeState], [CanSign], [ProxyRoleId], [CanSignOnBehalf], [ReasonId], [ReasonDetails]
	FROM [map].[LinesRequiredSignatures](@LineIds);
	
	RETURN;
END