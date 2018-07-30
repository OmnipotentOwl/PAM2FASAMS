using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS
{
    public class PAMValidations
    {
        public enum UpdateType
        {
            Unknown,
            Admission,
            Update,
            Discharge,
            ImDischarge
        }
        public enum TreatmentEpisodeType
        {
            Unknown,
            Admission,
            ImDischarge
        }
        public static UpdateType ValidateEvalPurpose(FileType type, string purpose)
        {
            switch (type)
            {
                case FileType.SAPERFA:
                    {
                        switch (purpose)
                        {
                            case "1": return UpdateType.Admission;
                            case "2": return UpdateType.ImDischarge;
                        }
                        break;
                    }
                case FileType.SAPERFD:
                    {
                        switch (purpose)
                        {
                            case "3": return UpdateType.Discharge;
                        }
                        break;
                    }
                case FileType.SADT:
                    {
                        switch (purpose)
                        {
                            case "5": return UpdateType.Update;
                        }
                        break;
                    }
                case FileType.PERF:
                    {
                        switch (purpose)
                        {
                            case "1": return UpdateType.Admission;
                            case "2": return UpdateType.Update;
                            case "3": return UpdateType.Discharge;
                            case "4": return UpdateType.Discharge;
                            case "5": return UpdateType.ImDischarge;
                        }
                        break;
                    }
                case FileType.CFAR:
                    {
                        switch (purpose)
                        {
                            case "1": return UpdateType.Admission;
                            case "2": return UpdateType.Update;
                            case "3": return UpdateType.Discharge;
                            case "4": return UpdateType.ImDischarge;
                        }
                        break;
                    }
                default:
                    break;
            }
            return UpdateType.Unknown;
        }
        public static string ValidateCoverdServiceCodeLocation(string covrdSvc, string location)
        {

            return locationMatrices.Where(l => l.CoveredService == covrdSvc && l.Location == location).SingleOrDefault()?.ValidCoveredService;
        }
        private class CoveredServiceLocationMatrix
        {
            public string CoveredService { get; set; }
            public string Location { get; set; }
            public string ValidCoveredService { get; set; }
        }
        private static List<CoveredServiceLocationMatrix> locationMatrices = new List<CoveredServiceLocationMatrix>
        {
            new CoveredServiceLocationMatrix(){ CoveredService ="08", Location="11", ValidCoveredService="14"},
        };
    }
}
