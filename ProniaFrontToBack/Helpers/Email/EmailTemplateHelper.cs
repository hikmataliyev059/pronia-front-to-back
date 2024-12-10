namespace ProniaFrontToBack.Helpers.Email;

public static class EmailTemplateHelper
{
    public static string GetTemplate(string filePath, Dictionary<string, string> placeholders)
    {
        var template = File.ReadAllText(filePath);
        foreach (var placeholder in placeholders)
        {
            template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }
        return template;
    }
}