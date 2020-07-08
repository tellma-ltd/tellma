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

