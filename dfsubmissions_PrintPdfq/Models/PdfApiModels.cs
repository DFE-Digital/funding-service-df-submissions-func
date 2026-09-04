using System.Collections.Generic;

namespace df_Submissions.Models
{
    /// <summary>
    /// Request model for PDF generation API
    /// </summary>
    public class GeneratePdfRequest
    {
        /// <summary>
        /// JSON submission data to be converted to PDF
        /// </summary>
        public string SubmissionData { get; set; }

        /// <summary>
        /// Form name for the PDF and email
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Whether to include attachment in email notification
        /// </summary>
        public bool IncludeAttachment { get; set; } = true;
    }

    /// <summary>
    /// Response model for PDF generation API
    /// </summary>
    public class GeneratePdfResponse
    {
        /// <summary>
        /// Success status of PDF generation
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Base64 encoded PDF data (null if failed)
        /// </summary>
        public string PdfDataBase64 { get; set; }

        /// <summary>
        /// Form name used for the PDF
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Error details if generation failed
        /// </summary>
        public string ErrorDetails { get; set; }
    }

    /// <summary>
    /// Request model for sending notification email with PDF
    /// </summary>
    public class SendNotificationRequest
    {
        /// <summary>
        /// Notification API key from Gov.uk Notify
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Email address to send notification to
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Template ID from Gov.uk Notify
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// Reference ID for the submission
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// PDF data as base64 string
        /// </summary>
        public string PdfDataBase64 { get; set; }

        /// <summary>
        /// Form name for email personalisation
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Whether to include PDF as attachment
        /// </summary>
        public bool IncludeAttachment { get; set; } = true;
    }

    /// <summary>
    /// Response model for notification API
    /// </summary>
    public class SendNotificationResponse
    {
        /// <summary>
        /// Success status
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Notification ID from Gov.uk Notify
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Email address notification was sent to
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Error details if sending failed
        /// </summary>
        public string ErrorDetails { get; set; }
    }
}
