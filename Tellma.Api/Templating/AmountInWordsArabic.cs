using System;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Credit to: https://bit.ly/3SwfBH1
    /// </summary>
    class AmountInWordsArabic
    {
        /// Group Levels: 987,654,321.234
        /// 234 : Group Level -1
        /// 321 : Group Level 0
        /// 654 : Group Level 1
        /// 987 : Group Level 2

        #region Varaibles & Properties

        /// <summary>
        /// integer part
        /// </summary>
        private long _intergerValue;

        /// <summary>
        /// Decimal Part
        /// </summary>
        private int _decimalValue;

        /// <summary>
        /// Number to be converted
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        /// Currency to use
        /// </summary>
        public CurrencyInfo Currency { get; set; }

        /// <summary>
        /// English text to be placed before the generated text
        /// </summary>
        public string EnglishPrefixText { get; set; }

        /// <summary>
        /// English text to be placed after the generated text
        /// </summary>
        public string EnglishSuffixText { get; set; }

        /// <summary>
        /// Arabic text to be placed before the generated text
        /// </summary>
        public string ArabicPrefixText { get; set; }

        /// <summary>
        /// Arabic text to be placed after the generated text
        /// </summary>
        public string ArabicSuffixText { get; set; }
        #endregion

        #region General

        /// <summary>
        /// Constructor: short version
        /// </summary>
        /// <param name="number">Number to be converted</param>
        /// <param name="currency">Currency to use</param>
        public AmountInWordsArabic(decimal number, CurrencyInfo currency)
        {
            InitializeClass(number, currency, string.Empty, "only.", "فقط", "لا غير.");
        }

        /// <summary>
        /// Constructor: Full Version
        /// </summary>
        /// <param name="number">Number to be converted</param>
        /// <param name="currency">Currency to use</param>
        /// <param name="englishPrefixText">English text to be placed before the generated text</param>
        /// <param name="englishSuffixText">English text to be placed after the generated text</param>
        /// <param name="arabicPrefixText">Arabic text to be placed before the generated text</param>
        /// <param name="arabicSuffixText">Arabic text to be placed after the generated text</param>
        public AmountInWordsArabic(decimal number, CurrencyInfo currency, string englishPrefixText, string englishSuffixText, string arabicPrefixText, string arabicSuffixText)
        {
            InitializeClass(number, currency, englishPrefixText, englishSuffixText, arabicPrefixText, arabicSuffixText);
        }

        /// <summary>
        /// Initialize Class Varaibles
        /// </summary>
        /// <param name="number">Number to be converted</param>
        /// <param name="currency">Currency to use</param>
        /// <param name="englishPrefixText">English text to be placed before the generated text</param>
        /// <param name="englishSuffixText">English text to be placed after the generated text</param>
        /// <param name="arabicPrefixText">Arabic text to be placed before the generated text</param>
        /// <param name="arabicSuffixText">Arabic text to be placed after the generated text</param>
        private void InitializeClass(decimal number, CurrencyInfo currency, string englishPrefixText, string englishSuffixText, string arabicPrefixText, string arabicSuffixText)
        {
            Number = number;
            Currency = currency;
            EnglishPrefixText = englishPrefixText;
            EnglishSuffixText = englishSuffixText;
            ArabicPrefixText = arabicPrefixText;
            ArabicSuffixText = arabicSuffixText;

            ExtractIntegerAndDecimalParts();
        }

        /// <summary>
        /// Get Proper Decimal Value
        /// </summary>
        /// <param name="decimalPart">Decimal Part as a String</param>
        /// <returns></returns>
        private int GetDecimalValue(string decimalPart)
        {
            if (Currency.PartPrecision == 0)
            {
                return 0;
            }

            string result = decimalPart;
            while (result.Length < Currency.PartPrecision)
            {
                result += "0";
            }

            if (result.Length > Currency.PartPrecision)
            {
                result = result[..Currency.PartPrecision];
            }

            return int.Parse(result);
        }

        /// <summary>
        /// Extract Interger and Decimal parts
        /// </summary>
        private void ExtractIntegerAndDecimalParts()
        {
            string[] splits = Number.ToString().Split('.');

            _intergerValue = Convert.ToInt32(splits[0]);

            if (splits.Length > 1)
                _decimalValue = GetDecimalValue(splits[1]);
        }
        #endregion

        #region English Number To Word

        #region Varaibles

        private static readonly string[] englishOnes =
           new string[] {
            "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };

        private static readonly string[] englishTens =
            new string[] {
            "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };

        private static readonly string[] englishGroup =
            new string[] {
            "Hundred", "Thousand", "Million", "Billion", "Trillion", "Quadrillion", "Quintillion", "Sextillian",
            "Septillion", "Octillion", "Nonillion", "Decillion", "Undecillion", "Duodecillion", "Tredecillion",
            "Quattuordecillion", "Quindecillion", "Sexdecillion", "Septendecillion", "Octodecillion", "Novemdecillion",
            "Vigintillion", "Unvigintillion", "Duovigintillion", "10^72", "10^75", "10^78", "10^81", "10^84", "10^87",
            "Vigintinonillion", "10^93", "10^96", "Duotrigintillion", "Trestrigintillion"
        };
        #endregion

        /// <summary>
        /// Process a group of 3 digits
        /// </summary>
        /// <param name="groupNumber">The group number to process</param>
        /// <returns></returns>
        private static string ProcessGroup(int groupNumber)
        {
            int tens = groupNumber % 100;

            int hundreds = groupNumber / 100;

            string retVal = string.Empty;

            if (hundreds > 0)
            {
                retVal = string.Format("{0} {1}", englishOnes[hundreds], englishGroup[0]);
            }
            if (tens > 0)
            {
                if (tens < 20)
                {
                    retVal += ((retVal != string.Empty) ? " " : string.Empty) + englishOnes[tens];
                }
                else
                {
                    int ones = tens % 10;

                    tens = (tens / 10) - 2; // 20's offset

                    retVal += ((retVal != string.Empty) ? " " : string.Empty) + englishTens[tens];

                    if (ones > 0)
                    {
                        retVal += ((retVal != string.Empty) ? " " : string.Empty) + englishOnes[ones];
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Convert stored number to words using selected currency
        /// </summary>
        /// <returns></returns>
        public string ConvertToEnglish()
        {
            decimal tempNumber = Number;

            if (tempNumber == 0)
                return "Zero";

            string decimalString = ProcessGroup(_decimalValue);

            string retVal = string.Empty;

            int group = 0;

            if (tempNumber < 1)
            {
                retVal = englishOnes[0];
            }
            else
            {
                while (tempNumber >= 1)
                {
                    int numberToProcess = (int)(tempNumber % 1000);

                    tempNumber /= 1000;

                    string groupDescription = ProcessGroup(numberToProcess);

                    if (groupDescription != string.Empty)
                    {
                        if (group > 0)
                        {
                            retVal = string.Format("{0} {1}", englishGroup[group], retVal);
                        }

                        retVal = string.Format("{0} {1}", groupDescription, retVal);
                    }

                    group++;
                }
            }

            string formattedNumber = string.Empty;
            formattedNumber += (EnglishPrefixText != string.Empty) ? string.Format("{0} ", EnglishPrefixText) : string.Empty;
            formattedNumber += (retVal != string.Empty) ? retVal : string.Empty;
            formattedNumber += (retVal != string.Empty) ? (_intergerValue == 1 ? Currency.EnglishCurrencyName : Currency.EnglishPluralCurrencyName) : string.Empty;
            formattedNumber += (decimalString != string.Empty) ? " and " : string.Empty;
            formattedNumber += (decimalString != string.Empty) ? decimalString : string.Empty;
            formattedNumber += (decimalString != string.Empty) ? " " + (_decimalValue == 1 ? Currency.EnglishCurrencyPartName : Currency.EnglishPluralCurrencyPartName) : string.Empty;
            formattedNumber += (EnglishSuffixText != string.Empty) ? string.Format(" {0}", EnglishSuffixText) : string.Empty;

            return formattedNumber;
        }

        #endregion

        #region Arabic Number To Word

        #region Varaibles

        private static readonly string[] arabicOnes =
           new string[] {
            string.Empty, "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة",
            "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
        };

        private static readonly string[] arabicFeminineOnes =
           new string[] {
            string.Empty, "إحدى", "اثنتان", "ثلاث", "أربع", "خمس", "ست", "سبع", "ثمان", "تسع",
            "عشر", "إحدى عشرة", "اثنتا عشرة", "ثلاث عشرة", "أربع عشرة", "خمس عشرة", "ست عشرة", "سبع عشرة", "ثماني عشرة", "تسع عشرة"
        };

        private static readonly string[] arabicTens =
            new string[] {
            "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"
        };

        private static readonly string[] arabicHundreds =
            new string[] {
            "", "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة","تسعمائة"
        };

        private static readonly string[] arabicAppendedTwos =
            new string[] {
            "مئتا", "ألفا", "مليونا", "مليارا", "تريليونا", "كوادريليونا", "كوينتليونا", "سكستيليونا"
        };

        private static readonly string[] arabicTwos =
            new string[] {
            "مئتان", "ألفان", "مليونان", "ملياران", "تريليونان", "كوادريليونان", "كوينتليونان", "سكستيليونان"
        };

        private static readonly string[] arabicGroup =
            new string[] {
            "مائة", "ألف", "مليون", "مليار", "تريليون", "كوادريليون", "كوينتليون", "سكستيليون"
        };

        private static readonly string[] arabicAppendedGroup =
            new string[] {
            "", "ألفاً", "مليوناً", "ملياراً", "تريليوناً", "كوادريليوناً", "كوينتليوناً", "سكستيليوناً"
        };

        private static readonly string[] arabicPluralGroups =
            new string[] {
            "", "آلاف", "ملايين", "مليارات", "تريليونات", "كوادريليونات", "كوينتليونات", "سكستيليونات"
        };
        #endregion

        /// <summary>
        /// Get Feminine Status of one digit
        /// </summary>
        /// <param name="digit">The Digit to check its Feminine status</param>
        /// <param name="groupLevel">Group Level</param>
        /// <returns></returns>
        private string GetDigitFeminineStatus(int digit, int groupLevel)
        {
            if (groupLevel == -1)
            { // if it is in the decimal part
                if (Currency.IsCurrencyPartNameFeminine)
                    return arabicFeminineOnes[digit]; // use feminine field
                else
                    return arabicOnes[digit];
            }
            else
                if (groupLevel == 0)
            {
                if (Currency.IsCurrencyNameFeminine)
                    return arabicFeminineOnes[digit];// use feminine field
                else
                    return arabicOnes[digit];
            }
            else
                return arabicOnes[digit];
        }

        /// <summary>
        /// Process a group of 3 digits
        /// </summary>
        /// <param name="groupNumber">The group number to process</param>
        /// <returns></returns>
        private string ProcessArabicGroup(int groupNumber, int groupLevel, decimal remainingNumber)
        {
            int tens = groupNumber % 100;

            int hundreds = groupNumber / 100;

            string retVal = string.Empty;

            if (hundreds > 0)
            {
                if (tens == 0 && hundreds == 2) // حالة المضاف
                    retVal = string.Format("{0}", arabicAppendedTwos[0]);
                else //  الحالة العادية
                    retVal = string.Format("{0}", arabicHundreds[hundreds]);
            }

            if (tens > 0)
            {
                if (tens < 20)
                { // if we are processing under 20 numbers
                    if (tens == 2 && hundreds == 0 && groupLevel > 0)
                    { // This is special case for number 2 when it comes alone in the group
                        if (_intergerValue == 2000 || _intergerValue == 2000000 || _intergerValue == 2000000000 || _intergerValue == 2000000000000 || _intergerValue == 2000000000000000 || _intergerValue == 2000000000000000000)
                            retVal = string.Format("{0}", arabicAppendedTwos[groupLevel]); // في حالة الاضافة
                        else
                            retVal = string.Format("{0}", arabicTwos[groupLevel]);//  في حالة الافراد
                    }
                    else
                    { // General case
                        if (retVal != string.Empty)
                            retVal += " و";

                        if (tens == 1 && groupLevel > 0)
                            retVal += arabicGroup[groupLevel];
                        else
                            if ((tens == 1 || tens == 2) && (groupLevel == 0 || groupLevel == -1) && hundreds == 0 && remainingNumber == 0)
                            retVal += string.Empty; // Special case for 1 and 2 numbers like: ليرة سورية و ليرتان سوريتان
                        else
                            retVal += GetDigitFeminineStatus(tens, groupLevel);// Get Feminine status for this digit
                    }
                }
                else
                {
                    int ones = tens % 10;
                    tens = (tens / 10) - 2; // 20's offset

                    if (ones > 0)
                    {
                        if (retVal != string.Empty)
                            retVal += " و";

                        // Get Feminine status for this digit
                        retVal += GetDigitFeminineStatus(ones, groupLevel);
                    }

                    if (retVal != string.Empty)
                        retVal += " و";

                    // Get Tens text
                    retVal += arabicTens[tens];
                }
            }

            return retVal;
        }

        /// <summary>
        /// Convert stored number to words using selected currency
        /// </summary>
        /// <returns></returns>
        public string ConvertToArabicOld()
        {
            decimal tempNumber = Number;

            if (tempNumber == 0)
                return "صفر";

            // Get Text for the decimal part
            string decimalString = ProcessArabicGroup(_decimalValue, -1, 0);

            string retVal = string.Empty;
            byte group = 0;
            while (tempNumber >= 1)
            {
                // seperate number into groups
                int numberToProcess = (int)(tempNumber % 1000);

                tempNumber /= 1000;

                // convert group into its text
                string groupDescription = ProcessArabicGroup(numberToProcess, group, Math.Floor(tempNumber));

                if (groupDescription != string.Empty)
                { // here we add the new converted group to the previous concatenated text
                    if (group > 0)
                    {
                        if (retVal != string.Empty)
                            retVal = string.Format("{0}{1}", "و", retVal);

                        if (numberToProcess != 2)
                        {
                            if (numberToProcess % 100 != 1)
                            {
                                if (numberToProcess >= 3 && numberToProcess <= 10) // for numbers between 3 and 9 we use plural name
                                    retVal = string.Format("{0} {1}", arabicPluralGroups[group], retVal);
                                else
                                {
                                    if (retVal != string.Empty) // use appending case
                                        retVal = string.Format("{0} {1}", arabicAppendedGroup[group], retVal);
                                    else
                                        retVal = string.Format("{0} {1}", arabicGroup[group], retVal); // use normal case
                                }
                            }
                        }
                    }

                    retVal = string.Format("{0} {1}", groupDescription, retVal);
                }

                group++;
            }

            string formattedNumber = string.Empty;
            formattedNumber += (ArabicPrefixText != string.Empty) ? string.Format("{0} ", ArabicPrefixText) : string.Empty;
            formattedNumber += (retVal != string.Empty) ? retVal : string.Empty;
            if (_intergerValue != 0)
            { // here we add currency name depending on _intergerValue : 1 ,2 , 3--->10 , 11--->99
                int remaining100 = (int)(_intergerValue % 100);

                if (remaining100 == 0)
                    formattedNumber += Currency.Arabic1CurrencyName;
                else
                    if (remaining100 == 1)
                    formattedNumber += Currency.Arabic1CurrencyName;
                else
                        if (remaining100 == 2)
                {
                    if (_intergerValue == 2)
                        formattedNumber += Currency.Arabic2CurrencyName;
                    else
                        formattedNumber += Currency.Arabic1CurrencyName;
                }
                else
                            if (remaining100 >= 3 && remaining100 <= 10)
                    formattedNumber += Currency.Arabic310CurrencyName;
                else
                                if (remaining100 >= 11 && remaining100 <= 99)
                    formattedNumber += Currency.Arabic1199CurrencyName;
            }
            formattedNumber += (_decimalValue != 0) ? " و" : string.Empty;
            formattedNumber += (_decimalValue != 0) ? decimalString : string.Empty;
            if (_decimalValue != 0)
            { // here we add currency part name depending on _intergerValue : 1 ,2 , 3--->10 , 11--->99
                formattedNumber += " ";

                int remaining100 = _decimalValue % 100;

                if (remaining100 == 0)
                    formattedNumber += Currency.Arabic1CurrencyPartName;
                else
                    if (remaining100 == 1)
                    formattedNumber += Currency.Arabic1CurrencyPartName;
                else
                        if (remaining100 == 2)
                    formattedNumber += Currency.Arabic2CurrencyPartName;
                else
                            if (remaining100 >= 3 && remaining100 <= 10)
                    formattedNumber += Currency.Arabic310CurrencyPartName;
                else
                                if (remaining100 >= 11 && remaining100 <= 99)
                    formattedNumber += Currency.Arabic1199CurrencyPartName;
            }
            formattedNumber += (ArabicSuffixText != string.Empty) ? string.Format(" {0}", ArabicSuffixText) : string.Empty;

            return formattedNumber;
        }


        /// <summary>
        /// Convert stored number to words using selected currency
        /// </summary>
        /// <returns></returns>
        public string ConvertToArabic()
        {
            decimal tempNumber = Number;

            if (tempNumber == 0)
                return "صفر";

            bool isNegative = tempNumber < 0;
            tempNumber = Math.Abs(tempNumber);

            // Get Text for the decimal part
            string decimalString = $"{_decimalValue}/{Math.Pow(10, Currency.PartPrecision)}";

            string retVal = string.Empty;
            byte group = 0;
            while (tempNumber >= 1)
            {
                // seperate number into groups
                int numberToProcess = (int)(tempNumber % 1000);

                tempNumber /= 1000;

                // convert group into its text
                string groupDescription = ProcessArabicGroup(numberToProcess, group, Math.Floor(tempNumber));

                if (groupDescription != string.Empty)
                { // here we add the new converted group to the previous concatenated text
                    if (group > 0)
                    {
                        if (retVal != string.Empty)
                            retVal = string.Format("{0}{1}", "و", retVal);

                        if (numberToProcess != 2)
                        {
                            if (numberToProcess % 100 != 1)
                            {
                                if (numberToProcess >= 3 && numberToProcess <= 10) // for numbers between 3 and 9 we use plural name
                                    retVal = string.Format("{0} {1}", arabicPluralGroups[group], retVal);
                                else
                                {
                                    if (retVal != string.Empty) // use appending case
                                        retVal = string.Format("{0} {1}", arabicAppendedGroup[group], retVal);
                                    else
                                        retVal = string.Format("{0} {1}", arabicGroup[group], retVal); // use normal case
                                }
                            }
                        }
                    }

                    retVal = string.Format("{0} {1}", groupDescription, retVal);
                }

                group++;
            }

            string formattedNumber = string.Empty;
            formattedNumber += (ArabicPrefixText != string.Empty) ? string.Format("{0} ", ArabicPrefixText) : string.Empty;
            formattedNumber += (retVal != string.Empty) ? retVal : string.Empty;
            if (_intergerValue != 0)
            { // here we add currency name depending on _intergerValue : 1 ,2 , 3--->10 , 11--->99
                int remaining100 = (int)(_intergerValue % 100);

                if (remaining100 == 0)
                    formattedNumber += Currency.Arabic1CurrencyName;
                else
                    if (remaining100 == 1)
                    formattedNumber += Currency.Arabic1CurrencyName;
                else
                        if (remaining100 == 2)
                {
                    if (_intergerValue == 2)
                        formattedNumber += Currency.Arabic2CurrencyName;
                    else
                        formattedNumber += Currency.Arabic1CurrencyName;
                }
                else
                            if (remaining100 >= 3 && remaining100 <= 10)
                    formattedNumber += Currency.Arabic310CurrencyName;
                else
                                if (remaining100 >= 11 && remaining100 <= 99)
                    formattedNumber += Currency.Arabic1199CurrencyName;
            }
            formattedNumber += (_decimalValue != 0) ? " و" : string.Empty;
            formattedNumber += (_decimalValue != 0) ? decimalString : string.Empty;
            if (_decimalValue != 0)
            { // here we add currency part name depending on _intergerValue : 1 ,2 , 3--->10 , 11--->99
                formattedNumber += " ";

                int remaining100 = _decimalValue % 100;

                if (remaining100 == 0)
                    formattedNumber += Currency.Arabic1CurrencyPartName;
                else
                    if (remaining100 == 1)
                    formattedNumber += Currency.Arabic1CurrencyPartName;
                else
                        if (remaining100 == 2)
                    formattedNumber += Currency.Arabic2CurrencyPartName;
                else
                            if (remaining100 >= 3 && remaining100 <= 10)
                    formattedNumber += Currency.Arabic310CurrencyPartName;
                else
                                if (remaining100 >= 11 && remaining100 <= 99)
                    formattedNumber += Currency.Arabic1199CurrencyPartName;
            }
            formattedNumber += (ArabicSuffixText != string.Empty) ? string.Format(" {0}", ArabicSuffixText) : string.Empty;


            if (isNegative)
            {
                formattedNumber = "سالب " + formattedNumber;
            }

            return formattedNumber;
        }

        #endregion
    }

    public class CurrencyInfo
    {
        #region Constructors

        public CurrencyInfo(byte decimals)
        {
            IsCurrencyNameFeminine = false;
            EnglishCurrencyName = "";
            EnglishPluralCurrencyName = "";
            EnglishCurrencyPartName = "";
            EnglishPluralCurrencyPartName = "";
            Arabic1CurrencyName = "";
            Arabic2CurrencyName = "";
            Arabic310CurrencyName = "";
            Arabic1199CurrencyName = "";
            Arabic1CurrencyPartName = "";
            Arabic2CurrencyPartName = "";
            Arabic310CurrencyPartName = "";
            Arabic1199CurrencyPartName = "";
            PartPrecision = decimals;
            IsCurrencyPartNameFeminine = false;
        }

        public CurrencyInfo(string iso)
        {
            switch (iso)
            {
                case "SYP":
                    IsCurrencyNameFeminine = true;
                    EnglishCurrencyName = "Syrian Pound";
                    EnglishPluralCurrencyName = "Syrian Pounds";
                    EnglishCurrencyPartName = "Piaster";
                    EnglishPluralCurrencyPartName = "Piasteres";
                    Arabic1CurrencyName = "ليرة سورية";
                    Arabic2CurrencyName = "ليرتان سوريتان";
                    Arabic310CurrencyName = "ليرات سورية";
                    Arabic1199CurrencyName = "ليرة سورية";
                    Arabic1CurrencyPartName = "قرش";
                    Arabic2CurrencyPartName = "قرشان";
                    Arabic310CurrencyPartName = "قروش";
                    Arabic1199CurrencyPartName = "قرشاً";
                    PartPrecision = 2;
                    IsCurrencyPartNameFeminine = false;
                    break;

                case "AED":
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "UAE Dirham";
                    EnglishPluralCurrencyName = "UAE Dirhams";
                    EnglishCurrencyPartName = "Fils";
                    EnglishPluralCurrencyPartName = "Fils";
                    Arabic1CurrencyName = "درهم إماراتي";
                    Arabic2CurrencyName = "درهمان إماراتيان";
                    Arabic310CurrencyName = "دراهم إماراتية";
                    Arabic1199CurrencyName = "درهماً إماراتياً";
                    Arabic1CurrencyPartName = "فلس";
                    Arabic2CurrencyPartName = "فلسان";
                    Arabic310CurrencyPartName = "فلوس";
                    Arabic1199CurrencyPartName = "فلساً";
                    PartPrecision = 2;
                    IsCurrencyPartNameFeminine = false;
                    break;

                case "SAR":
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "Saudi Riyal";
                    EnglishPluralCurrencyName = "Saudi Riyals";
                    EnglishCurrencyPartName = "Halala";
                    EnglishPluralCurrencyPartName = "Halalas";
                    Arabic1CurrencyName = "ريال سعودي";
                    Arabic2CurrencyName = "ريالان سعوديان";
                    Arabic310CurrencyName = "ريالات سعودية";
                    Arabic1199CurrencyName = "ريالاً سعودياً";
                    Arabic1CurrencyPartName = "هللة";
                    Arabic2CurrencyPartName = "هللتان";
                    Arabic310CurrencyPartName = "هللات";
                    Arabic1199CurrencyPartName = "هللة";
                    PartPrecision = 2;
                    IsCurrencyPartNameFeminine = true;
                    break;

                case "TND":
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "Tunisian Dinar";
                    EnglishPluralCurrencyName = "Tunisian Dinars";
                    EnglishCurrencyPartName = "milim";
                    EnglishPluralCurrencyPartName = "millimes";
                    Arabic1CurrencyName = "درهم إماراتي";
                    Arabic2CurrencyName = "درهمان إماراتيان";
                    Arabic310CurrencyName = "دراهم إماراتية";
                    Arabic1199CurrencyName = "درهماً إماراتياً";
                    Arabic1CurrencyPartName = "فلس";
                    Arabic2CurrencyPartName = "فلسان";
                    Arabic310CurrencyPartName = "فلوس";
                    Arabic1199CurrencyPartName = "فلساً";
                    PartPrecision = 3;
                    IsCurrencyPartNameFeminine = false;
                    break;

                case "XAU":
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "Gram";
                    EnglishPluralCurrencyName = "Grams";
                    EnglishCurrencyPartName = "Milligram";
                    EnglishPluralCurrencyPartName = "Milligrams";
                    Arabic1CurrencyName = "جرام";
                    Arabic2CurrencyName = "جرامان";
                    Arabic310CurrencyName = "جرامات";
                    Arabic1199CurrencyName = "جراماً";
                    Arabic1CurrencyPartName = "ملجرام";
                    Arabic2CurrencyPartName = "ملجرامان";
                    Arabic310CurrencyPartName = "ملجرامات";
                    Arabic1199CurrencyPartName = "ملجراماً";
                    PartPrecision = 2;
                    IsCurrencyPartNameFeminine = false;
                    break;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Is the currency name feminine ( Mua'anath مؤنث)
        /// ليرة سورية : مؤنث = true
        /// درهم : مذكر = false
        /// </summary>
        public bool IsCurrencyNameFeminine { get; set; }

        /// <summary>
        /// English Currency Name for single use
        /// Syrian Pound
        /// UAE Dirham
        /// </summary>
        public string EnglishCurrencyName { get; set; }

        /// <summary>
        /// English Plural Currency Name for Numbers over 1
        /// Syrian Pounds
        /// UAE Dirhams
        /// </summary>
        public string EnglishPluralCurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 1 unit only
        /// ليرة سورية
        /// درهم إماراتي
        /// </summary>
        public string Arabic1CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 2 units only
        /// ليرتان سوريتان
        /// درهمان إماراتيان
        /// </summary>
        public string Arabic2CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 3 to 10 units
        /// خمس ليرات سورية
        /// خمسة دراهم إماراتية
        /// </summary>
        public string Arabic310CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 11 to 99 units
        /// خمس و سبعون ليرةً سوريةً
        /// خمسة و سبعون درهماً إماراتياً
        /// </summary>
        public string Arabic1199CurrencyName { get; set; }

        /// <summary>
        /// Decimal Part Precision
        /// for Syrian Pounds: 2 ( 1 SP = 100 parts)
        /// for Tunisian Dinars: 3 ( 1 TND = 1000 parts)
        /// </summary>
        public byte PartPrecision { get; set; }

        /// <summary>
        /// Is the currency part name feminine ( Mua'anath مؤنث)
        /// هللة : مؤنث = true
        /// قرش : مذكر = false
        /// </summary>
        public bool IsCurrencyPartNameFeminine { get; set; }

        /// <summary>
        /// English Currency Part Name for single use
        /// Piaster
        /// Fils
        /// </summary>
        public string EnglishCurrencyPartName { get; set; }

        /// <summary>
        /// English Currency Part Name for Plural
        /// Piasters
        /// Fils
        /// </summary>
        public string EnglishPluralCurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 1 unit only
        /// قرش
        /// هللة
        /// </summary>
        public string Arabic1CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 2 unit only
        /// قرشان
        /// هللتان
        /// </summary>
        public string Arabic2CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 3 to 10 units
        /// قروش
        /// هللات
        /// </summary>
        public string Arabic310CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 11 to 99 units
        /// قرشاً
        /// هللةً
        /// </summary>
        public string Arabic1199CurrencyPartName { get; set; }
        #endregion
    }

}
