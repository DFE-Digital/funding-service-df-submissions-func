using Microsoft.Extensions.Logging;
using PDFService.models;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace df_Submissions.Services
{
    public interface IPdfGenerationService
    {
        Task<byte[]> GeneratePdfAsync(string submissionData, ILogger log);
        Task<Dictionary<string, dynamic>> CreatePersonalisationAsync(byte[] pdfData, string formName, bool includeAttachment);
    }

    public class PdfGenerationService : IPdfGenerationService
    {
        private readonly ISummaryDocumentDataSource _documentDataSource;

        public PdfGenerationService(ISummaryDocumentDataSource documentDataSource)
        {
            _documentDataSource = documentDataSource;
        }

        /// <summary>
        /// Generates a PDF from submission data
        /// </summary>
        public async Task<byte[]> GeneratePdfAsync(string submissionData, ILogger log)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Settings.License = LicenseType.Community;

                    log.LogInformation("Retrieving summary details from data source");
                    var model = _documentDataSource.GetSummaryeDetails(submissionData, log);

                    if (model == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve summary details from submission data");
                    }

                    log.LogInformation("Data model generated successfully");
                    var document = new SummaryDocument(model);

                    log.LogInformation("Creating PDF document from data model");
                    var pdfData = document.GeneratePdf();

                    log.LogInformation("PDF generation completed successfully");
                    return pdfData;
                }
                catch (Exception ex)
                {
                    log.LogError($"Error generating PDF: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Creates personalisation dictionary for email notification
        /// </summary>
        public async Task<Dictionary<string, dynamic>> CreatePersonalisationAsync(
            byte[] pdfData,
            string formName,
            bool includeAttachment)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "first_name", formName },
                { "application_date", DateTime.UtcNow },
                { "name", formName + ".pdf" }
            };

            if (includeAttachment && pdfData != null)
            {
                personalisation["link_to_file"] = Notify.Client.NotificationClient.PrepareUpload(pdfData);
            }

            return await Task.FromResult(personalisation);
        }
    }
}
