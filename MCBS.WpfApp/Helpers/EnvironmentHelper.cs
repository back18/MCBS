using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Helpers
{
    public static class EnvironmentHelper
    {
        private readonly static string[] Variables =
        [
            "ENVIRONMENT",
            "DOTNET_ENVIRONMENT",
            "ASPNETCORE_ENVIRONMENT",
        ];

        public const string DEVELOPMENT = "Development";
        public const string PRODUCTION = "Production";
        public const string STAGING = "Staging";
        public const string TEST = "Test";

        public static bool IsDevelopment => GetCurrentEnvironment() == DEVELOPMENT;

        public static bool IsProduction => GetCurrentEnvironment() == PRODUCTION;

        public static bool IsStaging => GetCurrentEnvironment() == STAGING;

        public static bool IsTest => GetCurrentEnvironment() == TEST;

        public static string GetCurrentEnvironment()
        {
            foreach (string variable in Variables)
            {
                string? value = Environment.GetEnvironmentVariable(variable);
                if (!string.IsNullOrWhiteSpace(value))
                    return NormalizeEnvironmentName(value);
            }

            return GetEnvironmentFromCompilation();
        }

        private static string NormalizeEnvironmentName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return PRODUCTION;

            return name.Trim().ToLower() switch
            {
                "dev" or "debug" => DEVELOPMENT,
                "prod" => PRODUCTION,
                "stage" or "preprod" or "pre-production" => STAGING,
                "test" or "qa" => TEST,
                _ => name
            };
        }

        private static string GetEnvironmentFromCompilation()
        {
#if DEBUG
            return DEVELOPMENT;
#elif STAGING
            return STAGING;
#elif TEST
            return TEST;
#else
            return PRODUCTION;
#endif
        }
    }
}
