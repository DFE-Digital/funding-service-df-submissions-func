using QuestPDF;
using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Notify.Client;
using Notify.Models.Responses;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using PDFService.models;
using dfsubmissions_pdfprintq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.Functions.Worker;

namespace df_Submissions
{
    public class dfsubmissions_pdfprintq
    {
        private readonly ILogger<dfsubmissions_pdfprintq> _logger;
        private readonly ISummaryDocumentDataSource _DocumentDataSource;
        private readonly IDocument _document;
        private readonly string WarningimgPath = Environment.GetEnvironmentVariable("WarningimgPath");

        public dfsubmissions_pdfprintq(ISummaryDocumentDataSource DocumentDataSource, IDocument document, ILogger<dfsubmissions_pdfprintq> _log)
        {
            _DocumentDataSource = DocumentDataSource;
            _document = document;
            _logger = _log;
        }

        [Function("dfsubmissions_pdfprintq")]
        public async Task Run([ServiceBusTrigger("%PDFPrintQName%", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
        {
            try
            {
                _logger.LogInformation("Processing Message : {id}", message.MessageId);
                _logger.LogInformation("Message data : {body}", message.Body);
                if (string.IsNullOrEmpty(WarningimgPath))
                    _logger.LogInformation("Message data : {body}", message.Body);
                string apikey, emailAddress, templateid, reference, linktofile, formname;
                bool isuat = false, submissiondatafound = false, emailsentalready = false;


                if (message.ApplicationProperties != null && message.ApplicationProperties.Count > 0)
                {
                    //setting up PDF data
                    Settings.License = LicenseType.Community;
                    var model = _DocumentDataSource.GetSummaryeDetails(Convert.ToString(message.Body), _logger);
                    _logger.LogInformation("Message model", model);
                    _logger.LogInformation("Data model generated");
                    model.pdfPrintStatus = "SUBMITTED";
                    var document = new SummaryDocument(model);//, context
                    _logger.LogInformation("Create PDF doc based on the data model");
                    var pdfdata = document.GeneratePdf();
                    _logger.LogInformation("Generate PDF stream completed");
                    formname = model.name;
                    //Create notify client                    
                    apikey = Convert.ToString(message.ApplicationProperties["API_KEY"]);
                    emailAddress = Convert.ToString(message.ApplicationProperties["emailId"]);
                    linktofile = Convert.ToString(message.ApplicationProperties["link_to_file"]);
                    reference = model.referenceNumber;
                    templateid = Convert.ToString(message.ApplicationProperties["templateId"]);
                    isuat = Convert.ToBoolean(message.ApplicationProperties["IS_UAT"]);
                    SubmissionAuditLog Auditdata = new SubmissionAuditLog();
                    Dictionary<String, dynamic> personalisation;
                    _logger.LogInformation($"Retrieving message properties key: {apikey} \r\n email: {emailAddress} \r\n ref: {reference} \r\n template: {templateid} \r\n uat: {isuat} \r\n filelink: {linktofile}");

                    if (Convert.ToInt16(linktofile) > 0)
                    {
                        personalisation = new Dictionary<String, dynamic>
                        {
                            {"first_name", formname},
                            {"application_date",DateTime.UtcNow},
                            { "name", formname + ".pdf" },
                            { "link_to_file", NotificationClient.PrepareUpload(pdfdata)}
                        };
                        _logger.LogInformation("created personalisation with attachment");
                    }
                    else
                    {
                        personalisation = new Dictionary<String, dynamic>
                        {
                            {"first_name", formname},
                            {"application_date",DateTime.UtcNow},
                            { "name", formname + ".pdf" }
                        };
                        _logger.LogInformation("created personalisation without attachment");
                    }


                    //check the form submission in SQL
                    _logger.LogInformation("Check for submission data in SQL db");
                    submissiondatafound = await GetSQLdbdata(reference, _logger);

                    //check email already sent for the ref
                    _logger.LogInformation("Check for email already sent and logged in db");
                    Auditdata = await GetSubmissionAuditLog(reference, _logger);
                    emailsentalready = Auditdata != null;

                    var objlog = new SubmissionAuditLog();
                    if (Auditdata == null)
                    {
                        objlog.id = reference;
                        objlog.TemplateId = templateid;
                        objlog.SubmissionFormName = formname;
                        objlog.EmailAddress = emailAddress;
                        objlog.EmailHadAttachment = Convert.ToInt16(linktofile) > 0;
                        objlog.EmailSentOn = DateTime.UtcNow;
                        objlog.EmailSent = true;
                    }
                    else
                    {
                        objlog = Auditdata;
                        objlog.EmailSent = true;
                        objlog.EmailSentOn = DateTime.UtcNow;
                    }

                    //trigger the email from the client.
                    if ((submissiondatafound) && (!emailsentalready))
                    {
                        _logger.LogInformation("submission data found in db and email not sent at all.. sending email now...");
                        await sendemail(apikey, emailAddress, templateid, personalisation, reference, _logger, objlog, messageActions, message);
                    }
                    //push msg back to queue
                    else
                    {
                        if (submissiondatafound)
                        {
                            if (emailsentalready && Auditdata.EmailSent)
                            {
                                _logger.LogInformation("submission data found in db and Since email sent already we are updating the same object back into db.");
                                await PostSubmissionFormLog(objlog, _logger);
                            }
                            else if (emailsentalready && !Auditdata.EmailSent)
                            {
                                _logger.LogInformation("submission data found in db and retyring email send....");
                                await sendemail(apikey, emailAddress, templateid, personalisation, reference, _logger, objlog, messageActions, message);
                            }

                        }
                        else
                        {
                            _logger.LogInformation("submission data not found in db");
                            message.ScheduledEnqueueTime.AddMinutes(10);
                            messageActions.AbandonMessageAsync(message).Wait();
                            _logger.LogInformation("rescheduling the message in the queue");
                        }
                    }
                }
            }
            catch (Notify.Exceptions.NotifyClientException ex)
            {
                _logger.LogError("Error: " + ex.Message + ex.StackTrace);
                _logger.LogInformation("rescheduling the message in the queue");
                message.ScheduledEnqueueTime.AddMinutes(10);
                messageActions.AbandonMessageAsync(message).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message + ex.StackTrace);
                _logger.LogInformation("rescheduling the message in the queue");
                message.ScheduledEnqueueTime.AddMinutes(10);
                messageActions.AbandonMessageAsync(message).Wait();
            }
        }

        private async Task<bool> GetSQLdbdata(string id, ILogger log)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("DFSQLAPIKEY"));
                string Url = $"{Environment.GetEnvironmentVariable("DFSQLAPIURL")}/api/getResponseById/" + id;
                var result = await client.GetAsync(Url);
                var data = Convert.ToBoolean(result.Content.ReadAsStringAsync().Result);
                log.LogInformation($"Data for reference in SQL: {id} present in db -> {data}");
                return (data);
            }
            catch (Exception ex)
            {
                log.LogInformation($"Data for reference in SQL: {id} not found with error {ex.Message}");
                throw;
            }
        }

        private async Task sendemail(string apikey, string emailAddress, string templateid, Dictionary<String, dynamic> personalisation, string reference, ILogger _logger, SubmissionAuditLog objlog, ServiceBusMessageActions messageActions, ServiceBusReceivedMessage message)
        {
            EmailNotificationResponse response;
            var notifyclient = new NotificationClient(apikey);
            response = notifyclient.SendEmail(emailAddress, templateid, personalisation, reference);
            _logger.LogInformation("email triggered");

            if (response != null && response.id != null)
            {
                objlog.EmailSent = true;
                messageActions.CompleteMessageAsync(message).Wait();
                _logger.LogInformation("completing the message from the queue");
            }
            else
            {
                objlog.EmailSent = false;
                message.ScheduledEnqueueTime.AddMinutes(10);
                messageActions.AbandonMessageAsync(message).Wait();
                _logger.LogInformation("rescheduling the message in the queue");
            }
            SubmissionAuditLog submissionAuditLog = new SubmissionAuditLog();
            submissionAuditLog.EmailAddress = emailAddress;
            submissionAuditLog.SubmissionFormName = objlog.SubmissionFormName;
            submissionAuditLog.EmailSentOn = DateTime.Now;
            submissionAuditLog.TemplateId = templateid;
            submissionAuditLog.EmailSent = objlog.EmailSent;
            submissionAuditLog.EmailHadAttachment = objlog.EmailHadAttachment;
            submissionAuditLog.id = reference;
            await PostSubmissionFormLog(submissionAuditLog, _logger);
        }

        private async Task<SubmissionAuditLog> GetSubmissionAuditLog(string id, ILogger log)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("DFSQLAPIKEY"));
                string Url = $"{Environment.GetEnvironmentVariable("DFSQLAPIURL")}/api/getSubmissionFormLog/" + id;
                var response = await client.GetAsync(Url);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                var auditLog = JsonConvert.DeserializeObject<SubmissionAuditLog>(responseData);
                log.LogInformation($"Data for reference in SQL: {id} present in db -> {auditLog != null}");
                return auditLog;
            }
            catch (Exception ex)
            {
                log.LogInformation($"Data for reference in SQL: {id} not found with error {ex.Message}");
                throw;
            }
        }
        private async Task<bool> PostSubmissionFormLog(SubmissionAuditLog submissionFormLog, ILogger log)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("DFSQLAPIKEY"));
                string Url = $"{Environment.GetEnvironmentVariable("DFSQLAPIURL")}/api/PostSubmissionFormLog";

                var json = JsonConvert.SerializeObject(submissionFormLog);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(Url, content);
                response.EnsureSuccessStatusCode();

                log.LogInformation($"Successfully posted submission form log for id: {submissionFormLog.id}");
                return true;
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to post submission form log for id: {submissionFormLog.id} with error {ex.Message}");
                return false;
            }
        }
    }
}

