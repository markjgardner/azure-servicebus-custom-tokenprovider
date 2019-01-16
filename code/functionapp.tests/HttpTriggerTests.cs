using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace functionapp.tests {
  public class HttpTriggerTests {

    [Fact]
    public void function1_inputMapping_mapsQueryStringValues () {
      var logger = new Mock<ILogger> ();

      var querystring = new Dictionary<string, StringValues> ();
      querystring.Add ("message", "hello world");
      querystring.Add ("storeid", "0");

      var request = new Mock<HttpRequest> ();
      request.SetupGet (x => x.Query).Returns (new QueryCollection (querystring));

      var message = microservice1.functions.function1 (request.Object, logger.Object);

      Assert.Equal (message.CorrelationId, querystring["storeid"]);
      Assert.Equal (Encoding.UTF8.GetString (message.Body), querystring["message"]);
    }
  }
}