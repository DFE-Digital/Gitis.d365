var DFEPhoneCallAdviser = DFEPhoneCallAdviser ? DFEPhoneCallAdviser : {};

DFEPhoneCallAdviser.formOnLoad = function(eCxt) {
	DFEPhoneCallAdviser.utils = DFEPhoneCallAdviser.utils || new window.dfe_common.utils();
    DFEPhoneCallAdviser.data = DFEPhoneCallAdviser.data || new window.dfe_common.data();	 
	window.CRM_FORM_TYPE_CREATE = 1;
    window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFEPhoneCallAdviser.utils.getFormType();
  
	//get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();
    //store the form context into the parent window object so we can retrieve it from within the html web resource
    window.parent.FormContextP = formContext;

    //Limit the list of entities in the party lookup to be Contacts and Leads only
    //NOTE - THESE 2 LINES ARE CAUSING AN ERROR IN THE UNIFIED INTERFACE
   // var associatedContactView = formContext.getAttribute('to').getLookupDataAttribute();
   // associatedContactView.setLookupTypes(['contact']);

	//Assign Phone NUmber from contact
	var contactId = DFEPhoneCallAdviser.utils.getAttributeValue("to");
	var phoneNumberControl = DFEPhoneCallAdviser.utils.getControl("phonenumber");
	var formphoneNumber = phoneNumberControl.getAttribute().getValue();
	//phoneNumberControl.clearNotification();	
	if(!formphoneNumber)
	{
		var phoneNumber = DFEPhoneCallAdviser.data.getPhoneNumberFromContact(contactId);
		DFEPhoneCallAdviser.utils.setAttributeValue("phonenumber",phoneNumber);
	}
	//DFEPhoneCallAdviser.UpdateSubject();//Do we need this??
	if(window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE)
	{
        DFEPhoneCallAdviser.utils.setAttributeValue("dfe_appointmentflag", true);
	}
    DFEPhoneCallAdviser.utils.setVisible("dfe_appointmentflag", false);

    //Assign currently logged in user if the value is balnk
    var callFrom = DFEPhoneCallAdviser.utils.getAttributeValue("from");

    if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE || !callFrom || callFrom.length == 0) {
        var userId = Xrm.Page.context.getUserId();
        var UserName = Xrm.Page.context.getUserName();
        var currentUser = DFEPhoneCallAdviser.utils.getLookup(userId, "systemuser", UserName);
        DFEPhoneCallAdviser.utils.setLookup(currentUser, "from");
    }
}

DFEPhoneCallAdviser.formOnSave = function(eCxt) {
	//var phoneNumberControl = DFEPhoneCallAdviser.utils.getControl("phonenumber");
	//var result = DFEPhoneCallAdviser.utils.isValidPhoneNumber(phoneNumberControl.getAttribute().getValue());
	//if (!result)	
	//{
	//	//alert("The format of the phone number is invalid");
	//	phoneNumberControl.setNotification("The format of the phone number is invalid");
	//	phoneNumberControl.setFocus();
	//	eCxt.getEventArgs().preventDefault();//will prevent CRM form save
	//}
	//else
	//{
	//	phoneNumberControl.clearNotification();		
	//}
}
DFEPhoneCallAdviser.OnContactChange = function(eCxt) {
    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext(); 
    var contactId = eCxt.getEventSource().getValue();	
	var formphoneNumber = DFEPhoneCallAdviser.utils.getAttributeValue("phonenumber");
	if(!formphoneNumber)
	{
		var phoneNumber = DFEPhoneCallAdviser.data.getPhoneNumberFromContact(contactId);
		DFEPhoneCallAdviser.utils.setAttributeValue("phonenumber",phoneNumber);
	}	
	DFEPhoneCallAdviser.UpdateSubject();
}

DFEPhoneCallAdviser.ValidatePhoneNumber = function(eCxt)
{
	var phoneNumberControl = DFEPhoneCallAdviser.utils.getControl("phonenumber");	
    var phonenumber = phoneNumberControl.getAttribute().getValue();
	var result = DFEPhoneCallAdviser.utils.isValidPhoneNumber(phonenumber);
	if (!result)	
	{		
		phoneNumberControl.setNotification("The format of the phone number is invalid");
		phoneNumberControl.setFocus();
	}
	else
	{
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
