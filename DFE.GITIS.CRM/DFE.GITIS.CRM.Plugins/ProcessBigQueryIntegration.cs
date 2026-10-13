using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace DFE.GITIS.CRM.Plugins
{
    public class ProcessBigQueryIntegration : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var OrganizationService = ServiceFactory.CreateOrganizationService(null);

            var bqTableConfig = pluginExecutionContext.MessageName == "Delete" ? pluginExecutionContext.PreEntityImages["PreImage"] : pluginExecutionContext.PostEntityImages["PostImage"];

            Entity serviceEndpoint = GetServiceEndpointId(OrganizationService, "datafromcrm");
            if (serviceEndpoint == null)
            {
                tracingService.Trace("Service Endpoint not found!");
                return;
            }

            tracingService.Trace($"Service Endpoint found! {serviceEndpoint.Id}");

            var stringFieldArray = JsonConvert.DeserializeObject<string[]>((string)bqTableConfig["dfe_tablejson"]);            

            string tableCSVString = string.Join(",", stringFieldArray.Where(x => x != "id"));

            if (pluginExecutionContext.MessageName == "Create")
            {
                //Creating Create,Update and Delete steps for BQ integration
                //create
                CreateStepAndImage(OrganizationService, tracingService, bqTableConfig, "Create", serviceEndpoint, tableCSVString);
                //Update
                CreateStepAndImage(OrganizationService, tracingService, bqTableConfig, "Update", serviceEndpoint, tableCSVString);
                //Delete
                CreateStepAndImage(OrganizationService, tracingService, bqTableConfig, "Delete", serviceEndpoint, tableCSVString);
            }
            else if (pluginExecutionContext.MessageName == "Delete")
            {
                UnRegisterSteps(OrganizationService, tracingService, bqTableConfig, serviceEndpoint, tableCSVString);
            }
            else if (pluginExecutionContext.MessageName == "Update")
            {
                UpdateAttributesInSteps(OrganizationService, tracingService, bqTableConfig, serviceEndpoint, tableCSVString);
                UpdateAttributesInImages(OrganizationService, tracingService, bqTableConfig, serviceEndpoint, tableCSVString);
            }

        }

        private void UpdateAttributesInImages(IOrganizationService service, ITracingService tracingService, Entity bqTableConfig, Entity serviceEndpoint, string tableCSVString)
        {
           

            // Instantiate QueryExpression query
            var query = new QueryExpression("sdkmessageprocessingstepimage");

            // Add all columns to query.ColumnSet
            query.ColumnSet.AllColumns = true;

            // Add link-entity query_sdkmessageprocessingstep
            var query_sdkmessageprocessingstep = query.AddLink(
                "sdkmessageprocessingstep",
                "sdkmessageprocessingstepid",
                "sdkmessageprocessingstepid");


            // Add link-entity query_sdkmessageprocessingstep_serviceendpoint
            var query_sdkmessageprocessingstep_serviceendpoint = query_sdkmessageprocessingstep.AddLink(
                "serviceendpoint",
                "eventhandler",
                "serviceendpointid");


            // Add conditions to query_sdkmessageprocessingstep_serviceendpoint.LinkCriteria
            query_sdkmessageprocessingstep_serviceendpoint.LinkCriteria.AddCondition("serviceendpointid", ConditionOperator.Equal, serviceEndpoint.Id);

            // Add link-entity query_sdkmessageprocessingstep_sdkmessagefilter
            var query_sdkmessageprocessingstep_sdkmessagefilter = query_sdkmessageprocessingstep.AddLink(
                "sdkmessagefilter",
                "sdkmessagefilterid",
                "sdkmessagefilterid");


            // Add conditions to query_sdkmessageprocessingstep_sdkmessagefilter.LinkCriteria
            query_sdkmessageprocessingstep_sdkmessagefilter.LinkCriteria.AddCondition("primaryobjecttypecode", ConditionOperator.Equal, (string)bqTableConfig["dfe_name"]);

            //RETRIEVE SDK MESSAGE FILTER
            EntityCollection entityCollection = service.RetrieveMultiple(query);

            if (entityCollection.Entities.Count != 0)
            {
                tracingService.Trace($"images count {entityCollection.Entities.Count}");
                foreach (var entity in entityCollection.Entities)
                {
                    
                    tracingService.Trace($"messagepropertyname {(string)entity["messagepropertyname"]}");
                    service.Update(new Entity("sdkmessageprocessingstepimage", entity.Id)
                    {
                        ["attributes"] = tableCSVString,
                        ["messagepropertyname"] = (string)entity["messagepropertyname"],
                        ["name"] = (string)entity["name"],
                        ["entityalias"] = (string)entity["entityalias"],
                        ["imagetype"] = entity["imagetype"],
                        ["sdkmessageprocessingstepid"] = entity["sdkmessageprocessingstepid"]
                    });
                }
            }

        }

        private void UpdateAttributesInSteps(IOrganizationService service, ITracingService tracingService, Entity bqTableConfig, Entity serviceEndpoint, string tableCSVString)
        {
            // Instantiate QueryExpression query
            var query = new QueryExpression("sdkmessageprocessingstep");

            // Add all columns to query.ColumnSet
            query.ColumnSet.AllColumns = true;

            // Add conditions to query.Criteria
            query.Criteria.AddCondition("eventhandler", ConditionOperator.Equal, serviceEndpoint.Id);

            // Add link-entity query_sdkmessagefilter
            var query_sdkmessagefilter = query.AddLink("sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid");


            // Add conditions to query_sdkmessagefilter.LinkCriteria
            query_sdkmessagefilter.LinkCriteria.AddCondition("primaryobjecttypecode", ConditionOperator.Equal, (string)bqTableConfig["dfe_name"]);

            // Add link-entity query_sdkmessagefilter_sdkmessage
            var query_sdkmessagefilter_sdkmessage = query_sdkmessagefilter.AddLink("sdkmessage", "sdkmessageid", "sdkmessageid");


            // Add conditions to query_sdkmessagefilter_sdkmessage.LinkCriteria
            query_sdkmessagefilter_sdkmessage.LinkCriteria.AddCondition("name", ConditionOperator.In, "Create", "Update");

            //RETRIEVE SDK MESSAGE FILTER
            EntityCollection entityCollection = service.RetrieveMultiple(query);

            if (entityCollection.Entities.Count != 0)
            {
                foreach (var entity in entityCollection.Entities)
                {
                    service.Update(new Entity("sdkmessageprocessingstep", entity.Id) { ["filteringattributes"] = tableCSVString });
                }
            }
        }

        private static void UnRegisterSteps(IOrganizationService service, ITracingService tracingService, Entity bqTableConfig, Entity serviceEndpoint, string tableCSVString)
        {

            //var query_sdkmessagefilter_primaryobjecttypecode = 12072;

            // Instantiate QueryExpression query
            var query = new QueryExpression("sdkmessageprocessingstep");

            // Add all columns to query.ColumnSet
            query.ColumnSet.AllColumns = true;

            // Add conditions to query.Criteria
            query.Criteria.AddCondition("eventhandler", ConditionOperator.Equal, serviceEndpoint.Id);

            // Add link-entity query_sdkmessagefilter
            var query_sdkmessagefilter = query.AddLink("sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid");


            // Add conditions to query_sdkmessagefilter.LinkCriteria
            query_sdkmessagefilter.LinkCriteria.AddCondition("primaryobjecttypecode", ConditionOperator.Equal, (string)bqTableConfig["dfe_name"]);

            //RETRIEVE SDK MESSAGE FILTER
            EntityCollection entityCollection = service.RetrieveMultiple(query);

            if (entityCollection.Entities.Count != 0)
            {
                foreach (var entity in entityCollection.Entities)
                {
                    service.Delete("sdkmessageprocessingstep", entity.Id);
                }
            }

        }

        private static void CreateStepAndImage(IOrganizationService service, ITracingService tracingService,Entity bqTableConfig, string messageName, Entity serviceEndpoint, string tableCSVString)
        {
            
            var sdkMessageId = GetSdkMessageId(service, messageName);
            Entity sdkMessageProcessingStep = new Entity("sdkmessageprocessingstep");
            sdkMessageProcessingStep["eventhandler"] = new EntityReference("serviceendpoint", serviceEndpoint.Id); // Lookup
            sdkMessageProcessingStep["description"] = $"{serviceEndpoint["name"]}: {messageName} of {bqTableConfig["dfe_name"]}"; // Text
            sdkMessageProcessingStep["mode"] = new OptionSetValue(1); // Choice
            sdkMessageProcessingStep["rank"] = 1; // Whole Number
            sdkMessageProcessingStep["name"] = $"{serviceEndpoint["name"]}: {messageName} of {bqTableConfig["dfe_name"]}"; // Text
            sdkMessageProcessingStep["sdkmessageid"] = new EntityReference("sdkmessage", sdkMessageId); // Lookup
            sdkMessageProcessingStep["sdkmessagefilterid"] = new EntityReference("sdkmessagefilter", GetSdkMessageFilterId(service, (string)bqTableConfig["dfe_name"],sdkMessageId)); // Lookup
            //sdkMessageProcessingStep["iscustomizable"] = "true"; // ManagedProperty
            sdkMessageProcessingStep["asyncautodelete"] = true; // Boolean
            sdkMessageProcessingStep["supporteddeployment"] = new OptionSetValue(0); // Choice
            sdkMessageProcessingStep["stage"] = new OptionSetValue(40); // Choice
            //sdkMessageProcessingStep["ishidden"] = "false"; // ManagedProperty
            sdkMessageProcessingStep["canusereadonlyconnection"] = false; // Boolean
            sdkMessageProcessingStep["invocationsource"] = new OptionSetValue(0); // Choice           


            if (messageName != "Delete")
            {
                sdkMessageProcessingStep["filteringattributes"] = tableCSVString; // Only trigger if these fields change
            }

            Guid stepId = service.Create(sdkMessageProcessingStep);
            tracingService.Trace($"Step registered successfully with ID: {stepId}");

            if (messageName == "Delete")
            {
                // Register a Pre-Image
                RegisterImage(service, tracingService, stepId, "PreImage", 0, "Target", tableCSVString); // 0 = PreImage
            }
            else if (messageName == "Create")
            {
                // Register a Post-Image
                RegisterImage(service, tracingService, stepId, "PostImage", 1, "id", tableCSVString); // 1 = PostImage

            }
            else
            {
                RegisterImage(service, tracingService, stepId, "PostImage", 1, "Target", tableCSVString); // 1 = PostImage
            }
        }


        private static Entity GetServiceEndpointId(IOrganizationService service, string endpointName)
        {
            QueryExpression query = new QueryExpression("serviceendpoint")
            {
                ColumnSet = new ColumnSet("serviceendpointid","name"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("name", ConditionOperator.BeginsWith, endpointName);

            EntityCollection results = service.RetrieveMultiple(query);
            return results.Entities.Count > 0 ? results.Entities.FirstOrDefault() : null;
        }

        private static Guid GetSdkMessageFilterId(IOrganizationService service, string EntityLogicalName, Guid sdkMessageId)
        {
            try
            {
                //GET SDK MESSAGE FILTER QUERY
                QueryExpression sdkMessageFilterQueryExpression = new QueryExpression("sdkmessagefilter");
                sdkMessageFilterQueryExpression.ColumnSet = new ColumnSet("sdkmessagefilterid");
                sdkMessageFilterQueryExpression.Criteria = new FilterExpression
                {
                    Conditions =
                {
                    new ConditionExpression
                    {
                        AttributeName = "primaryobjecttypecode",
                        Operator = ConditionOperator.Equal,
                        Values = {EntityLogicalName}
                    },
                    new ConditionExpression
                    {
                        AttributeName = "sdkmessageid",
                        Operator = ConditionOperator.Equal,
                        Values = {sdkMessageId}
                    },
                }
                };

                //RETRIEVE SDK MESSAGE FILTER
                EntityCollection sdkMessageFilters = service.RetrieveMultiple(sdkMessageFilterQueryExpression);

                if (sdkMessageFilters.Entities.Count != 0)
                {
                    return sdkMessageFilters.Entities.First().Id;
                }
                throw new Exception(String.Format("SDK Message Filter for {0} was not found.", EntityLogicalName));
            }
            catch (InvalidPluginExecutionException invalidPluginExecutionException)
            {
                throw invalidPluginExecutionException;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        private static Guid GetSdkMessageId(IOrganizationService service, string messageName)
        {
            QueryExpression query = new QueryExpression("sdkmessage")
            {
                ColumnSet = new ColumnSet("sdkmessageid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, messageName);

            EntityCollection results = service.RetrieveMultiple(query);
            return results.Entities.Count > 0 ? results.Entities[0].Id : Guid.Empty;
        }

        private static void RegisterImage(IOrganizationService service,ITracingService tracingService, Guid stepId, string imageName, int imageType, string mPropertyName, string tableCSVString)
        {
            Entity sdkMessageProcessingStepImage = new Entity("sdkmessageprocessingstepimage");
            sdkMessageProcessingStepImage["name"] = imageName;
            sdkMessageProcessingStepImage["entityalias"] = imageName;
            sdkMessageProcessingStepImage["imagetype"] = new OptionSetValue(imageType);
            sdkMessageProcessingStepImage["attributes"] = tableCSVString; // Fields to capture
            sdkMessageProcessingStepImage["messagepropertyname"] = mPropertyName;
            sdkMessageProcessingStepImage["sdkmessageprocessingstepid"] = new EntityReference("sdkmessageprocessingstep", stepId);

            Guid imageId = service.Create(sdkMessageProcessingStepImage);
           
            tracingService.Trace($"{imageName} registered successfully with ID: {imageId}");
        }


    }
}
