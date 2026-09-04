
public class Message
{
    public string pageTitle { get; set; }
    public string declaration { get; set; }
    public string customSummaryMessage { get; set; }
    public bool? skipSummary { get; set; }
    public dynamic endPage { get; set; }
    public List<Detail> details { get; set; }
    public string fees { get; set; }
    public string name { get; set; }
    public string feedbackLink { get; set; }
    public string phaseTag { get; set; }
    public string declarationError { get; set; }
    public string customSummaryMessageError { get; set; }
    public string errors { get; set; }
    public string referenceNumber { get; set; }
    public string currentTimeString { get; set; }
    public string pdfPrintStatus { get; set; }
    public List<dynamic> _outputs { get; set; } 
    public string _payApiKey { get; set; }
    public string accessibilityLink { get; set; }
    public string email { get; set; }
    public List<dynamic> outputType { get; set; }
}