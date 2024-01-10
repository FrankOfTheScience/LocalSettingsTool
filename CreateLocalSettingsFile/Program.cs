using CreateLocalSettingsFile;
using Newtonsoft.Json;

string basePath = $"BASE-DIRECTORY";
string jsonOriginalPath = $"ORIGINAL-CONFIG-FILE";
string csvReportName = "REPORT-NAME.csv";
string csvWebJobSettings = "WEB-JOBS-SETTINGS.csv";
string outputFileNameHTTP = "OUTPUT-FILE-NAME1.json";
string outputFileNameNotHTTP = "OUTPUT-FILE-NAME2.json";
string outputFileNameCOMPARE = "OUTPUT-FILE-NAME3.json";


Console.WriteLine($"Press a key to elaborate after dropping the report file in this path: {basePath}{csvReportName}");
Console.ReadKey();

try
{
    if (!File.Exists($"{basePath}{csvReportName}"))
    {
        Console.WriteLine("No file was elaborated, press a key to exit");
        return;
    }

    Console.WriteLine("Elaborating...");

    var httpFunctionsData = CreateLocalSettingsFileHelper.ReadCsvData($"{basePath}{csvReportName}", $"{basePath}{csvWebJobSettings}", ["HTTP", "ALL"]);
    var otherFunctionsData = CreateLocalSettingsFileHelper.ReadCsvData($"{basePath}{csvReportName}", $"{basePath}{csvWebJobSettings}", ["ALL", "TimerTrigger", "BlobTrigger", "ServiceBus"]);

    var allFunctionsData = httpFunctionsData
        .Union(otherFunctionsData)
        .ToDictionary(pair => pair.Key, pair => pair.Value
    );

    CreateLocalSettingsFileHelper.CreateJsonFile(
        $"{basePath}reportSettingFileForHttpTriggered.json", 
        httpFunctionsData.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToDictionary()
    );
    CreateLocalSettingsFileHelper.CreateJsonFile(
        $"{basePath}reportSettingFileForNonHttpTriggered.json", 
        otherFunctionsData.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToDictionary()
    );
    CreateLocalSettingsFileHelper.CreateJsonFile(
        $"{basePath}reportSettingFileForAllFunctions.json", 
        allFunctionsData.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToDictionary()
    );

    Console.WriteLine("Temp files created! Press a key to continue...");
    Console.ReadKey();

    dynamic jsonObject = JsonConvert.DeserializeObject(File.ReadAllText(jsonOriginalPath))!;
    var originalSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonObject.Values.ToString());

    List<Dictionary<string, object>> cleanedDictionaries = CreateLocalSettingsFileHelper.CleanJsonDictionary(originalSettings, new Dictionary<string, object>[] { httpFunctionsData, otherFunctionsData, allFunctionsData });

    if (cleanedDictionaries[0].Count() > 0)
    {
        Console.WriteLine("The following configs has been found without any corrispondency:");
        Console.WriteLine(string.Join(Environment.NewLine, cleanedDictionaries[0].Select(kvp => kvp.Key)));
    }
    Console.WriteLine("-----------------------------------------------------------------------------------------");
    Console.WriteLine($"The files {(CreateLocalSettingsFileHelper.AreFilesTheSame(cleanedDictionaries[3], originalSettings) ? "are not " : string.Empty) }the same; press a key to continue...");
    Console.ReadKey();
    Console.Clear();

    // Creating file for HTTP
    CreateLocalSettingsFileHelper.CreateJsonFile(
       $"{basePath}\\OUTPUT\\{outputFileNameHTTP}",
       cleanedDictionaries[1].OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToDictionary()
    );

    // Creating file for OTHER TRIGGERS
    CreateLocalSettingsFileHelper.CreateJsonFile(
       $"{basePath}\\OUTPUT\\{outputFileNameNotHTTP}",
       cleanedDictionaries[2].OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToDictionary()
    );

    // Creating file for COMPARE
    CreateLocalSettingsFileHelper.CreateJsonFile(
       $"{basePath}\\OUTPUT\\{outputFileNameCOMPARE}",
       cleanedDictionaries[3].OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToDictionary()
    );

    Console.WriteLine("Ended! Press a key to exit...");
}
catch (Exception e)
{
    Console.WriteLine($"The following exception was thrown:\n{e.Message}\n{e.StackTrace}\n\nPress a key to terminate.");
}
finally
{
    Console.ReadKey();
}








