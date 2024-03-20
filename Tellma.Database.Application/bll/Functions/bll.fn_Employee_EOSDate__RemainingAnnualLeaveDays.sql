CREATE FUNCTION [bll].[fn_Employee_EOSDate__RemainingAnnualLeaveDays]
(
	@EmployeeId INT,
	@EndOfServiceDate DATE,
	@YearlyAccrual INT = 21,
	@InactiveDays INT = 0
)
RETURNS DECIMAL (19, 6)
BEGIN
	DECLARE @CountryId NCHAR(2) = dal.fn_Settings__Country();
	DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();

	DECLARE @JoiningDate DATE = dal.fn_Agent__FromDate(@EmployeeId);

	DECLARE @AnnualLeaveRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'AnnualLeave');
	DECLARE @TotalProvisioned DECIMAL (19, 6), @TotalAccruedLeaveDays DECIMAL (19, 6),	@AdditionalDays DECIMAL (19, 6)

	SELECT	@TotalProvisioned = SUM(IIF(E.[Direction] = +1, E.[Quantity], 0))
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	WHERE L.[State] = 4
	AND AC.[Concept] = N'CurrentProvisionsForEmployeeBenefits'
	AND E.[AgentId] = @EmployeeId
	AND E.[ResourceId] = @AnnualLeaveRS;
--	AND L.[PostingDate] <= @EndOfServiceDate; MA 2024-03-09. May include adjustments from a later date.

	SELECT @TotalAccruedLeaveDays = dbo.fn_ActiveDates__AccruedLeaveDays(@JoiningDate, @EndOfServiceDate, @YearlyAccrual, @InactiveDays);
	SELECT @AdditionalDays = ISNULL(@TotalAccruedLeaveDays, 0) - ISNULL(@TotalProvisioned, 0)

	RETURN @AdditionalDays
END
GO