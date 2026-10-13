var DFECustomActivity = DFECustomActivity ? DFECustomActivity : {};

DFECustomActivity.formOnLoad = function() {
	DFECustomActivity.utils = DFECustomActivity.utils || new window.dfe_common.utils();
	DFECustomActivity.data = DFECustomActivity.data || new window.dfe_common.data();	 
	window.CRM_FORM_TYPE_CREATE = 1;
	window.CRM_FORM_TYPE_UPDATE = 2;
	window.FORM_TYPE = DFECustomActivity.utils.getFormType();

	//Assign currently logged in user to the to and from fields if the value is balnk
	var callFrom = DFECustomActivity.utils.getAttributeValue("from");
	var callTo = DFECustomActivity.utils.getAttributeValue("to");

	if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE || !callFrom || !callTo) {
		DFECustomActivity.OnDirectionChange();
	}
	//fill in a default date and time for the user if the value is blank
	var datetime = DFECustomActivity.utils.getAttributeValue("actualend");
	if (datetime == null) {
		var currentDatetime = new Date();
		DFECustomActivity.utils.setAttributeValue("actualend", currentDatetime);
	}

}
DFECustomActivity.OnDirectionChange = function () {

	var globalContext = Xrm.Utility.getGlobalContext();

	//Show or hide the To / From field based on the the value of the 'Direction' field
	var direction = DFECustomActivity.utils.getAttributeValue("dfe_directioncode");
	var userId = globalContext.userSettings.userId;
	var userName = globalContext.userSettings.userName;

	var regarding = DFECustomActivity.utils.getAttributeValue("regardingobjectid");
	if (regarding != null) { // regarding field value will be null if this is being created outside of a Contact record and not immediately linked to a Contact.
		var regardingTargetObj = { id: regarding[0].id, name: regarding[0].name, entityType: regarding[0].entityType };
	}
	if (direction) {//True means Outgoing
		//Set the 'From' field value to be the current user     
		DFECustomActivity.utils.setLookup({ id: userId, name: userName, entityType: "systemuser" }, "from");
			  
		if (regarding != null) {
			//Set the 'To' field value to be the Contact
			DFECustomActivity.utils.setLookup(regardingTargetObj, "to");
		}
		else {
			//blank any existing value
			DFECustomActivity.utils.setAttributeValue("to", null);
		}
		
	}
	else {//False means Incoming
		if (regarding != null) {
			//Set the 'From' field value to be the Contact
			DFECustomActivity.utils.setLookup(regardingTargetObj, "from");
		}
		else {
			//blank any existing value
			DFECustomActivity.utils.setAttributeValue("from", null);
		}
		//Set the 'To' field value to be the current user
		DFECustomActivity.utils.setLookup({ id: userId, name: userName, entityType: "systemuser" }, "to");
	}
}
