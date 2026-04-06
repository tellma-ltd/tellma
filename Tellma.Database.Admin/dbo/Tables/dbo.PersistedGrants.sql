CREATE TABLE [dbo].[PersistedGrants] (
    [Id]             BIGINT         NOT NULL IDENTITY,
    [Key]            NVARCHAR (200) NULL,
    [Type]           NVARCHAR (50)  NOT NULL,
    [SubjectId]      NVARCHAR (200) NULL,
    [SessionId]      NVARCHAR (100) NULL,
    [ClientId]       NVARCHAR (200) NOT NULL,
    [Description]    NVARCHAR (200) NULL,
    [CreationTime]   DATETIME2      NOT NULL,
    [Expiration]     DATETIME2      NULL,
    [ConsumedTime]   DATETIME2      NULL,
    [Data]           NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_PersistedGrants] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PersistedGrants_Key]
    ON [dbo].[PersistedGrants]([Key] ASC) WHERE ([Key] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_PersistedGrants_SubjectId_ClientId_Type]
    ON [dbo].[PersistedGrants]([SubjectId] ASC, [ClientId] ASC, [Type] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PersistedGrants_SubjectId_SessionId_Type]
    ON [dbo].[PersistedGrants]([SubjectId] ASC, [SessionId] ASC, [Type] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PersistedGrants_Expiration]
    ON [dbo].[PersistedGrants]([Expiration] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PersistedGrants_ConsumedTime]
    ON [dbo].[PersistedGrants]([ConsumedTime] ASC);
