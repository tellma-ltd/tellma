CREATE TYPE [dbo].[NotificationTemplateSubscriberList] AS TABLE
(
	[Index] INT	DEFAULT 0,
	[HeaderIndex] INT DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id] INT NOT NULL DEFAULT 0,
	[AddressType] NVARCHAR (50),
	[UserId] INT,
	[Email] NVARCHAR (255), -- Template
	[Phone] NVARCHAR (50) -- Template
)
