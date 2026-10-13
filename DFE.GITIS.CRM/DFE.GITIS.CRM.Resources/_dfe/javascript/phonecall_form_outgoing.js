var DFEPhoneCallOutGoing = DFEPhoneCallOutGoing ? DFEPhoneCallOutGoing : {};

DFEPhoneCallOutGoing.formOnLoad = function (eCxt) {
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

    var formContext = eCxt.getFormContext();
    //store the form context into the parent window object so we can retrieve it from within the html web resource
    window.parent.FormContextP = formContext;

    try {
        formContext.getControl("to").setEntityTypes(["contact"]);
    } catch (e) {
    }

    //Assign Phone NUmber from contact
    var contactId = DFEPhoneCallOutGoing.utils.getAttributeValue("to");
    var phoneNumberControl = DFEPhoneCallOutGoing.utils.getControl("phonenumber");
    var formphoneNumber = phoneNumberControl.getAttribute().getValue();

    if (!formphoneNumber) {
        var phoneNumber = DFEPhoneCallOutGoing.data.getPhoneNumberFromContact(contactId);
        DFEPhoneCallOutGoing.utils.setAttributeValue("phonenumber", phoneNumber);
    }

    if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE) {
        DFEPhoneCallOutGoing.utils.setAttributeValue("dfe_appointmentrequired", false);
    }
    DFEPhoneCallOutGoing.utils.setVisible("dfe_appointmentflag", false);
    //Assign currently logged in user if the value is balnk
    var callFrom = DFEPhoneCallOutGoing.utils.getAttributeValue("from");

    if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE || !callFrom || callFrom.length == 0) {
        var globalContext = Xrm.Utility.getGlobalContext();
        var userId = globalContext.userSettings.userId;
        var userName = globalContext.userSettings.userName;
        var currentUser = DFEPhoneCallOutGoing.utils.getLookup(userId, "systemuser", userName);
        DFEPhoneCallOutGoing.utils.setLookup(currentUser, "from");
    }
}

DFEPhoneCallOutGoing.OnContactChange = function (eCxt) {
    var contactId = eCxt.getEventSource().getValue();
    var formphoneNumber = DFEPhoneCallOutGoing.utils.getAttributeValue("phonenumber");
    if (!formphoneNumber) {
        var phoneNumber = DFEPhoneCallOutGoing.data.getPhoneNumberFromContact(contactId);
        DFEPhoneCallOutGoing.utils.setAttributeValue("phonenumber", phoneNumber);
    }
    DFEPhoneCallOutGoing.UpdateSubject();
}

DFEPhoneCallOutGoing.UpdateSubject = function () {
    var contactId = DFEPhoneCallOutGoing.utils.getAttributeValue("to");
    var subject = "";
    if (contactId && contactId[0]) {
        if (contactId[0].name) {
            subject = "Phone call to " + contactId[0].name;
        }
        else if (contactId[0].id) {
            var contactObject = DFEPhoneCallOutGoing.data.getContactFullNameById(contactId[0].id);
            if (contactObject) {
                subject = "Phone call to " + contactObject.fullname;
            }
        }
    }
    DFEPhoneCallOutGoing.utils.setAttributeValue("subject", subject);
}

DFEPhoneCallOutGoing.ClosePhoneCall_onOutcomeCodeChange = function (eCxt) {
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

    // get the current value and only show the choose email template item if successful has been selected
    var successSelection = 222750000;
    var webResourceChooseEmailTemplates = Xrm.Page.ui.controls.get("WebResource_onsuccesschooseemailtemplates");
    var webResourceWithCloseButton = Xrm.Page.ui.controls.get("WebResource_CloseThisCall");
    var selectedValue = DFEPhoneCallOutGoing.utils.getOptionValue('dfe_calloutcomecodeoutbound');

    if (selectedValue == successSelection) {
        webResourceChooseEmailTemplates.setVisible(true);
        webResourceWithCloseButton.setVisible(false);
        DFEPhoneCallOutGoing.utils.disableField("dfe_calloutcomecodeoutbound", true);
    } else {
        webResourceChooseEmailTemplates.setVisible(false);
        // clear any selections from the field
        DFEPhoneCallOutGoing.utils.setAttributeValue("dfe_selectedemailtemplate");
        DFEPhoneCallOutGoing.utils.setAttributeValue("dfe_selecteddiscussionpoints");

        //Set the phone call as complete (for Outgoing, this means setting a statuscode of 2 and a statecode of 1)
        DFEPhoneCallOutGoing.utils.setAttributeValue("statuscode", 2);
        DFEPhoneCallOutGoing.utils.setAttributeValue("statecode", 1);
        //Now save the record and close the form
        formContext.data.entity.save("saveandclose");
    }
}