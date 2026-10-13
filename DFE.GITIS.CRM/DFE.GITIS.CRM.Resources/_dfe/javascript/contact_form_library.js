var DFEContactForm = DFEContactForm ? DFEContactForm : {};

DFEContactForm.formOnLoad = function (eCxt) {
    DFEContactForm.utils = DFEContactForm.utils || new window.dfe_common.utils();
    DFEContactForm.data = DFEContactForm.data || new window.dfe_common.data();
    window.CRM_FORM_TYPE_CREATE = 1;
    window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFEContactForm.utils.getFormType();

    // Hide Navigation Items
    //var items = Xrm.Page.ui.navigation.items;
    DFEContactForm.utils.setNavVisible("navActivities", false);
    DFEContactForm.utils.setNavVisible("navConnections", false);
    DFEContactForm.utils.setNavVisible("navEntitlement", false);
    DFEContactForm.utils.setNavVisible("navLeads", false);
    DFEContactForm.utils.setNavVisible("navOpps", false);
    DFEContactForm.utils.setNavVisible("navOppsContactParent", false);
    DFEContactForm.utils.setNavVisible("navSocialprofile", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_appointment", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_contact_Candidates", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_barrierorconcern_Contact", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidatejourneystep_contact", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidatelanguageskills_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidatepastteachingposition_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidatepreferredcontacttime_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidatequalification_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidateschoolexperience_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidateteachingleavingreason_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidateteachingpostappliedfor_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidatetrainedtaughtteachingsubject_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_candidateworkexperience_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_sms_TPUK_ContactID", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_dfe_teachingcollegeappliedto_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_email_TPUK_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_fax_Tpuk_ContactID", false);
    DFEContactForm.utils.setNavVisible("nav_dfe_contact_phonecall_contactid", false);
    DFEContactForm.utils.setNavVisible("nav_msa_contact_opportunity", false);
    DFEContactForm.utils.setNavVisible("nav_msdyn_contact_msdyn_surveyinvite_ContactId", false);
    DFEContactForm.utils.setNavVisible("nav_msgdpr_contact_msgdpr_gdprparent", false);
    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();
    //Set up variables to control the Tab Visibility we will set later
    var RTTtab = formContext.ui.tabs.get("tab_RTT");
    var RTTtabVisibility = false;
    var ITTtab = formContext.ui.tabs.get("tab_ITTPreferences");
    var ITTProvidersTab = formContext.ui.tabs.get("tab_ittproviders");
    var ITTtabVisibility = false;
    var CandidateStepstab = formContext.ui.tabs.get("tab_CandidateSteps");
    var Appointmentstab = formContext.ui.tabs.get("tab_Appointments");
    var AppointmentstabVisibility = false;
    var MyTaskstab = formContext.ui.tabs.get("tab_MyTasks");
    var MyTaskstabVisibility = false;
    var SchoolExperiencetab = formContext.ui.tabs.get("tab_SchoolExperience");
    var SchoolExperiencetabVisibility = false;
    var Languagestab = formContext.ui.tabs.get("tab_Languages");
    var LanguagestabVisibility = false;
    var BarriersOrConcernstab = formContext.ui.tabs.get("tab_BarriersOrConcerns");
    var BarriersOrConcernstabVisibility = false;
    var Securitytab = formContext.ui.tabs.get("tab_Security");
    var SecuritytabVisibility = false;
    var MiscellaneousNotestab = formContext.ui.tabs.get("tab_MiscellaneousNotes");
    var MiscellaneousNotestabVisibility = false;


    //Check if the dfe_typeofcandidate field is null
    if (formContext.getAttribute("dfe_typeofcandidate").getValue() == null) {
        //Hide all the above sections of the form - leave all Tab visibilities set to false.
    }
    else {

        //Candidate type is not null, show most tabs whether ITT or RTT value has been previously chosen        
        var AppointmentstabVisibility = true;
        var MyTaskstabVisibility = true;
        var SchoolExperiencetabVisibility = true;
        var LanguagestabVisibility = true;
        var BarriersOrConcernstabVisibility = true;
        var SecuritytabVisibility = true;
        var MiscellaneousNotestabVisibility = true;

        var typeValue = formContext.getAttribute("dfe_typeofcandidate").getValue();

        if (typeValue == 222750000) {  //ITT Option Set Value - Show ITT and Hide RTT
            var ITTtabVisibility = true;
        }

        if (typeValue == 222750001) {  //RTT Option Set Value - Show RTT and Hide ITT
            var RTTtabVisibility = true;
        }
    }

    //Now set all of the tab visibilities correctly on the form
    RTTtab.setVisible(RTTtabVisibility);
    ITTtab.setVisible(ITTtabVisibility);
    ITTProvidersTab.setVisible(ITTtabVisibility);
    CandidateStepstab.setVisible(true);
    Appointmentstab.setVisible(AppointmentstabVisibility);
    MyTaskstab.setVisible(MyTaskstabVisibility);
    SchoolExperiencetab.setVisible(SchoolExperiencetabVisibility);
    Languagestab.setVisible(LanguagestabVisibility);
    BarriersOrConcernstab.setVisible(BarriersOrConcernstabVisibility);
    Securitytab.setVisible(SecuritytabVisibility);
    MiscellaneousNotestab.setVisible(MiscellaneousNotestabVisibility);
    var isAdvisorRequired = DFEContactForm.utils.getAttributeValue("dfe_isadvisorrequiredos");

    DFEContactForm.ShowHideFieldsOnLoad(isAdvisorRequired);
    DFEContactForm.OnAdvisorStatusChange();
}

DFEContactForm.candidateTypeOnChange = function (eCxt) {
    //get the formContext from the passed in execution context
    var formContext = eCxt.getFormContext();
    //get the new value of the Candidate Type
    var newCandidateTypeValue = eCxt.getEventSource().getValue();

    //Set up variables to control the Tab Visibility we will set later
    var RTTtab = formContext.ui.tabs.get("tab_RTT");
    var RTTtabVisibility = false;
    var ITTtab = formContext.ui.tabs.get("tab_ITTPreferences");
    var ITTProvidersTab = formContext.ui.tabs.get("tab_ittproviders");
    var ITTtabVisibility = false;
    var CandidateStepstab = formContext.ui.tabs.get("tab_CandidateSteps");
    var Appointmentstab = formContext.ui.tabs.get("tab_Appointments");
    var AppointmentstabVisibility = false;
    var MyTaskstab = formContext.ui.tabs.get("tab_MyTasks");
    var MyTaskstabVisibility = false;
    var SchoolExperiencetab = formContext.ui.tabs.get("tab_SchoolExperience");
    var SchoolExperiencetabVisibility = false;
    var Languagestab = formContext.ui.tabs.get("tab_Languages");
    var LanguagestabVisibility = false;
    var BarriersOrConcernstab = formContext.ui.tabs.get("tab_BarriersOrConcerns");
    var BarriersOrConcernstabVisibility = false;
    var Securitytab = formContext.ui.tabs.get("tab_Security");
    var SecuritytabVisibility = false;
    var MiscellaneousNotestab = formContext.ui.tabs.get("tab_MiscellaneousNotes");
    var MiscellaneousNotestabVisibility = false;


    //Check if the dfe_typeofcandidate field is null
    if (newCandidateTypeValue == null) {
        //Hide all the above sections of the form - leave all Tab visibilities set to false.
    }
    else {

        //Candidate type is not null, show most tabs whether ITT or RTT has been chosen
        var AppointmentstabVisibility = true;
        var MyTaskstabVisibility = true;
        var SchoolExperiencetabVisibility = true;
        var LanguagestabVisibility = true;
        var BarriersOrConcernstabVisibility = true;
        var SecuritytabVisibility = true;
        var MiscellaneousNotestabVisibility = true;

        if (newCandidateTypeValue == 222750000) {  //ITT Option Set Value - Show ITT and Hide RTT
            var ITTtabVisibility = true;
        }

        if (newCandidateTypeValue == 222750001) {  //RTT Option Set Value - Show RTT and Hide ITT
            var RTTtabVisibility = true;
        }
    }

    //Now set all of the tab visibilities correctly on the form
    RTTtab.setVisible(RTTtabVisibility);
    ITTtab.setVisible(ITTtabVisibility);
    ITTProvidersTab.setVisible(ITTtabVisibility);
    CandidateStepstab.setVisible(true);
    Appointmentstab.setVisible(AppointmentstabVisibility);
    MyTaskstab.setVisible(MyTaskstabVisibility);
    SchoolExperiencetab.setVisible(SchoolExperiencetabVisibility);
    Languagestab.setVisible(LanguagestabVisibility);
    BarriersOrConcernstab.setVisible(BarriersOrConcernstabVisibility);
    Securitytab.setVisible(SecuritytabVisibility);
    MiscellaneousNotestab.setVisible(MiscellaneousNotestabVisibility);
}

DFEContactForm.advisorRequiredOnChange = function () {
    var isAdvisorRequired = DFEContactForm.utils.getAttributeValue("dfe_isadvisorrequiredos");
    DFEContactForm.ShowHideFields(isAdvisorRequired);
}

DFEContactForm.ShowHideFields = function (isAdvisorRequired) {
    if (isAdvisorRequired == 222750001)//Yes
    {
        DFEContactForm.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactForm.utils.setVisible("dfe_callagentnotesforadvisor", true);
        if (DFEContactForm.AllowCandidateStatusChange()) {
            DFEContactForm.utils.setAttributeValue("dfe_candidatestatus", 222750001);//Waiting to be assigned
        }
        DFEContactForm.utils.setAttributeValue("dfe_waitingtobeassigneddate", new Date());

    }
    else if (isAdvisorRequired == 222750000)//No
    {
        DFEContactForm.utils.setVisible("dfe_reasonfornotwantinganadvisor", true);
        DFEContactForm.utils.setVisible("dfe_callagentnotesforadvisor", false);
        if (DFEContactForm.AllowCandidateStatusChange()) {
            DFEContactForm.utils.setAttributeValue("dfe_candidatestatus", 222750000);//unassigned
        }
        DFEContactForm.utils.setAttributeValue("dfe_waitingtobeassigneddate");

    }
    else {
        DFEContactForm.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactForm.utils.setVisible("dfe_callagentnotesforadvisor", false);
        if (DFEContactForm.AllowCandidateStatusChange()) {
            DFEContactForm.utils.setAttributeValue("dfe_candidatestatus", 222750000);//unassigned
        }
        DFEContactForm.utils.setAttributeValue("dfe_waitingtobeassigneddate");
    }
}
//----------------------------------------------------------------------
//Developer: Arjun, 05/02/2020
//Purpose: This function will be triggered onload of a 'Create' form. This is a mirror of ShowHideFields()
//excluding the events that sets 2 attributes.
DFEContactForm.ShowHideFieldsOnLoad = function (isAdvisorRequired) {
    if (isAdvisorRequired == 222750001)//Yes
    {
        DFEContactForm.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactForm.utils.setVisible("dfe_callagentnotesforadvisor", true);
    }
    else if (isAdvisorRequired == 222750000)//No
    {
        DFEContactForm.utils.setVisible("dfe_reasonfornotwantinganadvisor", true);
        DFEContactForm.utils.setVisible("dfe_callagentnotesforadvisor", false);
    }
    else {
        DFEContactForm.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactForm.utils.setVisible("dfe_callagentnotesforadvisor", false);
    }
}
//-----------------------------------------------------------------

DFEContactForm.AllowCandidateStatusChange = function () {
    var candidateStatus = DFEContactForm.utils.getAttributeValue("dfe_candidatestatus");
    if (!candidateStatus)
        return true;
    if (candidateStatus == 222750002)//assigned
    {
        return false;
    }
    else
        return true;
}

DFEContactForm.ownerOnChange = function (context) {
}

//Developer: Kavitha Mutyala, 14/08/2020
//Purpose: This function will be triggered on change of Candidate Adviser Status and on form load as well
DFEContactForm.OnAdvisorStatusChange = function () {

    //get option set control and its options
    var optionsetControl = Xrm.Page.ui.controls.get("dfe_candidateadviserstatusreason");
    optionsetControl.clearOptions();

    //get current selected value of option set
    var status = Xrm.Page.getAttribute("dfe_candidateadviserstatus").getValue();
    var passconfig = "GITIS";

    var configdata = DFEContactForm.data.getConfigByName(passconfig);
    var openstatusreasons = configdata.dfe_adviseropenreasons;
    var closedstatusreasons = configdata.dfe_adviserclosedreasons;

    var closedArraydata = closedstatusreasons.split(",");
    var openArraydata = openstatusreasons.split(",");

    var totalOptions = Xrm.Page.getAttribute("dfe_candidateadviserstatusreason").getOptions();
    for (var i = 0; i < totalOptions.length; i++) {
        optionsetControl.addOption(totalOptions[i]);
    }

    if (status == 222750000) //open status
    {
        for (var i = 0; i < closedArraydata.length; i++) {
            optionsetControl.removeOption(parseInt(closedArraydata[i]));
        }
    }

    if (status == 222750001)  //Closed status
    {
        for (var i = 0; i < openArraydata.length; i++) {
            optionsetControl.removeOption(parseInt(openArraydata[i]));
        }
    }
}