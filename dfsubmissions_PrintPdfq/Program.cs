
using df_Submissions;
using df_Submissions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PDFService.models;
using QuestPDF.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();        
        services.AddSingleton<ISummaryDocumentDataSource, SummaryDocumentDataSource>();
        services.AddSingleton<IDocument, SummaryDocument>();
        services.AddSingleton<PDFService.models.Message>();
        services.AddSingleton<PDFService.models.SummaryDocument>();
        // Register new PDF API services
        services.AddScoped<IPdfGenerationService, PdfGenerationService>();
    })
    .Build();

host.Run();