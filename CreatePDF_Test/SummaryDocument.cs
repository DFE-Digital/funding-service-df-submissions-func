using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class SummaryDocument : IDocument
{
    public static Image WarningImage { get; } = Image.FromFile(System.IO.Directory.GetCurrentDirectory() + "\\warning.png");

    public Message Model { get; }

    public SummaryDocument(Message model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page => {
            // Page Settings
            page.Size(PageSizes.A4);
            page.Margin(40, Unit.Point);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(
                x => x.FontFamily(Fonts.Arial)
            .FontSize(Constants.DEFAULT_FONT_SIZE)
            .FontColor(Constants.FONT_PRIMARY_COLOR)
            );
            // Header
            page.Header().Element(ComposeHeader);
            // Content
            page.Content().Element(ComposeContent);
            // Footer
            page.Footer().Element(ComposeFooter);
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.ShowOnce()
            .Text(Model.name)
            .SemiBold()
            .FontFamily(Fonts.Arial)
            .FontSize(Constants.HEADER_FONT_SIZE)
            .FontColor(Constants.FONT_PRIMARY_COLOR);
    }

    void ComposeContent(IContainer container)
    {
        //Image WarningImage = Image.FromFile(Path.Combine(_context.FunctionAppDirectory, WarningimgPath));
        container.PaddingVertical(1, Unit.Centimetre)
            .Column(x =>
            {
                x.Spacing(20);
                // Adds Submission ID, Submission Status and Timestamp
                x.Item().Component(new SubmissionInfoComponent(Model.referenceNumber, "SUBMITTED", Model.currentTimeString));
                // Warning Image and Text is not rendered for forms with 'Submitted' state
                // Adds Warning Image and Text
                //x.Item().Row(row =>
                //{
                //    row.AutoItem().Width(25).PaddingLeft(0).Image(WarningImage);
                //    row.AutoItem().PaddingLeft(10).PaddingTop(3).Text(Constants.WARNING_TEXT).Bold();
                //});
                // Adds form questions and answers as table
                x.Item().Element(ComposeTable);
            });
    }

    void ComposeTable(IContainer container)
    {
        static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(8);
        static IContainer Block(IContainer container) => container.BorderColor(Colors.Grey.Lighten2).PaddingTop(15).PaddingBottom(10);

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
            });

            foreach (var detail in Model.details)
            {
                if (detail.items.Count() == 0) break;
                var subtitle = detail.title;
                // Renders Section title only when available
                if (subtitle != null)
                {
                    table.Cell().ColumnSpan(2).Element(Block).Text(subtitle).Bold().FontSize(Constants.SECTION_FONT_SIZE);
                }
                foreach (var item in detail.items)
                {
                    switch (item.type)
                    {
                        case "CheckboxesField":
                            // Split the value by comma to get the multiple answers selected and iterate through them
                            // to print all the answers along with the question
                            string[] values = item.value.Split(",");
                            if (values.Length > 0)
                            {
                                foreach (var ans in values)
                                {
                                    // Question
                                    table.Cell().Element(CellStyle).PaddingRight(15).ShowOnce().Text(item.title);
                                    // Answer
                                    table.Cell().Element(CellStyle).ShowOnce().Text(string.IsNullOrEmpty(ans.Trim()) ? "Not supplied" : ans.Trim()).Bold();
                                }

                            }
                            break;
                        case "DataImport":
                        case "FileUploadField":
                            // Take only the file name from the entire filepath provided in answer for these components
                            var valueArr = item.value.Split("/");
                            string value = valueArr[valueArr.Length - 1];
                            // Question
                            table.Cell().Element(CellStyle).PaddingRight(15).ShowOnce().Text(item.title);
                            // Answer
                            table.Cell().Element(CellStyle).ShowOnce().Text(string.IsNullOrEmpty(value) ? "Not supplied" : value).Bold();
                            break;
                        default:
                            // Question
                            table.Cell().Element(CellStyle).PaddingRight(15).ShowOnce().Text(item.title);
                            // Answer
                            table.Cell().Element(CellStyle).ShowOnce().Text(string.IsNullOrEmpty(item.value) ? "Not supplied" : item.value).Bold();
                            break;


                    }
                }
            }
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.Row(footRow =>
        {
            footRow.AutoItem().Text(Utils.TruncateForDisplay(Model.name, 25));
            footRow.RelativeItem();
            footRow.AutoItem().Text(Model.referenceNumber).Bold();
            footRow.RelativeItem();
            footRow.AutoItem().Text(pageNumberText =>
            {
                pageNumberText.Span("Page ");
                pageNumberText.CurrentPageNumber();
                pageNumberText.Span(" of ");
                pageNumberText.TotalPages();
            });
        });
    }

    public class SubmissionInfoComponent : IComponent
    {
        private string SubmissionId { get; }
        private string SubmissionStatus { get; }
        private string SubmissionTimestamp { get; }

        public SubmissionInfoComponent(string submissionId, string submissionStatus, string timestamp)
        {
            SubmissionId = submissionId;
            SubmissionStatus = submissionStatus;
            SubmissionTimestamp = timestamp;
        }

        public void Compose(IContainer container)
        {
            container.Row(row =>

            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Row(subRow =>
                    {
                        subRow.AutoItem().Text(Constants.SUMBISSION_ID_TEXT).FontSize(Constants.SUBMISSION_INFO_FONT_SIZE);
                        subRow.AutoItem().PaddingLeft(33).PaddingRight(5).Text(":").FontSize(Constants.SUBMISSION_INFO_FONT_SIZE);
                        subRow.AutoItem().Text(SubmissionId).FontSize(Constants.SUBMISSION_INFO_FONT_SIZE).Bold();
                    });
                    column.Item().Row(subRow =>
                    {
                        subRow.AutoItem().Text(Constants.SUBMISSION_STATUS_TEXT).FontSize(Constants.SUBMISSION_INFO_FONT_SIZE);
                        subRow.AutoItem().PaddingLeft(7).PaddingRight(5).Text(":").FontSize(Constants.SUBMISSION_INFO_FONT_SIZE);
                        subRow.AutoItem().Text(SubmissionStatus).FontSize(Constants.SUBMISSION_INFO_FONT_SIZE).Bold();
                    });
                });
                row.ConstantItem(10);
                row.AutoItem().Text(SubmissionTimestamp).FontSize(Constants.SUBMISSION_INFO_FONT_SIZE);
            });
        }
    }

}