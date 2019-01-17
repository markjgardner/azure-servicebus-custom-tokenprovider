using System;
using Xunit;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace functionapp.integration {
  public class servicebusTests {
    static string sbconnection = Environment.GetEnvironmentVariable("SBCONN");
    static string sbtopicname = Environment.GetEnvironmentVariable("SBTOPIC");
    static TopicClient topic = new TopicClient(sbconnection, sbtopicname);
    
    [Theory]
    [InlineData ("hello", 0)]
    [InlineData ("foo", 1)]
    [InlineData ("dne", 99)]
    public async void servicebus_messageHandling_storeReceivedMessage(string text, int storeid) {
      var subname = $"store{storeid}";
      var subscription = new SubscriptionClient(sbconnection, sbtopicname, subname,ReceiveMode.PeekLock);
      var message = new Message(Encoding.UTF8.GetBytes(text));
      message.CorrelationId = Convert.ToString(storeid);

      await topic.SendAsync(message);
      
      //Test for store processing outcome
    }
  }
}