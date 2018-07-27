using PAM2FASAMS.DataContext;
using PAM2FASAMS.OutputFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using static PAM2FASAMS.PAMValidations;

namespace PAM2FASAMS.Utilities
{
    public class DataTools
    {
        public static TreatmentEpisode OpportuniticlyLoadTreatmentSession(TreatmentEpisodeDataSet currentJob, UpdateType type, string recordDate, string clientSourceRecordIdentifier, string federalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            switch (type)
            {
                case UpdateType.Admission:
                    {
                        List<TreatmentEpisode> existing = currentJob.TreatmentEpisodes.Where(c => c.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && c.FederalTaxIdentifier == federalTaxIdentifier).ToList();
                        if (existing == null || existing.Count == 0)
                        {
                            return OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
                        }
                        if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)))
                        {
                            return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)).FirstOrDefault();
                        }
                        if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "1")))
                        {
                            return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "1")).FirstOrDefault();
                        }
                        if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "2")))
                        {
                            return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "2")).FirstOrDefault();
                        }
                        return OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
                    }
                case UpdateType.Update:
                    {
                        if (currentJob.TreatmentEpisodes.Exists(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && e.FederalTaxIdentifier == federalTaxIdentifier 
                        && (e.Admissions.Exists(a => a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)))))
                        {
                            return currentJob.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && e.FederalTaxIdentifier == federalTaxIdentifier 
                            && (e.Admissions.Exists(a => a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)))).LastOrDefault();
                        }
                        else
                        {
                            return OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
                        }
                    }
                case UpdateType.Discharge:
                    {
                        if (currentJob.TreatmentEpisodes.Exists(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && e.FederalTaxIdentifier == federalTaxIdentifier 
                        && (e.Admissions.Exists(a => a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)))))
                        {
                            return currentJob.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && e.FederalTaxIdentifier == federalTaxIdentifier
                            && (e.Admissions.Exists(a => a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)))).LastOrDefault();
                        }
                        else
                        {
                            return OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
                        }
                    }
                case UpdateType.ImDischarge:
                    {
                        // added a search component here as there isnt much guidence on when you should use an existing Treatment Episode vs when you should make a new one.
                        //int daysToSearch = 30; // will probably make this item configurable via a config file.
                        //DateTime lowerSearchRange = date.AddDays(-daysToSearch);
                        //DateTime upperSearchRange = date.AddDays(daysToSearch);
                        if(currentJob.TreatmentEpisodes.Exists(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && e.FederalTaxIdentifier == federalTaxIdentifier
                        && (e.ImmediateDischarges.Exists(i => i.InternalEvaluationDate == date))))
                        {
                            return currentJob.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && e.FederalTaxIdentifier == federalTaxIdentifier 
                            && (e.ImmediateDischarges.Exists(i => i.InternalEvaluationDate == date ))).FirstOrDefault();
                        }
                        return OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.ImDischarge, recordDate,clientSourceRecordIdentifier, federalTaxIdentifier);
                    }
                default:
                    return null;
            }
            
        }
        public static TreatmentEpisode OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType type, string recordDate, string clientSourceRecordIdentifier, string federalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using(var db = new fasams_db())
            {
                List<TreatmentEpisode> existing = db.TreatmentEpisodes
                    .Include(x => x.Admissions.Select(a=> a.Discharge))
                    .Include(x => x.ImmediateDischarges)
                    .Where(c => c.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && c.FederalTaxIdentifier == federalTaxIdentifier)
                    .ToList();

                switch (type)
                {
                    case TreatmentEpisodeType.Admission:
                        {
                            if (existing == null || existing.Count == 0)
                            {
                                return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), ClientSourceRecordIdentifier = clientSourceRecordIdentifier, FederalTaxIdentifier = federalTaxIdentifier, Admissions = new List<Admission>() }; ;
                            }
                            if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)))
                            {
                                return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)).LastOrDefault();
                            }
                            if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "1")))
                            {
                                return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "1")).LastOrDefault();
                            }
                            if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "2")))
                            {
                                return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date && a.Discharge.TypeCode == "2")).LastOrDefault();
                            }
                            return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), ClientSourceRecordIdentifier = clientSourceRecordIdentifier, FederalTaxIdentifier = federalTaxIdentifier, Admissions = new List<Admission>() }; ;
                        }
                    case TreatmentEpisodeType.ImDischarge:
                        {
                            // added a search component here as there isnt much guidence on when you should use an existing Treatment Episode vs when you should make a new one.
                            //int daysToSearch = 30; // will probably make this item configurable via a config file.
                            //DateTime lowerSearchRange = date.AddDays(-daysToSearch);
                            //DateTime upperSearchRange = date.AddDays(daysToSearch);
                            if (existing == null || existing.Count == 0)
                            {
                                return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), ClientSourceRecordIdentifier = clientSourceRecordIdentifier, FederalTaxIdentifier = federalTaxIdentifier, ImmediateDischarges = new List<ImmediateDischarge>() }; ;
                            }
                            if (existing.Any(t => t.ImmediateDischarges.Any(i => i.InternalEvaluationDate == date)))
                            {
                                return existing.Where(t => t.ImmediateDischarges.Any(i => i.InternalEvaluationDate == date)).FirstOrDefault();
                            }
                            return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), ClientSourceRecordIdentifier = clientSourceRecordIdentifier, FederalTaxIdentifier = federalTaxIdentifier, ImmediateDischarges = new List<ImmediateDischarge>() }; ;
                        }
                    default:
                        return null;
                }
            }
        }
        public static ImmediateDischarge OpportuniticlyLoadImmediateDischarge(TreatmentEpisode episode, UpdateType type, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            switch (type)
            {
                case UpdateType.ImDischarge:
                    {
                        if(episode.ImmediateDischarges.Exists(i => i.TreatmentSourceId == episode.SourceRecordIdentifier && i.InternalEvaluationDate == date))
                        {
                            return episode.ImmediateDischarges.Where(i => i.TreatmentSourceId == episode.SourceRecordIdentifier && i.InternalEvaluationDate == date).SingleOrDefault();
                        }
                        else
                        {
                            return OpportuniticlyLoadImmediateDischarge(episode, recordDate);
                        }
                    }
                default:
                    return null;
            }
        }
        public static ImmediateDischarge OpportuniticlyLoadImmediateDischarge(TreatmentEpisode episode, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<ImmediateDischarge> existing = db.ImmediateDischarges
                    .Where(i => i.TreatmentSourceId == episode.SourceRecordIdentifier)
                    .ToList();
                if (existing == null || existing.Count == 0)
                {
                    return new ImmediateDischarge { SourceRecordIdentifier = Guid.NewGuid().ToString(), TreatmentSourceId = episode.SourceRecordIdentifier };
                }
                if(existing.Any(i => i.InternalEvaluationDate == date))
                {
                    return existing.Where(i => i.InternalEvaluationDate == date).FirstOrDefault();
                }
                return new ImmediateDischarge { SourceRecordIdentifier = Guid.NewGuid().ToString(), TreatmentSourceId = episode.SourceRecordIdentifier };
            }
        }
        public static Admission OpportuniticlyLoadAdmission(TreatmentEpisode episode, UpdateType type, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            switch (type)
            {
                case UpdateType.Admission:
                    {
                        return OpportuniticlyLoadAdmission(episode, recordDate);
                    }
                case UpdateType.Update:
                    {
                        if (episode.Admissions.Exists(a => a.TreatmentSourceId == episode.SourceRecordIdentifier && a.PerformanceOutcomeMeasures!=null && a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)))
                        {
                            return episode.Admissions.Where(a => a.TreatmentSourceId == episode.SourceRecordIdentifier && a.PerformanceOutcomeMeasures != null && a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)).SingleOrDefault();
                        }
                        else
                        {
                            return OpportuniticlyLoadAdmission(episode, recordDate);
                        }
                    }
                case UpdateType.Discharge:
                    {
                        if (episode.Admissions.Exists(a => a.TreatmentSourceId == episode.SourceRecordIdentifier && a.PerformanceOutcomeMeasures != null && a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)))
                        {
                            return episode.Admissions.Where(a => a.TreatmentSourceId == episode.SourceRecordIdentifier && a.PerformanceOutcomeMeasures != null && a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)).SingleOrDefault();
                        }
                        else
                        {
                            return OpportuniticlyLoadAdmission(episode, recordDate);
                        }
                    }
                default:
                    return null;
            }
        }
        public static Admission OpportuniticlyLoadAdmission(TreatmentEpisode episode, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            using(var db = new fasams_db())
            {
                List<Admission> existing = db.Admissions
                    .Include(x => x.PerformanceOutcomeMeasures.Select(p => p.SubstanceUseDisorders))
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Include(x => x.Discharge)
                    .Where(a => a.TreatmentSourceId == episode.SourceRecordIdentifier)
                    .ToList();

                if (existing == null || existing.Count == 0)
                {
                    return new Admission { SourceRecordIdentifier = Guid.NewGuid().ToString(), Evaluations = new List<Evaluation>(), Diagnoses = new List<Diagnosis>() };
                }
                if(existing.Any(a=>a.InternalAdmissionDate <= date && a.Discharge == null))
                {
                    return existing.Where(a => a.InternalAdmissionDate <= date && a.Discharge == null).FirstOrDefault();
                }
                if (existing.Any(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date))
                {
                    return existing.Where(a => a.InternalAdmissionDate <= date && a.Discharge.InternalDischargeDate >= date).FirstOrDefault();
                }
                return new Admission { SourceRecordIdentifier = Guid.NewGuid().ToString(), Evaluations = new List<Evaluation>(), Diagnoses = new List<Diagnosis>() };
            }
        }
        public static Discharge OpportuniticlyLoadDischarge(Admission admission, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            using(var db = new fasams_db())
            {
                List<Discharge> existing = db.Discharges
                    .Include(x => x.PerformanceOutcomeMeasures)
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Where(d => d.SourceRecordIdentifier == admission.Discharge_SourceRecordIdentifier)
                    .ToList();

                if (existing == null || existing.Count == 0)
                {
                    return new Discharge { SourceRecordIdentifier = Guid.NewGuid().ToString(), Diagnoses = new List<Diagnosis>(), Evaluations = new List<Evaluation>() };
                }
                if (existing.Any(i => i.SourceRecordIdentifier == admission.Discharge_SourceRecordIdentifier))
                {
                    return existing.Where(i => i.SourceRecordIdentifier == admission.Discharge_SourceRecordIdentifier).FirstOrDefault();
                }

                return null;
            }
        }
        public static ProviderClient OpportuniticlyLoadProviderClient(ProviderClients currentJob, ProviderClientIdentifier SSN, string FederalTaxIdentifier)
        {
            if (currentJob.clients.Exists(c => c.FederalTaxIdentifier == FederalTaxIdentifier && c.ProviderClientIdentifiers.Exists(i=> i.Identifier == SSN.Identifier && i.ClientSourceId == c.SourceRecordIdentifier)))
            {
                return currentJob.clients.Where(c => c.FederalTaxIdentifier == FederalTaxIdentifier && c.ProviderClientIdentifiers.Exists(i => i.Identifier == SSN.Identifier && i.ClientSourceId == c.SourceRecordIdentifier)).Single();
            }
            else
            {
                ProviderClientIdentifier existing = new ProviderClientIdentifier();
                using (var db = new fasams_db())
                {
                    existing = db.ProviderClientIdentifiers.Single(i => i.FederalTaxIdentifier == FederalTaxIdentifier && i.Identifier == SSN.Identifier);
                }
                return OpportuniticlyLoadProviderClient(currentJob, existing.ClientSourceId, FederalTaxIdentifier);
            }
        }
        public static ProviderClient OpportuniticlyLoadProviderClient(ProviderClientIdentifier SSN, string FederalTaxIdentifier)
        { 
            ProviderClientIdentifier existing = new ProviderClientIdentifier();
            using (var db = new fasams_db())
            {
                existing = db.ProviderClientIdentifiers.SingleOrDefault(i => i.FederalTaxIdentifier == FederalTaxIdentifier && i.Identifier == SSN.Identifier);
            }
            if(existing != null)
            {
                return OpportuniticlyLoadProviderClient(existing.ClientSourceId, FederalTaxIdentifier);
            }
            return new ProviderClient();
        }
        public static ProviderClient OpportuniticlyLoadProviderClient(ProviderClients currentJob, string SourceRecordIdentifier, string FederalTaxIdentifier)
        {
            if (currentJob.clients.Exists(c => c.SourceRecordIdentifier == SourceRecordIdentifier && c.FederalTaxIdentifier == FederalTaxIdentifier))
            {
                return currentJob.clients.Where(c => c.SourceRecordIdentifier == SourceRecordIdentifier && c.FederalTaxIdentifier == FederalTaxIdentifier).Single();
            }
            else
            {
                return OpportuniticlyLoadProviderClient(SourceRecordIdentifier, FederalTaxIdentifier);
            }
        }
        public static ProviderClient OpportuniticlyLoadProviderClient(string SourceRecordIdentifier, string FederalTaxIdentifier)
        {

            using (var db = new fasams_db())
            {
                ProviderClient existing = db.ProviderClients
                    .Include(x => x.ProviderClientIdentifiers)
                    .Include(x => x.ProviderClientPhones)
                    .Include(x => x.ProviderClientEmailAddresses)
                    .Include(x => x.ProviderClientPhysicalAddresses)
                    .SingleOrDefault(c => c.SourceRecordIdentifier == SourceRecordIdentifier && c.FederalTaxIdentifier == FederalTaxIdentifier);

                if (existing == null)
                {
                    existing = new ProviderClient
                    {
                        ProviderClientIdentifiers = new List<ProviderClientIdentifier>()
                    };
                }

                return existing;
            }
            
        }
        public static Subcontract OpportuniticlyLoadSubcontract(string contractNum, string subcontractNum, string recordDate, string FederalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<Subcontract> existing = db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .Where(c => c.FederalTaxIdentifier == FederalTaxIdentifier && c.ContractNumber == contractNum && c.SubcontractNumber == subcontractNum)
                    .ToList();

                if (existing == null || existing.Count == 0)
                {
                    throw new InvalidOperationException("Missing Contract Data, please add contract information to DB!");
                }
                if (existing.Any(c => c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date && c.InternalAmendmentDate <= date))
                {
                    return existing.Where(c => c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date && c.InternalAmendmentDate <= date).LastOrDefault();
                }
                return null;
            }
        }
        public static Subcontract OpportuniticlyLoadSubcontract(string recordDate, string FederalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<Subcontract> existing = db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .Where(c => c.FederalTaxIdentifier == FederalTaxIdentifier)
                    .ToList();

                if(existing == null || existing.Count == 0)
                {
                    throw new InvalidOperationException("Missing Contract Data, please add contract information to DB!");
                }
                if(existing.Any(c => c.InternalEffectiveDate<=date && c.InternalExpirationDate >= date && c.InternalAmendmentDate <= date))
                {
                    return existing.Where(c => c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date && c.InternalAmendmentDate <= date).LastOrDefault();
                }
                return null;
            }
        }
        public static void UpsertProviderClient(ProviderClient providerClient)
        {
            using(var db = new fasams_db())
            {
                ProviderClient existing = db.ProviderClients
                    .Include(x => x.ProviderClientIdentifiers)
                    .Include(x => x.ProviderClientPhones)
                    .Include(x => x.ProviderClientEmailAddresses)
                    .Include(x => x.ProviderClientPhysicalAddresses)
                    .SingleOrDefault(c => c.SourceRecordIdentifier == providerClient.SourceRecordIdentifier && c.FederalTaxIdentifier == providerClient.FederalTaxIdentifier);

                if (existing == null)
                {
                    db.ProviderClients.Add(providerClient);
                    foreach(var row in providerClient.ProviderClientIdentifiers)
                    {
                        db.ProviderClientIdentifiers.Add(row);
                    }
                    if(providerClient.ProviderClientPhones != null)
                    {
                        foreach (var row in providerClient.ProviderClientPhones)
                        {
                            db.ProviderClientPhones.Add(row);
                        }
                    }
                    if(providerClient.ProviderClientEmailAddresses != null)
                    {
                        foreach (var row in providerClient.ProviderClientEmailAddresses)
                        {
                            db.ProviderClientEmailAddresses.Add(row);
                        }
                    }
                    if(providerClient.ProviderClientPhysicalAddresses != null)
                    {
                        foreach (var row in providerClient.ProviderClientPhysicalAddresses)
                        {
                            db.ProviderClientPhysicalAddresses.Add(row);
                        }
                    }
                    
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(providerClient);

                }
                db.SaveChanges();
            }
        }
        public static void UpsertTreatmentSession(TreatmentEpisode treatmentEpisode)
        {
            using(var db = new fasams_db())
            {
                TreatmentEpisode existing = db.TreatmentEpisodes
                    .Include(x => x.Admissions.Select(a => a.Discharge))
                    .Include(x => x.ImmediateDischarges)
                    .SingleOrDefault(e => e.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier && e.FederalTaxIdentifier == treatmentEpisode.FederalTaxIdentifier);

                if(existing == null)
                {
                    db.TreatmentEpisodes.Add(treatmentEpisode);
                    if(treatmentEpisode.Admissions != null)
                    {
                        foreach (var row in treatmentEpisode.Admissions)
                        {
                            db.Admissions.Add(row);
                            if(row.PerformanceOutcomeMeasures != null)
                            {
                                foreach (var perf in row.PerformanceOutcomeMeasures)
                                {
                                    db.PerformanceOutcomeMeasures.Add(perf);
                                    if (perf.SubstanceUseDisorders != null)
                                    {
                                        foreach (var sad in perf.SubstanceUseDisorders)
                                        {
                                            db.SubstanceUseDisorders.Add(sad);
                                        }
                                    }
                                }
                            }
                            if(row.Evaluations != null)
                            {
                                foreach(var item in row.Evaluations)
                                {
                                    db.Evaluations.Add(item);
                                }
                            }
                            if(row.Diagnoses != null)
                            {
                                foreach (var item in row.Diagnoses)
                                {
                                    db.Diagnoses.Add(item);
                                }
                            }
                            if(row.Discharge != null && row.Discharge.SourceRecordIdentifier != null)
                            {
                                db.Discharges.Add(row.Discharge);
                            }
                        }
                    }
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(treatmentEpisode);
                    if (treatmentEpisode.Admissions != null)
                    {
                        //db.Entry(existing.Admissions).CurrentValues.SetValues(treatmentEpisode.Admissions);
                        foreach (var row in treatmentEpisode.Admissions)
                        {
                            var exAdmit = db.Admissions.Find(row.SourceRecordIdentifier);
                            db.Entry(exAdmit).CurrentValues.SetValues(row);
                            if (row.PerformanceOutcomeMeasures != null)
                            {
                                foreach (var perf in row.PerformanceOutcomeMeasures)
                                {
                                    var exPerf = db.PerformanceOutcomeMeasures.Find(perf.SourceRecordIdentifier);
                                    if (exPerf != null)
                                    {
                                        db.Entry(exPerf).CurrentValues.SetValues(perf);
                                    }
                                    else
                                    {
                                        db.PerformanceOutcomeMeasures.Add(perf);
                                    }
                                }
                            }
                            if (row.Evaluations != null)
                            {
                                foreach (var item in row.Evaluations)
                                {
                                    var exItem = db.Evaluations.Find(item.SourceRecordIdentifier);
                                    if (exItem != null)
                                    {
                                        db.Entry(exItem).CurrentValues.SetValues(item);
                                    }
                                    else
                                    {
                                        db.Evaluations.Add(item);
                                    }
                                }
                            }
                            if (row.Diagnoses != null)
                            {
                                foreach (var item in row.Diagnoses)
                                {
                                    var exItem = db.Diagnoses.Find(item.SourceRecordIdentifier);
                                    if (exItem != null)
                                    {
                                        db.Entry(exItem).CurrentValues.SetValues(item);
                                    }
                                    else
                                    {
                                        db.Diagnoses.Add(item);
                                    }
                                }
                            }
                            if (row.Discharge != null)
                            {

                                var exItem = db.Discharges.Find(row.Discharge.SourceRecordIdentifier);
                                if (exItem != null)
                                {
                                    db.Entry(exItem).CurrentValues.SetValues(row.Discharge);
                                }
                                else
                                {
                                    db.Discharges.Add(row.Discharge);
                                }
                                if(row.Discharge.Diagnoses != null)
                                {
                                    foreach(var dx in row.Discharge.Diagnoses)
                                    {
                                        var exDx = db.Diagnoses.Find(dx.SourceRecordIdentifier);
                                        if (exDx != null)
                                        {
                                            db.Entry(exDx).CurrentValues.SetValues(dx);
                                        }
                                        else
                                        {
                                            db.Diagnoses.Add(dx);
                                        }
                                    }
                                }
                                if (row.Discharge.Evaluations != null)
                                {
                                    foreach (var eval in row.Discharge.Evaluations)
                                    {
                                        var exEval = db.Evaluations.Find(eval.SourceRecordIdentifier);
                                        if (exEval != null)
                                        {
                                            db.Entry(exEval).CurrentValues.SetValues(eval);
                                        }
                                        else
                                        {
                                            db.Evaluations.Add(eval);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                db.SaveChanges();
            }
        }
        public static void UpsertServiceEvent(ServiceEvent serviceEvent)
        {
            using(var db = new fasams_db())
            {
                ServiceEvent existing = db.ServiceEvents
                    .Include(x => x.ServiceEventCoveredServiceModifiers)
                    .Include(x => x.ServiceEventHcpcsProcedureModifiers)
                    .Include(x => x.ServiceEventExpenditureModifiers)
                    .SingleOrDefault(s => s.SourceRecordIdentifier == serviceEvent.SourceRecordIdentifier);

                if (existing == null)
                {
                    db.ServiceEvents.Add(serviceEvent);
                    if (serviceEvent.ServiceEventCoveredServiceModifiers != null)
                    {
                        foreach(var row in serviceEvent.ServiceEventCoveredServiceModifiers)
                        {
                            db.CoveredServiceModifiers.Add(row);
                        }
                    }
                    if (serviceEvent.ServiceEventHcpcsProcedureModifiers != null)
                    {
                        foreach (var row in serviceEvent.ServiceEventHcpcsProcedureModifiers)
                        {
                            db.HcpcsProcedureModifiers.Add(row);
                        }
                    }
                    if (serviceEvent.ServiceEventExpenditureModifiers != null)
                    {
                        foreach (var row in serviceEvent.ServiceEventExpenditureModifiers)
                        {
                            db.ExpenditureModifiers.Add(row);
                        }
                    }
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(serviceEvent);
                }
                db.SaveChanges();
            }
        }
        public static void UpsertSubContract(Subcontract subcontract)
        {
            using(var db = new fasams_db())
            {
                Subcontract existing = db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .SingleOrDefault(s => s.ContractNumber == subcontract.ContractNumber && s.SubcontractNumber == subcontract.SubcontractNumber && s.AmendmentNumber == subcontract.AmendmentNumber);

                if (existing == null)
                {
                    db.Subcontracts.Add(subcontract);
                    if(subcontract.SubcontractServices != null)
                    {
                        foreach(var row in subcontract.SubcontractServices)
                        {
                            row.ContractNumber = subcontract.ContractNumber;
                            row.SubcontractNumber = subcontract.SubcontractNumber;
                            row.AmendmentNumber = subcontract.AmendmentNumber;
                            db.SubcontractServices.Add(row);
                        }
                    }
                    if (subcontract.SubcontractOutputMeasures != null)
                    {
                        foreach (var row in subcontract.SubcontractOutputMeasures)
                        {
                            row.ContractNumber = subcontract.ContractNumber;
                            row.SubcontractNumber = subcontract.SubcontractNumber;
                            row.AmendmentNumber = subcontract.AmendmentNumber;
                            db.SubcontractOutputMeasures.Add(row);
                        }
                    }
                    if (subcontract.SubcontractOutcomeMeasures != null)
                    {
                        foreach (var row in subcontract.SubcontractOutcomeMeasures)
                        {
                            row.ContractNumber = subcontract.ContractNumber;
                            row.SubcontractNumber = subcontract.SubcontractNumber;
                            row.AmendmentNumber = subcontract.AmendmentNumber;
                            db.SubcontractOutcomeMeasures.Add(row);
                        }
                    }
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(subcontract);
                    if (subcontract.SubcontractServices != null)
                    {
                        foreach (var row in subcontract.SubcontractServices)
                        {
                            var exRow = db.SubcontractServices.Find(row.SourceRecordIdentifier);
                            if (exRow != null)
                            {
                                db.Entry(exRow).CurrentValues.SetValues(row);
                            }
                            else
                            {
                                db.SubcontractServices.Add(row);
                            }
                        }
                        
                    }
                    if (subcontract.SubcontractOutputMeasures != null)
                    {
                        foreach (var row in subcontract.SubcontractOutputMeasures)
                        {
                            var exRow = db.SubcontractOutputMeasures.Find(row.ProgramAreaCode,row.ServiceCategoryCode, row.ContractNumber,row.SubcontractNumber, row.AmendmentNumber);
                            if (exRow != null)
                            {
                                db.Entry(exRow).CurrentValues.SetValues(row);
                            }
                            else
                            {
                                db.SubcontractOutputMeasures.Add(row);
                            }
                        }
                        
                    }
                    if (subcontract.SubcontractOutcomeMeasures != null)
                    {
                        foreach (var row in subcontract.SubcontractOutcomeMeasures)
                        {
                            var exRow = db.SubcontractOutcomeMeasures.Find(row.ProgramAreaCode, row.OutcomeMeasureCode, row.ContractNumber, row.SubcontractNumber, row.AmendmentNumber);
                            if (exRow != null)
                            {
                                db.Entry(exRow).CurrentValues.SetValues(row);
                            }
                            else
                            {
                                db.SubcontractOutcomeMeasures.Add(row);
                            }
                        }
                    }
                }
                db.SaveChanges();
            }
        }
    }
}
