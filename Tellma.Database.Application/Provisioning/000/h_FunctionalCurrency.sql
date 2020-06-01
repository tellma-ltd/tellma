
DECLARE @IsoCurrencies TABLE ([Index] INT PRIMARY KEY, [Id] NCHAR(3), [Name] NVARCHAR(60), [Description] NVARCHAR(255), E TINYINT)
INSERT INTO @ISOCurrencies VALUES
(784,N'AED',N'UAE Dirham',N'United Arab Emirates Dirham',2),
(971,N'AFN',N'Afghan afghani',N'Afghan afghani',2),
(8,N'ALL',N'Albanian lek',N'Albanian lek',2),
(51,N'AMD',N'Armenian dram',N'Armenian dram',2),
(532,N'ANG',N'Netherlands Antillean guilder',N'Netherlands Antillean guilder',2),
(973,N'AOA',N'Angolan kwanza',N'Angolan kwanza',2),
(32,N'ARS',N'Argentine peso',N'Argentine peso',2),
(36,N'AUD',N'Australian dollar',N'Australian dollar',2),
(533,N'AWG',N'Aruban florin',N'Aruban florin',2),
(944,N'AZN',N'Azerbaijani manat',N'Azerbaijani manat',2),
(977,N'BAM',N'Bosnia and Herzegovina convertible mark',N'Bosnia and Herzegovina convertible mark',2),
(52,N'BBD',N'Barbados dollar',N'Barbados dollar',2),
(50,N'BDT',N'Bangladeshi taka',N'Bangladeshi taka',2),
(975,N'BGN',N'Bulgarian lev',N'Bulgarian lev',2),
(48,N'BHD',N'Bahraini dinar',N'Bahraini dinar',3),
(108,N'BIF',N'Burundian franc',N'Burundian franc',0),
(60,N'BMD',N'Bermudian dollar',N'Bermudian dollar',2),
(96,N'BND',N'Brunei dollar',N'Brunei dollar',2),
(68,N'BOB',N'Boliviano',N'Boliviano',2),
(984,N'BOV',N'Bolivian Mvdol (funds code)',N'Bolivian Mvdol (funds code)',2),
(986,N'BRL',N'Brazilian real',N'Brazilian real',2),
(44,N'BSD',N'Bahamian dollar',N'Bahamian dollar',2),
(64,N'BTN',N'Bhutanese ngultrum',N'Bhutanese ngultrum',2),
(72,N'BWP',N'Botswana pula',N'Botswana pula',2),
(933,N'BYN',N'Belarusian ruble',N'Belarusian ruble',2),
(84,N'BZD',N'Belize dollar',N'Belize dollar',2),
(124,N'CAD',N'Canadian dollar',N'Canadian dollar',2),
(976,N'CDF',N'Congolese franc',N'Congolese franc',2),
(947,N'CHE',N'WIR Euro (complementary currency)',N'WIR Euro (complementary currency)',2),
(756,N'CHF',N'Swiss franc',N'Swiss franc',2),
(948,N'CHW',N'WIR Franc (complementary currency)',N'WIR Franc (complementary currency)',2),
(990,N'CLF',N'Unidad de Fomento (funds code)',N'Unidad de Fomento (funds code)',4),
(152,N'CLP',N'Chilean peso',N'Chilean peso',0),
(156,N'CNY',N'Yuan',N'Renminbi (Chinese) yuan',2),
(170,N'COP',N'Colombian peso',N'Colombian peso',2),
(970,N'COU',N'Unidad de Valor Real (UVR) (funds code)',N'Unidad de Valor Real (UVR) (funds code)',2),
(188,N'CRC',N'Costa Rican colon',N'Costa Rican colon',2),
(931,N'CUC',N'Cuban convertible peso',N'Cuban convertible peso',2),
(192,N'CUP',N'Cuban peso',N'Cuban peso',2),
(132,N'CVE',N'Cape Verdean escudo',N'Cape Verdean escudo',2),
(203,N'CZK',N'Czech koruna',N'Czech koruna',2),
(262,N'DJF',N'Djiboutian franc',N'Djiboutian franc',0),
(208,N'DKK',N'Danish krone',N'Danish krone',2),
(214,N'DOP',N'Dominican peso',N'Dominican peso',2),
(12,N'DZD',N'Algerian dinar',N'Algerian dinar',2),
(818,N'EGP',N'Egyptian pound',N'Egyptian pound',2),
(232,N'ERN',N'Eritrean nakfa',N'Eritrean nakfa',2),
(230,N'ETB',N'Birr',N'Ethiopian birr',2),
(978,N'EUR',N'Euro',N'Euro',2),
(242,N'FJD',N'Fiji dollar',N'Fiji dollar',2),
(238,N'FKP',N'Falkland Islands pound',N'Falkland Islands pound',2),
(826,N'GBP',N'Pound sterling',N'Pound sterling',2),
(981,N'GEL',N'Georgian lari',N'Georgian lari',2),
(936,N'GHS',N'Ghanaian cedi',N'Ghanaian cedi',2),
(292,N'GIP',N'Gibraltar pound',N'Gibraltar pound',2),
(270,N'GMD',N'Gambian dalasi',N'Gambian dalasi',2),
(324,N'GNF',N'Guinean franc',N'Guinean franc',0),
(320,N'GTQ',N'Guatemalan quetzal',N'Guatemalan quetzal',2),
(328,N'GYD',N'Guyanese dollar',N'Guyanese dollar',2),
(344,N'HKD',N'Hong Kong dollar',N'Hong Kong dollar',2),
(340,N'HNL',N'Honduran lempira',N'Honduran lempira',2),
(191,N'HRK',N'Croatian kuna',N'Croatian kuna',2),
(332,N'HTG',N'Haitian gourde',N'Haitian gourde',2),
(348,N'HUF',N'Hungarian forint',N'Hungarian forint',2),
(360,N'IDR',N'Indonesian rupiah',N'Indonesian rupiah',2),
(376,N'ILS',N'Israeli new shekel',N'Israeli new shekel',2),
(356,N'INR',N'Indian rupee',N'Indian rupee',2),
(368,N'IQD',N'Iraqi dinar',N'Iraqi dinar',3),
(364,N'IRR',N'Iranian rial',N'Iranian rial',2),
(352,N'ISK',N'Icelandic króna',N'Icelandic króna',0),
(388,N'JMD',N'Jamaican dollar',N'Jamaican dollar',2),
(400,N'JOD',N'Jordanian dinar',N'Jordanian dinar',3),
(392,N'JPY',N'Japanese yen',N'Japanese yen',0),
(404,N'KES',N'Kenyan shilling',N'Kenyan shilling',2),
(417,N'KGS',N'Kyrgyzstani som',N'Kyrgyzstani som',2),
(116,N'KHR',N'Cambodian riel',N'Cambodian riel',2),
(174,N'KMF',N'Comoro franc',N'Comoro franc',0),
(408,N'KPW',N'North Korean won',N'North Korean won',2),
(410,N'KRW',N'South Korean won',N'South Korean won',0),
(414,N'KWD',N'Kuwaiti dinar',N'Kuwaiti dinar',3),
(136,N'KYD',N'Cayman Islands dollar',N'Cayman Islands dollar',2),
(398,N'KZT',N'Kazakhstani tenge',N'Kazakhstani tenge',2),
(418,N'LAK',N'Lao kip',N'Lao kip',2),
(422,N'LBP',N'Lebanese pound',N'Lebanese pound',2),
(144,N'LKR',N'Sri Lankan rupee',N'Sri Lankan rupee',2),
(430,N'LRD',N'Liberian dollar',N'Liberian dollar',2),
(426,N'LSL',N'Lesotho loti',N'Lesotho loti',2),
(434,N'LYD',N'Libyan dinar',N'Libyan dinar',3),
(504,N'MAD',N'Moroccan dirham',N'Moroccan dirham',2),
(498,N'MDL',N'Moldovan leu',N'Moldovan leu',2),
(969,N'MGA',N'Malagasy ariary',N'Malagasy ariary',2),
(807,N'MKD',N'Macedonian denar',N'Macedonian denar',2),
(104,N'MMK',N'Myanmar kyat',N'Myanmar kyat',2),
(496,N'MNT',N'Mongolian tögrög',N'Mongolian tögrög',2),
(446,N'MOP',N'Macanese pataca',N'Macanese pataca',2),
(929,N'MRU',N'Mauritanian ouguiya',N'Mauritanian ouguiya',2),
(480,N'MUR',N'Mauritian rupee',N'Mauritian rupee',2),
(462,N'MVR',N'Maldivian rufiyaa',N'Maldivian rufiyaa',2),
(454,N'MWK',N'Malawian kwacha',N'Malawian kwacha',2),
(484,N'MXN',N'Mexican peso',N'Mexican peso',2),
(979,N'MXV',N'Mexican Unidad de Inversion (UDI) (funds code)',N'Mexican Unidad de Inversion (UDI) (funds code)',2),
(458,N'MYR',N'Malaysian ringgit',N'Malaysian ringgit',2),
(943,N'MZN',N'Mozambican metical',N'Mozambican metical',2),
(516,N'NAD',N'Namibian dollar',N'Namibian dollar',2),
(566,N'NGN',N'Nigerian naira',N'Nigerian naira',2),
(558,N'NIO',N'Nicaraguan córdoba',N'Nicaraguan córdoba',2),
(578,N'NOK',N'Norwegian krone',N'Norwegian krone',2),
(524,N'NPR',N'Nepalese rupee',N'Nepalese rupee',2),
(554,N'NZD',N'New Zealand dollar',N'New Zealand dollar',2),
(512,N'OMR',N'Omani rial',N'Omani rial',3),
(590,N'PAB',N'Panamanian balboa',N'Panamanian balboa',2),
(604,N'PEN',N'Peruvian sol',N'Peruvian sol',2),
(598,N'PGK',N'Papua New Guinean kina',N'Papua New Guinean kina',2),
(608,N'PHP',N'Philippine peso[13]',N'Philippine peso[13]',2),
(586,N'PKR',N'Pakistani rupee',N'Pakistani rupee',2),
(985,N'PLN',N'Polish złoty',N'Polish złoty',2),
(600,N'PYG',N'Paraguayan guaraní',N'Paraguayan guaraní',0),
(634,N'QAR',N'Qatari riyal',N'Qatari riyal',2),
(946,N'RON',N'Romanian leu',N'Romanian leu',2),
(941,N'RSD',N'Serbian dinar',N'Serbian dinar',2),
(643,N'RUB',N'Russian ruble',N'Russian ruble',2),
(646,N'RWF',N'Rwandan franc',N'Rwandan franc',0),
(682,N'SAR',N'KSA Riyal',N'Saudi Riyal',2),
(90,N'SBD',N'Solomon Islands dollar',N'Solomon Islands dollar',2),
(690,N'SCR',N'Seychelles rupee',N'Seychelles rupee',2),
(938,N'SDG',N'SD Pound',N'Sudanese Pound',2),
(752,N'SEK',N'Swedish krona/kronor',N'Swedish krona/kronor',2),
(702,N'SGD',N'Singapore dollar',N'Singapore dollar',2),
(654,N'SHP',N'Saint Helena pound',N'Saint Helena pound',2),
(694,N'SLL',N'Sierra Leonean leone',N'Sierra Leonean leone',2),
(706,N'SOS',N'Somali shilling',N'Somali shilling',2),
(968,N'SRD',N'Surinamese dollar',N'Surinamese dollar',2),
(728,N'SSP',N'South Sudanese pound',N'South Sudanese pound',2),
(930,N'STN',N'São Tomé and Príncipe dobra',N'São Tomé and Príncipe dobra',2),
(222,N'SVC',N'Salvadoran colón',N'Salvadoran colón',2),
(760,N'SYP',N'Syrian pound',N'Syrian pound',2),
(748,N'SZL',N'Swazi lilangeni',N'Swazi lilangeni',2),
(764,N'THB',N'Thai baht',N'Thai baht',2),
(972,N'TJS',N'Tajikistani somoni',N'Tajikistani somoni',2),
(934,N'TMT',N'Turkmenistan manat',N'Turkmenistan manat',2),
(788,N'TND',N'Tunisian dinar',N'Tunisian dinar',3),
(776,N'TOP',N'Tongan paʻanga',N'Tongan paʻanga',2),
(949,N'TRY',N'Turkish lira',N'Turkish lira',2),
(780,N'TTD',N'Trinidad and Tobago dollar',N'Trinidad and Tobago dollar',2),
(901,N'TWD',N'New Taiwan dollar',N'New Taiwan dollar',2),
(834,N'TZS',N'Tanzanian shilling',N'Tanzanian shilling',2),
(980,N'UAH',N'Ukrainian hryvnia',N'Ukrainian hryvnia',2),
(800,N'UGX',N'Ugandan shilling',N'Ugandan shilling',0),
(840,N'USD',N'US Dollar',N'United States dollar',2),
(997,N'USN',N'United States dollar (next day) (funds code)',N'United States dollar (next day) (funds code)',2),
(940,N'UYI',N'Uruguay Peso en Unidades Indexadas (URUIURUI) (funds code)',N'Uruguay Peso en Unidades Indexadas (URUIURUI) (funds code)',0),
(858,N'UYU',N'Uruguayan peso',N'Uruguayan peso',2),
(927,N'UYW',N'Unidad previsional',N'Unidad previsional',4),
(860,N'UZS',N'Uzbekistan som',N'Uzbekistan som',2),
(928,N'VES',N'Venezuelan bolívar soberano',N'Venezuelan bolívar soberano',2),
(704,N'VND',N'Vietnamese đồng',N'Vietnamese đồng',0),
(548,N'VUV',N'Vanuatu vatu',N'Vanuatu vatu',0),
(882,N'WST',N'Samoan tala',N'Samoan tala',2),
(950,N'XAF',N'CFA franc BEAC',N'CFA franc BEAC',0),
--(961,N'XAG',N'Silver (one troy ounce)',N'Silver (one troy ounce)',.),
--(959,N'XAU',N'Gold (one troy ounce)',N'Gold (one troy ounce)',.),
--(955,N'XBA',N'European Composite Unit (EURCO) (bond market unit)',N'European Composite Unit (EURCO) (bond market unit)',.),
--(956,N'XBB',N'European Monetary Unit (E.M.U.-6) (bond market unit)',N'European Monetary Unit (E.M.U.-6) (bond market unit)',.),
--(957,N'XBC',N'European Unit of Account 9 (E.U.A.-9) (bond market unit)',N'European Unit of Account 9 (E.U.A.-9) (bond market unit)',.),
--(958,N'XBD',N'European Unit of Account 17 (E.U.A.-17) (bond market unit)',N'European Unit of Account 17 (E.U.A.-17) (bond market unit)',.),
(951,N'XCD',N'East Caribbean dollar',N'East Caribbean dollar',2),
--(960,N'XDR',N'Special drawing rights',N'Special drawing rights',.),
(952,N'XOF',N'CFA franc BCEAO',N'CFA franc BCEAO',0),
--(964,N'XPD',N'Palladium (one troy ounce)',N'Palladium (one troy ounce)',.),
(953,N'XPF',N'CFP franc (franc Pacifique)',N'CFP franc (franc Pacifique)',0),
--(962,N'XPT',N'Platinum (one troy ounce)',N'Platinum (one troy ounce)',.),
--(994,N'XSU',N'SUCRE',N'SUCRE',.),
--(963,N'XTS',N'Code reserved for testing',N'Code reserved for testing',.),
--(965,N'XUA',N'ADB Unit of Account',N'ADB Unit of Account',.),
--(999,N'XXX',N'No currency',N'No currency',.),
(886,N'YER',N'Yemeni rial',N'Yemeni rial',2),
(710,N'ZAR',N'South African rand',N'South African rand',2),
(967,N'ZMW',N'Zambian kwacha',N'Zambian kwacha',2),
(932,N'ZWL',N'Zimbabwean dollar',N'Zimbabwean dollar',2);

DECLARE @Name NVARCHAR (50), @Name2 NVARCHAR (50), @Name3 NVARCHAR (50);
DECLARE @Description NVARCHAR (255), @Description2 NVARCHAR (255), @Description3 NVARCHAR (255);
WITH Translated (CurrencyId, Lang, [Name], [Description]) AS (
	SELECT N'ETB', 'am', N'ብር', N'የኢትዮጵያ ብር'
	UNION
	SELECT N'ETB', N'ar', N'بر', N'بر إثيوبي'
	UNION
	SELECT N'ETB', N'cn', N'比尔', N'埃塞俄比亚比尔'
	UNION
	SELECT N'SAR', 'am', N'ሪያል', N'ሳዑዲ ሪያል'
	UNION
	SELECT N'SAR', N'ar', N'ريال', N'ريال سعودي'
	UNION
	SELECT N'SAR', N'cn', N'里亚尔', N'沙特里亚尔'
	UNION
	SELECT N'SDG', N'ar', N'جنيه', N'جنيه سوداني'
	UNION
	SELECT N'USD', 'am', N'ዶላር', N'የአሜሪካ ዶላር'
	UNION
	SELECT N'USD', N'ar', N'دولار', N'دولار أمريكي'
	UNION
	SELECT N'USD', N'cn', N'美元', N'美国美元'
	UNION
	SELECT N'EUR', 'am', N'ዩሮ', N'ዩሮ'
	UNION
	SELECT N'EUR', N'ar', N'يورو', N'يورو'
	UNION
	SELECT N'EUR', N'cn', N'欧元', N'欧元'
)
INSERT INTO @FunctionalCurrencies([Id],	[Name],	[Name2], [Name3], [Description], [Description2], [Description3], [E]) 
SELECT
	@FunctionalCurrencyId,
	IIF (@PrimaryLanguageId=N'en', [Name], (SELECT [Name] FROM Translated WHERE [CurrencyId] = @FunctionalCurrencyId AND [Lang] = @PrimaryLanguageId)),
	IIF (@SecondaryLanguageId=N'en', [Name], (SELECT [Name] FROM Translated WHERE [CurrencyId] = @FunctionalCurrencyId AND [Lang] = @SecondaryLanguageId)),
	IIF (@TernaryLanguageId=N'en', [Name], (SELECT [Name] FROM Translated WHERE [CurrencyId] = @FunctionalCurrencyId AND [Lang] = @TernaryLanguageId)),
	IIF (@PrimaryLanguageId=N'en', [Name], (SELECT [Description] FROM Translated WHERE [CurrencyId] = @FunctionalCurrencyId AND [Lang] = @PrimaryLanguageId)),
	IIF (@SecondaryLanguageId=N'en', [Name], (SELECT [Description] FROM Translated WHERE [CurrencyId] = @FunctionalCurrencyId AND [Lang] = @SecondaryLanguageId)),
	IIF (@TernaryLanguageId=N'en', [Name], (SELECT [Description] FROM Translated WHERE [CurrencyId] = @FunctionalCurrencyId AND [Lang] = @TernaryLanguageId)),
	[E]
FROM @IsoCurrencies C
WHERE [Id] = @FunctionalCurrencyId;

EXEC [api].Currencies__Save
	@Entities = @FunctionalCurrencies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Functional Currency: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;