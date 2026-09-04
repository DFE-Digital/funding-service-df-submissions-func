using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using df_Submissions.Models;
using df_Submissions.Services;
using System.Net.Http.Json;

namespace df_Submissions.Controllers
{
    public class PdfController
    {
        private readonly IPdfGenerationService _pdfGenerationService;
        private readonly ILogger<PdfController> _logger;

        public PdfController(IPdfGenerationService pdfGenerationService, ILogger<PdfController> logger)
        {
            _pdfGenerationService = pdfGenerationService;
            _logger = logger;
        }

        /// <summary>
        /// Generates a PDF from submission data
        /// POST /api/pdf/generate
        /// ////comment
        /// </summary>
        [Function("GeneratePdf")]
        public async Task<HttpResponseData> GeneratePdf(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "pdf/generate")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("PDF generation request received");

                var requestBody = await req.ReadAsStringAsync();
                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("PDF generation request body is empty");
                    return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request body is required");
                }

                var pdfData = await _pdfGenerationService.GeneratePdfAsync(requestBody, _logger);
                var pdfBase64 = Convert.ToBase64String(pdfData);

                var response = req.CreateResponse(HttpStatusCode.OK);
                var responseData = new GeneratePdfResponse
                {
                    Success = true,
                    Message = "PDF generated successfully",
                    PdfDataBase64 = pdfBase64
                };

                await response.WriteAsJsonAsync(responseData);
                _logger.LogInformation("PDF generated successfully, size: {bytes}", pdfData.Length);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating PDF: {ex.Message}\n{ex.StackTrace}");
                return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Failed to generate PDF", ex.Message);
            }
        }


        /// <summary>
        /// Health check endpoint
        /// GET /api/health
        /// </summary>
        [Function("HealthCheck")]
        public HttpResponseData HealthCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
        {
            _logger.LogInformation("Health check request received");
            var response = req.CreateResponse(HttpStatusCode.OK);
            var healthData = new { status = "healthy", timestamp = DateTime.UtcNow };
            response.WriteAsJsonAsync(healthData);
            return response;
        }

        /// <summary>
        /// Helper method to create error responses
        /// </summary>
        private HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message, string details = null)
        {
            var response = req.CreateResponse(statusCode);
            var errorData = new { error = message, details = details };
            response.WriteAsJsonAsync(errorData);
            return response;
        }
    }
}
