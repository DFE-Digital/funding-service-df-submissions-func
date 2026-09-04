using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

class Program
{
    static void Main(string[] args)
    {
        Settings.License = LicenseType.Community;

        var dataSource = new SummaryDocumentDataSource();

        // For documentation and implementation details, please visit:
        // https://www.questpdf.com/documentation/getting-started.html
        // Use 'MockSummaryDocumentDataSource' to mock the data
        //var model = MockSummaryDocumentDataSource.GetSummaryeDetails();
        var model = dataSource.GetSummaryeDetails();
        var document = new SummaryDocument(model);

        // Generate PDF file and show it in the default viewer
        //document.GeneratePdfAndShow();

        // Generate PDF as test.pdf
        //var bytes = document.GeneratePdf();

        // Generate PDF as test.pdf
        document.GeneratePdf("test.pdf");

        // Or open the QuestPDF Previewer and experiment with the document's design
        // in real-time without recompilation after each code change
        // https://www.questpdf.com/document-previewer.html
        //document.ShowInPreviewer();
    }
}