CREATE FUNCTION [bll].[ft_ValidPeriodEntriesDetails]
(
-- used in Generate and Preprocess scripts for time-based agreements and events
	@ETAccountTypeConcept NVARCHAR (255),
	@EEAccountTypeConcept NVARCHAR (255),
	@AgentDefinitionCode NVARCHAR (255),
	@ResourceDefinitionCode NVARCHAR (255),
	@NotedResourceDefinitionCode NVARCHAR (255),
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @result TABLE (
	[CenterId] INT,
	[AgentId] INT,
	[Quantity]	DECIMAL (19,4),
	[MonetaryValue] DECIMAL (19,4),
	[ValidFrom] DATE,
	[ValidTill] Date,
	[NotedResourceId] INT
)
AS
BEGIN
	
	RETURN
END
GO