using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Executioncontext = Microsoft.Azure.WebJobs.ExecutionContext;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace df_Submissions
{
    public class dfsubmissions_dataq
    {
        private readonly ILogger<dfsubmissions_dataq> _logger;

        public dfsubmissions_dataq(ILogger<dfsubmissions_dataq> _log)
        {
            _logger = _log;

        }

        [Function("dfsubmissions_dataq")]
        public async Task Run([ServiceBusTrigger("%DataQName%", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, Executioncontext context)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }
}
