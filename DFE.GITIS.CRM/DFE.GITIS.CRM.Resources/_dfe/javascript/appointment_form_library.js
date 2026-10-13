var DFEAppointmentForm = DFEAppointmentForm ? DFEAppointmentForm : {};

DFEAppointmentForm.formOnLoad = function (eCxt) {
    DFEAppointmentForm.utils = DFEAppointmentForm.utils || new window.dfe_common.utils();
    DFEAppointmentForm.data = DFEAppointmentForm.data || new window.dfe_common.data();
    window.CRM_FORM_TYPE_CREATE = 1;
    window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFEAppointmentForm.utils.getFormType();
    var formContext = eCxt.getFormContext();
    DFEAppointmentForm.updateForm(formContext);
    DFEAppointmentForm.UpdateContactDetails();
    DFEAppointmentForm.utils.setVisible("dfe_requirephonecall", false);
}

DFEAppointmentForm.startTimeOnChange = function (eCxt) {
    var formContext = eCxt.getFormContext();
    var startDate = eCxt.getEventSource().getValue();
    if (!DFEAppointmentForm.RestrictPastDate(startDate)) {
        formContext.getAttribute("scheduledstart").setValue(null);
    }
}
DFEAppointmentForm.endTimeOnChange = function (eCxt) {
    var formContext = eCxt.getFormContext();
    var endDate = eCxt.getEventSource().getValue();
    if (!DFEAppointmentForm.RestrictPastDate(endDate)) {
        formContext.getAttribute("scheduledend").setValue(null);
    }
}

DFEAppointmentForm.contactOnChange = function (eCxt) {
    DFEAppointmentForm.UpdateContactDetails();
}

DFEAppointmentForm.updateForm = function (context) {

    var isPhoneCall = false;

    DFEAppointmentForm.utils.setTabSectionVisible("appointment", "appointment description", !isPhoneCall);
    DFEAppointmentForm.utils.setVisible("requiredattendees", !isPhoneCall);
    DFEAppointmentForm.utils.setVisible("location", !isPhoneCall);
    DFEAppointmentForm.utils.setVisible("isalldayevent", !isPhoneCall);
    DFEAppointmentForm.utils.setTabSectionVisible("appointment", "Phonecallsection", isPhoneCall);
}

DFEAppointmentForm.UpdateContactDetails = function () {
    var contactId = DFEAppointmentForm.utils.getAttributeValue("regardingobjectid");

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
    }    	
}

DFEAppointmentForm.RestrictPastDate = function (dateTime) {
    if (dateTime) {
        dateTime.setHours(0, 0, 0, 0);
        var today = new Date();
        today.setHours(0, 0, 0, 0);

        if (dateTime < today) {
            var alertStrings = { confirmButtonLabel: "Yes", text: "Date can not be less than today", title: "Invalid Date" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function (success) {
                    return false;
                },
                function (error) {
                    console.log(error.message);
                }
            );
            return false;
        }
    }
    return true;
}