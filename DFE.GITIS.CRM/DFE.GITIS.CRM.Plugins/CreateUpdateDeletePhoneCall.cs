/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 13-12-2018
 Description: This plugin is to create,update and delete of the appointment record based on the Phone call record create, update and delete.
 This is mainly used to sync between phone call records and Appointent records which are show in Adviser calendar.
 Create step: Post operation and Synchronous
 Update step: Post operation and Synchronous. PostImage was registered.
 Delete step: Pre validation and pre opertaion steps in synchronous mode. 

 Author: Arjun Musuvathy
 Date: 26-02-2020
 Description: Piece of code is added to update the Required Attendees in Appointment This is to fix GCLS-674 Reassign issue.
 "From" field from Phone call record is copied into the "Required Attendees" field in Appointment.
 
 Author: Arjun Musuvathy
 Date: 11-05-2020
 Description: To add a filter in 'Update Step of Appointments' to do update only open and scheduled appointments. 
 Idea is to exclude cancelled/completed appointments

 Author: Dane Alexander
 Date: 15-11-2021
 Description: Modified the code layout for better readability
 **************************************************************************************/
using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class CreateUpdateDeletePhoneCall : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            var PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var OrganizationService = ServiceFactory.CreateOrganizationService(PluginExecutionContext.UserId);

            tracingService.Trace($"Execution Depth is {PluginExecutionContext.Depth}.");
            //CheckFields(PluginExecutionContext, tracingService);

            if (PluginExecutionContext.Depth > 14)
            {
                tracingService.Trace("This plugin is notorious for causing infinite loops. Stopping operation now before we get a system error message.");
                return;
            }
            
            if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "create", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
                    {
                        return;
                    }

                    if (MessageComesFromAppointment(PluginExecutionContext.InputParameters["Target"] as Entity))
                        return;

                    CreateAppointment(PluginExecutionContext, tracingService, OrganizationService);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.ToString());
                }
            }
            else if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "update", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!PluginExecutionContext.PostEntityImages.Contains("PostImage") || PluginExecutionContext.PostEntityImages["PostImage"] as Entity == null)
                {
                    return;
                }

                if (MessageComesFromAppointment(PluginExecutionContext.InputParameters["Target"] as Entity))
                    return;

                UpdateAppointment(PluginExecutionContext, tracingService, OrganizationService);
            }
            else if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "delete", StringComparison.InvariantCultureIgnoreCase))
            {
                DeleteAppointment(PluginExecutionContext, tracingService, OrganizationService);

                return;
            }
        }

        private void DeleteAppointment(IPluginExecutionContext PluginExecutionContext, ITracingService tracingService, IOrganizationService OrganizationService)
        {
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as EntityReference == null)
            {
                return;
            }

            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as EntityReference;

            if (PluginExecutionContext.Stage == 10) // Pre Validation
            {
                var appointmentRecord = GetAppointment(TargetEntity.Id, OrganizationService);
                if (appointmentRecord != null)
                {
                    tracingService.Trace("appointmentRecord is not null");
                    PluginExecutionContext.SharedVariables.Clear();
                    PluginExecutionContext.SharedVariables.Add("appointmentid", appointmentRecord.Id);
                }
            }

            if (PluginExecutionContext.Stage == 20) //Pre Operation
            {
                if (PluginExecutionContext.ParentContext.SharedVariables.ContainsKey("appointmentid"))
                {
                    var appointmentId = new Guid(PluginExecutionContext.ParentContext.SharedVariables["appointmentid"].ToString());
                    OrganizationService.Delete("appointment", appointmentId);
                }
            }

            return;
        }

        private void UpdateAppointment(IPluginExecutionContext PluginExecutionContext, ITracingService tracingService, IOrganizationService OrganizationService)
        {
            var PostImage = PluginExecutionContext.PostEntityImages["PostImage"] as Entity;
            if (IsPhoneCallRequiresUpdateAppointment(PostImage))
            {
                var appointmentRecord = GetAppointment(PostImage.Id, OrganizationService);

                if (appointmentRecord != null)
                {
                    if (appointmentRecord.Contains("statecode") && appointmentRecord["statecode"] != null)
                    {
                        OptionSetValue apptStateCode = (OptionSetValue)appointmentRecord["statecode"];
                        Int32 appointmentStatus = apptStateCode.Value;

                        //Open and Scheduled appointments 
                        if (appointmentStatus == 0 || appointmentStatus == 3)
                        {
                            tracingService.Trace($"Appointment is not null - Id:{appointmentRecord.Id}");
                            var appointmentToUpdate = MapFields(PostImage, appointmentRecord,tracingService);
                            //appointment["activityid"] = appointmentRecord.Id;
                            OrganizationService.Update(appointmentToUpdate);
                        }
                    }
                }
            }
        }

        private void CreateAppointment(IPluginExecutionContext PluginExecutionContext, ITracingService tracingService, IOrganizationService OrganizationService)
        {
            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;
            if (IsPhoneCallRequiresAppointment(TargetEntity))
            {
                tracingService.Trace("This phone call requires an appointment");
                var appointment = new Entity("appointment");
                var appointmentToCreate = MapFields(TargetEntity, appointment, tracingService);
                OrganizationService.Create(appointmentToCreate);
            }
        }

        private Entity GetAppointment(Guid phoneCallId, IOrganizationService service)
        {
            var query = new QueryExpression("appointment")
            {
                ColumnSet = new ColumnSet(true)
            };
            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression("dfe_phonecall", ConditionOperator.Equal, phoneCallId));
            query.Criteria = filter;
            var results = service.RetrieveMultiple(query);
            if (results != null && results.Entities.Count > 0 && results.Entities[0] != null)
            {
                return results.Entities[0];
            }
            return null;
        }
        private bool IsPhoneCallRequiresAppointment(Entity phonecall)
        {
            if (phonecall != null && phonecall.Contains("dfe_appointmentflag") && phonecall["dfe_appointmentflag"] != null && phonecall.Contains("dfe_appointmentrequired") && phonecall["dfe_appointmentrequired"] != null)
            {
                return Convert.ToBoolean(phonecall["dfe_appointmentflag"]) && Convert.ToBoolean(phonecall["dfe_appointmentrequired"]);
            }
            return false;
        }
        private bool IsPhoneCallRequiresUpdateAppointment(Entity phonecall)
        {
            if (phonecall != null && phonecall["dfe_appointmentrequired"] != null)
            {
                return Convert.ToBoolean(phonecall["dfe_appointmentrequired"]);
            }
            return false;
        }

        private Entity MapFields(Entity TargetEntity, Entity appointment, ITracingService tracingService)
        {
            appointment["dfe_phonecall"] = TargetEntity.ToEntityReference();

            if (TargetEntity.Contains("directioncode") && TargetEntity["directioncode"] != null)
            {
                appointment["dfe_direction"] = TargetEntity["directioncode"];
            }
            if (TargetEntity.Contains("leftvoicemail") && TargetEntity["leftvoicemail"] != null)
            {
                appointment["dfe_leftvoicemail"] = TargetEntity["leftvoicemail"];
            }
            if (TargetEntity.Contains("phonenumber") && TargetEntity["phonenumber"] != null)
            {
                appointment["dfe_phonenumber"] = TargetEntity["phonenumber"];
            }
            if (TargetEntity.Contains("dfe_postcallnotes") && TargetEntity["dfe_postcallnotes"] != null)
            {
                appointment["dfe_postcallnotes"] = TargetEntity["dfe_postcallnotes"];
            }
            if (TargetEntity.Contains("description") && TargetEntity["description"] != null)
            {
                appointment["dfe_precallnotes"] = TargetEntity["description"];
            }
            appointment["dfe_type"] = false; // Phonecall
            appointment["dfe_requirephonecall"] = true; // to avoid looping in workflow
            appointment["dfe_phonecallflag"] = true; // to avoid looping in workflow
            if (TargetEntity.Contains("scheduledstart") && TargetEntity["scheduledstart"] != null)
            {
                appointment["scheduledstart"] = TargetEntity["scheduledstart"]; // Start Time
            }
            if (TargetEntity.Contains("scheduledend") && TargetEntity["scheduledend"] != null)
            {
                appointment["scheduledend"] = TargetEntity["scheduledend"]; // End Time
            }
            if (TargetEntity.Contains("subject") && TargetEntity["subject"] != null)
            {
                appointment["subject"] = TargetEntity["subject"]; // Subject
            }
            if (TargetEntity.Contains("statecode") && TargetEntity["statecode"] != null)
            {
                appointment["statecode"] = TargetEntity["statecode"];
            }
            if (TargetEntity.Contains("to") && TargetEntity["to"] != null)
            {
                var candidates = (TargetEntity["to"] as EntityCollection);
                if (candidates != null && candidates.Entities.Count == 1 && candidates.Entities[0] != null)
                {
                    var candidateEntity = candidates.Entities[0].ToEntity<Entity>();
                    if (candidateEntity["partyid"] is EntityReference candidate)
                    {
                        if (candidate.LogicalName == "systemuser")
                        {
                            tracingService.Trace("Cannot associate the system user to this record as it is not allowed. See documentation https://docs.microsoft.com/en-us/previous-versions/dynamics-crm4/developers-guide/cc151571(v=msdn.10)");
                        }
                        else
                            appointment["regardingobjectid"] = candidate; // Candidate
                    }
                }
            }
            if (TargetEntity.Contains("from") && TargetEntity["from"] != null)
            {
                var users = (TargetEntity["from"] as EntityCollection);
                if (users != null && users.Entities.Count == 1 && users.Entities[0] != null)
                {
                    var userEntity = users.Entities[0].ToEntity<Entity>();
                    if (userEntity["partyid"] is EntityReference user)
                    {
                        if(user.LogicalName == "systemuser")
                            appointment["dfe_callfrom"] = user; // Current                                     // }

                        //Arjun 26/02/2020 This line of code is added to fix GCLS-674 Reassign issue.
                        Entity requiredAttendees = new Entity("activityparty");
                        requiredAttendees["partyid"] = user;
                        appointment["requiredattendees"] = new Entity[] { requiredAttendees };

                        // adding the organizer
                        appointment["organizer"] = new Entity[] { requiredAttendees };
                    }
                }
                //appointment["requiredattendees"] = TargetEntity["from"]; ////Arjun 26/02/2020 This line of code is commented out to fix GCLS-674 Reassign issue.
            }

            return appointment;
        }

        private bool MessageComesFromAppointment(Entity TargetEntity)
        {
            bool returnVal = false;
            if (TargetEntity.Contains("dfe_comesfromappointment") && Convert.ToBoolean(TargetEntity["dfe_comesfromappointment"]))
            {
                returnVal = true;
                TargetEntity["dfe_comesfromappointment"] = false;
            }

            return returnVal;
        }


        //private void CheckFields(IPluginExecutionContext PluginExecutionContext, ITracingService tracingService)
        //{
        //    tracingService.Trace($"Message Name is {PluginExecutionContext.MessageName.ToLower()}");
        //    EntityReference target = (EntityReference)PluginExecutionContext.InputParameters["Target"];
        //    if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
        //    {
        //        return;
        //    }
        //    foreach (var t in target.Attributes)
        //    {
        //        tracingService.Trace($"Key: {t.Key}, Value: {t.Value}");
        //    }
        //}
    }
}
