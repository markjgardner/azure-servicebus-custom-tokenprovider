using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace microservice1 {
  public static class functions {
    [FunctionName("function1")]
    [return :ServiceBus("%functionTopicName%", Connection = "AzureWebJobsServiceBus")]
    public static Message function1 (
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, 
      ILogger log
    ) {
      var message = new Message();
      message.Body = System.Text.Encoding.UTF8.GetBytes(req.Query["message"]);
      message.CorrelationId = req.Query["storeid"];
      log.LogInformation ($"Message queued");
      return message;
    }
  }
}