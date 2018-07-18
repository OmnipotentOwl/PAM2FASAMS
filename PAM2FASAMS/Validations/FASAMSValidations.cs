﻿using PAM2FASAMS.OutputFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PAM2FASAMS
{
    public class FASAMSValidations
    {
        public static ProviderClientIdentifier ValidateClientIdentifier(string IdString)
        {
            ProviderClientIdentifier clientIdentifier = new ProviderClientIdentifier();
            string sudoSSPattern = "^[A-Z]{3}";
            if (Regex.IsMatch(IdString, sudoSSPattern))
            {
                clientIdentifier.TypeCode = "3";
                clientIdentifier.Identifier = IdString;
            }
            else
            {
                clientIdentifier.TypeCode = "1";
                clientIdentifier.Identifier = IdString;
            }
            return clientIdentifier;
        }
        public static string ValidateFASAMSDate(string dateRaw)
        {
            DateTime result = DateTime.ParseExact(dateRaw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
            return result.ToShortDateString();
        }
        public static string ValidateFASAMSStaffEduLvlCode(string pamId)
        {
            char delimiter = '-';
            string[] substring = pamId.Split(delimiter);
            return substring[0];
        }
        public static string ValidateFASAMSStaffId(string pamId)
        {
            char delimiter = '-';
            string[] substring = pamId.Split(delimiter);
            return substring[1];
        }
        public static string ValidateIncomeAvailable(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateFamilySizeAvailable(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateSchoolDaysKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateWorkDaysKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateCommunityDaysKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateArrestsKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateLegalStatus(string pamData)
        {
            switch (pamData)
            {
                case "1": return "1";
                case "2": return "1";
                case "3": return "0";
                case "4": return "0";
                case "5": return "0";
                case "6": return "0";
                case "97": return "0";
                default: return "";
            }
        }
    }
}
