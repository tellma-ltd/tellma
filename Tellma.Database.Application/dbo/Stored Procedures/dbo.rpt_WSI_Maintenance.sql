CREATE PROCEDURE [dbo].[rpt_WSI_Maintenance]
/*
Assumptions:
The resource for expense account is the resource we spent the money on. It is not the resource we actually spent
unless it is the direct cost of selling an item.
There are as many accounts for Maintenance as there are maintenance units, or conversion points.
Alternatively, we define an org unit for every maintenance unit (we do the same for production department
and production units)
*/
	@fromDate Datetime = '01.01.2020',
	@toDate Datetime = '01.01.2020'
AS
	WITH
	MaintenanceJournal AS (
		SELECT AccountId, COUNT(DISTINCT ResourceId) MachineCount, SUM([Direction] * [Value]) AS [Expense]
		FROM [dbo].[fi_Journal](@fromDate, @toDate)
		WHERE NoteId  = N'RepairsAndMaintenance'
	)
	SELECT A.Code, A.[Name], A.[Name2], MJ.MachineCount, MJ.Expense
	FROM MaintenanceJournal MJ
	JOIN dbo.Accounts A ON MJ.AccountId = A.Id
	GROUP BY A.Code, A.[Name], A.[Name2]