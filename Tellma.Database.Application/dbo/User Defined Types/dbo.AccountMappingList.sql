CREATE TYPE [dbo].[AccountMappingList] AS TABLE (
	[Index]					INT				PRIMARY KEY,
	[Id]					INT				NOT NULL DEFAULT 0,
	[AccountTypeId]			INT				NOT NULL,
	[CenterId]				INT,
	[ContractId]			INT,
	[ResourceId]			INT,
	[CurrencyId]			NCHAR (3),
	[AccountId]				INT
);