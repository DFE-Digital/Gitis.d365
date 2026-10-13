var utils = dfe_common.utils(parent);
var data = dfe_common.data(parent);

$(document).ready(SetPrivacyPolicy);

function SetPrivacyPolicy() {

		var pp = data.getConfigByName("GITIS");
		if(pp)
		{
			//Update the contents of the Privacy policy div            
			$("#privacypolicy").html("<p>" + pp.dfe_privacypolicy + "</p>");
		}
}
