using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFService.models;
public class SummaryDocumentDataSource : ISummaryDocumentDataSource
{
    private  Random Random = new Random();

    private  string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    public Message GetSummaryeDetails(string inputdata,ILogger log)
    {
        log.LogDebug("data received for serialisation: " + inputdata);
        var Msgobj = JsonConvert.DeserializeObject<Summary>(inputdata);       
        var summaryObj = Msgobj.message;
        log.LogDebug("Message obj: "+ JsonConvert.SerializeObject(Msgobj));
        log.LogDebug("summary obj: "+ JsonConvert.SerializeObject(summaryObj));
        var summaryObjNotNull = summaryObj != null;
        var items = summaryObj.details
                .Select(detail => GenerateSummaryDetail(detail))
                .ToList();

        return new Message
        {
            pageTitle = summaryObjNotNull ? summaryObj.pageTitle : "",
            declaration = summaryObjNotNull ? summaryObj.declaration : "",
            customSummaryMessage = summaryObjNotNull ? summaryObj.customSummaryMessage : "",
            skipSummary = summaryObjNotNull ? summaryObj.skipSummary : false,
            fees = summaryObjNotNull ? summaryObj.fees : "",
            name = summaryObjNotNull ? summaryObj.name : "",
            feedbackLink = summaryObjNotNull ? summaryObj.feedbackLink : "",
            phaseTag = summaryObjNotNull ? summaryObj.phaseTag : "",
            declarationError = summaryObjNotNull ? summaryObj.declarationError : "",
            customSummaryMessageError = summaryObjNotNull ? summaryObj.customSummaryMessageError : "",
            errors = summaryObjNotNull ? summaryObj.errors : "",
            referenceNumber = summaryObjNotNull ? summaryObj.referenceNumber : "",
            currentTimeString = summaryObjNotNull ? summaryObj.currentTimeString : "",
            pdfPrintStatus = summaryObjNotNull ? summaryObj.pdfPrintStatus : "",
            accessibilityLink = summaryObjNotNull ? summaryObj.accessibilityLink : "",
            _payApiKey = summaryObjNotNull ? summaryObj._payApiKey : "",
            email = summaryObjNotNull ? summaryObj.email : "",
            details = items,
        };
    }

    private  Detail GenerateSummaryDetail(Detail detail)
    {
        var subtitle = detail.items.Count() > 0 ? detail.items[0].subTitle : "";
        var items = detail.items.Where(w => !(w.type == "Result" && w.options?.hideResult == true) && !(w.type == "Result" && string.IsNullOrEmpty(w.value)))
                .Select(item => GenerateSummaryDetailItem(item, subtitle))
                .ToList();
        List<Item> updatedItem = new List<Item>();
        foreach (var item in items)
        {
            updatedItem.Add(item);
            if (item.type == "DSIAccess" && !string.IsNullOrEmpty(item.organizationName))
            {
                Item newitem = new Item
                {
                    type = item.type,
                    title = "Provider Name",
                    rawValue = item.organizationName,
                    subTitle = item.subTitle,
                    value = item.organizationName,
                    options = item.options,
                    organizationName = item.organizationName
                };
                updatedItem.Add(newitem);
            }
        }

        return new Detail
        {
            name = detail.name,
            title = detail.title,
            items = updatedItem,
        };
    }

    private  Item GenerateSummaryDetailItem(Item item, string? subtitle)
    {
        return new Item
        {
            name = item.name,
            path = item.path,
            label = item.label,
            value = item.value,
            rawValue = item.rawValue,
            url = item.url,
            pageId = item.pageId,
            type = item.type,
            title = item.title,
            dataType = item.dataType,
            result = item.result,
            options = item.options,
            subTitle = item.subTitle,
            subTitleNum = item.subTitleNum,
            organizationName = item.organizationName
        };
    }
}