CREATE TABLE [dbo].[BillOfMaterials]
-- Sort of definition. It gets multiplied by a factor.
-- Exactly One debit item in it is the primary resource, defined in the header.
-- The other debit items are secondary, and they have standard values.
-- The credit items are valued according to policy (AVOC, FIFO, etc.)
-- The primary item value will also include abnormal loss
(
	[Id]				INT NOT NULL PRIMARY KEY IDENTITY,
	[Direction]			SMALLINT,-- Credit values are divided over debit resources.
	[CenterId]			INT,
	[IfrsTypeId]		INT,-- RM, WIP, FG, O/H Control, Normal Loss w/scrap, Normal loss w/o scrap, Abnormal Loss
	[AgentDefinitionId]	INT,-- inventory-custodians, needed only for completed signatures
	[ResourceId]		INT,-- Material, Labor, O/H. IF AccountType = O/H control, then this = driver resource (Labor, etc..)
	[Quantity]			DECIMAL (19,4),
	[UnitId]			INT
)
