var DFETaskForm = DFETaskForm ? DFETaskForm : {};

//This is only for adviser form
DFETaskForm.formOnLoad = function (eCxt) {
    DFETaskForm.utils = DFETaskForm.utils || new window.dfe_common.utils();
    //DFETaskForm.data = DFETaskForm.data || new window.dfe_common.data();
    window.CRM_FORM_TYPE_CREATE = 1;
    //window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFETaskForm.utils.getFormType();
    var formContext = eCxt.getFormContext();
    DFETaskForm.ShowHideFields();
}

DFETaskForm.ShowHideFields = function () {
    var statusCode = DFETaskForm.utils.getAttributeValue("statuscode");

    
    if (window.FORM_TYPE == window.CRM_FORM_TYPE_CREATE) {        
        //Assign currently logged in user to Adviser field
        var userId = Xrm.Page.context.getUserId();
        var UserName = Xrm.Page.context.getUserName();
        var currentUser = DFETaskForm.utils.getLookup(userId, "systemuser", UserName);
        DFETaskForm.utils.setLookup(currentUser, "dfe_adviser");
    }
    if (statusCode == 2) //Not yet started
    {
        DFETaskForm.utils.setVisible("dfe_callagent", false);
        DFETaskForm.utils.setVisible("dfe_datecompleted", false);
        DFETaskForm.utils.setVisible("dfe_callagentoutcomenotes", false);
        DFETaskForm.utils.setVisible("dfe_adviseroutcomenotes", false);
        DFETaskForm.utils.setVisible("createdon", false);
    }
    else {
        DFETaskForm.utils.disableField("dfe_callagent", true);
        DFETaskForm.utils.disableField("dfe_callagentoutcomenotes", true);
        DFETaskForm.utils.disableField("regardingobjectid", true);
        DFETaskForm.utils.disableField("subject", true);
        DFETaskForm.utils.disableField("description", true);
    }

}