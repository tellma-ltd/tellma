CREATE PROCEDURE dal.Documents__CarryForward
@lineDefinitionId int,
@terminatedAfter date = null
AS
BEGIN
	declare @archiveDate date = (select top 1 archivedate from dbo.Settings);

	update dbo.Lines
	Set
		PostingDate  = DATEADD(DAY, 1, @archiveDate)
	WHERE Id in (
		select distinct l.Id from 
		dbo.Entries e
		join dbo.Lines l on l.Id = e.lineId
		where l.DefinitionId = @lineDefinitionId
		and l.PostingDate <= @archiveDate
		and e.Time2 > isnull(@terminatedAfter, getdate())
		and l.[State] >= 0
	)

	update dbo.Documents
	set [PostingDate] = DATEADD(DAY, 1, @archiveDate)
	where Id in (
		select l.[DocumentId] from dbo.Entries e
		join dbo.Lines l on l.Id = e.LineId
		where l.[DefinitionId] = @lineDefinitionId
		and l.[PostingDate] = DATEADD(DAY, 1, @archiveDate)
		and e.[Time2] > isnull(@terminatedAfter, getdate())
		and l.[State] >= 0
	)
END