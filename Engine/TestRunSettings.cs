using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CSharpSeleniumFramework.Engine
{
    /// <summary>
    /// Haal settings op uit het actieve runsettings bestand en stel deze beschikbaar voor gebruik
    /// </summary>
    public static class TestRunSettings
    {
        // Framework settings
        public static int DefaultWaitTime => ParameterAsInt("DefaultWaitTime", "30");
        public static int NumberOfAdministrationsCopiesInPool => ParameterAsInt("NumberOfAdministrationsCopiesInPool", "5");
        public static Uri SeleniumCluster => ParameterAsUri("SeleniumCluster");
        public static Uri AppiumLokaalDev => ParameterAsUri("AppiumLokaalDev");
        public static bool AppiumScreenRecording => ParameterAsBool("AppiumScreenRecording");
        public static bool ScannerEnabled => ParameterAsBool("ScannerEnabled");
        public static string ScannerType => ParameterAsString("ScannerType", "GeenScannerTypeIngesteld");
        public static string SspDbConnectionstringTest => ParameterAsString("SspDbConnectionstringTest");

        //Environment settings
        public static string RealWorldApp => ParameterAsString("RealWorldApp");
        public static string AndroidAppLocation => ParameterAsString("AndroidAppLocation");
        public static string AndroidAppIdentifier => ParameterAsString("AndroidAppIdentifier");
        public static string IosAppLocation => ParameterAsString("IosAppLocation");
        public static string ApplicationName => "Selenium";
        public static string EnvironmentName => ParameterAsString("EnvironmentName", "Test");

        //Lokale debugging parameters
        public static bool ForceLocalRun => ParameterAsBool("ForceLocalRun");
        public static string ForceBrowser => ParameterAsString("ForceBrowser");
        public static bool ForceCluster => ParameterAsBool("ForceCluster");
        
        private static List<string> ParameterAsStringList(string parameterName, string defaultValue = "")
        {
            return GetRunParameter(parameterName, defaultValue).Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static string GetRunParameter(string parameterName, string defaultValue)
        {
            String returnValue;
            String ExceptionMessage =
                $"'{parameterName}' not found in Runsettings file.\n Is the runsettings file selected?";
            try
            {
                if (defaultValue != string.Empty)
                {
                    returnValue = TestContext.Parameters.Get(parameterName, defaultValue);
                }
                else
                {
                    returnValue = TestContext.Parameters.Get(parameterName);
                }
            }
            catch (NullReferenceException)
            {
                throw new ArgumentNullException(ExceptionMessage);
            }

            if (returnValue == null)
            {
                throw new ArgumentNullException(ExceptionMessage);
            }

            return returnValue;
        }

        private static bool ParameterAsBool(string parameterName, string defaultValue = "false")
        {
            bool.TryParse(GetRunParameter(parameterName, defaultValue), out bool result);
            return result;
        }

        private static int ParameterAsInt(string parameterName, string defaultValue = "")
        {
            bool success = int.TryParse(GetRunParameter(parameterName, defaultValue), out int result);
            if (success)
            {
                return result;
            }

            throw new ArgumentException(parameterName + " in runsettings");
        }

        private static string ParameterAsString(string parameterName, string defaultValue = "")
        {
            string result = GetRunParameter(parameterName, defaultValue);
            return result;
        }

        private static Uri ParameterAsUri(string parameterName, string defaultValue = "")
        {
            return new Uri(ParameterAsString(parameterName, defaultValue));
        }
    }
}
