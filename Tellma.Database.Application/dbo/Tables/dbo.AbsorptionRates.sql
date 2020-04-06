CREATE TABLE [dbo].[AbsorptionRates]
(
	[Id]				INT NOT NULL PRIMARY KEY,
	-- For process costing, when producing output from a d
	[CostEntityId]		INT,
	[Quantity]			INT
)
