DELETE FROM @Roles;
DELETE FROM @Members;
DELETE FROM @Permissions;

INSERT INTO @Roles([Index],[Code],[Name],[Name2],[Name3],[IsPublic]) VALUES
(0, N'Administrator', N'Administrator', N'አስተዳዳሪ', NULL, 0),
(1, N'FinanceManager', N'Finance Manager', N'የፋይናንስ አስተዳዳሪ', NULL, 0),
(2, N'GeneralManager', N'General Manager', N'ሰላም ነው', NULL, 0),
(3, N'Reader', N'Reader', N'አንባቢ', NULL, 0),
(4, N'AccountManager', N'Account Manager', N'የደንበኛ መለያ አቀናባሪ', NULL, 0),
(5, N'Comptroller', N'Comptroller', N'የመለያ ኮምፒተር', NULL, 0),
(6, N'CashCustodian', N'Cashier', N'ገንዘብ ተቀባይ', NULL, 0),
(7, N'AdminAffairs', N'Admin. Affairs', N'አስተዳደራዊ ጉዳዮች', NULL, 0),
(8, N'ProductionManager', N'Production Manager', N'የምርት ሥራ አስኪያጅ', NULL, 0),
(9, N'HrManager', N'HR Manager', N'የሰው ኃይል ሥራ አስኪያጅ', NULL, 0),
(10, N'SalesManager', N'Sales Manager', N'የሽያጭ ሃላፊ', NULL, 0),
(11, N'SalesPerson', N'Sales Person', N'የሽያጭ ሰው', NULL, 0),
(12, N'InventoryCustodian', N'Inventory Custodian', N'ኢን Custስትሜንት ባለሞያ', NULL, 0),
(99, N'Public', N'Public', N'ሕዝባዊ', NULL, 1);



UPDATE FE
SET FE.[Id] = BE.[Id]
FROM @Roles FE
JOIN dbo.Roles BE ON FE.[Code] = BE.[Code]

INSERT INTO @Permissions([Index], [HeaderIndex],
--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'State', N'All'))
	[Action],	[Criteria],			[View]) VALUES
(0,0,N'All',	NULL,				N'all'),
(0,1,N'All',	NULL,				N'all'),
(0,2,N'Read',	NULL,				N'all'),
(0,3,N'All',	N'CreatedById = Me',N'documents/revenue-recognition-vouchers'),
(1,3,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
									N'documents/cash-payment-vouchers'),
(2,3,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
									N'documents/cash-receipt-vouchers'),
(0,4,N'All',	NULL,				N'documents/manual-journal-vouchers'),
(1,4,N'All',	NULL,				N'documents/cash-payment-vouchers'),
(2,4,N'All',	NULL,				N'documents/revenue-recognition-vouchers'),
(3,4,N'Read',	NULL,				N'accounts'),
(0,5,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
									N'documents/cash-payment-vouchers'),
(1,5,N'All',	N'Agent/UserId = Me or AssigneeId = Me',
									N'documents/cash-receipt-vouchers'),
(2,5,N'Update', NULL,				N'contracts/suppliers'),

(0,9,N'Read',	NULL,				N'contracts/cash-custodians'),
(1,9,N'Read',	NULL,				N'centers'),
(2,9,N'Read',	NULL,				N'currencies'),
(3,9,N'Update',	N'CreatedById = Me',N'documents/cash-payment-vouchers'),
(4,9,N'Read',	NULL,				N'resources/employee-benefits-expenses'),
(5,9,N'Read',	NULL,				N'entry-types'),
(6,9,N'Read',	NULL,				N'lookups/it-equipment-manufacturers'),
(7,9,N'Read',	NULL,				N'units'),
(8,9,N'Read',	NULL,				N'lookups/operating-systems'),
(9,9,N'Read',	NULL,				N'centers'),
(10,9,N'Read',	NULL,				N'resources/services-expenses'),
(11,9,N'Read',	NULL,				N'roles'),
(12,9,N'Read',	NULL,				N'contracts/suppliers'),
(13,9,N'Read',	NULL,				N'users');

--DELETE FROM @Roles WHERE [Name] IN (SELECT [Name] FROM dbo.Roles);
--DELETE FROM @Members WHERE [HeaderIndex] NOT IN (SELECT [Index] FROM @Roles);
--DELETE FROM @Permissions WHERE [HeaderIndex] NOT IN (SELECT [Index] FROM @Roles);
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