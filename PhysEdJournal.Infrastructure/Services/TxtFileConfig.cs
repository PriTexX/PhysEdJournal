namespace PhysEdJournal.Infrastructure.Services;

public class TxtFileConfig
{
    private readonly string _filePath;

    public TxtFileConfig(string filePath)
    {
        _filePath = filePath;
    }

    public void WriteTextToFile(string textToWrite)
    {
        File.WriteAllText(_filePath, textToWrite);
    }

    public string ReadTextFromFile()
    {
        return File.ReadAllText(_filePath);
    }
}