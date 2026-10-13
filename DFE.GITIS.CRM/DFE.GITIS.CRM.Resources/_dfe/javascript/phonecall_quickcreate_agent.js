var DFEPhoneCallAgentQuickCreate = DFEPhoneCallAgentQuickCreate ? DFEPhoneCallAgentQuickCreate : {};

DFEPhoneCallAgentQuickCreate.formOnLoad = function (eCxt) {
    DFEPhoneCallAgentQuickCreate.utils = DFEPhoneCallAgentQuickCreate.utils || new window.dfe_common.utils();
    DFEPhoneCallAgentQuickCreate.data = DFEPhoneCallAgentQuickCreate.data || new window.dfe_common.data();           

    //Assign Phone Number from contact
    var contactId = DFEPhoneCallAgentQuickCreate.utils.getAttributeValue("to");
    var phoneNumberControl = DFEPhoneCallAgentQuickCreate.utils.getControl("phonenumber");
    var formphoneNumber = phoneNumberControl.getAttribute().getValue();
    phoneNumberControl.clearNotification();
    if (!formphoneNumber) {
        var phoneNumber = DFEPhoneCallAgentQuickCreate.data.getPhoneNumberFromContact(contactId);
        DFEPhoneCallAgentQuickCreate.utils.setAttributeValue("phonenumber", phoneNumber);
    }
    DFEPhoneCallAgentQuickCreate.utils.setAttributeValue("dfe_appointmentrequired", false);
}

DFEPhoneCallAgentQuickCreate.formOnSave = function (eCxt) {
    var phoneNumberControl = DFEPhoneCallAgentQuickCreate.utils.getControl("phonenumber");
    var result = DFEPhoneCallAgentQuickCreate.utils.isValidPhoneNumber(phoneNumberControl.getAttribute().getValue());
    if (!result) {
        //alert("The format of the phone number is invalid");
        phoneNumberControl.setNotification("The format of the phone number is invalid");
        phoneNumberControl.setFocus();
        eCxt.getEventArgs().preventDefault();//will prevent CRM form save
    }
    else {
        phoneNumberControl.clearNotification();
    }
}
DFEPhoneCallAgentQuickCreate.OnContactChange = function (eCxt) {    
    var contactId = eCxt.getEventSource().getValue();
    var formphoneNumber = DFEPhoneCallAgentQuickCreate.utils.getAttributeValue("phonenumber");
    if (!formphoneNumber) {
        var phoneNumber = DFEPhoneCallAgentQuickCreate.data.getPhoneNumberFromContact(contactId);
        DFEPhoneCallAgentQuickCreate.utils.setAttributeValue("phonenumber", phoneNumber);
    }
    DFEPhoneCallAgentQuickCreate.UpdateSubject();
}

DFEPhoneCallAgentQuickCreate.ValidatePhoneNumber = function (eCxt) {
    var phoneNumberControl = DFEPhoneCallAgentQuickCreate.utils.getControl("phonenumber");
    var phonenumber = phoneNumberControl.getAttribute().getValue();
    var result = DFEPhoneCallAgentQuickCreate.utils.isValidPhoneNumber(phonenumber);
    if (!result) {
        phoneNumberControl.setNotification("The format of the phone number is invalid");
        phoneNumberControl.setFocus();
    }
    else {
        phoneNumberControl.clearNotification();
    }
}

DFEPhoneCallAgentQuickCreate.UpdateSubject = function () {
    var contactId = DFEPhoneCallAgentQuickCreate.utils.getAttributeValue("to");
    var subject = "";
    if (contactId && contactId[0]) {
        if (contactId[0].name) {
            subject = "Phone call to " + contactId[0].name;
        }
        else if (contactId[0].id) {
            var contactObject = DFEPhoneCallAgentQuickCreate.data.getContactFullNameById(contactId[0].id);
            if (contactObject) {
                subject = "Phone call to " + contactObject.fullname;
            }
        }
    }
    DFEPhoneCallAgentQuickCreate.utils.setAttributeValue("subject", subject);
}