CREATE FUNCTION [dbo].[fw_TransactionEntries__Json] (
	@Json NVARCHAR(MAX)
)
RETURNS TABLE
AS
RETURN
	SELECT c.*
	FROM OPENJSON (@Json) p
		CROSS APPLY OpenJson(p.value, N'$.Entries') 
		WITH (
			[Index]					INT,
			[DocumentIndex]			INT,
			[Id]					INT,
			[DocumentId]			INT,
			[LineType]				NVARCHAR (255),
			[OperationId]			INT,
			[AccountId]				INT,
			[ResourceId]			INT,
			[CustodyId]				INT,
			[Direction]				SMALLINT,
			[Amount]				DECIMAL (19,4),
			[Mass]					DECIMAL,
			[Volume]				DECIMAL,
			[Count]					DECIMAL,
			[ServiceTime]			DECIMAL,
			[ServiceCount]			DECIMAL,
			[ServiceDistance]		DECIMAL,
			[Value]					DECIMAL (19,4),
			[NoteId]				NVARCHAR (255),
			[Memo]					NVARCHAR (255),
			[Reference]				NVARCHAR (255),
			[RelatedReference]		NVARCHAR (255),
			[RelatedResourceId]		INT,
			[RelatedAmount]			DECIMAL (19,4)
		) c;