CREATE PROCEDURE [bll].[DashboardDefinitions_Validate__Save]
	@Entities [dbo].[DashboardDefinitionList] READONLY,
	@Widgets [dbo].[DashboardDefinitionWidgetList] READONLY,
	@Roles [dbo].[DashboardDefinitionRoleList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: No inactive role
	---- Cannot assign an inactive center
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT DISTINCT TOP(@Top)
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CenterId',
	--	N'Error_TheCenter0IsInactive',
	--	dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3])
	--FROM @Entities FE
	--JOIN dbo.Centers C ON FE.CenterId = C.Id
	--WHERE C.IsActive = 0

	SELECT TOP (@Top) * FROM @ValidationErrors;