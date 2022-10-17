CREATE FUNCTION [dal].[fn_Employee_Benefit__ExpiryDate] (
	@EmployeeId INT,
	@EmployeeBenefitId INT
)
RETURNS DATE
AS BEGIN
	DECLARE @Result DATE
	DECLARE @NewSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M');
	DECLARE @AmendedSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M');
	DECLARE @TerminatedSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');

	SELECT @Result = MAX(ISNULL(E.[Time2], DATEADD(DAY, -1, E.[Time1])))
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE L.DefinitionId IN (@NewSalariesLD, @AmendedSalariesLD, @TerminatedSalariesLD)
	AND L.EmployeeId = @EmployeeId
	AND E.[ResourceId] = @EmployeeBenefitId
	AND E.[Value] <> 0
	AND L.[State] = 2
	AND E.[Index] = 0

	RETURN @Result
END
GO