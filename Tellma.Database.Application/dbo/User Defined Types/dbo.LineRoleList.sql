CREATE TYPE [dbo].[LineRoleList] AS TABLE -- used in bll.DLines_RelevantIndexIds
(
	[LineId]	INT,
	[RoleId]			INT
);