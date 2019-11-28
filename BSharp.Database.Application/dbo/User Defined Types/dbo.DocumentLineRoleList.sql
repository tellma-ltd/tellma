CREATE TYPE [dbo].[DocumentLineRoleList] AS TABLE -- used in bll.DocumentLines_RelevantIndexIds
(
	[DocumentLineId]	INT,
	[RoleId]			INT
);