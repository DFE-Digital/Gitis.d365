var DFEPhoneCallOutGoing = DFEPhoneCallOutGoing ? DFEPhoneCallOutGoing : {};

DFEPhoneCallOutGoing.formOnLoad = function(eCxt) {
    DFEPhoneCallOutGoing.utils = DFEPhoneCallOutGoing.utils || new window.dfe_common.utils();
    DFEPhoneCallOutGoing.data = DFEPhoneCallOutGoing.data || new window.dfe_common.data();
    window.CRM_FORM_TYPE_CREATE = 1;
    window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFEPhoneCallOutGoing.utils.getFormType();
    if (window.FORM_TYPE != window.CRM_FORM_TYPE_CREATE) {        
        DFEPhoneCallOutGoing.utils.disableField("to", true);
        DFEPhoneCallOutGoing.utils.disableField("subject", true);
        DFEPhoneCallOutGoing.utils.disableField("scheduledstart", true);        
    }

    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();
    //store the form context into the parent window object so we can retrieve it from within the html web resource
    window.parent.FormContextP = formContext;

    //Limit the list of entities in the party lookup to be Contacts only
    //NOTE - THESE 2 LINES ARE CAUSING AN ERROR IN THE UNIFIED INTERFACE
    //var associatedContactView = formContext.getAttribute('to').getLookupDataAttribute();
    //associatedContactView.setLookupTypes(['contact']);
    try {
        formContext.getControl("to").setEntityTypes(["contact"]); 
    } catch (e) {
    }       

    //Assign Phone NUmber from contact
    var contactId = DFEPhoneCallOutGoing.utils.getAttributeValue("to");
    var phoneNumberControl = DFEPhoneCallOutGoing.utils.getControl("phonenumber");
    var formphoneNumber = phoneNumberControl.getAttribute().getValue();
    //phoneNumberControl.clearNotification();
    if(!formphoneNumber)
    {
        var phoneNumber = DFEPhoneCallOutGoing.data.getPhoneNumberFromContact(contactId);
        DFEPhoneCallOutGoing.utils.setAttributeValue("phonenumber",phoneNumber);
    }
    //DFEPhoneCallOutGoing.UpdateSubject();//Do we need this??
    if(window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE)
    {
        DFEPhoneCallOutGoing.utils.setAttributeValue("dfe_appointmentrequired", false);
    }
    DFEPhoneCallOutGoing.utils.setVisible("dfe_appointmentflag", false);
    //Assign currently logged in user if the value is balnk
    var callFrom = DFEPhoneCallOutGoing.utils.getAttributeValue("from");

    if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE || !callFrom || callFrom.length == 0) {
        var userId = Xrm.Page.context.getUserId();
        var UserName = Xrm.Page.context.getUserName();
        var currentUser = DFEPhoneCallOutGoing.utils.getLookup(userId, "systemuser", UserName);
        DFEPhoneCallOutGoing.utils.setLookup(currentUser, "from");
    }
}

DFEPhoneCallOutGoing.formOnSave = function(eCxt) {
    //var phoneNumberControl = DFEPhoneCallOutGoing.utils.getControl("phonenumber");
    //var result = DFEPhoneCallOutGoing.utils.isValidPhoneNumber(phoneNumberControl.getAttribute().getValue());
    //if (!result)
    //{
    //    //alert("The format of the phone number is invalid");
    //    phoneNumberControl.setNotification("The format of the phone number is invalid");
    //    phoneNumberControl.setFocus();
    //    eCxt.getEventArgs().preventDefault();//will prevent CRM form save
    //}
    //else
    //{
    //    phoneNumberControl.clearNotification();
    //}
}
DFEPhoneCallOutGoing.OnContactChange = function(eCxt) {
    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();
    var contactId = eCxt.getEventSource().getValue();
    var formphoneNumber = DFEPhoneCallOutGoing.utils.getAttributeValue("phonenumber");
    if(!formphoneNumber)
    {
        var phoneNumber = DFEPhoneCallOutGoing.data.getPhoneNumberFromContact(contactId);
        DFEPhoneCallOutGoing.utils.setAttributeValue("phonenumber",phoneNumber);
    }
    DFEPhoneCallOutGoing.UpdateSubject();
}

DFEPhoneCallOutGoing.ValidatePhoneNumber = function(eCxt)
{
    //var phoneNumberControl = DFEPhoneCallOutGoing.utils.getControl("phonenumber");
    //var phonenumber = phoneNumberControl.getAttribute().getValue();
    //var result = DFEPhoneCallOutGoing.utils.isValidPhoneNumber(phonenumber);
    //if (!result)
    //{
    //    phoneNumberControl.setNotification("The format of the phone number is invalid");
    //    phoneNumberControl.setFocus();
    //}
    //else
    //{
    //    phoneNumberControl.clearNotification();
    //}
}

DFEPhoneCallOutGoing.UpdateSubject = function()
{
    var contactId = DFEPhoneCallOutGoing.utils.getAttributeValue("to");
    var subject = "";
    if(contactId && contactId[0])
    {
        if(contactId[0].name)
        {
            subject = "Phone call to " + contactId[0].name;
        }
        else if(contactId[0].id)
        {
            var contactObject = DFEPhoneCallOutGoing.data.getContactFullNameById(contactId[0].id);
            if(contactObject)
            {
                subject = "Phone call to " + contactObject.fullname;
            }
        }
    }
    DFEPhoneCallOutGoing.utils.setAttributeValue("subject",subject);
}

DFEPhoneCallOutGoing.ClosePhoneCall_onOutcomeCodeChange = function (eCxt) {
    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();
    //Check if the call start and end times are filled in and provide the current time value if not.
    var currentTime = new Date();
    var actualEnd = DFEPhoneCallOutGoing.utils.getAttributeValue("actualend");
    if (actualEnd == null) {
        DFEPhoneCallOutGoing.utils.setAttributeValue("actualend", currentTime);
    }
    var actualStart = DFEPhoneCallOutGoing.utils.getAttributeValue("actualstart");
    if (actualStart == null) {
        DFEPhoneCallOutGoing.utils.setAttributeValue("actualstart", currentTime);
    }
    //Set the phone call as complete (for Outgoing, this means setting a statuscode of 2 and a statecode of 1)
    DFEPhoneCallOutGoing.utils.setAttributeValue("statuscode", 2);
    DFEPhoneCallOutGoing.utils.setAttributeValue("statecode", 1);
    //Now save the record and close the form
    formContext.data.entity.save("saveandclose");
}

DFEPhoneCallOutGoing.RedirectToForm = function () {
    var direction = DFEPhoneCallOutGoing.utils.getAttributeValue("directioncode");
    var currentForm = Xrm.Page.ui.formSelector.getCurrentItem();
    if (direction == false)//Incoming
    {
        if (currentForm.getLabel().toLowerCase() != "incoming call (tpuk workspace)") {
            var availableForms = Xrm.Page.ui.formSelector.items.get();
            for (var i in availableForms) {
                // try to find a form based on the name 
                if (availableForms[i].getLabel().toLowerCase() == "incoming call (tpuk workspace)") {
                    availableForms[i].navigate(); // redirect once we find it
                    return true;
                }
            }
        }

    }
    else if (direction == true)//Outgoing
    {
        if (currentForm.getLabel().toLowerCase() != "outgoing call (tpuk workspace)") {
            var availableForms = Xrm.Page.ui.formSelector.items.get();
            for (var i in availableForms) {
                // try to find a form based on the name 
                if (availableForms[i].getLabel().toLowerCase() == "outgoing call (tpuk workspace)") {
                    availableForms[i].navigate(); // redirect once we find it
                    return true;
                }
            }
        }
    }

}