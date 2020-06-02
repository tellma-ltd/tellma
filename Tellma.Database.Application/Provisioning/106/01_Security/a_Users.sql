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
(15, N'Mohamad Akra', N'Mohamad Akra', N'mohamad.akra@tellma.com'),
(16, N'Ahmad Akra', N'Ahmad Akra', N'ahmad.akra@tellma.com');
DELETE FROM @Users WHERE [Email] IN (SELECT [Email] FROM dbo.Users); -- in case admin is also in the list

EXEC [dal].[Users__Save]
	@Entities = @Users

DECLARE @106DerejeMulat INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'dereje1@soreti.net');
DECLARE @106BulbulaTulle INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'bulbula1@soreti.net');
DECLARE @106DammaSheko INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'demma1@soreti.net');
DECLARE @106TujarKassim INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'tujar1@soreti.net');
DECLARE @106BirhanuTakele INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'birhanu1@soreti.net');
DECLARE @106WakeGizaw INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'wakeyeyab@gmail.com');
DECLARE @106AmanuelBayissa INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'amanuelbayisa64@gmail.com');
DECLARE @106GaddisaDemise INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'gadisademissie51@gmail.com');
DECLARE @106GetanehAseb INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'asabegetaneh@gmail.com');
DECLARE @106LalisoGemechu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'lelisogem2017@gmail.com');
DECLARE @106KeliliKoreso INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'kelilkorso2004@gmail.com');
DECLARE @106AbuBakrelHadi INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abubakr.elhadi@banan-it.com');
DECLARE @106AbrahamTenker INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abrham.Tenker@banan-it.com');
DECLARE @106MosabelHafiz INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mosab.elhafiz@banan-it.com');
DECLARE @106YisakFikadu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'yisak.fikadu@banan-it.com');
DECLARE @106MohamadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mohamad.akra@tellma.com');
DECLARE @106AhmadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'ahmad.akra@tellma.com');

--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106DerejeMulat
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106BulbulaTulle
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106DammaSheko
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106TujarKassim
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106BirhanuTakele
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106WakeGizaw
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AmanuelBayissa
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106GaddisaDemise
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106GetanehAseb
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106LalisoGemechu
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106KeliliKoreso
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AbuBakrelHadi
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AbrahamTenker
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106MosabelHafiz
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106YisakFikadu
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106MohamadAkra
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AhmadAkra