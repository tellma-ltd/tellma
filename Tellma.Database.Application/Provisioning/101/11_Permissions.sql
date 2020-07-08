DELETE FROM @Roles;

INSERT INTO @Roles([Index],[Id], [Code],[Name],[Name2],[Name3],[IsPublic])
SELECT [Id], [Id], [Code],[Name],[Name2],[Name3],[IsPublic]
FROM dbo.Roles;

INSERT INTO @Members
	([Index],	[HeaderIndex],		[UserId]) VALUES
	(0,			@AdministratorRL,	@AdminUserId),
	(5,			@GeneralManagerRL,	@amtaam),
	(0,			@FinanceManagerRL,	@Jiad_akra),
	(0,			@ComptrollerRL,		@alaeldin),
	(0,			@AdminAffairsRL,	@omer)

--IF @101MohamadAkra <> @AdminUserId
--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
--	VALUES(10,@AdministratorRL,@101MohamadAkra);
--IF @101AhmadAkra <> @AdminUserId
--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
--	VALUES(11,@AdministratorRL,@101AhmadAkra);

INSERT INTO @Permissions([Index], [HeaderIndex], [Id], [View], [Action], [Criteria], [Mask], [Memo])
SELECT [Id], [RoleId], [Id], [View], [Action], [Criteria], [Mask], [Memo]
FROM dbo.[Permissions];

EXEC api.Roles__Save
	@Entities = @Roles,
	@Members = @Members,
	@Permissions = @Permissions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Permissions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
/*
DECLARE @AdministratorRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Administrator');
DECLARE @FinanceManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'FinanceManager');
DECLARE @GeneralManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'GeneralManager');
DECLARE @ReaderRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Reader');
DECLARE @AccountManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AccountManager');
DECLARE @ComptrollerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Comptroller');
DECLARE @CashCustodianRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'CashCustodian');
DECLARE @AdminAffairsRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AdminAffairs');
DECLARE @ProductionManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'ProductionManager');
DECLARE @HrManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'HrManager');
DECLARE @SalesManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesManager');
DECLARE @SalesPersonRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesPerson');
DECLARE @InventoryCustodianRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'InventoryCustodian');
DECLARE @PublicRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Public');

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
DECLARE @106AbuBakerelHadi INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abubakr.elhadi@banan-it.com');
DECLARE @106AbrahamTenker INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abrham.Tenker@banan-it.com');
DECLARE @106MosabelHafiz INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mosab.elhafiz@banan-it.com');
DECLARE @106YisakFikadu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'yisak.fikadu@banan-it.com');
DECLARE @106MohamadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mohamad.akra@tellma.com');
DECLARE @106AhmadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'ahmad.akra@tellma.com');
*/