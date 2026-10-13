var utils = dfe_common.utils(parent);
var data = dfe_common.data(parent);

$(document).ready(SetPrivacyPolicy);

function SetPrivacyPolicy() {

		var pp = data.getConfigByName("GITIS");
		if(pp)
		{
			//Update the contents of the Privacy policy div
            //document.getElementById("privacypolicy").innerHTML = "<p>" + pp.dfe_privacyPolicy + "</p>";
			$("#privacypolicy").html("<p>" + pp.dfe_privacypolicy + "</p>");
		}
}
