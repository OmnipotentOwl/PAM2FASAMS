﻿using PAM2FASAMS.DataContext;
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
        public static TreatmentEpisode OpportuniticlyLoadTreatmentSession(TreatmentEpisodeDataSet currentJob, UpdateType type, string recordDate, string clientSourceRecordIdentifier)
        {
            switch (type)
            {
                case UpdateType.Admission:
                    {
                        using (var db = new fasams_db())
                        {
                            TreatmentEpisode existing = db.TreatmentEpisodes
                                .Include(x => x.Admissions.Select(a => a.Discharge))
                                .Include(x => x.ImmediateDischarges)
                                .SingleOrDefault(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier);

                            if (existing == null)
                            {
                                existing = new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), Admissions = new List<Admission>() };
                            }

                            return existing;
                        }
                    }
                case UpdateType.Update:
                    {
                        if (currentJob.TreatmentEpisodes.Exists(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))))
                        {
                            return currentJob.TreatmentEpisodes.Where(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier && (e.Admissions.Exists(a => a.AdmissionDate == recordDate && a.Discharge == null))).SingleOrDefault();
                        }
                        else
                        {
                            using(var db = new fasams_db())
                            {
                                TreatmentEpisode existing = db.TreatmentEpisodes
                                    .Include(x => x.Admissions)
                                    .Include(x => x.ImmediateDischarges)
                                    .SingleOrDefault(e => e.ClientSourceRecordIdentifier == clientSourceRecordIdentifier);

                                return existing;
                            }
                        }
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
        public static TreatmentEpisode OpportuniticlyLoadTreatmentSession(string recordDate, string clientSourceRecordIdentifier, string FederalTaxIdentifier)
        {
            DateTime date = DateTime.Parse(recordDate);
            using(var db = new fasams_db())
            {
                List<TreatmentEpisode> existing = db.TreatmentEpisodes
                    .Include(x => x.Admissions.Select(a=> a.Discharge))
                    .Include(x => x.ImmediateDischarges)
                    .Where(c => c.SourceRecordIdentifier == clientSourceRecordIdentifier && c.FederalTaxIdentifier == FederalTaxIdentifier)
                    .ToList();

                if(existing == null)
                {
                    return null;
                }
                if(existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)))
                {
                    return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge == null)).FirstOrDefault();
                }
                if (existing.Any(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.Any(d=>d.InternalDischargeDate>=date))))
                {
                    return existing.Where(t => t.Admissions.Any(a => a.InternalAdmissionDate <= date && a.Discharge.Any(d => d.InternalDischargeDate >= date))).FirstOrDefault();
                }
                return null;
            }
        }
        public static Admission OpportuniticlyLoadAdmission(TreatmentEpisode episode, UpdateType type, string recordDate)
        {
            switch (type)
            {
                case UpdateType.Admission:
                    {
                        using(var db = new fasams_db())
                        {
                            Admission existing = db.Admissions
                                .Include(x => x.PerformanceOutcomeMeasures.Select(p => p.SubstanceUseDisorders))
                                .Include(x => x.Evaluations)
                                .Include(x => x.Diagnoses)
                                .Include(x => x.Discharge)
                                .SingleOrDefault(a => a.TreatmentSourceId == episode.SourceRecordIdentifier);

                            if(existing == null)
                            {
                                existing = new Admission { SourceRecordIdentifier = Guid.NewGuid().ToString() };
                            }

                            return existing;
                        }
                    }
                case UpdateType.Update:
                    {
                        if (episode.Admissions.Exists(a => a.TreatmentSourceId == episode.SourceRecordIdentifier && a.PerformanceOutcomeMeasures!=null))
                        {
                            return episode.Admissions.Where(a => a.TreatmentSourceId == episode.SourceRecordIdentifier).SingleOrDefault();
                        }
                        else
                        {
                            using (var db = new fasams_db())
                            {
                                Admission existing = db.Admissions
                                    .Include(x => x.PerformanceOutcomeMeasures.Select(p => p.SubstanceUseDisorders))
                                    .Include(x => x.Evaluations)
                                    .Include(x => x.Diagnoses)
                                    .Include(x => x.Discharge)
                                    .SingleOrDefault(a => a.TreatmentSourceId == episode.SourceRecordIdentifier);

                                return existing;
                            }
                        }
                    }
                case UpdateType.Discharge:
                    {
                        return null;
                    }
                default:
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
                    .Include(x => x.Admissions)
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
                            if(row.Discharge != null)
                            {
                                foreach (var item in row.Discharge)
                                {
                                    db.Discharges.Add(item);
                                }
                            }
                        }
                    }
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(treatmentEpisode);
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
    }
}
