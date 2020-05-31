DELETE FROM @Users;

IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @Users
	([Index],	[Name],				[Name2],				[Email]) VALUES
	(0,			N'Jiad Akra',		N'جياد عكره',			N'jiad.akra@banan-it.com'),
	(1,			N'elAmin alTayyib',	N'الأمين الطيب',		N'elamin.altayeb@ebanan.com'),
	(2,			N'Mohamad Akra',	N'محمد عكره',			N'mohamad.akra@tellma.com'),
	(3,			N'Ahmad Abdussalam',N'أحمد عبد السلام',		N'Elhelalaby1@gmail.com'),
	(4,			N'Alaeldin Ismail',	N'علاء الدين اسماعيل',	N'alaeldin.ismail@ebanan.com'),
	(5,			N'Omer al-Samani',	N'عمر السماني',		N'omer.alsamani@ebanan.com'),
	(6,			N'Ahmad Akra',		N'أحمد عكره',			N'ahmad.akra@tellma.com');

IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @Users
	([Index],	[Name],					[Email]) VALUES
	(0,			N'Jiad Akra',			N'jiad.akra@banan-it.com'),
	(1,			N'Mohamad Akra',		N'mohamad.akra@tellma.com'),
	(2,			N'Ahmad Akra',			N'ahmad.akra@banan-it.com'),
	(3,			N'Yisak Fikadu',		N'yisakfikadu79@gmail.com'),
	(4,			N'Abrham Tenker',		N'abrham.tenker@banan-it.com'),
	(5,			N'Wondewsen Semaneh',	N'wendylulu99@gmail.com');
IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	INSERT INTO @Users
	([Index],[Name],			[Name2],		[Email]) VALUES
	(0,		N'Salman Al-Juhani',N'萨尔曼·朱哈尼（Salman Al-Juhani)',	N'salman.aljuhani@lifan.com'),
	(1,		N'Sami Shubaily',	N'萨米·希比利（Sami Shibili)',		N'sami.shubaily@lifan.com');
IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Users
	([Index],	[Name],						[Email]) VALUES
	(0,			N'Badege Kebede',			N'badege.kebede@gmail.com'),
	(1,			N'Mesfin Wolde',			N'mesfinwolde47@gmail.com'),
	(2,			N'Ashenafi Fantahun',		N'ashenafi935@gmail.com'),
	(3,			N'Ayelech Hora',			N'ayelech.hora@gmail.com'),
	(4,			N'Tizita Nigussie',			N'tizitanigussie@gmail.com'),
	(5,			N'Natnael Giragn',			N'natnaelgiragn340@gmail.com'),
	(6,			N'Sara Birhanu',			N'sarabirhanuk@gmail.com'),
	(7,			N'Sisay Tesfaye Bekele',	N'sisay41@yahoo.com'),
	(8,			N'Tigist Negash',			N'tigistnegash74@gmail.com'),
	(9,			N'Yisak Fikadu',			N'yisakfikadu79@gmail.com'),
	(10,		N'Zewdinesh Hora',			N'zewdnesh.hora@gmail.com'),
	(11,		N'Mestawet G/Egziyabhare',	N'mestawetezige@gmail.com'),
	(12,		N'Belay Abagero',			N'belayabagero07@gmail.com'),
	(13,		N'Kalkidan Asemamaw',		N'kasmamaw5@gmail.com');
IF @DB = N'105' -- Simpex, SAR, en/ar
	INSERT INTO @Users
	([Index],[Name],			[Name2],		[Email]) VALUES
	(0,		N'Nazih Kalo',		N'نزيه كالو',	N'nazih.kalo@simpex.co.sa'),
	(1,		N'Mahdi Mrad',		N'مهدي مراد',	N'mahdi.mrad@simpex.co.sa'),
	(2,		N'Hisham Saqour',	N'هشام صقور',	N'hisham.saqour@simpex.co.sa'),
	(3,		N'Tareq Fakhrani',	N'طارق فخراني',N'tareq.Fakhrani@simpex.co.sa'),
	(4,		N'Mazen',			N'مازن مراد',	N'mazen.mrad@simpex.co.sa')	
	;
IF @DB = N'106' -- Soreti, ETB, en/am
	INSERT INTO @Users([Index],[Name], [Name2], [Email]) VALUES
	(0, N'Dereje Mulat', N'ደረጀ ሙላት', N'dereje1@soreti.net'),
	(1, N'Bulbula Tulle', N'ቡልቡላ ቱሌ', N'bulbula1@soreti.net'),
	(2, N'Damma Sheko', N'ደማ ሸኮ', N'demma1@soreti.net'),
	(3, N'Tujar Kassim', N'ቱጃር ቃሲም', N'tujar1@soreti.net'),
	(4, N'Birhanu Takele', N'ብርሃኑ ተክሌ', N'birhanu1@soreti.net'),
	(5, N'Wake Gizaw', N'ዋቄ ግዛዉ', N'wakeyeyab@gmail.com'),
	(6, N'Amanuel Bayissa', N'አማንኤል ባይሳ', N'amanuelbayisa64@gmail.com'),
	(7, N'Gaddisa Demise', N'ጋዲሳ ደምሴ', N'gadisademissie51@gmail.com'),
	(8, N'Getaneh Aseb', N'ጌታነህ አሰበ', N'asabegetaneh@gmail.com'),
	(9, N'Laliso Gemechu', N'ሌሊሶ ገመቹ', N'lelisogem2017@gmail.com'),
	(10, N'Kelili Koreso', N'ከሊል ቀርሶ', N'kelilkorso2004@gmail.com'),
	(11, N'Abu Bakr elHadi', N'Abu Bakr elHadi', N'abubakr.elhadi@banan-it.com'),
	(12, N'Abraham Tenker', N'Abraham Tenker', N'abrham.Tenker@banan-it.com'),
	(13, N'Mosab elHafiz', N'Mosab elHafiz', N'mosab.elhafiz@banan-it.com'),
	(14, N'Yisak Fikadu', N'Yisak Fikadu', N'yisak.fikadu@banan-it.com'),
	(15, N'Mohamad Akra', N'Mohamad Akra', N'mohamad.akra@banan-it.com'),
	(16, N'Ahmad Akra', N'Ahmad Akra', N'ahmad.akra@tellma.com');


DELETE FROM @Users WHERE [Email] IN (SELECT [Email] FROM dbo.Users);
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