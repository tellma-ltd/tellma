DECLARE @WorkflowId INT;
DECLARE @Workflows dbo.[WorkflowList];
DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

INSERT INTO @Workflows([Index],
[LineDefinitionId], FromState, ToState) Values
--(N'ManualLine',	N'Draft',	N'Reviewed');
(0, N'ManualLine',		0,			+4);

--IF @DB = N'101' -- Banan SD, USD, en

--IF @DB = N'102' -- Banan ET, ETB, en

--IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh


IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RoleId])
	SELECT 0, 0, [Id] FROM dbo.Roles WHERE [Code] = N'AE';