var DFEContactFormWebsite = DFEContactFormWebsite ? DFEContactFormWebsite : {};

DFEContactFormWebsite.formOnLoad = function (eCxt) {
    DFEContactFormWebsite.utils = DFEContactFormWebsite.utils || new window.dfe_common.utils();
    DFEContactFormWebsite.data = DFEContactFormWebsite.data || new window.dfe_common.data();
    window.CRM_FORM_TYPE_CREATE = 1;
    window.CRM_FORM_TYPE_UPDATE = 2;
    window.FORM_TYPE = DFEContactFormWebsite.utils.getFormType();

    // Hide Navigation Items
    //var items = Xrm.Page.ui.navigation.items;
    DFEContactFormWebsite.utils.setNavVisible("navActivities", false);
    DFEContactFormWebsite.utils.setNavVisible("navConnections", false);
    DFEContactFormWebsite.utils.setNavVisible("navEntitlement", false);
    DFEContactFormWebsite.utils.setNavVisible("navLeads", false);
    DFEContactFormWebsite.utils.setNavVisible("navOpps", false);
    DFEContactFormWebsite.utils.setNavVisible("navOppsContactParent", false);
    DFEContactFormWebsite.utils.setNavVisible("navSocialprofile", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_appointment", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_contact_Candidates", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_barrierorconcern_Contact", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidatejourneystep_contact", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidatelanguageskills_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidatepastteachingposition_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidatepreferredcontacttime_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidatequalification_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidateschoolexperience_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidateteachingleavingreason_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidateteachingpostappliedfor_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidatetrainedtaughtteachingsubject_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_candidateworkexperience_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_sms_TPUK_ContactID", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_dfe_teachingcollegeappliedto_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_email_TPUK_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_fax_Tpuk_ContactID", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_dfe_contact_phonecall_contactid", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_msa_contact_opportunity", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_msdyn_contact_msdyn_surveyinvite_ContactId", false);
    DFEContactFormWebsite.utils.setNavVisible("nav_msgdpr_contact_msgdpr_gdprparent", false);
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
    if(ITTtab != null) {
    	ITTtab.setVisible(ITTtabVisibility);
    }
	if(ITTProvidersTab != null) {
    	ITTProvidersTab.setVisible(ITTtabVisibility);
    }    
    if(CandidateStepstab != null) {
    	CandidateStepstab.setVisible(true);
    }    
    if(Appointmentstab != null) {
    	Appointmentstab.setVisible(AppointmentstabVisibility);
    }  
    if(MyTaskstab != null) {
    	MyTaskstab.setVisible(MyTaskstabVisibility);
    }
    if(SchoolExperiencetab != null) {
    	SchoolExperiencetab.setVisible(SchoolExperiencetabVisibility);
    }
    if(Languagestab != null) {
    	Languagestab.setVisible(LanguagestabVisibility);
    }
    if(BarriersOrConcernstab != null) {
    	 BarriersOrConcernstab.setVisible(BarriersOrConcernstabVisibility);
    }
    if(Securitytab != null) {
    	Securitytab.setVisible(SecuritytabVisibility);
    }    
    
	if(MiscellaneousNotestab != null) {
    	MiscellaneousNotestab.setVisible(MiscellaneousNotestabVisibility);
    } 

    var isAdvisorRequired = DFEContactFormWebsite.utils.getAttributeValue("dfe_isadvisorrequiredos");

    DFEContactFormWebsite.ShowHideFieldsOnLoad(isAdvisorRequired);
    DFEContactFormWebsite.OnAdvisorStatusChange(eCxt);
    DFEContactFormWebsite.ShowHideFieldsGCSEQualification(eCxt);
    DFEContactFormWebsite.OnReturnYearLoad(formContext); 
    DFEContactFormWebsite.ShowRTTHeaderFieldsIFRttCandidate(eCxt);
	var formType= Xrm.Page.ui.getFormType();
	if(formType == 2){
		DFEContactFormWebsite.CandidateStepsUpdate(formContext);
	}
}

DFEContactFormWebsite.candidateTypeOnChange = function (eCxt) {
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
    if(ITTtab != null) {
    	ITTtab.setVisible(ITTtabVisibility);
    }    
	if(ITTProvidersTab != null) {
    	ITTProvidersTab.setVisible(ITTtabVisibility);
    }  
    if(CandidateStepstab != null) {
    	CandidateStepstab.setVisible(true);
    } 
    if(Appointmentstab != null) {
    	Appointmentstab.setVisible(AppointmentstabVisibility);
    }
    if(MyTaskstab != null) {
    	MyTaskstab.setVisible(MyTaskstabVisibility);
    }    
    if(SchoolExperiencetab != null) {
    	SchoolExperiencetab.setVisible(SchoolExperiencetabVisibility);
    }
    if(Languagestab != null) {
    	Languagestab.setVisible(LanguagestabVisibility);
    }
    if(BarriersOrConcernstab != null) {
    	BarriersOrConcernstab.setVisible(BarriersOrConcernstabVisibility);
    }
    if(Securitytab != null) {
    	Securitytab.setVisible(SecuritytabVisibility);
    }
	if(MiscellaneousNotestab != null) {
    	MiscellaneousNotestab.setVisible(MiscellaneousNotestabVisibility);
    } 
    
    DFEContactFormWebsite.OnAdvisorStatusChange(eCxt);

    DFEContactFormWebsite.ShowRTTHeaderFieldsIFRttCandidate(eCxt);
}

DFEContactFormWebsite.advisorRequiredOnChange = function () {
    var isAdvisorRequired = DFEContactFormWebsite.utils.getAttributeValue("dfe_isadvisorrequiredos");
    DFEContactFormWebsite.ShowHideFields(isAdvisorRequired);
}

DFEContactFormWebsite.ShowHideFields = function (isAdvisorRequired) {
    if (isAdvisorRequired == 222750001)//Yes
    {
        DFEContactFormWebsite.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactFormWebsite.utils.setVisible("dfe_callagentnotesforadvisor", true);
        if (DFEContactFormWebsite.AllowCandidateStatusChange()) {
            DFEContactFormWebsite.utils.setAttributeValue("dfe_candidatestatus", 222750001);//Waiting to be assigned
        }
        DFEContactFormWebsite.utils.setAttributeValue("dfe_waitingtobeassigneddate", new Date());

    }
    else if (isAdvisorRequired == 222750000)//No
    {
        DFEContactFormWebsite.utils.setVisible("dfe_reasonfornotwantinganadvisor", true);
        DFEContactFormWebsite.utils.setVisible("dfe_callagentnotesforadvisor", false);
        if (DFEContactFormWebsite.AllowCandidateStatusChange()) {
            DFEContactFormWebsite.utils.setAttributeValue("dfe_candidatestatus", 222750000);//unassigned
        }
        DFEContactFormWebsite.utils.setAttributeValue("dfe_waitingtobeassigneddate");

    }
    else {
        DFEContactFormWebsite.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactFormWebsite.utils.setVisible("dfe_callagentnotesforadvisor", false);
        if (DFEContactFormWebsite.AllowCandidateStatusChange()) {
            DFEContactFormWebsite.utils.setAttributeValue("dfe_candidatestatus", 222750000);//unassigned
        }
        DFEContactFormWebsite.utils.setAttributeValue("dfe_waitingtobeassigneddate");
    }
}
//----------------------------------------------------------------------
//Developer: Arjun, 05/02/2020
//Purpose: This function will be triggered onload of a 'Create' form. This is a mirror of ShowHideFields()
//excluding the events that sets 2 attributes.
DFEContactFormWebsite.ShowHideFieldsOnLoad = function (isAdvisorRequired) {
    if (isAdvisorRequired == 222750001)//Yes
    {
        DFEContactFormWebsite.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactFormWebsite.utils.setVisible("dfe_callagentnotesforadvisor", true);
    }
    else if (isAdvisorRequired == 222750000)//No
    {
        DFEContactFormWebsite.utils.setVisible("dfe_reasonfornotwantinganadvisor", true);
        DFEContactFormWebsite.utils.setVisible("dfe_callagentnotesforadvisor", false);
    }
    else {
        DFEContactFormWebsite.utils.setVisible("dfe_reasonfornotwantinganadvisor", false);
        DFEContactFormWebsite.utils.setVisible("dfe_callagentnotesforadvisor", false);
    }
}
//-----------------------------------------------------------------

DFEContactFormWebsite.AllowCandidateStatusChange = function () {
    var candidateStatus = DFEContactFormWebsite.utils.getAttributeValue("dfe_candidatestatus");
    if (!candidateStatus)
        return true;
    if (candidateStatus == 222750002)//assigned
    {
        return false;
    }
    else
        return true;
}

DFEContactFormWebsite.ownerOnChange = function (context) {
}

//Developer: Kavitha Mutyala, 14/08/2020
//Purpose: This function will be triggered on change of Candidate Adviser Status, on form load and when candidate type is changed
DFEContactFormWebsite.OnAdvisorStatusChange = function (eCxt) {

    var formContext = eCxt.getFormContext();

    //get option set control and its options
    var optionsetControl = DFEContactFormWebsite.utils.getControl("dfe_candidateadviserstatusreason");
    optionsetControl.clearOptions();

    //get current selected value of option set
    var status = DFEContactFormWebsite.utils.getAttributeValue("dfe_candidateadviserstatus");
    var passconfig = "GITIS";

    var configdata = DFEContactFormWebsite.data.getConfigByName(passconfig);
    var openstatusreasons = configdata.dfe_adviseropenreasons;
    var closedstatusreasons = configdata.dfe_adviserclosedreasons;
    var ittadvisorstatusreasons = configdata.dfe_ittadvisorstatuses;
    var rttadvisorstatusreasons = configdata.dfe_rttadvisorstatuses;

    var closedArraydata = closedstatusreasons.split(",");
    var openArraydata = openstatusreasons.split(",");
    var ittArrayData = ittadvisorstatusreasons.split(",");
    var rttArrayData = rttadvisorstatusreasons.split(",");

    var totalOptions = DFEContactFormWebsite.utils.getOptions("dfe_candidateadviserstatusreason");

    for (var i = 0; i < totalOptions.length; i++) {
        optionsetControl.addOption(totalOptions[i]);
    }

    // IF OPEN STATUS remove all closed statuses
    if (status == 222750000) //open status
    {
        for (var i = 0; i < closedArraydata.length; i++) {
            optionsetControl.removeOption(parseInt(closedArraydata[i]));
        }
    }

    // IF Closed Status remove all open statuses
    if (status == 222750001)  //Closed status
    {
        for (var i = 0; i < openArraydata.length; i++) {
            optionsetControl.removeOption(parseInt(openArraydata[i]));
        }
    }

    // Remove all ITT Status Reason from RTT candidates
    if (formContext.getAttribute("dfe_typeofcandidate").getValue() == 222750001) // RTT
    {
        for (var i = 0; i < ittArrayData.length; i++) {
            optionsetControl.removeOption(parseInt(ittArrayData[i]));
        }
    }

    // Remove all RTT statuses from ITT Type candidates
    if (formContext.getAttribute("dfe_typeofcandidate").getValue() == 222750000) // ITT
    {
        for (var i = 0; i < rttArrayData.length; i++) {
            optionsetControl.removeOption(parseInt(rttArrayData[i]));
        }
    }
}



//--------------------------------------

//Developer: Arjun, 18/09/2020
//Purpose: To hide and show GCSE Qualifications based on the relevant questions
DFEContactFormWebsite.ShowHideFieldsGCSEQualification = function (exCxt) {

    var formContext = exCxt.getFormContext();
    var hasGcseEnglish = formContext.getAttribute("dfe_websitehasgcseenglish");
    var planningGcseEnglish = formContext.getControl("dfe_websiteplanningretakeenglishgcse");
    var hasGcseMaths = formContext.getAttribute("dfe_websitehasgcsemaths");
    var planningGcseMaths = formContext.getControl("dfe_websiteplanningretakemathsgcse");
    var hasGcseScience = formContext.getAttribute("dfe_websitehasgcsescience");
    var planningGcseScience = formContext.getControl("dfe_websiteplanningretakesciencegcse");


    //GCSE English
    if (hasGcseEnglish != null) {
        if (hasGcseEnglish.getValue() == 222750001)  //Not Answered
            //planningGcseEnglish.setVisible(true);
            //There are now two copies of this field on different tabs so make sure both are updated
            formContext.getAttribute("dfe_websiteplanningretakeenglishgcse").controls.forEach(control => control.setVisible(true));
        else
            //planningGcseEnglish.setVisible(false);
            //There are now two copies of this field on different tabs so make sure both are updated
            formContext.getAttribute("dfe_websiteplanningretakeenglishgcse").controls.forEach(control => control.setVisible(false));
    }

    //GCSE Maths
    if (hasGcseMaths != null) {
        if (hasGcseMaths.getValue() == 222750001)  //Not Answered
            //planningGcseMaths.setVisible(true);
            //There are now two copies of this field on different tabs so make sure both are updated
            formContext.getAttribute("dfe_websiteplanningretakemathsgcse").controls.forEach(control => control.setVisible(true));
        else
            //planningGcseMaths.setVisible(false);
            //There are now two copies of this field on different tabs so make sure both are updated
            formContext.getAttribute("dfe_websiteplanningretakemathsgcse").controls.forEach(control => control.setVisible(false));
    }

    //GCSE Science
    if (hasGcseScience != null) {
        if (hasGcseScience.getValue() == 222750001)  //Not Answered
            //planningGcseScience.setVisible(true);
            //There are now two copies of this field on different tabs so make sure both are updated
            formContext.getAttribute("dfe_websiteplanningretakesciencegcse").controls.forEach(control => control.setVisible(true));
        else
            //planningGcseScience.setVisible(false);
            //There are now two copies of this field on different tabs so make sure both are updated
            formContext.getAttribute("dfe_websiteplanningretakesciencegcse").controls.forEach(control => control.setVisible(false));
    }
}

//-----------------------------------------------------------------------------------

//Developer: Arjun, 21/09/2020
//Purpose: To set TTA Channel and TTA Subscription Date depending on isGitisSubscriber value
DFEContactFormWebsite.ShowHideFieldsTTASubscription = function (exCxt) {

    var formContext = exCxt.getFormContext();
    var isGitisSubscriber = formContext.getAttribute("dfe_gitisttaserviceissubscriber");
    var ttaChannel = formContext.getAttribute("dfe_gitisttaservicesubscriptionchannel");
    var ttaSubscriptionDate = formContext.getAttribute("dfe_gitisttaservicestartdate");
    var confirmStringsTTA = { text: "This will remove the existing TTA Service Subscription Channel value and date. Would you like to proceed?", title: "Confirmation" };
    var confirmOptions = { height: 200, width: 500 };

    if (isGitisSubscriber != null) {
        if (isGitisSubscriber.getValue() == true) {
            ttaChannel.setValue(222750001) //dropdown option = Within Dynamics
            ttaSubscriptionDate.setValue(new Date()); //set the start date to current date time
        }

        else {
            if ((ttaChannel.getValue() != null) || ttaSubscriptionDate.getValue() != null) {
                Xrm.Navigation.openConfirmDialog(confirmStringsTTA, confirmOptions).then(
                    function (success) {
                        if (success.confirmed == true) {
                            ttaChannel.setValue(null);
                            ttaSubscriptionDate.setValue(null);

                        }
                        else {
                            isGitisSubscriber.setValue(true);
                        }
                    });
            }
        }
    }
}
//-------------------------------------------------------

//Developer: Arjun, 21/09/2020
//Purpose: To set Events Channel and Events Subscription Date depending on isGitisEventsSubscriber value
DFEContactFormWebsite.ShowHideFieldsEventsSubscription = function (exCxt) {

    var formContext = exCxt.getFormContext();
    var isGitisEventSubscriber = formContext.getAttribute("dfe_gitiseventsserviceissubscriber");
    var eventsChannel = formContext.getAttribute("dfe_gitiseventsservicesubscriptionchannel");
    var eventsSubscriptionDate = formContext.getAttribute("dfe_gitiseventsservicestartdate");
    var confirmStringsEvents = { text: "This will remove the existing Events Service Subscription Channel value and date. Would you like to proceed?", title: "Confirmation" };
    var confirmOptions = { height: 200, width: 500 };

    if (isGitisEventSubscriber != null) {
        if (isGitisEventSubscriber.getValue() == true) {
            eventsChannel.setValue(222750001) //dropdown option = Within Dynamics			

            eventsSubscriptionDate.setValue(new Date()); //set the start date to current date time
        }

        else {
            if ((eventsChannel.getValue() != null) || eventsSubscriptionDate.getValue() != null) {
                Xrm.Navigation.openConfirmDialog(confirmStringsEvents, confirmOptions).then(
                    function (success) {
                        if (success.confirmed == true) {
                            eventsChannel.setValue(null);
                            eventsSubscriptionDate.setValue(null);

                        }
                        else {
                            isGitisEventSubscriber.setValue(true);
                        }
                    });
            }
        }
    }
}
//-------------------------------------------------------


//Developer: Arjun, 21/09/2020
//Purpose: To set Mailing list Channel and Mailing list Subscription Date depending on isGitisMailinglistSubscriber value
DFEContactFormWebsite.ShowHideFieldsMailingListSubscription = function (exCxt) {

    var formContext = exCxt.getFormContext();
    var isGitisMailinglistSubscriber = formContext.getAttribute("dfe_gitismailinglistserviceissubscriber");
    var mailingListChannel = formContext.getAttribute("dfe_gitismlservicesubscriptionchannel");
    var mailingListSubscriptionDate = formContext.getAttribute("dfe_gitismailinglistservicestartdate");
    var confirmStringsMailingList = { text: "This will remove the existing Mailing List Service Subscription Channel value and date. Would you like to proceed?", title: "Confirmation" };
    var confirmOptions = { height: 200, width: 500 };

    if (isGitisMailinglistSubscriber != null) {
        if (isGitisMailinglistSubscriber.getValue() == true) {
            mailingListChannel.setValue(222750001) //dropdown option = Within Dynamics			
            mailingListSubscriptionDate.setValue(new Date()); //set the start date to current date time
        }

        else {
            if ((mailingListChannel.getValue() != null) || mailingListSubscriptionDate.getValue() != null) {
                Xrm.Navigation.openConfirmDialog(confirmStringsMailingList, confirmOptions).then(
                    function (success) {
                        if (success.confirmed == true) {
                            mailingListChannel.setValue(null);
                            mailingListSubscriptionDate.setValue(null);

                        }
                        else {
                            isGitisMailinglistSubscriber.setValue(true);
                        }
                    });
            }
        }
    }
}

//Developer: Kavitha Mutyala, 30/10/2020
//Purpose: This function will be triggered on change of Teacher Record Sanctions in RTT tab
DFEContactFormWebsite.OnTeacherRecordSanctionsChange = function () {
    var teacherRecordSanctions = DFEContactFormWebsite.utils.getAttributeValue("dfe_teacherrecordsanctions");
    if (teacherRecordSanctions != null) {
        DFEContactFormWebsite.utils.setAttributeValue("dfe_teachingrecorddatestamp", new Date());

    }
}
//Developer: Kavitha Mutyala, 18/11/2020
//Purpose: This function will be triggered onload of contact form to remove few optionset values for return year in RTT tab
DFEContactFormWebsite.OnReturnYearLoad = function (formContext) {
    var optionsetControl = formContext.getControl("dfe_returnyear");
    optionsetControl.removeOption(222750009);
    optionsetControl.removeOption(222750004);
    optionsetControl.removeOption(222750005);
    optionsetControl.removeOption(222750000);
}

//Purpose: This function will be triggered on change of Candidate Adviser Status Reason 
//to ensure the advisor has performed the minimum amount of candidate chasing before closing
var canCloseRecord = true;
DFEContactFormWebsite.OnAdvisorStatusReasonChange = async function (context) {
    var formContext = context.getFormContext();
    var candidateReasonControl = formContext.getAttribute("dfe_candidateadviserstatusreason");
    var candidateType = formContext.getAttribute("dfe_typeofcandidate");

    if (candidateReasonControl.getValue() != "222750020" /* No Response - Closed*/ || candidateType.getValue() != "222750000" /* ITT */) { 
        return;
    }

    var homePhone = formContext.getAttribute("address1_telephone1").getValue();
    var mobilePhone = formContext.getAttribute("mobilephone").getValue();
    var workPhone = formContext.getAttribute("telephone1").getValue();
    var candidateHasNumber = DFEContactFormWebsite.CheckCandidateHasContactNumber(homePhone, mobilePhone, workPhone, false);
    var candidateHasMobileNumber = DFEContactFormWebsite.CheckCandidateHasContactNumber(homePhone, mobilePhone, workPhone, true);
    var noPhone = formContext.getAttribute("donotphone").getValue();
    var noSMS = formContext.getAttribute("dfe_optoutsms").getValue();
    var noEmail = formContext.getAttribute("donotemail").getValue();
    var email = formContext.getAttribute("emailaddress1").getValue();
    var ownerId = formContext.getAttribute("ownerid").getValue()[0].id;
    var recordId = formContext.data.entity.getId();
    var ownerIdNoBrackets = ownerId.substring(1, ownerId.length - 1);
    var recordIdNoBrackets = recordId.substring(1, recordId.length - 1);

    if (!dayjs.week) {
        dayjs.extend(window.dayjs_plugin_weekOfYear)
    }

    if (noPhone == false && candidateHasNumber) { //1st check for at least 3 outbound calls, each made in seperate weeks
        await DFEContactFormWebsite.CheckPhoneCalls(recordIdNoBrackets, ownerIdNoBrackets);
    }

    if (noSMS == false && candidateHasMobileNumber && canCloseRecord == true) { //2nd check for at least 1 SMS or Whatsapp message logged
        await DFEContactFormWebsite.CheckSMSMessages(recordIdNoBrackets, ownerIdNoBrackets);

        if (canCloseRecord == false)
            await DFEContactFormWebsite.CheckWhatsAppMessages(recordIdNoBrackets, ownerIdNoBrackets);
    }

    if (noEmail == false && email && canCloseRecord == true) { //3rd check for at least 1 email messages sent
        await DFEContactFormWebsite.CheckEmailsByRegarding(recordIdNoBrackets, ownerIdNoBrackets);

        if (canCloseRecord == false) 
            await DFEContactFormWebsite.CheckEmailsByPartyList(recordIdNoBrackets, ownerIdNoBrackets);
    }

    if (canCloseRecord == false) {

        candidateReasonControl.setValue(null);

        var message = "To close a candidate for this reason you need to log at least 3 call attempts in three different calendar weeks, one SMS and one email, please make more contact attempts or choose another Adviser Status Reason.";

        var alertStrings = { confirmButtonLabel: "Ok", text: message, title: "Cannot close candidate record" };
        var alertOptions = { height: 240, width: 400 };
        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
            function (success) {
            },
            function (error) {
                console.log(error.message);
            }
        );
    }
}

DFEContactFormWebsite.CheckPhoneCalls = async function (recordIdNoGuid, ownerIdNoGuid) {
    await Xrm.WebApi.retrieveMultipleRecords("phonecall",
        "?$select=createdon&$filter=_dfe_contactid_value eq '" + recordIdNoGuid + "' and _ownerid_value eq '" + ownerIdNoGuid + "' and directioncode eq true&$count=true").then(
            function success(result) {

                if (result.entities.length < 3) {
                    canCloseRecord = false;
                    return;
                }

                let weeks = new Set();
                for (var i = 0; i < result.entities.length; i++) {
                    var weekNum = dayjs(result.entities[i].createdon).week();
                    weeks.add(weekNum);

                    if (weeks.size == 3)
                        return;
                }

                canCloseRecord = false;
            },
            function (error) {
                console.log(error.message);
            });
}

DFEContactFormWebsite.CheckSMSMessages = async function (recordIdNoGuid, ownerIdNoGuid) {
    await Xrm.WebApi.retrieveMultipleRecords("dfe_sms",
        "?$select=createdon&$filter=_regardingobjectid_value eq '" + recordIdNoGuid + "' and _ownerid_value eq '" + ownerIdNoGuid + "'&$count=true", 1).then(
            function success(result) {

                if (result.entities.length == 0) {
                    canCloseRecord = false;
                    return;
                }
            },
            function (error) {
                console.log(error.message);
            });
}

DFEContactFormWebsite.CheckWhatsAppMessages = async function (recordIdNoGuid, ownerIdNoGuid) {
    await Xrm.WebApi.retrieveMultipleRecords("dfe_whatsappmessage",
        "?$select=createdon&$filter=_regardingobjectid_value eq '" + recordIdNoGuid + "' and _ownerid_value eq '" + ownerIdNoGuid + "'&$count=true", 1).then(
            function success(result) {

                if (result.entities.length > 0) {
                    canCloseRecord = true;
                    return;
                }
            },
            function (error) {
                console.log(error.message);
            });
}

DFEContactFormWebsite.CheckEmailsByRegarding = async function (recordIdNoGuid, ownerIdNoGuid) {
    await Xrm.WebApi.retrieveMultipleRecords("email",
        "?$select=createdon&$filter=_regardingobjectid_value eq '" + recordIdNoGuid + "' and _ownerid_value eq '" + ownerIdNoGuid + "' and directioncode eq true and actualend ne null&$count=true", 1).then(
            function success(result) {

                if (result.entities.length == 0) {
                    canCloseRecord = false;
                    return;
                }
            },
            function (error) {
                console.log(error.message);
            });
}

DFEContactFormWebsite.CheckEmailsByPartyList = async function (recordIdNoGuid, ownerIdNoGuid) {
    await Xrm.WebApi.retrieveMultipleRecords("email",
        "?$select=modifiedon&$expand=email_activity_parties($filter=(participationtypemask eq 2 and _partyid_value eq '" + recordIdNoGuid + "'))&$filter=(Microsoft.Dynamics.CRM.EqualUserId(PropertyName='ownerid')) and (email_activity_parties/any(o1:(o1/participationtypemask eq 2 and o1/_partyid_value eq '" + recordIdNoGuid + "')))", 1).then(
            function success(result) {

                if (result.entities.length > 0) {
                    canCloseRecord = true;
                    return;
                }
            },
            function (error) {
                console.log(error.message);
            });
}

DFEContactFormWebsite.CheckCandidateHasContactNumber = function (homePhone, mobilePhone, workPhone, checkMobile) {

    if ((homePhone != null && length > 10) || (mobilePhone != null && mobilePhone.length > 10) || (workPhone != null && workPhone.length > 10)) {
        if (checkMobile == false)
            return true;

        if ((homePhone != null && homePhone.substring(0, 2) == "07") || (mobilePhone != null && mobilePhone.substring(0, 2)) == "07" || (workPhone != null && workPhone.substring(0, 2) == "07")){
            return true;
        }
    }

    return false;
}

DFEContactFormWebsite.ShowRTTHeaderFieldsIFRttCandidate = function (ecTx) {

    var formContext = ecTx.getFormContext();
    var typeValue = formContext.getAttribute("dfe_typeofcandidate").getValue();
    
    var registeredOnKttGroup = formContext.getControl("header_dfe_registeredonrttkhgroup");
    var preferredTeachingSubject = formContext.getControl("header_dfe_preferredteachingsubject01");

    if (registeredOnKttGroup && preferredTeachingSubject) {
        // If this form is RTT then show
        if (typeValue == 222750001) {
            registeredOnKttGroup.setVisible(true);
            preferredTeachingSubject.setVisible(true);
        } else {

            // First we null the fields then set to false
            DFEContactFormWebsite.utils.setAttributeValue("dfe_registeredonrttkhgroup", null);

            registeredOnKttGroup.setVisible(false);
            preferredTeachingSubject.setVisible(false);
        }
    }
}


DFEContactFormWebsite.onGridRowSave = function(context,fields) {
	var id = context.getFormContext().getData().getEntity().getEntityReference().id;
	
	context
	.getFormContext()
	.getData()
	.getEntity()
	.attributes.forEach(function (attr) {
		if (attr.getName() === "dfe_statusreason") { //statuscode
			var statusValue = attr.getValue();
			var record = {};
			record.statuscode = statusValue; // Status

			Xrm.WebApi.updateRecord("dfe_candidatejourneystep", id, record).then(
				function success(result) {
					var updatedId = result.id;
					console.log(updatedId);
				},
				function(error) {
					console.log(error.message);
				}
			);			
		}
	});
 	
}

DFEContactFormWebsite.fnLockFields = function(context,fields) {
	var id = context.getFormContext().getData().getEntity().getEntityReference().id;
	
 	context
        .getFormContext()
        .getData()
        .getEntity()
        .attributes.forEach(function (attr) {
	  	
           if (attr.getName() === "dfe_coursename") { 
		 let attributeToDisable = attr.controls.get(0);
                 attributeToDisable.setDisabled(true);
		
            }
        });
 	
}


DFEContactFormWebsite.CandidateStepsUpdate = function(formContext) {
	var id = formContext.data.entity.getId().replace("{", "").replace("}", "");
	debugger;
	Xrm.WebApi.retrieveMultipleRecords("dfe_candidatejourneystep", "?$select=statecode,dfe_statusreason,statuscode&$filter=(_dfe_contact_value eq " + id + " and statecode eq 0)").then(
	function success(results) {			
			for (var i = 0; i < results.entities.length; i++) {
				var result = results.entities[i];
				// Columns
				var dfe_candidatejourneystepid = result["dfe_candidatejourneystepid"]; // Guid
				var statecode = result["statecode"]; // State
				var statecode_formatted = result["statecode@OData.Community.Display.V1.FormattedValue"];
				var statuscode = result["statuscode"]; // State
				var dfe_statusreason = result["dfe_statusreason"]; // Choice
				if(dfe_statusreason == null){
					debugger;
					var record = {};
					record.dfe_statusreason = statuscode; // Status
					Xrm.WebApi.updateRecord("dfe_candidatejourneystep", dfe_candidatejourneystepid, record).then(
						function success(result) {
							var updatedId = result.id;
							console.log(updatedId);
						},
						function(error) {
							console.log(error.message);
						}
					);
				}			
			}
		},
		function(error) {
			console.log(error.message);
		}
	); 	
}


