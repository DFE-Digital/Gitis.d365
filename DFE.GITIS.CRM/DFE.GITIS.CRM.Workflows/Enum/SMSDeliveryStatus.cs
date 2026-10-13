using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.GITIS.CRM.Workflows.Enum
{
    public enum SMSDeliveryStatus
    {
        Created = 222750000,
        Sending = 222750001,
        Delivered = 222750002,
        Failed = 222750003,
        Duplicate = 222750004,
        InvalidNumber = 222750005,
        ManuallyCreated = 222750006,
    }
    
}
