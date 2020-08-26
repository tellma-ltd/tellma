SET @DefinitionId = @BankLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name], [Name2]) VALUES
(0, N'ARB', N'Animal Resources’ Bank', N'بنك الثروة الحيوانية'),
(1, N'IDB', N'Industrial Development Bank .', N'مصرف التنمية الصناعية'),
(2, N'ONB', N'Omdurman National Bank', N'بنك أم درمان الوطني'),
(3, N'FIIB', N'Financial Investment Bank', N'بنك الاستثمار المالي'),
(4, N'TIB', N'Tadamon Islamic Bank', N'بنك التضامن الإسلامي'),
(5, N'BBA', N'Byblos Bank ( Africa )', N'بنك بيبلوس أفريقيا'),
(6, N'WNB', N'Workers’ National Bank', N'بنك العمال الوطني'),
(7, N'FAIB', N'Faisal Islamic Bank', N'بنك فيصل الإسلامي السوداني'),
(8, N'AB', N'Agricultural Bank', N'بنك الزراعي السوداني'),
(9, N'ABTD', N'African Bank for Trade and Development', N'مصرف الساحل والصحراء للاستثمار والتجارة'),
(10, N'SEB', N'Sudanese Egyptian Bank', N'البنك السوداني المصري'),
(11, N'BLDB', N'Balad Bank', N'مصرف البلد'),
(12, N'RECB', N'Real Estates Commercial Bank', N'البنك العقاري التجاري'),
(13, N'BBS', N'Baraka Bank (Sudan )', N'بنك البركة السوداني'),
(14, N'SFB', N'Sudanese French Bank', N'البنك السوداني الفرنسي'),
(15, N'SSB', N'Saudi Sudanese Bank', N'البنك السعودي السوداني'),
(16, N'BNMB', N'Blue Nile Mashreq Bank', N'بنك النيل الأزرق المشرق'),
(17, N'ENB', N'El-Nilien Bank', N'بنك النيلين'),
(18, N'FCB', N'Farmer’s Commercial Bank', N'مصرف المزارع التجاري'),
(19, N'ASJB', N'Aljazeera Sudanese Jordanian Bank', N'بنك الجزيرة السوداني الأردني'),
(20, N'QNB', N'Qatar National Bank', N'الفرع المصرفي لبنك قطر الوطني الإسلامي'),
(21, N'UCB', N'United Capital Bank', N'بنك المال المتحد'),
(22, N'EDB', N'Export Development Bank', N'بنك تنمية الصادرات'),
(23, N'NBS', N'National Bank of Sudan', N'البنك الأهلي السوداني'),
(24, N'SB', N'Alsalam Bank', N'مصرف السلام'),
(25, N'FB', N'Family Bank', N'بنك الأسرة'),
(26, N'SSDB', N'Savings and Social Development Bank', N'مصرف الادخار والتنمية الاجتماعية'),
(27, N'BOK', N'Bank of Khartoum', N'بنك الخرطوم'),
(28, N'SIB', N'Sudanese Islamic Bank', N'البنك الإسلامي السوداني'),
(29, N'ADNB', N'Abu Dhabi National Bank', N'بنك أبوظبي الوطني'),
(30, N'ASB', N'Arab Sudanese Bank', N'البنك العربي السوداني'),
(31, N'NBE', N'National Bank of EGYPT (Khartoum)', N'البنك الأهلي المصري (الخرطوم)'),
(32, N'QIB', N'Qatar Islamic Bank', N'مصرف قطر الإسلامي'),
(33, N'ADIB', N'Abu Dhabi Islamic Bank', N'مصرف أبوظبي الإسلامي'),
(34, N'KB', N'ALKHALEEJ BANK', N'بنك الخليج'),
(35, N'ZKB', N'Ziraat Katilim Bank', N'بنك زراعات كاتيليم.'),
(36, N'NBCD', N'Al Nile Bank For Commerce and Development', N'بنك النيل للتجارة والتنمية');

EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Banks Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;