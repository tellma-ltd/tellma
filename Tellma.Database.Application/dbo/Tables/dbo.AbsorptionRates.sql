CREATE TABLE [dbo].[AbsorptionRates]
(
	[Id]				INT CONSTRAINT [PK_AbsorptionRates] PRIMARY KEY IDENTITY,
	-- For process costing, when producing output from a d
	[CostEntityId]		INT,
	[Quantity]			INT
)
