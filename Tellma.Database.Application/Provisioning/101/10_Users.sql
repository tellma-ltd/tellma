INSERT INTO @Users
	([Index],	[Name],				[Name2],				[Email]) VALUES
	(0,			N'Jiad Akra',		N'جياد عكره',			N'jiad.akra@banan-it.com'),
	(1,			N'elAmin alTayyib',	N'الأمين الطيب',		N'elamin.altayeb@ebanan.com'),
	(2,			N'Mohamad Akra',	N'محمد عكره',			N'mohamad.akra@tellma.com'),
	(3,			N'Ahmad Abdussalam',N'أحمد عبد السلام',		N'Elhelalaby1@gmail.com'),
	(4,			N'Alaeldin Ismail',	N'علاء الدين اسماعيل',	N'alaeldin.ismail@ebanan.com'),
	(5,			N'Omer al-Samani',	N'عمر السماني',		N'omer.alsamani@ebanan.com'),
	(6,			N'Ahmad Akra',		N'أحمد عكره',			N'ahmad.akra@tellma.com');

DELETE FROM @Users WHERE [Email] IN (SELECT [Email] FROM dbo.Users); -- in case admin was in the list
EXEC [dal].[Users__Save]
	@Entities = @Users

DECLARE @Jiad_akra INT, @amtaam INT, @mohamad_akra INT, @aasalam INT, @alaeldin INT, @omer INT;
SELECT @Jiad_akra = [Id] FROM dbo.Users WHERE [Email] = N'jiad.akra@banan-it.com';
SELECT @amtaam = [Id] FROM dbo.Users WHERE [Email] = N'elamin.altayeb@ebanan.com';
SELECT @mohamad_akra = [Id] FROM dbo.Users WHERE [Email] = N'mohamad.akra@tellma.com';
SELECT @aasalam = [Id] FROM dbo.Users WHERE [Email] = N'Elhelalaby1@gmail.com';
SELECT @alaeldin = [Id] FROM dbo.Users WHERE [Email] = N'alaeldin.ismail@ebanan.com';
SELECT @omer = [Id] FROM dbo.Users WHERE [Email] = N'omer.alsamani@ebanan.com';

IF @DB = N'101' AND HOST_NAME() = N'TRUSTED' --	AZURE
BEGIN
	update dbo.Users Set ImageId = N'12bd9b52-3166-4e4c-a352-c0aced6dfb99' WHERE [Id] = @mohamad_akra
	update dbo.Users Set ImageId = N'0299190d-b960-457b-bd83-c12aaf1fb138' WHERE [Id] = @Jiad_akra
	update dbo.Users Set ImageId = N'91484b38-f69f-4ec0-a59a-a9d2bd2d71c2' WHERE [Id] = @amtaam
	update dbo.Users Set ImageId = N'5fd77b73-ac4c-41ea-8907-9333dae6eeb6' WHERE [Id] = @alaeldin
	update dbo.Users Set ImageId = N'1359eccb-bf0a-428a-833a-ff9c0a7be6b6' WHERE Email = N'ahmad.akra@tellma.com'
END