using PAM2FASAMS.DataContext;
using PAM2FASAMS.OutputFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PAM2FASAMS.PAMValidations;

namespace PAM2FASAMS.Utilities
{
    public class DataTools
    {
        public static TreatmentEpisode OpportuniticlyLoadTreatmentSession(TreatmentEpisodeDataSet currentJob, UpdateType type, string recordDate, string clientSourceRecordIdentifier)
        {
            switch (type)
            {
                case UpdateType.Admission:
                    {
                        return null;
                    }
                case UpdateType.Update:
                    {
                        if (currentJob.TreatmentEpisodes.Exists(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))))
                        {
                            return currentJob.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))).Single();
                        }
                        else
                        {
                            using(var db = new fasams_db())
                            {
                                if(db.TreatmentEpisodes.Any(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))))
                                {
                                    return db.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))).Single();
                                }
                            }
                        }
                        return null;
                    }
                case UpdateType.Discharge:
                    {
                        if (currentJob.TreatmentEpisodes.Exists(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))))
                        {
                            return currentJob.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))).Single();
                        }
                        else
                        {
                            using (var db = new fasams_db())
                            {
                                if (db.TreatmentEpisodes.Any(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))))
                                {
                                    return db.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))).Single();
                                }
                            }
                        }
                        return null;
                    }
                case UpdateType.ImDischarge:
                    {
                        return null;
                    }
                default:
                    return null;
            }
            
        }
    }
}
