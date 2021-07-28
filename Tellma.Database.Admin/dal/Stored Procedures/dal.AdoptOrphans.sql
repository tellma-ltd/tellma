CREATE PROCEDURE [dal].[AdoptOrphans]
	@InstanceId UNIQUEIDENTIFIER,
	@KeepAliveInSeconds INT,
	@OrphanCount INT
AS
	-- Just in case the instance executes [AdoptOrphans] before [Heartbeat] for the very first time, and someone else snatches the same orphans
	EXEC [dal].[Heartbeat] @InstanceId = @InstanceId, @KeepAliveInSeconds = @KeepAliveInSeconds;

	-- Adopt the top N orphans
	-- TODO: can the default isolation level cause two instances to retrieve the same orphans? That would be dangerous
	UPDATE TOP (@OrphanCount) DB
	SET [AdopterId] = @InstanceId
	OUTPUT INSERTED.Id
	FROM [dbo].[SqlDatabases] DB 
	LEFT JOIN [dbo].[Instances] I ON DB.[AdopterId] = I.[Id]
	WHERE I.[Id] IS NULL OR DATEDIFF(second, I.[LastHeartbeat], SYSDATETIMEOFFSET()) > @KeepAliveInSeconds -- Criteria for orphan
