var dfe_common = dfe_common ? dfe_common : {};

dfe_common.utils = function (contextWindow) {
    var
        accessor = contextWindow != null ? contextWindow : window,
        serverUrl = accessor.Xrm.Page.context.getClientUrl(),
        checkUndefinedOrNull = function (amIDefinedOrNull, defaultValue) {
            return (amIDefinedOrNull === undefined || amIDefinedOrNull === null) ? defaultValue : amIDefinedOrNull;
        },
        doesControlHaveAttribute = function (control) {
            /// <summary>
            /// Checks control for attribute
            /// </summary>
            /// <param name="control">Can be a string or a crm control. 
            /// If it's a string, assumes it's an attribute name and tries to locate it</param>
            /// <returns type=""></returns>
            var controlType;
            if (typeof control === "string") {
                control = accessor.Xrm.Page.getControl(control);
            }
            if (control && control.getControlType) {
                controlType = control.getControlType();

            } else {
                return false;
            }
            return controlType !== "iframe" && controlType !== "webresource" && controlType !== "subgrid";
        },
        disableFormFields = function(onOff) {
            accessor.Xrm.Page.ui.controls.forEach(function (control, index) {
                if (doesControlHaveAttribute(control)) {
                    control.setDisabled(onOff);
                }
            });
        },
    
        disableField = function (fieldName, disabled) {
            disabled = checkUndefinedOrNull(disabled);
            if (accessor.Xrm.Page.getControl(fieldName) && accessor.Xrm.Page.getControl(fieldName).setDisabled) {
                accessor.Xrm.Page.getControl(fieldName).setDisabled(disabled);
            } else {
                // alert("can't disable field " + fieldName);
            }

        },
        disableFields = function (fieldsToDisable, disabled) {
            /// <summary>
            ///     Iterates controls, checks they are correct type & disables them
            /// </summary>
            /// <param name="fieldsToDisable">
            /// Caller can pass a string array of fields to disable, or function defaults to all fields
            /// </param>
            /// <returns type=""></returns>
            //console.log("disableFields start");
            disabled = (disabled === undefined || disabled === null) ? true : disabled;
            if (fieldsToDisable && Array.isArray(fieldsToDisable)) {
                // switch field array to control array
                var controlArr = [];
                fieldsToDisable.forEach(function (controlName) {
                    controlArr.push(accessor.Xrm.Page.getControl(controlName));
                });
                fieldsToDisable = controlArr;
            } else {
                fieldsToDisable = accessor.Xrm.Page.ui.controls;
            }

            fieldsToDisable.forEach(function (control) {
                if (doesControlHaveAttribute(control)) {
                    control.setDisabled(disabled);
                }
            });
            //console.log("disableFields end");
            return;

        },
        getLookup = function (id, entity, attribute) {
            var lookupObj = {};
            lookupObj.id = id;
            lookupObj.entityType = entity;
            lookupObj.name = attribute;
            return lookupObj;
        },
        getFormType = function () {
            return accessor.Xrm.Page.ui.getFormType();
        },
        setAttributeValue = function (fieldName, value, submitMode) {
            //console.log("setAttributeValue");
            if (value === undefined) value = null;
            if (!submitMode) submitMode = "always";
            if (accessor.Xrm.Page.getAttribute(fieldName)) {
                accessor.Xrm.Page.getAttribute(fieldName).setValue(value);
                accessor.Xrm.Page.getAttribute(fieldName).setSubmitMode(submitMode);
            } else {
                // alert("setAttributeValue: " + fieldName);
            }
        },
        clearFieldAudit = function (fieldUser, fieldDate) {
            //console.log("clearFieldAudit");
            if (accessor.Xrm.Page.getAttribute(fieldUser)) {
                accessor.Xrm.Page.getAttribute(fieldUser).setValue(null);
                accessor.Xrm.Page.getAttribute(fieldUser).setSubmitMode("always");


            } else {
                // alert("can't clear field " + fieldUser);
            }
            if (accessor.Xrm.Page.getAttribute(fieldDate)) {
                accessor.Xrm.Page.getAttribute(fieldDate).setValue(null);
                accessor.Xrm.Page.getAttribute(fieldDate).setSubmitMode("always");
            } else {
                // alert("can't clear field " + fieldDate);
            }
        },
        getAttributeValue = function (fieldName) {
            //console.log("getAttributeValue");
            return accessor.Xrm.Page.getAttribute(fieldName) ? accessor.Xrm.Page.getAttribute(fieldName).getValue() : null;
        },
        setSubmitMode = function (fieldName, submitMode) {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="submitMode">Defaults to "always</param>
            /// <returns type="">Optional  </returns>
            //console.log("setSubmitMode");
            if (!submitMode) submitMode = "always";
            if (accessor.Xrm.Page.getAttribute(fieldName)) {
                accessor.Xrm.Page.getAttribute(fieldName).setSubmitMode(submitMode);
            } else {
                // alert("setSubmitMode " + fieldName);
            }
        },
        setRequiredLevel = function (fieldName, requiredLevel) {
            /// <summary>
            /// CRM 4: none = 0, recommended  = 1, required = 2
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="requiredLevel">Defaults to "none". Valid options are: none, required, recommended</param>
            /// <returns type=""></returns>
            //console.log("setRequiredLevel");
            if (!requiredLevel) requiredLevel = "none";
            if (accessor.Xrm.Page.getAttribute(fieldName)) {
                accessor.Xrm.Page.getAttribute(fieldName).setRequiredLevel(requiredLevel);
            } else {
                // alert("setRequiredLevel" + fieldName);
            }
        },
        getRequiredLevel = function (fieldName) {
            return accessor.Xrm.Page.getAttribute(fieldName) ? accessor.Xrm.Page.getAttribute(fieldName).getRequiredLevel() : null;
        },
        setVisible = function (fieldName, flag) {
            //console.log("setVisible");
            flag = checkUndefinedOrNull(flag, true);
            if (accessor.Xrm.Page.getControl(fieldName)) {
                accessor.Xrm.Page.getControl(fieldName).setVisible(flag);
            } else {
                // alert("setVisible: can't clear field " + fieldName);
            }
        },        
        
        getControl = function (controlName) {
            //console.log("getControl");
            return accessor.Xrm.Page.getControl(controlName);
        },
        filterLookup = function () {
            var filter;
            var HideEntities = ["contact"];
            var i;
            for (i = 0; i < HideEntities.length; i++) {
                filter =
                    "<filter type='and'>" +
                    "<condition attribute='" + HideEntities[i] + "id' operator='null' />" +
                    "</filter>";
                getControl("to").addCustomFilter(filter, HideEntities[i]);
            }
        },
        disableControl = function (controlName, disabled) {
            //console.log("disableControl");
            disabled = checkUndefinedOrNull(disabled, true);
            if (accessor.Xrm.Page.getControl(controlName)) {
                accessor.Xrm.Page.getControl(controlName).setDisabled(disabled);
            } else {
                // alert("disableControl: can't clear field " + controlName);
            }

        },
        getValueFromResult = function (result, columnName) {
            /// <summary>
            ///     Calls SOAP sdk getEntities on XML result set & filters for specific col name
            /// </summary>
            /// <param name="result">
            ///     SOAP response from Sdk.Q.retrieveMultiple
            /// </param>
            /// <param name="columnName">
            ///     column name including any alias. eg. "accexec.accountid"
            /// </param>
            /// <returns type=""></returns>
            //console.log("getValueFromResult");
            var value = null;
            result.getEntities().forEach(function (e) {
                if (e.view().attributes[columnName]) {
                    value = e.view().attributes[columnName].fValue;
                }
            });
            return value;
        },
        setLookup = function (targetObj, crmField) {
            /// <summary>
            ///     sets CRM form field with new lookup value (targetObj)
            ///     checks targetObj for null to save caller doing this and keeping it DRY
            /// </summary>
            /// <param name="targetObj">
            ///     {id:GUID, name:"a name", entityType: entityType}
            /// </param>
            /// <param name="crmField">
            ///     form lookup field name
            /// </param>
            /// <returns type=""></returns>
            //console.log("setLookup");
            if (targetObj) {
                var attribute = accessor.Xrm.Page.getAttribute(crmField);
                var lookup = [];
                lookup[0] = targetObj;
                attribute.setValue(lookup);
                attribute.setSubmitMode("always");
            } else {
                accessor.Xrm.Page.getAttribute(crmField).setValue(null);
            }
        },
        createEntityRef = function (id, name, entityType) {
            //console.log("createEntityRef");
            return { id: id, name: name, entityType: entityType };
        },        
        setTabSectionVisible = function (tabName, sectionName, isVisible) {
            //console.log("setTabSectionVisible");
            isVisible = isVisible !== undefined ? isVisible : true;
            if (accessor.Xrm.Page.ui.tabs.get(tabName)) {
                if (accessor.Xrm.Page.ui.tabs.get(tabName).sections.get(sectionName)) {
                    accessor.Xrm.Page.ui.tabs.get(tabName).sections.get(sectionName).setVisible(isVisible);
                } else {
                    // alert("setTabSectionVisible " + sectionName);
                }
            } else {
                // alert("setTabSectionVisible " + tabName);
            }
        },
        redirectToForm = function (formName) {
            var currentForm = accessor.Xrm.Page.ui.formSelector.getCurrentItem();
            if (currentForm != null) {
                if (currentForm.getLabel().toLowerCase() != formName.toLowerCase()) { //make sure it's not already this form
                    var availableForms = accessor.Xrm.Page.ui.formSelector.items.get();
                    for (var i in availableForms) {
                        var form = availableForms[i];
                        if (form.getLabel().toLowerCase() == formName.toLowerCase()) {
                            form.navigate();
                        }
                    }
                }
            }
        },
        setFocus = function (attributeName) {
            //console.log("setFocus");
            if (accessor.Xrm.Page.getControl(attributeName) && accessor.Xrm.Page.getControl(attributeName).setFocus) {
                accessor.Xrm.Page.getControl(attributeName).setFocus();
            } else {
                // alert("setFocus " + attributeName);
            }
        },
        guidsAreEqual = function (guid1, guid2) {
            //console.log("guidsAreEqual");
            var isEqual = false;

            if (guid1 == null || guid2 == null) {
                isEqual = false;
            } else {
                isEqual = guid1.replace(/[{}]/g, "").toLowerCase() == guid2.replace(/[{}]/g, "").toLowerCase();
            }

            return isEqual;
        },
        gLookupGetValue = function (lookupFieldName) {
            /// <summary>
            /// Returns values for a Lookup Field
            /// </summary>
            /// <param name="lookupFieldName">string name for lookup field</param>
            /// <returns type=""></returns>
            var objLookup = accessor.Xrm.Page.getAttribute(lookupFieldName);
            var arr = new Array();
            arr[0] = ""; //returns lookup GUID
            arr[1] = ""; //returns lookup text value   
            arr[2] = ""; //returns lookup entity type name (e.g. account)
            arr[3] = ""; //returns lookup entity type code  (e.g. 1=account), 2=contact, etc)
            // Get a reference to the lookup control on the form.
            var lookupItem = objLookup.getValue();

            // If there is data in the field
            if (lookupItem != null) {
                // Return the GUID of the lookup.
                arr[0] = lookupItem[0].id;
                // Return the text value of the lookup.
                arr[1] = lookupItem[0].name;

                // Return the entity type name of the lookup.
                arr[2] = lookupItem[0].typename;

                // Return the entity type code of the lookup.
                arr[3] = lookupItem[0].type;
            }
            return arr;
        },        
        setTabVisible = function (tabName, isVisible) {

            isVisible = isVisible !== undefined ? isVisible : true;
            if (accessor.Xrm.Page.ui.tabs.get(tabName)) {
                accessor.Xrm.Page.ui.tabs.get(tabName).setVisible(isVisible);

                if (isVisible) {
                    accessor.Xrm.Page.ui.tabs.get(tabName).setDisplayState("expanded");                    
                }
            }
        },

        getLabel = function (fieldName) {            
            return accessor.Xrm.Page.getAttribute(fieldName) ? accessor.Xrm.Page.getAttribute(fieldName).getLabel() : null;
        },

        getOptionValue = function (fieldName) {
            if (accessor.Xrm.Page.getAttribute(fieldName)) {
                if (accessor.Xrm.Page.getAttribute(fieldName).getSelectedOption()) {
                    return accessor.Xrm.Page.getAttribute(fieldName).getSelectedOption().value;
                }
            }
            return null;
        },
        getOptionText = function (fieldName) {
            if (accessor.Xrm.Page.getAttribute(fieldName)) {
                if (accessor.Xrm.Page.getAttribute(fieldName).getSelectedOption()) {
                    return accessor.Xrm.Page.getAttribute(fieldName).getSelectedOption().text;
                } 
            }
            return null;
        },
        getOptions = function (fieldName) {
               return accessor.Xrm.Page.getAttribute(fieldName) ? accessor.Xrm.Page.getAttribute(fieldName).getOptions(): null;
        },        
        getAttribute = function (fieldName) {
                      return accessor.Xrm.Page.getAttribute(fieldName) ? accessor.Xrm.Page.getAttribute(fieldName) : null;
        },
        getControl = function (fieldName) {
                      return accessor.Xrm.Page.getAttribute(fieldName) ? accessor.Xrm.Page.ui.controls.get(fieldName) : null;
        },
		dateAddHours = function (d, hours) {
            /// <summary>
            /// Add x hours to Date
            /// </summary>
            /// <param name="d"></param>
            /// <param name="hours"></param>
            /// <returns type=""></returns>
            var t = new Date(d);
            t.setHours(d.getHours() + hours);
            if (t.getDate() < d.getDate()) {
                t.setDate(0);
            }
            return t;
        },
		dateAddMinutes = function (d, minutes) {
            /// <summary>
            /// Add x Minutes to Date
            /// </summary>
            /// <param name="d"></param>
            /// <param name="minutes"></param>
            /// <returns type=""></returns>
            var t = new Date(d);
            t.setMinutes(d.getMinutes() + minutes);
            if (t.getDate() < d.getDate()) {
                t.setDate(0);
            }
            return t;
        },
		isValidPhoneNumber = function(phonenumber) {
			if(phonenumber)
			{
				var mobiRegex = /^\d{3}[-\s]?\d{3}[-\s]?\d{4}$/;
				if (phonenumber.indexOf('0044') === 0)
				{
					phonenumber = phonenumber.substring(4);					
				}												
				else if (phonenumber.indexOf('+') === 0)
				{
					phonenumber = phonenumber.substring(1);
					mobiRegex = /^\d{5}[-\s]?\d{3}[-\s]?\d{4}$/;
				}
				else if (phonenumber.indexOf('0') === 0) {
					phonenumber = phonenumber.substring(1);					
				}
				else
				{
					return false;
				}
				if (phonenumber.match(mobiRegex))	
				{
					return true;
				}
			}
			return false;
		}
        reloadWebResource = function (fieldName) {
            if (accessor.Xrm.Page.getControl(fieldName)) {
                var webResourceControl = accessor.Xrm.Page.getControl(fieldName);
                var src = webResourceControl.getSrc();
                webResourceControl.setSrc(null);
                webResourceControl.setSrc(src);
            }
        },
        setNavVisible = function(navItemName,visible) {
            var objNavItem = accessor.Xrm.Page.ui.navigation.items.get(navItemName);
            if (objNavItem != null) {                
                objNavItem.setVisible(visible);                
            }
        },
        getNavVisible = function (navItemName) {
            var objNavItem = accessor.Xrm.Page.ui.navigation.items.get(navItemName);
            if (objNavItem != null) {
                return objNavItem.getVisible();
            }
            return false;
        },
        setNavFocus = function (navItemName) {
            var objNavItem = accessor.Xrm.Page.ui.navigation.items.get(navItemName);
            if (objNavItem != null) {
                objNavItem.setFocus();
            }
        };


    return {
        clearFieldAudit: clearFieldAudit,
        createEntityRef: createEntityRef,
        disableControl: disableControl,
        disableFields: disableFields,
        disableField: disableField,
        disableFormFields: disableFormFields, 
        filterLookup: filterLookup,
        getAttributeValue: getAttributeValue,
        getControl: getControl,
        getFormType: getFormType,        
        getLookup: getLookup,
        gLookupGetValue: gLookupGetValue,
        getValueFromResult: getValueFromResult,
        guidsAreEqual: guidsAreEqual,
        redirectToForm: redirectToForm,
        serverUrl: serverUrl,
        setAttributeValue: setAttributeValue,
        setFocus: setFocus,
        getRequiredLevel: getRequiredLevel,
        setRequiredLevel: setRequiredLevel,
        setLookup: setLookup,
        setSubmitMode: setSubmitMode,
        setTabVisible: setTabVisible,
        setTabSectionVisible: setTabSectionVisible,
        setVisible: setVisible,
        getLabel: getLabel,
        getOptionValue: getOptionValue,
        getOptionText: getOptionText,
        getOptions: getOptions,
        getAttribute: getAttribute,
		dateAddHours: dateAddHours,
		dateAddMinutes: dateAddMinutes,
        reloadWebResource: reloadWebResource,
        setNavVisible: setNavVisible,
        getNavVisible: getNavVisible,
        setNavFocus: setNavFocus,
		isValidPhoneNumber:isValidPhoneNumber
};
};