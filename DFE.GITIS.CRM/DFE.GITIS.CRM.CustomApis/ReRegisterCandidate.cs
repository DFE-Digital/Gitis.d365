using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DFE.GITIS.CRM.CustomApis
{
    public class ReRegisterCandidate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracing = serviceProvider.Get<ITracingService>();
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var service = serviceProvider.Get<IOrganizationServiceFactory>().CreateOrganizationService(null);

            tracing.Trace("Executing ReRegisterCandidate custom API...");

            // Input parameter: candidateId (string GUID)
            if (!context.InputParameters.Contains("Target") || context.InputParameters["Target"] == null)
            {
                tracing.Trace("Input parameter 'candidateId' is missing or null.");
                return;
            }

            var candidateReference = context.InputParameters["Target"] as EntityReference;         

            // Candidate is Contact
            const string dfe_candidateadviserstatus = "dfe_candidateadviserstatus";                 // int/OptionSet? (or whole number)
            const string dfe_candidatestatus = "dfe_candidatestatus";              // OptionSet
            const string dfe_gitisttaservicestartdate = "dfe_gitisttaservicestartdate";
            const string dfe_candidatereregisterstatus = "dfe_candidatereregisterstatus";
            const string dfe_candidateadviserstatusreason = "dfe_candidateadviserstatusreason";

            // Retrieve candidate with required fields
            Entity candidate;
            try
            {
                candidate = service.Retrieve(
                    "contact",
                    candidateReference.Id,
                    new ColumnSet(dfe_candidateadviserstatus, dfe_candidateadviserstatusreason, dfe_candidatestatus, dfe_candidatereregisterstatus, dfe_gitisttaservicestartdate)
                );
            }
            catch (Exception ex)
            {
                tracing.Trace($"Failed to retrieve contact {candidateReference.Id}: {ex}");
                throw;
            }

            // Read AdviserStatusId
            int? candidateReRegisterStatus = null;

            var candidateReRegisterStatusOptionset = candidate.GetAttributeValue<OptionSetValue>(dfe_candidatereregisterstatus);
            if (candidateReRegisterStatusOptionset != null)
            {
                candidateReRegisterStatus = candidateReRegisterStatusOptionset.Value;
            }

            tracing.Trace($"candidateReRegisterStatus: {(candidateReRegisterStatus.HasValue ? candidateReRegisterStatus.Value.ToString() : "null")}");

            //if (candidateReRegisterStatus != (int)Candidate.RegistrationStatus.ReRegistered)
            //{
                // Apply updates
                var update = new Entity("contact", candidateReference.Id);

                update[dfe_candidatestatus] = new OptionSetValue((int)Candidate.CandidateStatus.WaitingToBeAssigned);
                update[dfe_candidatereregisterstatus] = new OptionSetValue((int)Candidate.RegistrationStatus.ReRegistered);
                update[dfe_candidateadviserstatus] = new OptionSetValue((int)Candidate.AdviserStatus.Open);
                update[dfe_candidateadviserstatusreason] = null;
                update[dfe_gitisttaservicestartdate] = DateTime.UtcNow;


                try
                {
                    service.Update(update);
                    context.OutputParameters["success"] = true;
                    tracing.Trace("Candidate updated: WaitingToBeAssigned + ReRegistered + StatusIsWaitingToBeAssignedAt set.");
                }
                catch (Exception ex)
                {
                    context.OutputParameters["success"] = false;
                    tracing.Trace($"Failed to update contact {candidateReference.Id}: {ex}");
                    throw;
                }
            //}
        }
    }

    // --- You already have these elsewhere; included as stubs to show expected shapes ---

    public enum ResubscribableAdviserStatus
    {
        // TODO: Put your valid resubscribable adviser statuses here.
        // Example:
        // Active = 1,
        // Dormant = 2
    }

    public static class Candidate
    {
        public enum CandidateStatus
        {
            WaitingToBeAssigned = 222750001
        }

        public enum AdviserStatus
        {
            Open = 222750000
        }

        public enum RegistrationStatus
        {
            ReRegistered = 222750000
        }
    }
}



