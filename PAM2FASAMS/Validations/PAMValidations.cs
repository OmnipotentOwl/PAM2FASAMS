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
        public static UpdateType ValidateEvalPurpose(FileType type, string purpose)
        {
            switch (type)
            {
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

                        break;
                    }
                default:
                    break;
            }
            return UpdateType.Unknown;
        }
    }
}
