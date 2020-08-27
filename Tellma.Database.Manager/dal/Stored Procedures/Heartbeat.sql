CREATE PROCEDURE [dal].[Heartbeat]
	@InstanceId UNIQUEIDENTIFIER,
	@KeepAliveInSeconds INT
AS
	DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();
	DECLARE @UpdatedId UNIQUEIDENTIFIER;

	-- Rejuvenate the current instance
	UPDATE [dbo].[Instances]
	SET [LastHeartbeat] = @Now, @UpdatedId = [Id]
	WHERE [Id] = @InstanceId

	-- Or create it if it doesn't exist
	IF (@UpdatedId IS NULL)
		INSERT INTO [dbo].[Instances] ([Id], [LastHeartbeat])
		VALUES (@InstanceId, @Now);

	-- Delete "dead" instances
	DELETE FROM [dbo].[Instances] 
	WHERE DATEDIFF(second, [LastHeartbeat], @Now) > @KeepAliveInSeconds
