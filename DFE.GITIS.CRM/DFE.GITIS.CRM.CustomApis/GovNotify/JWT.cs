using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DFE.GITIS.CRM.CustomApis.GovNotify
{
    public class JWT
    {
        public string GenerateToken(string secret, string serviceId)

        {
            var header = new Header

            { typ = "JWT", alg = "HS256" };

            var headerJson = new JavaScriptSerializer().Serialize(header);

            var payload = new Payload { iss = serviceId, iat = GetCurrentTimeInSeconds() };

            var payloadJson = new JavaScriptSerializer().Serialize(payload);

            var headerEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson));

            var payloadEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

            var data = headerEncoded + "." +payloadEncoded;

            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(secret));

            var hashedData = hasher.ComputeHash(Encoding.UTF8.GetBytes(data));

            var signature = System.Convert.ToBase64String(hashedData);

            var token = headerEncoded.ToString() + "." +payloadEncoded.ToString() + "." +signature;

            return token;

        }

        public static double GetCurrentTimeInSeconds()
        {

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return Math.Round((DateTime.UtcNow - epoch).TotalSeconds);

        }
    }

}
