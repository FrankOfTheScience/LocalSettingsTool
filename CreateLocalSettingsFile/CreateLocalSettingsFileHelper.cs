using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace CreateLocalSettingsFile
{
    public static class CreateLocalSettingsFileHelper
    {

        public static Dictionary<string, object> ReadCsvData(string filePath, string webJobsFilePath, string[] filters)
        {
            var data = new Dictionary<string, object>();
            using var csvParser = new TextFieldParser(filePath);
            csvParser.SetDelimiters(";");
            using var csvWebJobsParser = new TextFieldParser(webJobsFilePath);
            csvWebJobsParser.SetDelimiters(";");

            while (!csvParser.EndOfData)
            {
                var fields = csvParser.ReadFields()!;
                foreach (var filter in filters)
                {
                    if (!fields[3].Equals("N.A.") && fields[3] is not null && !fields[4].Equals("N.A."))
                    {
                        if ((fields[2] is not null && fields[2].Equals(filter)) && !data.Any(x => x.Key.Equals(fields[3])))
                            data.Add(fields[3], fields[4]);
                    }
                }
            }
            while (!csvWebJobsParser.EndOfData)
            {
                var fields = csvWebJobsParser.ReadFields()!;
                if (!data.Any(x => x.Key.Equals(fields[0])))
                    data.Add(fields[0], fields[1]);
            }

            return data;
        }

        public static List<Dictionary<string, object>> CleanJsonDictionary(Dictionary<string, object> controlDictionary, params Dictionary<string, object>[] dictionariesToClean)
        {
            var returnList = new List<Dictionary<string, object>>();
            var missingSettings = new Dictionary<string, object>();

            for (int i = 0; i < dictionariesToClean.Length; i++)
            {
                var cleanedDictionaryTemp = new Dictionary<string, object>();

                foreach (var couple in dictionariesToClean[i])
                {
                    if (!controlDictionary.ContainsKey(couple.Key))
                        missingSettings.TryAdd(couple.Key, dictionariesToClean[0].Values);
                    else if (controlDictionary[couple.Key] == null)
                        missingSettings.TryAdd(couple.Key, couple.Value!);
                    else
                    {
                        var valueType = controlDictionary[couple.Key].GetType();
                        cleanedDictionaryTemp.TryAdd(
                            couple.Key,
                            couple.Value != null && valueType.IsInstanceOfType(couple.Value)
                                ? couple.Value
                                : Convert.ChangeType(controlDictionary[couple.Key], valueType)
                        );
                    }
                }
                returnList.Add(cleanedDictionaryTemp);
            }
            returnList.Insert(0, missingSettings);

            return returnList;
        }

        public static void CreateJsonFile(string path, Dictionary<string, object> contentValues)
        {
            try
            {
                var resultObject = new
                {
                    IsEncrypted = false,
                    Values = contentValues
                };

                File.WriteAllText(path, JsonConvert.SerializeObject(resultObject, Formatting.Indented));
            }
            catch (Exception) { throw; }
        }

        public static bool AreFilesTheSame(Dictionary<string, object> generatedDic, Dictionary<string, object> expectedDic)
        {
            bool flag = true;
            foreach (var item in generatedDic)
            {
                if (!(expectedDic.TryGetValue(item.Key, out var expectedValue) && string.Equals(expectedValue?.ToString(), item.Value?.ToString())))
                {
                    Console.WriteLine($"Setting not found in original file: {item.Key}");
                    flag = false;
                }
            }
            foreach (var item in expectedDic)
            {
                if (!(generatedDic.TryGetValue(item.Key, out var expectedValue) && string.Equals(expectedValue?.ToString(), item.Value?.ToString())))
                {
                    Console.WriteLine($"Setting not found in generated file: {item.Key}");
                    flag = false;
                }
            }
            return flag;
        }
    }
}
