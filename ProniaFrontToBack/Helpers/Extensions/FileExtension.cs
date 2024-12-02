namespace ProniaFrontToBack.Helpers.Extensions;

public static class FileExtension
{
    public static string Upload(this IFormFile file, string rootPath, string folderName)
    {
        string fileName = file.FileName;
        if (fileName.Length > 64)
        {
            fileName = fileName.Substring(0, 64);
        }

        fileName = Guid.NewGuid() + fileName;

        string path = Path.Combine(rootPath, folderName, fileName);

        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            file.CopyTo(fileStream);
        }

        return fileName;
    }

    public static bool Delete(string rootPath, string folderName, string fileName)
    {
        string filePath = Path.Combine(rootPath, folderName, fileName);
        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }
}