using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace PDFService.models
{
    public interface ISummaryDocumentDataSource
    {
        Message GetSummaryeDetails(string inputData, ILogger log);
    }
}