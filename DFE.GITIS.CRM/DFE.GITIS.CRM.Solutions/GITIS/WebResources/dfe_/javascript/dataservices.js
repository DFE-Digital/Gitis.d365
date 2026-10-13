/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="Sdk.Retrieve.vsdoc.js" />
/*global Q*/
var dfe_common = dfe_common ? dfe_common : {};

dfe_common.data = dfe_common.data ? dfe_common.data : function (contextWindow) {
    var
        accessor = contextWindow != null ? contextWindow : window,
        context = accessor.Xrm.Page.context,
        serverUrl = accessor.Xrm.Page.context.getClientUrl(),
        oDataPath = serverUrl + "/api/data/v9.1",
        userId = context.getUserId(),
        webHelperServiceUrl = null,
        retrieveOData = function (filter, errorMessage) {
            try {
                var userRequest = new XMLHttpRequest();
                userRequest.open("GET", oDataPath + filter, false);
                userRequest.setRequestHeader("Accept", "application/json");
                userRequest.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                userRequest.send();
                if (userRequest.status === 200) {
                    var results = JSON.parse(userRequest.responseText);
                    return results;
                } else {
                    throw errorMessage + " err code: " + userRequest.status;
                }
            } catch (err) {
                throw err;
            }
        },
        createEntity = function (entityName, entity) {
            try {
                var jsonEntity = JSON.stringify(entity);
                var userRequest = new XMLHttpRequest();
                userRequest.open("POST", oDataPath + "/" + entityName + "s", false);
                userRequest.setRequestHeader("Accept", "application/json");
                userRequest.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                userRequest.send(jsonEntity);
                if (userRequest.status === 200 || userRequest.status === 201) {
                    var response = JSON.parse(userRequest.responseText).d;
                    return response;
                } else {
                    throw new Error("Unable to create entity " + entityName + ", status code: " + status);
                }
            } catch (err) {
                throw err;
            }
        },
        updateEntity = function (id, entityName, entity) {
            id = id.replace(/[{}]/g, "");
            var jsonEntity = JSON.stringify(entity);
            var userRequest = new XMLHttpRequest();

            userRequest.open("PATCH", oDataPath + "/" + entityName + "s(" + id + ")", false);
            userRequest.setRequestHeader("Accept", "application/json");
            userRequest.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            userRequest.send(jsonEntity);
            var status = userRequest.status;
            if (status === 200 || status === 201 || status === 204 || status === 1223) {

                return true;
            } else {
                throw new Error("Unable to update entity " + entityName + ", id: " + id + ", status code: " + status);
            }
        },
        userHasRole = function (roleName) {
            /**** AB: Optimized to call the web service to retrieve the roles only for the first call to this function. 
                For all subsequent calls oXml will not be null and hence can retrieve the role information form it *****/
            if (typeof (oXml) == "undefined" || oXml == null) {
                //get Current User Roles, oXml is a global object since we have omitted the keyword "var" declaring it
                getCurrentUserRoles(function (results) {
                    oXml = results;
                });
            }
            if (oXml != null && oXml.length > 0) {
                for (var i = 0; i < oXml.length; i++) {
                    var userRole = oXml[i];

                    if (userRole.name == roleName) {
                        return true;
                    }
                }
            }
            return false;
        },
        userHasAdviserRole = function (userId) {
            userId = userId.replace(/[{}]/g, "");
            var returnResult = false;
            getUserRoles(userId, function (results) {
                if (results != null && results.length > 0) {
                    for (var i = 0; i < results.length; i++) {
                        var userRole = results[i];
                        if (userRole.name == "GITIS - TPUK - One to One Advisor") {
                            returnResult = true;
                            return;
                        }
                    }
                }
            });
            return returnResult;
        },
        getUserRoles = function (userId, successCallBack) {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
                + "<entity name='role'>"
                + "<attribute name='name' />"
                + "<attribute name='businessunitid' />"
                + "<attribute name='roleid' />"
                + "<order attribute='name' descending='false' />"
                + "<link-entity name='systemuserroles' from='roleid' to='roleid'>"
                + " <link-entity name='systemuser' from='systemuserid' to='systemuserid'>"
                + "<filter type='and'>"
                + "<condition attribute='systemuserid' operator='eq' value='" + userId + "'/>"
                // + "<condition attribute="systemuserid" value="{ A7A1B4B3 - 98E2 - E811 - A97D - 000D3A276620 } " uitype="systemuser" uiname="# Aaron Spittles" operator="eq"/>"
                + "</filter>"
                + " </link-entity>"
                + " </link-entity>"
                + "</entity>"
                + "</fetch> ";
            executeFetchXmlRest(fetchXml, "roles", function (results) {
                successCallBack(results);
            }
            );
        },
        getCurrentUser = function () {
            var
                filter = "/systemusers(" + userId.replace(/[{}]/g, "") + ")?$select=systemuserid,fullname,jobtitle",
                currentUser = {},
                user = retrieveOData(filter, "There has been a problem getting the current user.");
            currentUser.id = user.systemuserid;
            currentUser.entityType = "systemuser";
            currentUser.name = user.fullname;
            currentUser.jobTitle = user.jobtitle;
            return currentUser;
        },
        getContactById = function (contactId) {
            var
                filter = "/contacts(" + contactId.replace(/[{}]/g, "") + ")?$select=contactid,donotphone,telephone1,mobilephone,dfe_preferredphonenumbertype,address1_telephone1",
                contactObject = {},
                contact = retrieveOData(filter, "There has been a problem getting the contact by id.");
            contactObject.id = contact.contactid;
            contactObject.entityType = "contact";
            contactObject.telephone1 = contact.telephone1;
            contactObject.mobilephone = contact.mobilephone;
            contactObject.address1_telephone1 = contact.address1_telephone1;
            contactObject.dfe_preferredphonenumbertype = contact.dfe_preferredphonenumbertype;
            contactObject.donotphone = contact.donotphone;
            return contactObject;
        },
        getConfigByName = function (configname) {
            var
                filter = "/dfe_configs?$select=dfe_tpworkspaceroot,dfe_outboundwebsitequeueguid,dfe_govnotifyapikey,dfe_tpworkspacedial,dfe_tpworkspacehangup,dfe_tpworkspacedialprefix,dfe_tpworkspacehold,dfe_privacypolicy,dfe_tpworkspaceretrieve,dfe_tpworkspacetransfer,dfe_tpworkspacecanceltransfer,dfe_outboundcallqueueguid,dfe_adviseropenreasons,dfe_adviserclosedreasons&$filter=dfe_name eq '" + configname + "'",
                configObject = {},
                configRecord = retrieveOData(filter, "There has been a problem getting the config");
            var config = configRecord.value[0];
            configObject.id = config.dfe_configid;
            configObject.entityType = "dfe_config";
            configObject.dfe_tpworkspaceroot = config.dfe_tpworkspaceroot;
            configObject.dfe_tpworkspacedial = config.dfe_tpworkspacedial;
            configObject.dfe_tpworkspacehangup = config.dfe_tpworkspacehangup;
            configObject.dfe_tpworkspacedialprefix = config.dfe_tpworkspacedialprefix;
            configObject.dfe_tpworkspacehold = config.dfe_tpworkspacehold;
            configObject.dfe_tpworkspaceretrieve = config.dfe_tpworkspaceretrieve;
            configObject.dfe_tpworkspacetransfer = config.dfe_tpworkspacetransfer;
            configObject.dfe_tpworkspacecanceltransfer = config.dfe_tpworkspacecanceltransfer;
            configObject.dfe_privacypolicy = config.dfe_privacypolicy;
            configObject.dfe_outboundcallqueueguid = config.dfe_outboundcallqueueguid;
            configObject.dfe_adviseropenreasons = config.dfe_adviseropenreasons;
            configObject.dfe_adviserclosedreasons = config.dfe_adviserclosedreasons;
            configObject.dfe_outboundwebsitequeueguid = config.dfe_outboundwebsitequeueguid;
            configObject.dfe_govnotifyapikey = config.dfe_govnotifyapikey;            
            return configObject;
        },
        getContactFullNameById = function (contactId) {
            var
                filter = "/contacts(" + contactId.replace(/[{}]/g, "") + ")?$select=contactid,fullname",
                contactObject = {},
                contact = retrieveOData(filter, "There has been a problem getting the contact full name.");
            contactObject.id = contact.contactid;
            contactObject.entityType = "contact";
            contactObject.fullname = contact.fullname;
            return contactObject;
        },
        getPhoneNumberFromContact = function(contactId)
        {
            if(contactId && contactId.length > 0 && contactId[0])
            {
                var contact = getContactById(contactId[0].id);
                var numberToCall = "";
                if(contact)
                {
                    if(contact.donotphone != null && contact.donotphone == false) //Allow
                    {
                        if(contact.dfe_preferredphonenumbertype)
                        {
                            if(contact.dfe_preferredphonenumbertype == 222750000)//Mobile
                            {
                                numberToCall = contact.mobilephone;
                            }
                            else if(contact.dfe_preferredphonenumbertype == 222750001)//Home
                            {
                                numberToCall = contact.address1_telephone1;
                            }
                            else if(contact.dfe_preferredphonenumbertype == 222750002)//Work
                            {
                                numberToCall = contact.telephone1;
                            }
                        }
                        else if(contact.mobilephone)
                        {
                            numberToCall = contact.mobilephone;
                        }
                        else if(contact.address1_telephone1)
                        {
                            numberToCall = contact.address1_telephone1;
                        }
                        else if(contact.telephone1)
                        {
                            numberToCall = contact.telephone1;
                        }
                    }
                }
                return numberToCall;
            }
        },
        getUserById = function (aUserId) {
            try {
                aUserId = aUserId.replace(/[{}]/g, "");
                var
                    filter = "/systemusers(" + aUserId.replace(/[{}]/g, "") + ")?$select=systemuserid,fullname",
                    user = {},
                    response = retrieveOData(filter, "There has been a problem getting the user.");
                if (response) {
                    user.id = response.systemuserid;
                    user.entityType = "systemuser";
                    user.name = response.fullname;
                    return user;
                } else {
                    return null;
                }
            } catch (err) {
                throw err;
            }
        },

        getSystemConfig = function (paramName) {
            var result;
            webHelperServiceUrl = "http://integrityweb2016:8002/api/Home/";
            var req = new XMLHttpRequest();
            var url = webHelperServiceUrl +
                "GetSystemConfigValue?configName=" + paramName;
            req.open("GET", url, false);
            req.setRequestHeader("Accept", "application/json");
            req.onreadystatechange = function () {
                if (this.readyState === 4) //Complete
                {
                    req.onreadystatechange = null;
                    if (this.status === 200) {
                        var results = JSON.parse(this.response);
                        result = results[0].ConfigValue;
                    }
                }
            };
            req.send();
            return result;
        },

        executeFetchXmlRest = function (fetchXml, entityname, successCallBack) {
            try {
                var fetch = encodeURI(fetchXml);
                var query = entityname + "?fetchXml=" + fetch;
                var req = new XMLHttpRequest();
                req.open("GET", oDataPath + "/" + query, false);
                req.setRequestHeader("Accept", "application/json");
                req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.setRequestHeader("OData-MaxVersion", "4.0");
                req.setRequestHeader("OData-Version", "4.0");
                req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                req.onreadystatechange = function () {
                    if (this.readyState === 4) //Complete
                    {
                        req.onreadystatechange = null;
                        if (this.status === 200) {
                            var results = JSON.parse(this.response);
                            successCallBack(results.value);
                        }
                    }
                };
                req.send();
            } catch (e) {
                alert("Fetchxml request failed: " + e.toString());
            }
        },
        getCurrentUserRoles = function (successCallBack) {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
                + "<entity name='role'>"
                + "<attribute name='name' />"
                + "<attribute name='businessunitid' />"
                + "<attribute name='roleid' />"
                + "<order attribute='name' descending='false' />"
                + "<link-entity name='systemuserroles' from='roleid' to='roleid'>"
                + " <link-entity name='systemuser' from='systemuserid' to='systemuserid'>"
                + "<filter type='and'>"
                + "<condition attribute='systemuserid' operator='eq-userid' />"
                + "</filter>"
                + " </link-entity>"
                + " </link-entity>"
                + "</entity>"
                + "</fetch> ";
            executeFetchXmlRest(fetchXml, "roles", function (results) {
                successCallBack(results);
            }
            );

        };

    return {
        serverUrl: serverUrl,
        oDataPath: oDataPath,
        createEntity: createEntity,
        getCurrentUser: getCurrentUser,
        getConfigByName: getConfigByName,
        getUserById: getUserById,
        retrieveOData: retrieveOData,
        updateEntity: updateEntity,
        executeFetchXmlRest: executeFetchXmlRest,
        getCurrentUserRoles: getCurrentUserRoles,
        getContactById:getContactById,
        getContactFullNameById:getContactFullNameById,
        getPhoneNumberFromContact: getPhoneNumberFromContact,
        userHasRole: userHasRole,
        userHasAdviserRole: userHasAdviserRole
    };
};