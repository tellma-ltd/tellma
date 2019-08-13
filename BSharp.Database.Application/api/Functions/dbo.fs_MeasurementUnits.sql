CREATE FUNCTION [dbo].[fs_MeasurementUnits] ()
RETURNS TABLE
AS 
RETURN
	SELECT MU.Code, MU.[Name], MU.[Description], MU.BaseAmount, MU.IsActive, 
	LUC.[Name] AS CreatedBy, MU.CreatedAt, LUM.[Name] AS ModifiedBy, MU.ModifiedAt
	FROM [dbo].MeasurementUnits MU
	JOIN dbo.[Agents] LUC ON MU.CreatedById = LUC.Id
	JOIN dbo.[Agents] LUM ON MU.ModifiedById = LUM.Id;