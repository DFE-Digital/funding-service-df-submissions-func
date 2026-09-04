using df_Submissions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDFService.models;

[assembly: FunctionsStartup(typeof(df_Submissions.Startup))]
namespace df_Submissions
{
    public class Startup : FunctionsStartup
    {

        public Startup()
        {
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ICdbConnectivity, CdbConnectivity>();
            builder.Services.AddSingleton<SummaryDocumentDataSource, SummaryDocumentDataSource>();
            builder.Services.AddSingleton<SummaryDocument, SummaryDocument>();
        }

    }   
}