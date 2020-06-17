INSERT INTO @Users([Index],[Name], [Name2], [Email]) VALUES
(0, N'Bulbula Tulle', N'ቡልቡላ ቱሌ', N'bulbulatulle1@gmail.com'),
(1, N'Damma Sheko', N'ደማ ሸኮ', N'dammakadir@gmail.com'),
(2, N'Tujar Kassim', N'ቱጃር ቃሲም', N'tujar2006@gmail.com'),
(3, N'Dereje Mulat', N'ደረጀ ሙላት', N'mulatderege@gmail.com'),
(4, N'Tsigereda Tekle', N'ፅጌረዳ ተክሌ', N'sitco20@gmail.com'),
(5, N'Chali Hailu', N'ጫሌ ኃይሉ', N'chalihailu@gmail.com'),
(6, N'Adamu Tola', N'አዳሙ ቶላ', N'soreti2020a@gmail.com'),
(7, N'Birhanu Takele', N'ብርሃኑ ተክሌ', N'berhan123takele@gmail.com'),
(8, N'Hirpo Eressa', N'ሂርፖ ኢሬሶ', N'hirpoeresso@gmail.com'),
(9, N'Wake Gizaw', N'ዋቄ ግዛዉ', N'wakeyeyab@gmail.com'),
(10, N'Anteneh Assefa', N'አንተነህ አሰፋ', N'yadante@2@gmail.com'),
(11, N'Laliso Gemechu', N'ሌሊሶ ገመቹ', N'lelisogem2017@gmail.com'),
(12, N'Amanuel Bayissa', N'አማንኤል ባይሳ', N'amanuelbayisa64@gmail.com'),
(13, N'Gaddisa Demise', N'ጋዲሳ ደምሴ', N'gadisademissie51@gmail.com'),
(14, N'Kelili Koreso', N'ከሊል ቀርሶ', N'kelilkorso2004@gmail.com'),
(15, N'Dinku Abera', N'ድንቁ አበራ', N'dinku2290@gmail.com'),
(16, N'Eyob Getachew', N'እዮብ ጌታቸዉ', N'eyobgetachew6077@gmail.com'),
(17, N'Tesfaye Bisewer', N'ተስፋየ ቢሰዉር', N'tesfaye.bisewer@gmail.com'),
(18, N'Getaneh Aseb', N'ጌታነህ አሰበ', N'asabegetaneh@gmail.com'),
(19, N'Abu Bakr elHadi', N'አቡበከር ኢልሃዲ', N'abubakr.elhadi@banan-it.com'),
(20, N'Abraham Tenker', N'አብርሃም ጠንክር', N'abrham.Tenker@banan-it.com'),
(21, N'Mosab elHafiz', N'ሞሳብ ኤልሃፊዝ', N'mosab.elhafiz@banan-it.com'),
(22, N'Yisak Fikadu', N'ይሳቅ ፍቃዱ', N'yisak.fikadu@banan-it.com'),
(23, N'Mohamad Akra', N'Mohamad Akra', N'mohamad.akra@tellma.com'),
(24, N'Ahmad Akra', N'አህመድ አክራ', N'ahmad.akra@tellma.com');


DELETE FROM @Users WHERE [Email] IN (SELECT [Email] FROM dbo.Users); -- in case admin is also in the list

EXEC [dal].[Users__Save]
	@Entities = @Users

-- Declaration
DECLARE @106BulbulaTulle INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'bulbulatulle1@gmail.com');
DECLARE @106DammaSheko INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'dammakadir@gmail.com');
DECLARE @106TujarKassim INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'tujar2006@gmail.com');
DECLARE @106DerejeMulat INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mulatderege@gmail.com');
DECLARE @106TsigeredaTekle INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'sitco20@gmail.com');
DECLARE @106ChaliHailu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'chalihailu@gmail.com');
DECLARE @106AdamuTola INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'soreti2020a@gmail.com');
DECLARE @106BirhanuTakele INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'berhan123takele@gmail.com');
DECLARE @106HirpoEressa INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'hirpoeresso@gmail.com');
DECLARE @106WakeGizaw INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'wakeyeyab@gmail.com');
DECLARE @106AntenehAssefa INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'yadante@2@gmail.com');
DECLARE @106LalisoGemechu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'lelisogem2017@gmail.com');
DECLARE @106AmanuelBayissa INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'amanuelbayisa64@gmail.com');
DECLARE @106GaddisaDemise INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'gadisademissie51@gmail.com');
DECLARE @106KeliliKoreso INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'kelilkorso2004@gmail.com');
DECLARE @106DinkuAbera INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'dinku2290@gmail.com');
DECLARE @106EyobGetachew INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'eyobgetachew6077@gmail.com');
DECLARE @106TesfayeBisewer INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'tesfaye.bisewer@gmail.com');
DECLARE @106GetanehAseb INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'asabegetaneh@gmail.com');
DECLARE @106AbuBakrelHadi INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abubakr.elhadi@banan-it.com');
DECLARE @106AbrahamTenker INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abrham.Tenker@banan-it.com');
DECLARE @106MosabelHafiz INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mosab.elhafiz@banan-it.com');
DECLARE @106YisakFikadu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'yisak.fikadu@banan-it.com');
DECLARE @106MohamadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mohamad.akra@tellma.com');
DECLARE @106AhmadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'ahmad.akra@tellma.com');

--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106BulbulaTulle
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106DammaSheko
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106TujarKassim
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106DerejeMulat
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106TsigeredaTekle
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106ChaliHailu
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AdamuTola
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106BirhanuTakele
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106HirpoEressa
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106WakeGizaw
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AntenehAssefa
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106LalisoGemechu
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AmanuelBayissa
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106GaddisaDemise
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106KeliliKoreso
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106DinkuAbera
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106EyobGetachew
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106TesfayeBisewer
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106GetanehAseb
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AbuBakrelHadi
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AbrahamTenker
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106MosabelHafiz
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106YisakFikadu
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106MohamadAkra
--UPDATE dbo.Users SET [ImageId] = N'' WHERE [Id] = @106AhmadAkra
