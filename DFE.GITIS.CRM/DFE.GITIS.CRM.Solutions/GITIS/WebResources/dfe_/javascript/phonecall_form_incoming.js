var DFEPhoneCallInComing = DFEPhoneCallInComing ? DFEPhoneCallInComing : {};

DFEPhoneCallInComing.formOnLoad = function(eCxt) {

    DFEPhoneCallInComing.utils = DFEPhoneCallInComing.utils || new window.dfe_common.utils();
    DFEPhoneCallInComing.data = DFEPhoneCallInComing.data || new window.dfe_common.data();
    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();   
    
    DFEPhoneCallInComing.utils.setRequiredLevel("from", "required");    
    var vDate = formContext.getAttribute("actualstart").getValue();
    if (vDate == null) {
        formContext.getAttribute("actualstart").setValue(new Date());
    }

    //Limit the list of entities in the party lookup to be Contacts only
    //NOTE - THESE 2 LINES ARE CAUSING AN ERROR IN THE UNIFIED INTERFACE - need to find a UI equivalent replacement
    //var associatedContactView =  formContext.getAttribute('from').getLookupDataAttribute();
    //associatedContactView.setLookupTypes(['contact']);
    try {
        formContext.getControl("from").setEntityTypes(["contact"]);   
    } catch (e) {

    }     
}
DFEPhoneCallInComing.ClosePhoneCall_onOutcomeCodeChange = function (eCxt) {
    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();
    ////var selectedOutCome = formContext.getAttribute("dfe_calloutcomecodeinbound").getValue();
    //var selectedOutCome = DFEPhoneCallInComing.utils.getAttributeValue("dfe_calloutcomecodeinbound");
    //if (selectedOutCome && (selectedOutCome == 222750001 || selectedOutCome == 222750013)) //Anonymous caller, Third party enquiring on candidate's behalf
    //{
    //    DFEPhoneCallInComing.utils.setRequiredLevel("from", "none");
    //}
    //else
    //{
    //    DFEPhoneCallInComing.utils.setRequiredLevel("from", "required"); 
    //}
    //Check if the call start and end times are filled in and provide the current time value if not.
    var currentTime = new Date();
    var actualEnd = DFEPhoneCallInComing.utils.getAttributeValue("actualend");
    if (actualEnd == null) {
        DFEPhoneCallInComing.utils.setAttributeValue("actualend", currentTime);
    }
    var actualStart = DFEPhoneCallInComing.utils.getAttributeValue("actualstart");
    if (actualStart == null) {
        DFEPhoneCallInComing.utils.setAttributeValue("actualstart", currentTime);
    }
    //Set the phone call as complete (for Incoming, this means setting a statuscode of 4 and a statecode of 1) but cannot get 4 to be accepted
    DFEPhoneCallInComing.utils.setAttributeValue("statuscode", 4);
    DFEPhoneCallInComing.utils.setAttributeValue("statecode", 1);
    //Now save the record and close the form
    formContext.data.entity.save("saveandclose");
}