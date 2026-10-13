var DFEAppointmentForm = DFEAppointmentForm ? DFEAppointmentForm : {};

DFEAppointmentForm.formOnLoad = function (eCxt) {
    DFEAppointmentForm.utils = DFEAppointmentForm.utils || new window.dfe_common.utils();
    DFEAppointmentForm.data = DFEAppointmentForm.data || new window.dfe_common.data();
    window.CRM_FORM_TYPE_CREATE = 1;
    window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFEAppointmentForm.utils.getFormType();
    var formContext = eCxt.getFormContext();
    var type = formContext.getAttribute("dfe_type").getValue();
    DFEAppointmentForm.updateForm(type);
    DFEAppointmentForm.UpdateContactDetails();
    DFEAppointmentForm.utils.setVisible("dfe_requirephonecall", false);
    if (window.FORM_TYPE != window.CRM_FORM_TYPE_CREATE) {
        DFEAppointmentForm.utils.disableField("dfe_type", true);
    }
    else {
        DFEAppointmentForm.utils.setAttributeValue("dfe_phonecallflag", true);
    }
}

DFEAppointmentForm.appointmentTypeOnChange = function (eCxt) {
    var formContext = eCxt.getFormContext();
    var appointmentTypeValue = eCxt.getEventSource().getValue();
    DFEAppointmentForm.updateForm(appointmentTypeValue);
    DFEAppointmentForm.UpdateContactDetails();
}

DFEAppointmentForm.startTimeOnChange = function (eCxt) {
    var formContext = eCxt.getFormContext();
    var startDate = eCxt.getEventSource().getValue();
    if (!DFEAppointmentForm.RestrictPastDate(startDate)) {
        formContext.getAttribute("scheduledstart").setValue("");
        //eCxt.getEventSource().setAttributeValue("");
    }
}
DFEAppointmentForm.endTimeOnChange = function (eCxt) {
    var formContext = eCxt.getFormContext();
    var endDate = eCxt.getEventSource().getValue();
    if (!DFEAppointmentForm.RestrictPastDate(endDate)) {
        formContext.getAttribute("scheduledend").setValue("");
    }
}

DFEAppointmentForm.contactOnChange = function (eCxt) {
    DFEAppointmentForm.UpdateContactDetails();
}

DFEAppointmentForm.updateForm = function (type) {
    //scheduleddurationminutes	
    var isPhoneCall = false;
    var phoneCallRequired = "none";
    if (type == false) //Phone call
    {
        isPhoneCall = true;
        //phoneCallRequired = "required";
        if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE) {
            DFEAppointmentForm.utils.setAttributeValue("dfe_requirephonecall", true);
            //Assign currently logged in user
            var userId = Xrm.Page.context.getUserId();
            var UserName = Xrm.Page.context.getUserName();
            var currentUser = DFEAppointmentForm.utils.getLookup(userId, "systemuser", UserName);
            DFEAppointmentForm.utils.setLookup(currentUser, "dfe_callfrom");
        }
    }
    DFEAppointmentForm.utils.setTabSectionVisible("appointment", "appointment description", !isPhoneCall);
    DFEAppointmentForm.utils.setVisible("requiredattendees", !isPhoneCall);
    //DFEAppointmentForm.utils.setVisible("optionalattendees", !isPhoneCall);
    DFEAppointmentForm.utils.setVisible("location", !isPhoneCall);
    DFEAppointmentForm.utils.setVisible("isalldayevent", !isPhoneCall);
    DFEAppointmentForm.utils.setVisible("dfe_phonecall", isPhoneCall);
    //DFEAppointmentForm.utils.setVisible("msdyncrm_associatedcustomerjourneyiteration",!isPhoneCall);
    DFEAppointmentForm.utils.setTabSectionVisible("appointment", "Phonecallsection", isPhoneCall);
    DFEAppointmentForm.utils.setRequiredLevel("dfe_phonecall", phoneCallRequired);
}

DFEAppointmentForm.UpdateContactDetails = function () {
    var contactId = DFEAppointmentForm.utils.getAttributeValue("regardingobjectid");
    var type = DFEAppointmentForm.utils.getAttributeValue("dfe_type");
    var subjectField = DFEAppointmentForm.utils.getAttributeValue("subject");
    var subject = "";   
    if (contactId && contactId[0])
    {
        var contactName = "";
        if (contactId[0].name) {
            contactName = contactId[0].name;
        }
        else if (contactId[0].id) {
            var contactObject = DFEAppointmentForm.data.getContactFullNameById(contactId[0].id);
            if (contactObject) {
                contactName = contactObject.fullname;
            }
        }
        //Set Contact lookup
        var selectedContact = DFEAppointmentForm.utils.getLookup(contactId[0].id, "contact", contactName);
        DFEAppointmentForm.utils.setLookup(selectedContact, "dfe_contactid");

        if (!subjectField || subjectField == "") {
            if (type == false) //Phone call
            {
                subject = "Phone call to " + contactName;
                DFEAppointmentForm.utils.setAttributeValue("subject", subject);

                //Assign Phone number
                var formphoneNumber = DFEAppointmentForm.utils.getAttributeValue("dfe_phonenumber");
                if (!formphoneNumber) {
                    var phoneNumber = DFEAppointmentForm.data.getPhoneNumberFromContact(contactId);
                    DFEAppointmentForm.utils.setAttributeValue("dfe_phonenumber", phoneNumber);
                }
            }
        }
    }    	
}

DFEAppointmentForm.RestrictPastDate = function (dateTime) {
    if (dateTime) {
        dateTime.setHours(0, 0, 0, 0);
        var today = new Date();
        today.setHours(0, 0, 0, 0);

        if (dateTime < today) {
            alert("Date can not be less than today");
            return false;
        }
    }
    return true;
}