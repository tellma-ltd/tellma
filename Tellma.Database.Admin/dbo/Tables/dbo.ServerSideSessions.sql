CREATE TABLE [dbo].[ServerSideSessions] (
    [Id]          BIGINT          NOT NULL IDENTITY,
    [Key]         NVARCHAR (100)  NOT NULL,
    [Scheme]      NVARCHAR (100)  NOT NULL,
    [SubjectId]   NVARCHAR (100)  NOT NULL,
    [SessionId]   NVARCHAR (100)  NULL,
    [DisplayName] NVARCHAR (100)  NULL,
    [Created]     DATETIME2       NOT NULL,
    [Renewed]     DATETIME2       NOT NULL,
    [Expires]     DATETIME2       NULL,
    [Data]        NVARCHAR (MAX)  NOT NULL,
    CONSTRAINT [PK_ServerSideSessions] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ServerSideSessions_Key]
    ON [dbo].[ServerSideSessions]([Key] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_SubjectId]
    ON [dbo].[ServerSideSessions]([SubjectId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_SessionId]
    ON [dbo].[ServerSideSessions]([SessionId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_DisplayName]
    ON [dbo].[ServerSideSessions]([DisplayName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ServerSideSessions_Expires]
    ON [dbo].[ServerSideSessions]([Expires] ASC);
