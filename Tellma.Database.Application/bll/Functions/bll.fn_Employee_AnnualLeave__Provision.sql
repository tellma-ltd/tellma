CREATE FUNCTION [bll].[fn_Employee_AnnualLeave__Provision]
(
	@EmployeeIds IdList READONLY,
	@AsOfDate DATE
)
RETURNS @Result TABLE (
	[EmployeeId] INT PRIMARY KEY,
	[BasicCurrencyId] NCHAR (3),
	[FromDate]	DATE,
	[ToDate] DATE,
	[ServiceDaysLost] INT,
	[Provision] DECIMAL (19, 6),
	[Quantity] DECIMAL (19, 6),
	[Years] INT,
	[Months] INT,
	[Days] INT,
	[Salary] DECIMAL (19, 6),
	[CenterId] INT,
	[AgentId] INT,
	[NotedResourceId] INT,
	[EntryTypeId] INT
)
AS
BEGIN
	DECLARE @CountryId NCHAR (2) = dal.fn_Settings__Country();
		--WHEN @CountryId = N'SD' THEN
		--	bll.fn_Employee_EOS__Provision_SD(@EmployeeIds, @AsOfDate)
		--WHEN @CountryId = N'BA' THEN
		--	bll.fn_Employee_EOS__Provision_BA(@EmployeeIds, @AsOfDate)
		IF @CountryId = N'AE'
			INSERT INTO @Result SELECT * FROM bll.fn_Employee_AnnualLeave__Provision_AE(@EmployeeIds, @AsOfDate) WHERE [Provision] IS NOT NULL;
		IF @CountryId = N'ET'
			INSERT INTO @Result
			SELECT [EmployeeId], [BasicCurrencyId], [FromDate], [ToDate], [ServiceDaysLost], [Provision],
					[Quantity],	[Years], [Months], [Days], [Salary], [CenterId], [AgentId],	[NotedResourceId],
					[EntryTypeId]
			FROM bll.fn_Employee_AnnualLeave__Provision_ET(@EmployeeIds, @AsOfDate) WHERE [Provision] IS NOT NULL;
		IF @CountryId = N'LB'
			INSERT INTO @Result SELECT * FROM bll.fn_Employee_AnnualLeave__Provision_LB(@EmployeeIds, @AsOfDate) WHERE [Provision] IS NOT NULL;
		IF @CountryId = N'SA'
			INSERT INTO @Result SELECT * FROM bll.fn_Employee_AnnualLeave__Provision_SA(@EmployeeIds, @AsOfDate) WHERE [Provision] IS NOT NULL;
	RETURN;
END
GO