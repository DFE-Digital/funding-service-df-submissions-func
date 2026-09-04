using QuestPDF.Helpers;
using PDFService.models;

public static class MockSummaryDocumentDataSource
{
    private static Random Random = new Random();

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    public static Message GetSummaryeDetails()
    {
        var items = Enumerable
                .Range(1, 5)
                .Select(_ => GenerateRandomSummaryDetail())
                .ToList();

        return new Message
        {
            pageTitle = Placeholders.Label(),
            declaration = Placeholders.Sentence(),
            customSummaryMessage = Placeholders.Sentence(),
            skipSummary = Random.Next(1, 10) < 6 ? false : true,
            fees = Placeholders.Sentence(),
            name = Placeholders.Name(),
            feedbackLink = Placeholders.Label(),
            phaseTag = Placeholders.Label(),
            declarationError = Placeholders.Label(),
            customSummaryMessageError = Placeholders.Label(),
            errors = Placeholders.Sentence(),
            referenceNumber = RandomString(10),
            currentTimeString = DateTime.Now.ToString(),
            pdfPrintStatus = Random.Next(1, 10) < 6 ? "NOT SUBMITTED" : "SUBMITTED",
            accessibilityLink = Placeholders.Label(),
            _payApiKey = RandomString(20),
            email = Placeholders.Email(),
            details = items,
        };
    }

    private static Detail GenerateRandomSummaryDetail()
    {
        var subtitle = Random.Next(1, 10) < 6 ? Placeholders.Name(): null;
        var items = Enumerable
                .Range(1, 20)
                .Select(_ => GenerateRandomSummaryDetailItem(subtitle))
                .ToList();

        

        return new Detail
        {
            name = Placeholders.Name(),
            title = Placeholders.Name(),
            items = items,
        };
    }

    private static Item GenerateRandomSummaryDetailItem(string? subtitle)
    {
        var option = new Options { classes =  "govuk - input--width - 20" };
        var question = Placeholders.Question();
        var answer = Random.Next(1, 10) < 6 ? Placeholders.Sentence(): Placeholders.LoremIpsum();
        return new Item
        {
            name = RandomString(10),
            path = "/" + RandomString(6) + "-page",
            label = question,
            value = answer,
            rawValue = answer,
            url = Placeholders.Sentence(),
            pageId = Placeholders.Sentence(),
            type = Placeholders.Label(),
            title = question,
            dataType = Placeholders.Label(),
            result = Placeholders.Label(),
            options = Random.Next(1, 10) < 6 ? option : null,
            subTitle = subtitle,
            subTitleNum = Random.Next(1, 10)
        };
    }
}
