CREATE FUNCTION [bll].[fn_Employee_TerminationReason__EOS]
(
	@EmployeeId INT,
	@TerminationReasonId INT
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @CountryId NCHAR (2) = dal.fn_Settings__Country();
	RETURN CASE
		WHEN @EmployeeId IS NULL THEN NULL
		WHEN dal.fn_Agent__ToDate(@EmployeeId) IS NULL THEN NULL
		--WHEN @CountryId = N'ET' THEN
		--	bll.fn_Employee_TerminationReason__EOS_ET(@EmployeeId, @TerminationReasonId)
		--WHEN @CountryId = N'LB' THEN
			--bll.fn_Employee_TerminationReason__EOS_LB(@EmployeeId, @TerminationReasonId)
		WHEN @CountryId = N'SA' THEN
			bll.fn_Employee_TerminationReason__EOS_SA(@EmployeeId, @TerminationReasonId)
		--WHEN @CountryId = N'SD' THEN
		--	bll.fn_Employee_TerminationReason__EOS_SD(@EmployeeId, @TerminationReasonId)
		--WHEN @CountryId = N'BA' THEN
		--	bll.fn_Employee_TerminationReason__EOS_BA(@EmployeeId, @TerminationReasonId)
		WHEN @CountryId = N'AE' THEN
			bll.fn_Employee_TerminationReason__EOS_AE(@EmployeeId, @TerminationReasonId)
		ELSE 0
	END
END
GO