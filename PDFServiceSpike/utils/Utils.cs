using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using PDFService.models;
public static class Utils
{
    public static string TruncateForDisplay(this string value, int length)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var returnValue = value;
        if (value.Length > length)
        {
            var tmp = value.Substring(0, length);
            if (tmp.LastIndexOf(' ') > 0)
                returnValue = tmp.Substring(0, tmp.LastIndexOf(' ')) + "...";
        }
        return returnValue;
    }

    public static Summary? LoadJson(string filePath)
    {
        var serializer = new JsonSerializer();

        using (StreamReader reader = new StreamReader(filePath))
        using (JsonTextReader jsonReader = new JsonTextReader(reader))
        {
            JsonSerializer ser = new JsonSerializer();
            return serializer.Deserialize<Summary>(jsonReader);
            
        }
 
    }

    public static Message LoadJsonStream(Stream data)
    {
        var serializer = new JsonSerializer();

        using (StreamReader reader = new StreamReader(data))
        using (JsonTextReader jsonReader = new JsonTextReader(reader))
        {
            return serializer.Deserialize<Message>(jsonReader);
        }
    }
}

