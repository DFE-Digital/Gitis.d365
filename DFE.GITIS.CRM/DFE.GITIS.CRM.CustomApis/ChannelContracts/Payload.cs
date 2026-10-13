using System;

namespace DFE.GITIS.CRM.CustomApis.ChannelContracts
{    

    public class MarketingAppContext
    {
        public string CustomerJourneyId { get; set; }
        public Guid UserId { get; set; }
        public string UserEntityType { get; set; }
        public bool IsTestSend { get; set; }
    }

    public class Message
    {
        public string contact { get; set; }
        public string text { get; set; }
    }

    public class Payload
    {
        public string OrganizationId { get; set; }
        public Guid ChannelDefinitionId { get; set; }
        public string ConsumingApplicationId { get; set; }
        public string RequestId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public Message Message { get; set; }
        public MarketingAppContext MarketingAppContext { get; set; }
    }

}
