var DFETaskAgentForm = DFETaskAgentForm ? DFETaskAgentForm : {};

DFETaskAgentForm.formOnLoad = function (eCxt) {
    DFETaskAgentForm.utils = DFETaskAgentForm.utils || new window.dfe_common.utils();
    window.CRM_FORM_TYPE_CREATE = 1;
    window.FORM_TYPE = DFETaskAgentForm.utils.getFormType();    
    DFETaskAgentForm.ShowHideFields();
}

DFETaskAgentForm.statusCodeOnChange = function (eCxt) {
    var formContext = eCxt.getFormContext();
    var statusCode = formContext.getAttribute("statuscode").getValue();
    if (statusCode == 2) //Not yet started
    {
        DFETaskAgentForm.utils.setLookup(null, "dfe_callagent");
        DFETaskAgentForm.utils.setAttributeValue("dfe_datecompleted");
    }
    else if (statusCode == 3) //In progress
    {
        //Assign currently logged in user to Call agent field
        var globalContext = Xrm.Utility.getGlobalContext();
        var userId = globalContext.userSettings.userId;
        var userName = globalContext.userSettings.userName;
        var currentUser = DFETaskAgentForm.utils.getLookup(userId, "systemuser", userName);
        DFETaskAgentForm.utils.setLookup(currentUser, "dfe_callagent");
        DFETaskAgentForm.utils.setAttributeValue("dfe_datecompleted");
        
    }
    else if (statusCode == 4) //Done
    {        
        DFETaskAgentForm.utils.setAttributeValue("dfe_datecompleted", new Date());        
    }
    DFETaskAgentForm.ShowHideFields();
}

DFETaskAgentForm.ShowHideFields = function () {
    var statusCode = DFETaskAgentForm.utils.getAttributeValue("statuscode");
    
    if (statusCode == 2) //Not yet started
    {
        DFETaskAgentForm.utils.setVisible("dfe_callagent", false);
        DFETaskAgentForm.utils.setVisible("dfe_datecompleted", false);
        DFETaskAgentForm.utils.setVisible("dfe_callagentoutcomenotes", false);
        DFETaskAgentForm.utils.setVisible("dfe_adviseroutcomenotes", false);        
    }
    else if (statusCode == 3) //In progress
    {
        DFETaskAgentForm.utils.setVisible("dfe_callagent", true);
        DFETaskAgentForm.utils.setVisible("dfe_callagentoutcomenotes", true);
    }
    else if (statusCode == 4) //Done
    {
        DFETaskAgentForm.utils.setVisible("dfe_callagent", true);
        DFETaskAgentForm.utils.setVisible("dfe_callagentoutcomenotes", true);
    }    
}