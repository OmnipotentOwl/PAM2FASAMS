using PAM2FASAMS.DataContext;
using PAM2FASAMS.OutputFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using static PAM2FASAMS.PAMValidations;
using PAM2FASAMS.Models.Utils;

namespace PAM2FASAMS.Utilities
{
    public class DataTools
    {
        public async Task<TreatmentEpisode> OpportuniticlyLoadTreatmentSession(TreatmentEpisodeDataSet currentJob, UpdateType type, string recordDate, 
            string clientSourceRecordIdentifier, string federalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            switch (type)
            {
                case UpdateType.Admission:
                    {
                        List<TreatmentEpisode> existing = currentJob.TreatmentEpisodes.Where(c => c.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && c.FederalTaxIdentifier == federalTaxIdentifier).ToList();
                        if (existing == null || existing.Count == 0)
                        {
                            return await OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
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
                        return await OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
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
                            return await OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
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
                            return await OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.Admission, recordDate, clientSourceRecordIdentifier, federalTaxIdentifier);
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
                        return await OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType.ImDischarge, recordDate,clientSourceRecordIdentifier, federalTaxIdentifier);
                    }
                default:
                    return null;
            }
            
        }
        public async Task<TreatmentEpisode> OpportuniticlyLoadTreatmentSession(TreatmentEpisodeType type, string recordDate, string clientSourceRecordIdentifier, string federalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using(var db = new fasams_db())
            {
                List<TreatmentEpisode> existing = await db.TreatmentEpisodes
                    .Include(x => x.Admissions.Select(a=> a.Discharge))
                    .Include(x => x.ImmediateDischarges)
                    .Where(c => c.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && c.FederalTaxIdentifier == federalTaxIdentifier)
                    .ToListAsync();

                switch (type)
                {
                    case TreatmentEpisodeType.Admission:
                        {
                            if (existing == null || existing.Count == 0)
                            {
                                return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                    ClientSourceRecordIdentifier = clientSourceRecordIdentifier,
                                    FederalTaxIdentifier = federalTaxIdentifier,
                                    Admissions = new List<Admission>() }; ;
                            }
                            if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)))
                            {
                                return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)).LastOrDefault();
                            }
                            bool predicate1(Admission a) => a.InternalAdmissionDate <= date && a.Discharge?.InternalDischargeDate >= date;
                            if (existing.Any(t => t.Admissions.Any(a => predicate1(a) && a.Discharge?.TypeCode == "1")))
                            {
                                return existing.Where(t => t.Admissions.Any(a => predicate1(a) && a.Discharge.TypeCode == "1")).LastOrDefault();
                            }
                            if (existing.Any(t => t.Admissions.Any(a => predicate1(a) && a.Discharge?.TypeCode == "2")))
                            {
                                return existing.Where(t => t.Admissions.Any(a => predicate1(a) && a.Discharge.TypeCode == "2")).LastOrDefault();
                            }
                            return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                ClientSourceRecordIdentifier = clientSourceRecordIdentifier,
                                FederalTaxIdentifier = federalTaxIdentifier,
                                Admissions = new List<Admission>() }; ;
                        }
                    case TreatmentEpisodeType.ImDischarge:
                        {
                            // added a search component here as there isnt much guidence on when you should use an existing Treatment Episode vs when you should make a new one.
                            //int daysToSearch = 30; // will probably make this item configurable via a config file.
                            //DateTime lowerSearchRange = date.AddDays(-daysToSearch);
                            //DateTime upperSearchRange = date.AddDays(daysToSearch);
                            if (existing == null || existing.Count == 0)
                            {
                                return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                    ClientSourceRecordIdentifier = clientSourceRecordIdentifier,
                                    FederalTaxIdentifier = federalTaxIdentifier,
                                    ImmediateDischarges = new List<ImmediateDischarge>() }; ;
                            }
                            if (existing.Any(t => t.ImmediateDischarges.Any(i => i.InternalEvaluationDate == date)))
                            {
                                return existing.Where(t => t.ImmediateDischarges.Any(i => i.InternalEvaluationDate == date)).FirstOrDefault();
                            }
                            return new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                ClientSourceRecordIdentifier = clientSourceRecordIdentifier,
                                FederalTaxIdentifier = federalTaxIdentifier,
                                ImmediateDischarges = new List<ImmediateDischarge>() }; ;
                        }
                    default:
                        return null;
                }
            }
        }
        public async Task<TreatmentEpisode> OpportuniticlyLoadTreatmentSession(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<TreatmentEpisode> existing = await db.TreatmentEpisodes
                    .Include(x => x.Admissions.Select(a => a.Discharge))
                    .Include(x => x.ImmediateDischarges)
                    .Where(t => t.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();

                if(existing.Any(t => t.SourceRecordIdentifier == sourceRecordIdentifier))
                {
                    return existing.Where(t => t.SourceRecordIdentifier == sourceRecordIdentifier).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<ImmediateDischarge> OpportuniticlyLoadImmediateDischarge(TreatmentEpisode episode, UpdateType type, string recordDate)
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
                            return await OpportuniticlyLoadImmediateDischarge(episode, recordDate);
                        }
                    }
                default:
                    return null;
            }
        }
        public async Task<ImmediateDischarge> OpportuniticlyLoadImmediateDischarge(TreatmentEpisode episode, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<ImmediateDischarge> existing = await db.ImmediateDischarges
                    .Where(i => i.TreatmentSourceId == episode.SourceRecordIdentifier)
                    .ToListAsync();
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
        public async Task<ImmediateDischarge> OpportuniticlyLoadImmediateDischarge(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<ImmediateDischarge> existing = await db.ImmediateDischarges
                    .Where(i => i.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();
                
                if (existing.Any(i => i.SourceRecordIdentifier == sourceRecordIdentifier))
                {
                    return existing.Where(i => i.SourceRecordIdentifier == sourceRecordIdentifier).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<Admission> OpportuniticlyLoadAdmission(TreatmentEpisode episode, UpdateType type, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            switch (type)
            {
                case UpdateType.Admission:
                    {
                        return await OpportuniticlyLoadAdmission(episode, recordDate);
                    }
                case UpdateType.Update:
                    {
                        if (episode.Admissions.Exists(a => a.TreatmentSourceId == episode.SourceRecordIdentifier && a.PerformanceOutcomeMeasures!=null && a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)))
                        {
                            return episode.Admissions.Where(a => a.TreatmentSourceId == episode.SourceRecordIdentifier && a.PerformanceOutcomeMeasures != null && a.InternalAdmissionDate <= date && (a.Discharge == null || a.Discharge.InternalDischargeDate >= date)).SingleOrDefault();
                        }
                        else
                        {
                            return await OpportuniticlyLoadAdmission(episode, recordDate);
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
                            return await OpportuniticlyLoadAdmission(episode, recordDate);
                        }
                    }
                default:
                    return null;
            }
        }
        public async Task<Admission> OpportuniticlyLoadAdmission(TreatmentEpisode episode, string recordDate, string treatmentSetting)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<Admission> existing = await db.Admissions
                    .Include(x => x.PerformanceOutcomeMeasures.Select(p => p.SubstanceUseDisorders))
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Include(x => x.Discharge)
                    .Where(a => a.TreatmentSourceId == episode.SourceRecordIdentifier)
                    .ToListAsync();

                if (existing == null || existing.Count == 0)
                {
                    return new Admission { SourceRecordIdentifier = Guid.NewGuid().ToString(), Evaluations = new List<Evaluation>(), Diagnoses = new List<Diagnosis>() };
                }
                bool predicate1(Admission a) => a.InternalAdmissionDate <= date && a.Discharge == null && a.TreatmentSettingCode == treatmentSetting;
                if (existing.Any(predicate1))
                {
                    return existing.Where(predicate1).FirstOrDefault();
                }
                bool predicate2(Admission a) => a.InternalAdmissionDate <= date && a.Discharge?.InternalDischargeDate >= date && a.TreatmentSettingCode == treatmentSetting;
                if (existing.Any(predicate2))
                {
                    return existing.Where(predicate2).FirstOrDefault();
                }
                return await OpportuniticlyLoadAdmission(episode, recordDate);
            }
        }
        public async Task<Admission> OpportuniticlyLoadAdmission(TreatmentEpisode episode, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            using(var db = new fasams_db())
            {
                List<Admission> existing = await db.Admissions
                    .Include(x => x.PerformanceOutcomeMeasures.Select(p => p.SubstanceUseDisorders))
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Include(x => x.Discharge)
                    .Where(a => a.TreatmentSourceId == episode.SourceRecordIdentifier)
                    .ToListAsync();

                if (existing == null || existing.Count == 0)
                {
                    return new Admission { SourceRecordIdentifier = Guid.NewGuid().ToString(), Evaluations = new List<Evaluation>(), Diagnoses = new List<Diagnosis>() };
                }
                bool predicate1(Admission a) => a.InternalAdmissionDate <= date && a.Discharge == null;
                if (existing.Any(predicate1))
                {
                    return existing.Where(predicate1).FirstOrDefault();
                }
                bool predicate2(Admission a) => a.InternalAdmissionDate <= date && a.Discharge?.InternalDischargeDate >= date;
                if (existing.Any(predicate2))
                {
                    return existing.Where(predicate2).FirstOrDefault();
                }
                return new Admission { SourceRecordIdentifier = Guid.NewGuid().ToString(), Evaluations = new List<Evaluation>(), Diagnoses = new List<Diagnosis>() };
            }
        }
        public async Task<Admission> OpportuniticlyLoadAdmission(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<Admission> existing = await db.Admissions
                    .Include(x => x.PerformanceOutcomeMeasures.Select(p => p.SubstanceUseDisorders))
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Include(x => x.Discharge)
                    .Where(a => a.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();

                bool predicate2(Admission a) => a.SourceRecordIdentifier == sourceRecordIdentifier;
                if (existing.Any(predicate2))
                {
                    return existing.Where(predicate2).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<Admission> OpportuniticlyLoadAdmission(Discharge discharge)
        {
            using (var db = new fasams_db())
            {
                List<Admission> existing = await db.Admissions
                    .Include(x => x.PerformanceOutcomeMeasures.Select(p => p.SubstanceUseDisorders))
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Include(x => x.Discharge)
                    .Where(a => a.Discharge_SourceRecordIdentifier == discharge.SourceRecordIdentifier)
                    .ToListAsync();

                bool predicate2(Admission a) => a.Discharge_SourceRecordIdentifier == discharge.SourceRecordIdentifier;
                if (existing.Any(predicate2))
                {
                    return existing.Where(predicate2).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<Discharge> OpportuniticlyLoadDischarge(Admission admission, string recordDate)
        {
            DateTime date = DateTime.Parse(recordDate);
            using(var db = new fasams_db())
            {
                List<Discharge> existing = await db.Discharges
                    .Include(x => x.PerformanceOutcomeMeasures)
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Where(d => d.SourceRecordIdentifier == admission.Discharge_SourceRecordIdentifier)
                    .ToListAsync();

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
        public async Task<Discharge> OpportuniticlyLoadDischarge(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<Discharge> existing = await db.Discharges
                    .Include(x => x.PerformanceOutcomeMeasures)
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Where(d => d.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();

                if (existing.Any(i => i.SourceRecordIdentifier == sourceRecordIdentifier))
                {
                    return existing.Where(i => i.SourceRecordIdentifier == sourceRecordIdentifier).SingleOrDefault();
                }
                return null;
            }
        }
        public async Task<Discharge> OpportuniticlyLoadDischarge(PerformanceOutcomeMeasure performanceOutcome)
        {
            using (var db = new fasams_db())
            {
                List<Discharge> existing = await db.Discharges
                    .Include(x => x.PerformanceOutcomeMeasures)
                    .Include(x => x.Evaluations)
                    .Include(x => x.Diagnoses)
                    .Where(d => d.PerformanceOutcomeMeasures.SourceRecordIdentifier == performanceOutcome.SourceRecordIdentifier)
                    .ToListAsync();

                if (existing.Any(i => i.PerformanceOutcomeMeasures?.SourceRecordIdentifier == performanceOutcome.SourceRecordIdentifier))
                {
                    return existing.Where(i => i.PerformanceOutcomeMeasures?.SourceRecordIdentifier == performanceOutcome.SourceRecordIdentifier).SingleOrDefault();
                }
                return null;
            }
        }
        public async Task<ProviderClient> OpportuniticlyLoadProviderClient(ProviderClients currentJob, ProviderClientIdentifier SSN, string FederalTaxIdentifier)
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
                    existing = await db.ProviderClientIdentifiers.SingleAsync(i => i.FederalTaxIdentifier == FederalTaxIdentifier && i.Identifier == SSN.Identifier);
                }
                return await OpportuniticlyLoadProviderClient(currentJob, existing.ClientSourceId, FederalTaxIdentifier);
            }
        }
        public async Task<ProviderClient> OpportuniticlyLoadProviderClient(ProviderClientIdentifier SSN, string FederalTaxIdentifier)
        { 
            ProviderClientIdentifier existing = new ProviderClientIdentifier();
            using (var db = new fasams_db())
            {
                existing = await db.ProviderClientIdentifiers.SingleOrDefaultAsync(i => i.FederalTaxIdentifier == FederalTaxIdentifier && i.Identifier == SSN.Identifier);
            }
            if(existing != null)
            {
                return await OpportuniticlyLoadProviderClient(existing.ClientSourceId, FederalTaxIdentifier);
            }
            return new ProviderClient { ProviderClientIdentifiers = new List<ProviderClientIdentifier>() };
        }
        public async Task<ProviderClient> OpportuniticlyLoadProviderClient(ProviderClients currentJob, string SourceRecordIdentifier, string FederalTaxIdentifier)
        {
            if (currentJob.clients.Exists(c => c.SourceRecordIdentifier == SourceRecordIdentifier && c.FederalTaxIdentifier == FederalTaxIdentifier))
            {
                return currentJob.clients.Where(c => c.SourceRecordIdentifier == SourceRecordIdentifier && c.FederalTaxIdentifier == FederalTaxIdentifier).Single();
            }
            else
            {
                return await OpportuniticlyLoadProviderClient(SourceRecordIdentifier, FederalTaxIdentifier);
            }
        }
        public async Task<ProviderClient> OpportuniticlyLoadProviderClient(string SourceRecordIdentifier, string FederalTaxIdentifier)
        {
            using (var db = new fasams_db())
            {
                ProviderClient existing = await db.ProviderClients
                    .Include(x => x.ProviderClientIdentifiers)
                    .Include(x => x.ProviderClientPhones)
                    .Include(x => x.ProviderClientEmailAddresses)
                    .Include(x => x.ProviderClientPhysicalAddresses)
                    .SingleOrDefaultAsync(c => c.SourceRecordIdentifier == SourceRecordIdentifier && c.FederalTaxIdentifier == FederalTaxIdentifier);

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
        public async Task<ProviderClient> OpportuniticlyLoadProviderClient(string SourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                ProviderClient existing = await db.ProviderClients
                    .Include(x => x.ProviderClientIdentifiers)
                    .Include(x => x.ProviderClientPhones)
                    .Include(x => x.ProviderClientEmailAddresses)
                    .Include(x => x.ProviderClientPhysicalAddresses)
                    .SingleOrDefaultAsync(c => c.SourceRecordIdentifier == SourceRecordIdentifier);

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
        public async Task<Subcontract> OpportuniticlyLoadSubcontract(string contractNum, string subcontractNum, string recordDate, string FederalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<Subcontract> existing = await db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .Where(c => c.FederalTaxIdentifier == FederalTaxIdentifier && c.ContractNumber == contractNum && c.SubcontractNumber == subcontractNum)
                    .ToListAsync();

                if (existing == null || existing.Count == 0)
                {
                    throw new InvalidOperationException("Missing Contract Data, please add contract information to DB!");
                }
                if (existing.Any(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "2" && c.InternalAmendmentDate <= date))
                {
                    return existing.Where(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "2" && c.InternalAmendmentDate <= date).LastOrDefault();
                }
                if (existing.Any(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "1"))
                {
                    return existing.Where(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "1").LastOrDefault();
                }
                return await OpportuniticlyLoadSubcontract(subcontractNum,recordDate,FederalTaxIdentifier);
            }
        }
        public async Task<Subcontract> OpportuniticlyLoadSubcontract(string subcontractNum, string recordDate, string FederalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<Subcontract> existing = await db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .Where(c => c.FederalTaxIdentifier == FederalTaxIdentifier && c.SubcontractNumber == subcontractNum)
                    .ToListAsync();

                if (existing == null || existing.Count == 0)
                {
                    throw new InvalidOperationException("Missing Contract Data, please add contract information to DB!");
                }
                if (existing.Any(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date ) && c.TypeCode == "2" && c.InternalAmendmentDate <= date))
                {
                    return existing.Where(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "2" && c.InternalAmendmentDate <= date).LastOrDefault();
                }
                if (existing.Any(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "1"))
                {
                    return existing.Where(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "1").LastOrDefault();
                }
                return await OpportuniticlyLoadSubcontract(recordDate,FederalTaxIdentifier);
            }
        }
        public async Task<Subcontract> OpportuniticlyLoadSubcontract(string recordDate, string FederalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using (var db = new fasams_db())
            {
                List<Subcontract> existing = await db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .Where(c => c.FederalTaxIdentifier == FederalTaxIdentifier)
                    .ToListAsync();

                if(existing == null || existing.Count == 0)
                {
                    throw new InvalidOperationException("Missing Contract Data, please add contract information to DB!");
                }
                if (existing.Any(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "2" && c.InternalAmendmentDate <= date))
                {
                    return existing.Where(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "2" && c.InternalAmendmentDate <= date).LastOrDefault();
                }
                if (existing.Any(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "1"))
                {
                    return existing.Where(c => (c.InternalEffectiveDate <= date && c.InternalExpirationDate >= date) && c.TypeCode == "1").LastOrDefault();
                }
                return null;
            }
        }
        public async Task<ServiceEvent> OpportuniticlyLoadServiceEvent(ServiceEvents currentJob, ServiceEventType type, ServiceEvent service)
        {
            return await OpportuniticlyLoadServiceEvent(type, service);
        }
        public async Task<ServiceEvent> OpportuniticlyLoadServiceEvent(ServiceEventType type, ServiceEvent service)
        {
            switch (type)
            {
                case ServiceEventType.Service:
                    {
                        using (var db = new fasams_db())
                        {
                            List<ServiceEvent> existing = await db.ServiceEvents
                                .Include(x => x.ServiceEventCoveredServiceModifiers)
                                .Include(x => x.ServiceEventHcpcsProcedureModifiers)
                                .Include(x => x.ServiceEventExpenditureModifiers)
                                .Where(s => s.TypeCode == "1" && s.AdmissionSourceRecordIdentifier == service.AdmissionSourceRecordIdentifier 
                                && s.EpisodeSourceRecordIdentifier == service.EpisodeSourceRecordIdentifier)
                                .ToListAsync();
                            if (existing == null || existing.Count == 0)
                            {
                                return new ServiceEvent {
                                    ServiceEventCoveredServiceModifiers = new List<ServiceEventCoveredServiceModifier>(),
                                    ServiceEventHcpcsProcedureModifiers = new List<ServiceEventHcpcsProcedureModifier>(),
                                    ServiceEventExpenditureModifiers = new List<ServiceEventExpenditureModifier>()
                                };
                            }
                            bool predicate1(ServiceEvent s) => s.SiteIdentifier == service.SiteIdentifier && s.CoveredServiceCode == service.CoveredServiceCode
                                && s.HcpcsProcedureCode == service.HcpcsProcedureCode && s.ServiceDate == service.ServiceDate && s.ServiceCountyAreaCode == service.ServiceCountyAreaCode
                                && s.StaffEducationLevelCode == service.StaffEducationLevelCode && s.StaffIdentifier == service.StaffIdentifier
                                && s.TreatmentSettingCode == service.TreatmentSettingCode;
                            if (existing.Any(predicate1))
                            {
                                return existing.Where(predicate1).LastOrDefault();
                            }
                            return new ServiceEvent {
                                ServiceEventCoveredServiceModifiers = new List<ServiceEventCoveredServiceModifier>(),
                                ServiceEventHcpcsProcedureModifiers = new List<ServiceEventHcpcsProcedureModifier>(),
                                ServiceEventExpenditureModifiers = new List<ServiceEventExpenditureModifier>()
                            }; ;
                        }
                    }
                case ServiceEventType.Event:
                    {
                        using (var db = new fasams_db())
                        {
                            List<ServiceEvent> existing = await db.ServiceEvents
                                .Include(x => x.ServiceEventCoveredServiceModifiers)
                                .Include(x => x.ServiceEventHcpcsProcedureModifiers)
                                .Include(x => x.ServiceEventExpenditureModifiers)
                                .Where(s => s.TypeCode == "2" && s.FederalTaxIdentifier == service.FederalTaxIdentifier && s.ContractNumber == service.ContractNumber 
                                && s.SubcontractNumber == service.SubcontractNumber)
                                .ToListAsync();
                            if (existing == null || existing.Count == 0)
                            {
                                return new ServiceEvent {
                                    ServiceEventCoveredServiceModifiers = new List<ServiceEventCoveredServiceModifier>(),
                                    ServiceEventHcpcsProcedureModifiers = new List<ServiceEventHcpcsProcedureModifier>(),
                                    ServiceEventExpenditureModifiers = new List<ServiceEventExpenditureModifier>() };
                            }
                            bool predicate1(ServiceEvent s) => s.SiteIdentifier == service.SiteIdentifier && s.CoveredServiceCode == service.CoveredServiceCode 
                                && s.HcpcsProcedureCode == service.HcpcsProcedureCode && s.ServiceDate == service.ServiceDate && s.ServiceCountyAreaCode == service.ServiceCountyAreaCode 
                                && s.StaffEducationLevelCode == service.StaffEducationLevelCode && s.StaffIdentifier == service.StaffIdentifier 
                                && s.TreatmentSettingCode == service.TreatmentSettingCode;
                            if (existing.Any(predicate1))
                            {
                                return existing.Where(predicate1).LastOrDefault();
                            }
                            return new ServiceEvent {
                                ServiceEventCoveredServiceModifiers = new List<ServiceEventCoveredServiceModifier>(),
                                ServiceEventHcpcsProcedureModifiers = new List<ServiceEventHcpcsProcedureModifier>(),
                                ServiceEventExpenditureModifiers = new List<ServiceEventExpenditureModifier>()
                            }; ;
                        }
                    }
                default:
                    return null;
            }
            
        }
        public async Task<ServiceEvent> OpportuniticlyLoadServiceEvent(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<ServiceEvent> existing = await db.ServiceEvents
                    .Include(x => x.ServiceEventCoveredServiceModifiers)
                    .Include(x => x.ServiceEventHcpcsProcedureModifiers)
                    .Include(x => x.ServiceEventExpenditureModifiers)
                    .Where(s => s.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();
                if (existing.Any(s => s.SourceRecordIdentifier == sourceRecordIdentifier))
                {
                    return existing.Where(s => s.SourceRecordIdentifier == sourceRecordIdentifier).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<PerformanceOutcomeMeasure> OpportuniticlyLoadPerformanceOutcomeMeasure(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<PerformanceOutcomeMeasure> existing = await db.PerformanceOutcomeMeasures
                    .Include(x => x.SubstanceUseDisorders)
                    .Where(p => p.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();
                if (existing.Any(s => s.SourceRecordIdentifier == sourceRecordIdentifier))
                {
                    return existing.Where(s => s.SourceRecordIdentifier == sourceRecordIdentifier).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<Evaluation> OpportuniticlyLoadEvaluation(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<Evaluation> existing = await db.Evaluations
                    .Where(p => p.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();
                if (existing.Any(s => s.SourceRecordIdentifier == sourceRecordIdentifier))
                {
                    return existing.Where(s => s.SourceRecordIdentifier == sourceRecordIdentifier).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<Diagnosis> OpportuniticlyLoadDiagnosis(string sourceRecordIdentifier)
        {
            using (var db = new fasams_db())
            {
                List<Diagnosis> existing = await db.Diagnoses
                    .Where(p => p.SourceRecordIdentifier == sourceRecordIdentifier)
                    .ToListAsync();
                if (existing.Any(s => s.SourceRecordIdentifier == sourceRecordIdentifier))
                {
                    return existing.Where(s => s.SourceRecordIdentifier == sourceRecordIdentifier).FirstOrDefault();
                }
                return null;
            }
        }
        public async Task<List<JobLog>> LoadPendingJobs()
        {
            using (var db = new fasams_db())
            {
                return await db.JobLogs.Where(j => j.UpdatedAt == null).ToListAsync();
            }
        }
        public async Task<List<Subcontract>> GetAllSubcontracts()
        {
            using(var db = new fasams_db())
            {
                return await db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .ToListAsync();
            }
        }
        public int GetMaxJobNumber()
        {
            using (var db = new fasams_db())
            {
                return db.JobLogs.Select(j => j.JobNumber).DefaultIfEmpty(0).Max();
            }
        }
        public async Task UpsertProviderClient(ProviderClient providerClient)
        {
            using(var db = new fasams_db())
            {
                ProviderClient existing = await db.ProviderClients
                    .Include(x => x.ProviderClientIdentifiers)
                    .Include(x => x.ProviderClientPhones)
                    .Include(x => x.ProviderClientEmailAddresses)
                    .Include(x => x.ProviderClientPhysicalAddresses)
                    .SingleOrDefaultAsync(c => c.SourceRecordIdentifier == providerClient.SourceRecordIdentifier && c.FederalTaxIdentifier == providerClient.FederalTaxIdentifier);

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
                await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "CL", SourceRecordId = providerClient.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
                await db.SaveChangesAsync();
            }
        }
        public async Task UpsertTreatmentSession(TreatmentEpisode treatmentEpisode)
        {
            using(var db = new fasams_db())
            {
                TreatmentEpisode existing = await db.TreatmentEpisodes
                    .Include(x => x.Admissions.Select(a => a.Discharge))
                    .Include(x => x.ImmediateDischarges)
                    .SingleOrDefaultAsync(e => e.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier && e.FederalTaxIdentifier == treatmentEpisode.FederalTaxIdentifier);

                if(existing == null)
                {
                    db.TreatmentEpisodes.Add(treatmentEpisode);
                    await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "TE", SourceRecordId = treatmentEpisode.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
                    if (treatmentEpisode.Admissions != null)
                    {
                        foreach (var row in treatmentEpisode.Admissions)
                        {
                            await UpsertAdmission(row, db);
                            if (row.PerformanceOutcomeMeasures != null)
                            {
                                foreach (var perf in row.PerformanceOutcomeMeasures)
                                {
                                    await UpsertPerformanceOutcomeMeasure(perf, db);
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
                                    await UpsertEvaluation(item, db);
                                }
                            }
                            if(row.Diagnoses != null)
                            {
                                foreach (var item in row.Diagnoses)
                                {
                                    await UpsertDiagnosis(item, db);
                                }
                            }
                            if(row.Discharge != null && row.Discharge.SourceRecordIdentifier != null)
                            {
                                await UpsertDischarge(row.Discharge, db);
                            }
                        }
                    }
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(treatmentEpisode);
                    await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "TE", SourceRecordId = treatmentEpisode.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
                    if (treatmentEpisode.Admissions != null)
                    {
                        foreach (var row in treatmentEpisode.Admissions)
                        {
                            await UpsertAdmission(row, db);
                            if (row.PerformanceOutcomeMeasures != null)
                            {
                                foreach (var perf in row.PerformanceOutcomeMeasures)
                                {
                                    await UpsertPerformanceOutcomeMeasure(perf, db);
                                    if (perf.SubstanceUseDisorders != null)
                                    {
                                        foreach (var sad in perf.SubstanceUseDisorders)
                                        {
                                            throw new MissingMethodException();
                                        }
                                    }
                                }
                            }
                            if (row.Evaluations != null)
                            {
                                foreach (var item in row.Evaluations)
                                {
                                    await UpsertEvaluation(item, db);
                                }
                            }
                            if (row.Diagnoses != null)
                            {
                                foreach (var item in row.Diagnoses)
                                {
                                    await UpsertDiagnosis(item, db);
                                }
                            }
                            if (row.Discharge != null)
                            {
                                await UpsertDischarge(row.Discharge, db);
                                if (row.Discharge.Diagnoses != null)
                                {
                                    foreach(var dx in row.Discharge.Diagnoses)
                                    {
                                        await UpsertDiagnosis(dx, db);
                                    }
                                }
                                if (row.Discharge.Evaluations != null)
                                {
                                    foreach (var eval in row.Discharge.Evaluations)
                                    {
                                        await UpsertEvaluation(eval, db);
                                    }
                                }
                            }
                        }
                    }
                }
                await db.SaveChangesAsync();
            }           
        }
        public async Task UpsertAdmission(Admission admission, fasams_db db, JobLog job)
        {
            Admission existing = await db.Admissions.FindAsync(admission.SourceRecordIdentifier);
            if(existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(admission);
            }
            else
            {
                db.Admissions.Add(admission);
            }
            await UpsertJobLog(job, db);
        }
        public async Task UpsertAdmission(Admission admission, fasams_db db)
        {
            Admission existing = await db.Admissions.FindAsync(admission.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(admission);
            }
            else
            {
                db.Admissions.Add(admission);
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "AD", SourceRecordId = admission.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
        }
        public async Task UpsertAdmission(Admission admission)
        {
            using(var db = new fasams_db())
            {
                Admission existing = await db.Admissions
                    .SingleOrDefaultAsync(a => a.SourceRecordIdentifier == admission.SourceRecordIdentifier);
                if (existing == null)
                {
                    db.Admissions.Add(admission);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(admission);
                }
                await db.SaveChangesAsync();
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "AD", SourceRecordId = admission.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow });
        }
        public async Task UpsertPerformanceOutcomeMeasure(PerformanceOutcomeMeasure perf, fasams_db db, JobLog job)
        {
            PerformanceOutcomeMeasure existing = await db.PerformanceOutcomeMeasures.FindAsync(perf.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(perf);
            }
            else
            {
                db.PerformanceOutcomeMeasures.Add(perf);
            }
            await UpsertJobLog(job, db);
        }
        public async Task UpsertPerformanceOutcomeMeasure(PerformanceOutcomeMeasure perf, fasams_db db)
        {
            PerformanceOutcomeMeasure existing = await db.PerformanceOutcomeMeasures.FindAsync(perf.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(perf);
            }
            else
            {
                db.PerformanceOutcomeMeasures.Add(perf);
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "PM", SourceRecordId = perf.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
        }
        public async Task UpsertPerformanceOutcomeMeasure(PerformanceOutcomeMeasure perf)
        {
            using (var db = new fasams_db())
            {
                PerformanceOutcomeMeasure existing = await db.PerformanceOutcomeMeasures
                    .SingleOrDefaultAsync(a => a.SourceRecordIdentifier == perf.SourceRecordIdentifier);
                if (existing == null)
                {
                    db.PerformanceOutcomeMeasures.Add(perf);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(perf);
                }
                await db.SaveChangesAsync();
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "PM", SourceRecordId = perf.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow });
        }
        public async Task UpsertEvaluation(Evaluation evaluation, fasams_db db, JobLog job)
        {
            Evaluation existing = await db.Evaluations.FindAsync(evaluation.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(evaluation);
            }
            else
            {
                db.Evaluations.Add(evaluation);
            }
            await UpsertJobLog(job, db);
        }
        public async Task UpsertEvaluation(Evaluation evaluation, fasams_db db)
        {
            Evaluation existing = await db.Evaluations.FindAsync(evaluation.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(evaluation);
            }
            else
            {
                db.Evaluations.Add(evaluation);
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "EV", SourceRecordId = evaluation.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
        }
        public async Task UpsertEvaluation(Evaluation evaluation)
        {
            using (var db = new fasams_db())
            {
                Evaluation existing = await db.Evaluations
                    .SingleOrDefaultAsync(a => a.SourceRecordIdentifier == evaluation.SourceRecordIdentifier);
                if (existing == null)
                {
                    db.Evaluations.Add(evaluation);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(evaluation);
                }
                await db.SaveChangesAsync();
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "EV", SourceRecordId = evaluation.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow });
        }
        public async Task UpsertDiagnosis(Diagnosis diagnosis, fasams_db db, JobLog job)
        {
            Diagnosis existing = await db.Diagnoses.FindAsync(diagnosis.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(diagnosis);
            }
            else
            {
                db.Diagnoses.Add(diagnosis);
            }
            await UpsertJobLog(job, db);
        }
        public async Task UpsertDiagnosis(Diagnosis diagnosis, fasams_db db)
        {
            Diagnosis existing = await db.Diagnoses.FindAsync(diagnosis.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(diagnosis);
            }
            else
            {
                db.Diagnoses.Add(diagnosis);
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "DX", SourceRecordId = diagnosis.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
        }
        public async Task UpsertDiagnosis(Diagnosis diagnosis)
        {
            using (var db = new fasams_db())
            {
                Diagnosis existing = await db.Diagnoses
                    .SingleOrDefaultAsync(a => a.SourceRecordIdentifier == diagnosis.SourceRecordIdentifier);
                if (existing == null)
                {
                    db.Diagnoses.Add(diagnosis);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(diagnosis);
                }
                await db.SaveChangesAsync();
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "DX", SourceRecordId = diagnosis.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow });
        }
        public async Task UpsertDischarge(Discharge discharge, fasams_db db, JobLog job)
        {
            Discharge existing = await db.Discharges.FindAsync(discharge.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(discharge);
            }
            else
            {
                db.Discharges.Add(discharge);
            }
            await UpsertJobLog(job, db);
        }
        public async Task UpsertDischarge(Discharge discharge, fasams_db db)
        {
            Discharge existing = await db.Discharges.FindAsync(discharge.SourceRecordIdentifier);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(discharge);
            }
            else
            {
                db.Discharges.Add(discharge);
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "DC", SourceRecordId = discharge.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
        }
        public async Task UpsertDischarge(Discharge discharge)
        {
            using (var db = new fasams_db())
            {
                Discharge existing = await db.Discharges
                    .SingleOrDefaultAsync(a => a.SourceRecordIdentifier == discharge.SourceRecordIdentifier);
                if (existing == null)
                {
                    db.Discharges.Add(discharge);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(discharge);
                }
                await db.SaveChangesAsync();
            }
            await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "DC", SourceRecordId = discharge.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow });
        }
        public async Task UpsertServiceEvent(ServiceEvent serviceEvent)
        {
            using(var db = new fasams_db())
            {
                ServiceEvent existing = await db.ServiceEvents
                    .Include(x => x.ServiceEventCoveredServiceModifiers)
                    .Include(x => x.ServiceEventHcpcsProcedureModifiers)
                    .Include(x => x.ServiceEventExpenditureModifiers)
                    .SingleOrDefaultAsync(s => s.SourceRecordIdentifier == serviceEvent.SourceRecordIdentifier);

                if (existing == null)
                {
                    db.ServiceEvents.Add(serviceEvent);
                    await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "SE", SourceRecordId = serviceEvent.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
                    if (serviceEvent.ServiceEventCoveredServiceModifiers != null)
                    {
                        foreach(var row in serviceEvent.ServiceEventCoveredServiceModifiers)
                        {
                            db.ServiceEventCoveredServiceModifiers.Add(row);
                        }
                    }
                    if (serviceEvent.ServiceEventHcpcsProcedureModifiers != null)
                    {
                        foreach (var row in serviceEvent.ServiceEventHcpcsProcedureModifiers)
                        {
                            db.ServiceEventHcpcsProcedureModifiers.Add(row);
                        }
                    }
                    if (serviceEvent.ServiceEventExpenditureModifiers != null)
                    {
                        foreach (var row in serviceEvent.ServiceEventExpenditureModifiers)
                        {
                            db.ServiceEventExpenditureModifiers.Add(row);
                        }
                    }
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(serviceEvent);
                    await UpsertJobLog(new JobLog { JobNumber = PAMConvert.JobNumber, RecordType = "SE", SourceRecordId = serviceEvent.SourceRecordIdentifier, CreatedAt = DateTime.UtcNow }, db);
                }
                await db.SaveChangesAsync();
            }
        }
        public async Task UpsertSubContract(Subcontract subcontract)
        {
            using(var db = new fasams_db())
            {
                Subcontract existing = await db.Subcontracts
                    .Include(x => x.SubcontractServices)
                    .Include(x => x.SubcontractOutputMeasures)
                    .Include(x => x.SubcontractOutcomeMeasures)
                    .SingleOrDefaultAsync(s => s.ContractNumber == subcontract.ContractNumber 
                    && s.SubcontractNumber == subcontract.SubcontractNumber && s.AmendmentNumber == subcontract.AmendmentNumber);

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
                await db.SaveChangesAsync();
            }
        }
        public async Task UpsertJobLog(JobLog job, fasams_db db)
        {
            
            JobLog existing = await db.JobLogs
                .SingleOrDefaultAsync(j => j.JobNumber == job.JobNumber && j.RecordType == job.RecordType && j.SourceRecordId == job.SourceRecordId);

            if (existing == null)
            {
                db.JobLogs.Add(job);
            }
            else
            {
                db.Entry(existing).CurrentValues.SetValues(job);
            }
            //db.SaveChanges();
        }
        public async Task UpsertJobLog(JobLog job)
        {
            using (var db = new fasams_db())
            {
                JobLog existing = await db.JobLogs
                    .SingleOrDefaultAsync(j => j.JobNumber == job.JobNumber && j.RecordType == job.RecordType && j.SourceRecordId == job.SourceRecordId);

                if (existing == null)
                {
                    db.JobLogs.Add(job);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(job);
                }
                await db.SaveChangesAsync();
            }
        }
        public async Task MarkJobBatchComplete(int JobId)
        {
            var CompletedDate = DateTime.UtcNow;
            using (var db = new fasams_db())
            {
                await db.Database.ExecuteSqlCommandAsync("UPDATE JobLogs SET UpdatedAt = {0} WHERE JobNumber = {1}", CompletedDate, JobId);
            }
        }
    }
}
