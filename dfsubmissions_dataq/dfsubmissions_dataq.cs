using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace df_Submissions
{
    public class dfsubmissions_dataq
    {
        private readonly ILogger<dfsubmissions_dataq> _logger;

        public dfsubmissions_dataq(ILogger<dfsubmissions_dataq> logger)
        {
            _logger = logger;
        }

        [FunctionName("dfsubmissions_dataq")]
        public void Run([ServiceBusTrigger("%DataQName%", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }
}
