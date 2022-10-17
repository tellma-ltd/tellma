CREATE FUNCTION [dal].[fn_Employee_Deduction__ExpiryDate] (
	@EmployeeId INT,
	@TaxDepartmentId INT
)
RETURNS DATE
AS BEGIN
	DECLARE @Result DATE
	DECLARE @NewDeductionsLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayables.M');
	DECLARE @AmendedDeductionsLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayablesAmended.M');
	DECLARE @TerminatedDeductionsLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');
	
	SELECT @Result = MAX(ISNULL(E.[Time2], DATEADD(DAY, -1, E.[Time1])))
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE L.DefinitionId IN (@NewDeductionsLD, @AmendedDeductionsLD, @TerminatedDeductionsLD)
	AND L.EmployeeId = @EmployeeId
	AND E.[AgentId] = @TaxDepartmentId
	AND E.[Value] <> 0
	AND L.[State] = 2
	AND E.[Index] = 1

	RETURN @Result
END
GO