using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System;

namespace microservice1 {

  public static class tokenProvider {
    private static TimeSpan ttl = new TimeSpan(3600);
    private static string servicebusKey = Environment.GetEnvironmentVariable("ServiceBusKey");
    private static string servicebusKeyName = Environment.GetEnvironmentVariable("ServiceBusKeyName");
    
    [FunctionName("getServiceBusToken")]
    public static HttpResponseMessage getToken(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage  req, 
      ClaimsPrincipal principal,
      ILogger log
    ) {
      if (principal is null) {
        return req.CreateResponse(HttpStatusCode.Unauthorized);  
      }

      var subscriptionUri = HttpUtility.ParseQueryString(req.RequestUri.ToString()).Get("subscriptionUri");

      //Convert the TTL to a unix epoch
      var expires = Convert.ToString((Int64)(DateTime.UtcNow - new DateTime(1970, 1, 1) + ttl).TotalSeconds);
      string stringToSign = HttpUtility.UrlEncode(subscriptionUri) + "\n" + expires; 
      HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(servicebusKey)); 

      //Sign the string
      var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign))); 

      //Construct the token
      var sasToken = String.Format(CultureInfo.InvariantCulture, "sr={0}&sig={1}&se={2}&skn={3}",  
          HttpUtility.UrlEncode(subscriptionUri), HttpUtility.UrlEncode(signature), expires, servicebusKeyName);
      
      return req.CreateResponse(HttpStatusCode.OK, sasToken);
    }
  }
}