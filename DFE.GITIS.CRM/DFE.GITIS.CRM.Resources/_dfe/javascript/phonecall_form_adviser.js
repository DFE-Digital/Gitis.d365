var DFEPhoneCallAdviser = DFEPhoneCallAdviser ? DFEPhoneCallAdviser : {};

DFEPhoneCallAdviser.formOnLoad = function(eCxt) {
	DFEPhoneCallAdviser.utils = DFEPhoneCallAdviser.utils || new window.dfe_common.utils();
    DFEPhoneCallAdviser.data = DFEPhoneCallAdviser.data || new window.dfe_common.data();	 
	window.CRM_FORM_TYPE_CREATE = 1;
    window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFEPhoneCallAdviser.utils.getFormType();
  
    var formContext = eCxt.getFormContext();
    //store the form context into the parent window object so we can retrieve it from within the html web resource
    window.parent.FormContextP = formContext;

	//Assign Phone number from contact
	var contactId = DFEPhoneCallAdviser.utils.getAttributeValue("to");
	var phoneNumberControl = DFEPhoneCallAdviser.utils.getControl("phonenumber");
	var formphoneNumber = phoneNumberControl.getAttribute().getValue();

	if(!formphoneNumber)
	{
		var phoneNumber = DFEPhoneCallAdviser.data.getPhoneNumberFromContact(contactId);
		DFEPhoneCallAdviser.utils.setAttributeValue("phonenumber",phoneNumber);
	}


	if(window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE)
	{
        DFEPhoneCallAdviser.utils.setAttributeValue("dfe_appointmentflag", true);
	}
    DFEPhoneCallAdviser.utils.setVisible("dfe_appointmentflag", false);

    //Assign currently logged in user if the value is blank
    var callFrom = DFEPhoneCallAdviser.utils.getAttributeValue("from");

	if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE || !callFrom || callFrom.length == 0) {
		var globalContext = Xrm.Utility.getGlobalContext(); 
		var userId = globalContext.userSettings.userId;
		var userName = globalContext.userSettings.userName;
        var currentUser = DFEPhoneCallAdviser.utils.getLookup(userId, "systemuser", userName);
		DFEPhoneCallAdviser.utils.setLookup(currentUser, "from");
		var now = new Date();
		DFEPhoneCallAdviser.utils.setAttributeValue("scheduledstart", now);
		DFEPhoneCallAdviser.utils.setAttributeValue("scheduledend", now);
    }
}

DFEPhoneCallAdviser.OnContactChange = function(eCxt) {
    var contactId = eCxt.getEventSource().getValue();	
	var formphoneNumber = DFEPhoneCallAdviser.utils.getAttributeValue("phonenumber");
	if(!formphoneNumber)
	{
		var phoneNumber = DFEPhoneCallAdviser.data.getPhoneNumberFromContact(contactId);
		DFEPhoneCallAdviser.utils.setAttributeValue("phonenumber",phoneNumber);
	}	
	DFEPhoneCallAdviser.UpdateSubject();
}

DFEPhoneCallAdviser.ValidatePhoneNumber = function(eCxt){
	var phoneNumberControl = DFEPhoneCallAdviser.utils.getControl("phonenumber");
	var phonenumber = phoneNumberControl.getAttribute().getValue();
	var result = DFEPhoneCallAdviser.utils.isValidPhoneNumber(phonenumber);
	if (!result) {
		phoneNumberControl.setNotification("The format of the phone number is invalid");
		phoneNumberControl.setFocus();
	}
	else {
		phoneNumberControl.clearNotification();
	}
}

DFEPhoneCallAdviser.UpdateSubject = function()
{
	var contactId = DFEPhoneCallAdviser.utils.getAttributeValue("to");		
	var subject = "";		
	if(contactId && contactId[0])
	{
		if(contactId[0].name)
		{
			subject = "Phone call to " + contactId[0].name;
		}
		else if(contactId[0].id)
		{
			var contactObject = DFEPhoneCallAdviser.data.getContactFullNameById(contactId[0].id);
			if(contactObject)
			{
				subject = "Phone call to " + contactObject.fullname;				
			}
		}		
	}		
	DFEPhoneCallAdviser.utils.setAttributeValue("subject",subject);
}
