using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace storeclient {
  class customTokenProvder : ITokenProvider {
    private static string tenant = System.Environment.GetEnvironmentVariable("AADTENANTID");
    private static string tokenProviderUri = System.Environment.GetEnvironmentVariable("TOKENPROVIDERURI");
    private static string clientId = System.Environment.GetEnvironmentVariable("CLIENTID");
    private static AuthenticationContext authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenant));    
    private static HttpClient httpClient = new HttpClient();

    public customTokenProvder() { }

    private static async Task<AuthenticationResult> AuthenticateUser() {
      AuthenticationResult result = null;

      var uri = new Uri(tokenProviderUri);
      var resourceId = string.Format("{0}://{1}/", uri.Scheme, uri.Host);

      try {
        DeviceCodeResult codeResult = await authContext.AcquireDeviceCodeAsync(resourceId, clientId);
        Console.ResetColor();
        Console.WriteLine("You need to sign in.");
        Console.WriteLine("Message: " + codeResult.Message + "\n");
        result = await authContext.AcquireTokenByDeviceCodeAsync(codeResult);
      }
      catch (Exception ex) {
        Console.Error.WriteLine("Message: " + ex.Message + "\n");
      }

      return result;
    }

    public async Task<SecurityToken> GetTokenAsync(string appliesTo, TimeSpan timeout) {
      var auth = await AuthenticateUser();

      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

      var ub = new UriBuilder(tokenProviderUri);
      var query = HttpUtility.ParseQueryString(ub.Query);
      query["subscriptionUri"] = appliesTo;
      ub.Query = query.ToString();

      var response = await httpClient.GetAsync(ub.Uri);
      var token = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());

      return ParseSAS(token);
    }

    private static SecurityToken ParseSAS(string sas) {
      //Remove the "SharedAccessSignature " prefix
      var parts = HttpUtility.ParseQueryString(sas.Replace("SharedAccessSignature ", string.Empty));
      var expires = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(Double.Parse(parts.Get("se")));
      var audience = parts.Get("sr");
      return new SecurityToken(sas, expires, audience, "SharedAccessSignature");
    }
  }
}