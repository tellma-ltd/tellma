DECLARE @Accountant INT, @Comptroller INT;
INSERT INTO dbo.Roles([Name]) VALUES
(N'Accountant'),
(N'Comptroller');
SELECT @Accountant = [Id] FROM dbo.[Roles] WHERE [Name] = N'Accountant';
SELECT @Comptroller = [Id] FROM dbo.[Roles] WHERE [Name] = N'Comptroller';

INSERT INTO dbo.RoleMemberships([AgentId], [RoleId]) VALUES
(@UserId, @Accountant),
(@UserId, @Comptroller);