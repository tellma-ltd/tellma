CREATE FUNCTION [dbo].[fw_TransactionEntries__Json] (
	@Json NVARCHAR(MAX)
)
RETURNS TABLE
AS
RETURN
	SELECT c.*
	FROM OPENJSON (@json) p
		CROSS APPLY OpenJson(p.value, N'$.TransactionEntries') 
		WITH (
			[Index]					INT,
			[DocumentIndex]			INT,
			[Id]					INT,
			[DocumentId]			INT,
			[LineType]				NVARCHAR (255),
			[OperationId]			INT,
			[AccountId]				INT,
			[AgentId]				INT,
			[AgentAccountId]		INT,
			[ResourceId]			INT,
			[Direction]				SMALLINT,
			[MoneyAmount]			MONEY,
			[Mass]					DECIMAL,
			[Volume]				DECIMAL,
			[Count]					DECIMAL,
			[ServiceTime]			DECIMAL,
			[ServiceCount]			DECIMAL,
			[ServiceDistance]		DECIMAL,
			[Value]					VTYPE,
			[NoteId]				NVARCHAR (255),
			[Memo]					NVARCHAR (255),
			[Reference]				NVARCHAR (255),
			[RelatedReference]		NVARCHAR (255),
			[RelatedAgentId]		INT,
			[RelatedResourceId]		INT,
			[RelatedAmount]			MONEY
		) c;