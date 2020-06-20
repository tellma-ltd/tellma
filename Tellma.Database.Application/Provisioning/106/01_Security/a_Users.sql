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
(10, N'Anteneh Assefa', N'አንተነህ አሰፋ', N'yadante2@gmail.com'),
(11, N'Laliso Gemechu', N'ሌሊሶ ገመቹ', N'lelisogem2017@gmail.com'),
(12, N'Amanuel Bayissa', N'አማንኤል ባይሳ', N'amanuelbayisa64@gmail.com'),
(13, N'Gaddisa Demise', N'ጋዲሳ ደምሴ', N'gadisademissie51@gmail.com'),
(14, N'Kelili Koreso', N'ከሊል ቀርሶ', N'kelilkorso2004@gmail.com'),
(15, N'Dinku Abera', N'ድንቁ አበራ', N'dinku2290@gmail.com'),
(16, N'Eyob Getachew', N'እዮብ ጌታቸዉ', N'eyobgetachew6077@gmail.com'),
(17, N'Tesfaye Bisewer', N'ተስፋየ ቢሰዉር', N'tesfaye.bisewer@gmail.com'),
(18, N'Getaneh Aseb', N'ጌታነህ አሰበ', N'asabegetaneh@gmail.com'),
(19, N'Abu Bakr elHadi', N'አቡበከር ኢልሃዲ', N'abubaker.elhadi@banan-it.com'),
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
DECLARE @106AbuBakerelHadi INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abubaker.elhadi@banan-it.com');
DECLARE @106AbrahamTenker INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abrham.Tenker@banan-it.com');
DECLARE @106MosabelHafiz INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mosab.elhafiz@banan-it.com');
DECLARE @106YisakFikadu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'yisak.fikadu@banan-it.com');
DECLARE @106MohamadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mohamad.akra@tellma.com');
DECLARE @106AhmadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'ahmad.akra@tellma.com');

/*
UPDATE dbo.Users SET ImageId = N'77cc53d6-277b-4f51-be0b-2cc7a5ecb51b' WHERE [Email] = N'mohamad.akra@tellma.com'
UPDATE dbo.Users SET ImageId = N'52054fe2-68c1-438e-98c5-b313050d7f22' WHERE [Email] = N'bulbulatulle1@gmail.com'
UPDATE dbo.Users SET ImageId = N'6b4b46c2-88d6-4552-a8bd-c2b86b576d5e' WHERE [Email] = N'dammakadir@gmail.com'
UPDATE dbo.Users SET ImageId = N'b6c9dd1d-4259-4902-866a-f5a2687214c1' WHERE [Email] = N'tujar2006@gmail.com'
UPDATE dbo.Users SET ImageId = N'2ce8de61-4f27-4c43-a3a0-efdb7bdda651' WHERE [Email] = N'mulatderege@gmail.com'
UPDATE dbo.Users SET ImageId = N'5bbeefb9-fba1-419c-913a-27d58c5456bd' WHERE [Email] = N'sitco20@gmail.com'
UPDATE dbo.Users SET ImageId = N'b7c5d412-305f-4663-8c1d-7fc5e78f4757' WHERE [Email] = N'chalihailu@gmail.com'
UPDATE dbo.Users SET ImageId = N'fb410f46-de90-4e7f-ab37-6ecc26b1d9eb' WHERE [Email] = N'soreti2020a@gmail.com'
UPDATE dbo.Users SET ImageId = N'6fd07d00-3a01-47f0-ba2b-aa8cc626f339' WHERE [Email] = N'berhan123takele@gmail.com'
UPDATE dbo.Users SET ImageId = N'728f7f78-4f32-439c-ad76-e3eb2776fa81' WHERE [Email] = N'hirpoeresso@gmail.com'
UPDATE dbo.Users SET ImageId = N'de56b10f-aaa7-4276-8347-2f6eff4b0c04' WHERE [Email] = N'wakeyeyab@gmail.com'
UPDATE dbo.Users SET ImageId = N'05027101-f653-43b8-9577-7a2de395f853' WHERE [Email] = N'lelisogem2017@gmail.com'
UPDATE dbo.Users SET ImageId = N'9ae77a3e-fdef-441b-9ba8-6a54a510f9ee' WHERE [Email] = N'amanuelbayisa64@gmail.com'
UPDATE dbo.Users SET ImageId = N'ae8fd813-f2f7-45a1-b7ff-df26d8f0be84' WHERE [Email] = N'gadisademissie51@gmail.com'
UPDATE dbo.Users SET ImageId = N'c430175a-66cc-4f81-b577-f8ffa634bfd2' WHERE [Email] = N'kelilkorso2004@gmail.com'
UPDATE dbo.Users SET ImageId = N'391f34e0-92fc-40cb-ab8a-234d336c0d2a' WHERE [Email] = N'dinku2290@gmail.com'
UPDATE dbo.Users SET ImageId = N'c40ebed5-8588-44e5-be62-5237045735f3' WHERE [Email] = N'eyobgetachew6077@gmail.com'
UPDATE dbo.Users SET ImageId = N'08872688-d53e-4af2-ae21-f4f69faa0e4b' WHERE [Email] = N'tesfaye.bisewer@gmail.com'
UPDATE dbo.Users SET ImageId = N'35f59fbe-40a3-4cec-8396-d6b57ce37bcf' WHERE [Email] = N'asabegetaneh@gmail.com'
UPDATE dbo.Users SET ImageId = N'4445dce4-9ef1-4ef9-979e-9eb3cd690ba2' WHERE [Email] = N'abrham.tenker@banan-it.com'
UPDATE dbo.Users SET ImageId = N'cbc0ae05-f8f7-4384-a3ab-71c6671149a8' WHERE [Email] = N'mosab.elhafiz@banan-it.com'
UPDATE dbo.Users SET ImageId = N'1db7ab47-fec7-499c-a1a2-d31a2abcdfd0' WHERE [Email] = N'yisak.fikadu@banan-it.com'
UPDATE dbo.Users SET ImageId = N'79177e84-ca1f-4ef7-bab2-8bcd2844cc52' WHERE [Email] = N'ahmad.akra@tellma.com'
UPDATE dbo.Users SET ImageId = N'7a1a7c60-0767-4b77-9905-dbe3b6e68133' WHERE [Email] = N'abubaker.elhadi@banan-it.com'
UPDATE dbo.Users SET ImageId = N'b7697b42-eae5-4e76-989f-cd0acf6a11ca' WHERE [Email] = N'yadante2@gmail.com'
*/