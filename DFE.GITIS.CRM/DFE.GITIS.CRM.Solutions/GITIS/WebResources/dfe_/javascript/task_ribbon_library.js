var DFETaskRibbon = DFETaskRibbon ? DFETaskRibbon : {};

DFETaskRibbon.RibbonButtonClick = function (control) {
    if (control) {
        if (control._controlName && control._controlName == "Component6f84519") {
            DFETaskRibbon.OpenEntityFrom("0");
        }
        else if (control._controlName && control._controlName == "Component4584862") {
            DFETaskRibbon.OpenEntityFrom("1");
        }
        else {
            return;
        }
    }
}

DFETaskRibbon.OpenEntityFrom = function (taskType) {
    var parameters = {};
    parameters["dfe_tasktype"] = taskType;
    Xrm.Utility.openEntityForm("task", null, parameters);    
}
