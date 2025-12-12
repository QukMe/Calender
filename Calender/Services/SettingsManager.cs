using System.IO;

namespace Calender.Services
{
    public static class SettingsManager
    {
        private const string FileName = "settings.txt";

        public static void SaveTeacherId(string id)
        {
            File.WriteAllText(FileName, id);
        }

        public static string LoadTeacherId()
        {
            if (File.Exists(FileName))
            {
                return File.ReadAllText(FileName);
            }

            return "20200";
        }
    }
}