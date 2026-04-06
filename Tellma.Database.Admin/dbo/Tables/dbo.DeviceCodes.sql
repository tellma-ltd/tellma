CREATE TABLE [dbo].[DeviceCodes] (
    [UserCode]     NVARCHAR (200) NOT NULL,
    [DeviceCode]   NVARCHAR (200) NOT NULL,
    [SubjectId]    NVARCHAR (200) NULL,
    [SessionId]    NVARCHAR (100) NULL,
    [ClientId]     NVARCHAR (200) NOT NULL,
    [Description]  NVARCHAR (200) NULL,
    [CreationTime] DATETIME2      NOT NULL,
    [Expiration]   DATETIME2      NOT NULL,
    [Data]         NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_DeviceCodes] PRIMARY KEY CLUSTERED ([UserCode] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DeviceCodes_DeviceCode]
    ON [dbo].[DeviceCodes]([DeviceCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_DeviceCodes_Expiration]
    ON [dbo].[DeviceCodes]([Expiration] ASC);
