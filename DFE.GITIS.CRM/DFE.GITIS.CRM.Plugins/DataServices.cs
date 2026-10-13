using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DFE.GITIS.CRM.Plugins
{
    public static class DataServices
    {
        public static int WebRegistartionCallWeighting;
        public static int WebRegistartionEEACallWeighting;
        public static int RmpCallWeighting;
        public static int RmpEEACallWeighting;
        public static int TeachFirstCallWeighting;
        public static int LatestPrivacyPolicyId;
        public static int WebsiteInboundPrivacyPolicyId;
        public static int UcasRegistartionCallWeighting;
        public static int CallbackRequestCallWeighting;
        public static int SummerCampaignCallWeighting;
        public static int ScholarshipCallWeighting;
        public static int SesCallWeighting;
        public static int WebsiteCallWeighting;
        public static int WebsiteBookACallCallWeighting;
        public static string WebRegistartionTalkingPoints;
        public static string ScholarshipTalkingPoints;
        public static string WebsiteTalkingPoints;        
        public static string RmpTalkingPoints;
        public static string TeachFirstTalkingPoints;
        public static string UcasRegistartionTalkingPoints;
        public static string CallbackRequestTalkingPoints;
        public static string SummerCampaignTalkingPoints;
        public static string SesTalkingPoints;
        public static Guid OutboundCallQueueGuid;
        public static Guid OutboundTeamGuid;
        public static Guid OutboundWebsiteTeamGuid;
        public static int MaxNoOfCallAttempts;
        public static int WeightReductionForFollowups;
        public static List<int> OutcomeCodesForFollowupCall = new List<int>();

        public static Entity GetContactById(IOrganizationService service, Guid contactId)
        {
            return service.Retrieve("contact", contactId, new ColumnSet(true));
        }

        public static Entity GetContactPhonePreference(IOrganizationService service, Guid contactId)
        {
            return service.Retrieve("contact", contactId, new ColumnSet("donotphone"));
        }

        public static Entity GetPhoneCallById(IOrganizationService service, Guid phoneCallId, string[] columns)
        {
            return service.Retrieve("phonecall", phoneCallId, new ColumnSet(columns));
        }

        public static Entity GetSmsByPhonecallId(IOrganizationService service, Guid phoneCallId)
        {           
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("dfe_phonecallid", ConditionOperator.Equal, phoneCallId));
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Open

            var query = new QueryExpression("dfe_sms")
            {
                ColumnSet = new ColumnSet("activityid"),
                Criteria = criteria
            };

            var smsList = service.RetrieveMultiple(query);

            if (smsList != null && smsList.Entities.Count > 0)
            {
                return smsList.Entities.FirstOrDefault();
            }
            return null;                
        }
        public static Entity GetTeamById(IOrganizationService service, Guid teamGuid)
        {
            return service.Retrieve("team", teamGuid, new ColumnSet("name"));
        }

        public static string GetPhoneNumberFromContact(IOrganizationService service, Entity contact)
        {
            var numberToCall = "";
            if (contact != null)
            {
                if (contact["donotphone"] != null && Convert.ToBoolean(contact["donotphone"]) == false) //Allow
                {
                    if (contact["dfe_preferredphonenumbertype"] != null)
                    {
                        var preferredPhoneNumberType = (OptionSetValue)contact["dfe_preferredphonenumbertype"];
                        if (preferredPhoneNumberType.Value == 222750000)//Mobile
                        {
                            numberToCall = (contact.Contains("mobilephone") && contact["mobilephone"] != null) ? contact["mobilephone"].ToString() : "";
                        }
                        else if (preferredPhoneNumberType.Value == 222750001)//Home
                        {
                            numberToCall = (contact.Contains("address1_telephone1") && contact["address1_telephone1"] != null) ? contact["address1_telephone1"].ToString() : "";
                        }
                        else if (preferredPhoneNumberType.Value == 222750002)//Work
                        {
                            numberToCall = (contact.Contains("telephone1") && contact["telephone1"] != null) ? contact["telephone1"].ToString() : "";
                        }
                    }
                    else if (contact.Contains("mobilephone") && contact["mobilephone"] != null)
                    {
                        numberToCall = contact["mobilephone"].ToString();
                    }
                    else if (contact.Contains("address1_telephone1") && contact["address1_telephone1"] != null)
                    {
                        numberToCall = contact["address1_telephone1"].ToString();
                    }
                    else if (contact.Contains("telephone1") && contact["telephone1"] != null)
                    {
                        numberToCall = contact["telephone1"].ToString();
                    }
                }
            }
            return numberToCall;
        }

        public static Guid GetSummerCampaignTopic(IOrganizationService service)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Status
            criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));//Status Reason
            criteria.Conditions.Add(new ConditionExpression("dfe_name", ConditionOperator.Equal, "Summer Campaign"));            

            var query = new QueryExpression("dfe_topic")
            {
                ColumnSet = new ColumnSet("dfe_topicid"),
                Criteria = criteria
            };
            var topicRecord = service.RetrieveMultiple(query);
            if (topicRecord != null && topicRecord.Entities.Count == 1 && topicRecord.Entities[0] != null)
            {
                return topicRecord.Entities[0].Id;
            }
            else
                throw new Exception("Candiadte channel creation is not matching with call topic");
        }

        public static Guid GetTopicByValue(IOrganizationService service,int value)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Status
            criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));//Status Reason
            criteria.Conditions.Add(new ConditionExpression("dfe_sourcechannelvalue", ConditionOperator.Equal, value));//Optionset Value            

            var query = new QueryExpression("dfe_topic")
            {
                ColumnSet = new ColumnSet("dfe_topicid"),
                Criteria = criteria
            };
            var topicRecord = service.RetrieveMultiple(query);
            if (topicRecord != null && topicRecord.Entities.Count == 1 && topicRecord.Entities[0] != null)
            {
                return topicRecord.Entities[0].Id;
            }
            else
                throw new Exception("Candidate channel creation is not matching with call topic");
        }
        public static EntityCollection GetCallTopicsForPhoneCall(IOrganizationService service, Guid phoneCallId)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            //criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Status
            //criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));//Status Reason
            criteria.Conditions.Add(new ConditionExpression("dfe_phonecall", ConditionOperator.Equal, phoneCallId));//Phone call id            

            var query = new QueryExpression("dfe_calltopics")
            {
                ColumnSet = new ColumnSet("dfe_topic"),
                Criteria = criteria
            };
            return service.RetrieveMultiple(query);
        }
        public static void PopulateConfigValues(IOrganizationService service)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Status
            criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));//Status Reason
            criteria.Conditions.Add(new ConditionExpression("dfe_name", ConditionOperator.Equal, "GITIS"));//Name            

            var query = new QueryExpression("dfe_config")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = criteria
            };
            var configRecords = service.RetrieveMultiple(query);
            if (configRecords != null && configRecords.Entities.Count == 1 && configRecords.Entities[0] != null)
            {
                var config = configRecords.Entities[0];
                LatestPrivacyPolicyId = Convert.ToInt32(CheckNull(config["dfe_latestprivacypolicyid"], "Latest privacy policy id"));
                WebsiteInboundPrivacyPolicyId = Convert.ToInt32(CheckNull(config["dfe_websiteinboundprivacypolicyid"], "Latest privacy policy id"));
                WebRegistartionCallWeighting = Convert.ToInt32(CheckNull(config["dfe_webregistrationcallweighting"], "Web Registration Call Weighting"));
                WebRegistartionEEACallWeighting = Convert.ToInt32(CheckNull(config["dfe_webregistrationeeacallweighting"], "Web Registration EEA Call Weighting"));
                RmpCallWeighting = Convert.ToInt32(CheckNull(config["dfe_rmpcallweigting"], "RMP Call Weighting"));
                RmpEEACallWeighting = Convert.ToInt32(CheckNull(config["dfe_rmpeeacallweigting"], "RMP EEA Call Weighting"));
                TeachFirstCallWeighting = Convert.ToInt32(CheckNull(config["dfe_teachfirstcallweighting"], "TeachFirst Call Weighting"));
                UcasRegistartionCallWeighting = Convert.ToInt32(CheckNull(config["dfe_ucasregistrationcallweighting"], "UCAS Registration Call Weighting"));
                CallbackRequestCallWeighting = Convert.ToInt32(CheckNull(config["dfe_callbackrequestcallweighting"], "Callback Request Call Weighting"));
                SummerCampaignCallWeighting = Convert.ToInt32(CheckNull(config["dfe_summercampaigncallweighting"], "Summer Campaign Call Weighting"));
                SesCallWeighting = Convert.ToInt32(CheckNull(config["dfe_sescallweighting"], "School Experience Call Weighting"));
                WebsiteCallWeighting = Convert.ToInt32(CheckNull(config["dfe_websitecallbackrequestcallweighting"], "Website Call Weighting"));
                WebsiteBookACallCallWeighting = Convert.ToInt32(CheckNull(config["dfe_websitebookacallbackcallweighting"], "Website Book a Call back Weighting"));
                OutboundCallQueueGuid = Guid.Parse(CheckNull(config["dfe_outboundcallqueueguid"], "Outbound Call Queue Guid").ToString());
                OutboundTeamGuid = Guid.Parse(CheckNull(config["dfe_outboundteamid"], "Outbound Team Guid").ToString());
                OutboundWebsiteTeamGuid = Guid.Parse(CheckNull(config["dfe_outboundwebsiteteamguid"], "Outbound Website Team Guid").ToString());
                MaxNoOfCallAttempts = Convert.ToInt32(CheckNull(config["dfe_maxnumberofoutboundcallattempts"], "Max Number of outbound Call Attempts"));
                WebRegistartionTalkingPoints = CheckNull(config["dfe_talkingpointswebreg"], "Talking Points Web Reg").ToString();
                RmpTalkingPoints = CheckNull(config["dfe_talkingpointsrmp"], "Talking Points for RMP").ToString();
                TeachFirstTalkingPoints = CheckNull(config["dfe_teachfirsttalkingpoints"], "Talking Points for TeachFirst").ToString();
                UcasRegistartionTalkingPoints = CheckNull(config["dfe_talkingpointsucas"], "Talking Points UCAS").ToString();
                CallbackRequestTalkingPoints = CheckNull(config["dfe_talkingpointscallbackrequest"], "Talking Points Callback Request").ToString();
                SummerCampaignTalkingPoints = CheckNull(config["dfe_talkingpointssummercampaign"], "Talking Points Summer Campaign").ToString();
                SesTalkingPoints = CheckNull(config["dfe_talkingpointsses"], "Talking Points School Experience").ToString();
                WebsiteTalkingPoints = CheckNull(config["dfe_talkingpointswebsite"], "Talking Points Website").ToString();
                ScholarshipTalkingPoints = CheckNull(config["dfe_talkingpointsscholarships"], "Talking Points Scholarships").ToString();
                ScholarshipCallWeighting = Convert.ToInt32(CheckNull(config["dfe_scholashipsregistrationcallweighting"], "Scholarships Call Weighting"));
                WeightReductionForFollowups = Convert.ToInt32(CheckNull(config["dfe_weightreductionforfollowups"], "Weight Reduction For Followups"));
                var outcomeCodes = CheckNull(config["dfe_outcomecodesforfollowup"], "Outcome codes for followup").ToString().Split(',');
                foreach (var outcomeCode in outcomeCodes)
                {
                    OutcomeCodesForFollowupCall.Add(int.Parse(outcomeCode));
                }                
            }
            else
                throw new Exception("Invalid config entries");
        }

        public static object CheckNull(object value, string configname)
        {
            if (value != null)
            {
                return value;
            }
            else
                throw new Exception($"Config value is empty - {configname}");
        }

        public static EntityCollection GetOpenOutboundPhoneCalls(IOrganizationService service, Guid contactId)
        {
            if(contactId == null || contactId == Guid.Empty)
                return null;
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Open
            criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));//Open
            criteria.Conditions.Add(new ConditionExpression("directioncode", ConditionOperator.Equal, true));//Outbound            
            var linkEntityTo = new LinkEntity
            {
                LinkFromEntityName = "phonecall",
                LinkFromAttributeName = "activityid",
                LinkToEntityName = "activityparty",
                LinkToAttributeName = "activityid",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "partyid",
                            Operator = ConditionOperator.Equal,
                            Values =
                            {
                                contactId
                            }
                        }
                    }
                }
                //LinkEntities = {
                //    new LinkEntity
                //    {
                //        LinkFromEntityName = "activityparty",
                //        LinkFromAttributeName = "partyid",
                //        LinkToEntityName = "contact",
                //        LinkToAttributeName = "contactid",
                //        LinkCriteria = new FilterExpression
                //        {
                //            FilterOperator = LogicalOperator.And,
                //            Conditions =
                //            {
                //                new ConditionExpression
                //                {
                //                    AttributeName = "dfe_assignedadvisor",
                //                    Operator = ConditionOperator.Null
                //                }
                //            }
                //        }
                //    }
                //}
            };

            var query = new QueryExpression("phonecall")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = criteria,
                LinkEntities = { linkEntityTo }
            };
            return service.RetrieveMultiple(query);
        }

        public static EntityCollection GetOpenOutboundPhoneCallsByContact(IOrganizationService service, Guid contactId)
        {
            if (contactId == null || contactId == Guid.Empty)
                return null;
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Open
            criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));//Open
            criteria.Conditions.Add(new ConditionExpression("directioncode", ConditionOperator.Equal, true));//Outbound            
            var linkEntityTo = new LinkEntity
            {
                LinkFromEntityName = "phonecall",
                LinkFromAttributeName = "regardingobjectid",
                LinkToEntityName = "contact",
                LinkToAttributeName = "contactid",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "contactid",
                            Operator = ConditionOperator.Equal,
                            Values =
                            {
                                contactId
                            }
                        }
                    }
                }                
            };

            var query = new QueryExpression("phonecall")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = criteria,
                LinkEntities = { linkEntityTo }
            };
            return service.RetrieveMultiple(query);
        }

        public static string GetOptionSetValueLabel(this IOrganizationService service, string entityName, string fieldName, int optionSetValue, int lcid = 1033)
        {
            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = fieldName,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAttributeResponse)service.Execute(request);
            var metadata = (EnumAttributeMetadata)response.AttributeMetadata;
            var firstOrDefault = metadata.OptionSet.Options.FirstOrDefault(x => x.Value == optionSetValue);
            var localizedLabel = firstOrDefault?.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == lcid);
            return localizedLabel != null ? localizedLabel.Label : string.Empty;
        }

        public static bool CandidateProfileAllowPhoneCalls(IOrganizationService organizationService, Entity contact, Guid contactId)
        {
            if (contact != null && contact.Contains("donotphone") && contact["donotphone"] != null)
            {
                return Convert.ToBoolean(contact["donotphone"]) ? false : true;
            }
            else if (contactId != null && contactId != Guid.Empty)
            {
                var result = GetContactPhonePreference(organizationService, contactId);
                if (result != null && result.Contains("donotphone") && result["donotphone"] != null)
                {
                    return Convert.ToBoolean(result["donotphone"]) ? false : true;
                }
            }
            return false;
        }

        public static bool UserIsInRole(IOrganizationService organizationService, string rolename, Guid userId)
        {
            // Establish a SystemUser link for a query.
            LinkEntity systemUserLink = new LinkEntity()
            {
                LinkFromEntityName = "systemuserroles",
                LinkFromAttributeName = "systemuserid",
                LinkToEntityName = "systemuser",
                LinkToAttributeName = "systemuserid",
                LinkCriteria =
                {
                    Conditions = {new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)}
                }
            };

            // Build the query
            QueryExpression linkQuery = new QueryExpression()
            {
                EntityName = "role",
                ColumnSet = new ColumnSet("parentrootroleid"),
                LinkEntities =
                {
                    new LinkEntity()
                    {
                        LinkFromEntityName = "role",
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = "systemuserroles",
                        LinkToAttributeName = "roleid",
                        LinkEntities = {systemUserLink}
                    }
                },
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, rolename)
                    }
                }
            };

            // Retrieve matching roles.
            var matchEntities = organizationService.RetrieveMultiple(linkQuery);

            // if an entity is returned then the user is a member of the role
            return matchEntities.Entities.Count > 0;
        }             

        public static DateTime GetNextWorkingDay(DateTime currentDate)
        {
            DateTime date = currentDate;
            do
            {
                date = date.AddDays(1);
            }
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
            return date;
        }
    }
}
