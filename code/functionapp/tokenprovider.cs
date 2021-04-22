using System.Net.Http;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System;

namespace microservice1
{

  public static class tokenProvider {
    private static TimeSpan ttl = new TimeSpan(1,0,0);
    private static string servicebusKey = Environment.GetEnvironmentVariable("ServiceBusKey");
    private static string servicebusKeyName = Environment.GetEnvironmentVariable("ServiceBusKeyName");
    
    [FunctionName("getServiceBusToken")]
    public static async Task<HttpResponseMessage> getToken(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage  req, 
      ClaimsPrincipal principal,
      ILogger log
    ) {
      if (principal is null) {
        return req.CreateResponse(HttpStatusCode.Unauthorized);  
      }

      var sasProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(servicebusKeyName, servicebusKey, ttl);
      var subscriptionUri = HttpUtility.ParseQueryString(req.RequestUri.Query).Get("subscriptionUri");
      var sasToken = await sasProvider.GetTokenAsync(subscriptionUri, new TimeSpan(0,0,15));
      
      return req.CreateResponse(HttpStatusCode.OK, sasToken.TokenValue);
    }
  }
}
