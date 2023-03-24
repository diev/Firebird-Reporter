#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov
// Open ource software https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
using System.Linq;

namespace FireReporter
{
    internal class ConfigHelper
    {
        internal static string GetConnectionString(string key = "DB")
        {
            string value = ConfigurationManager.ConnectionStrings[key].ConnectionString;

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(key, $"Не указана строка подключения '{key}'.");

            value = GetString(value);

            if (value.Contains("User=SYSDBA;") && value.Contains("Password=;"))
                value = value.Replace("Password=;", "Password=masterkey;");

            return value;
        }

        internal static string GetSqlText(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(key, $"Не указана строка SQL запроса '{key}'.");

            value = GetString(value);

            string[] blacklist = { "CREATE ", "UPDATE ", "DELETE ", "DROP ", "TRUNCATE " };
            string test = value.ToUpper();

            foreach (var keyword in blacklist)
                if (test.Contains(keyword))
                    throw new InvalidOperationException($"В '{key}' высока вероятность SQL-injection с '{keyword}' .");

            if (test.Contains(';') && !test.Contains("';'")) //";"?
                throw new InvalidOperationException($"В '{key}' высока вероятность SQL-injection с добавлением ';'.");

            return value;
        }

        internal static bool GetBoolean(string key, bool required = false)
        {
            if (bool.TryParse(ConfigurationManager.AppSettings[key], out bool value))
                return value;
            else if (required)
                throw new FormatException($"В '{key}' требуется 'true' или 'false'.");

            return false;
        }

        internal static string GetSeparator(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? " ";
        }

        internal static string GetText(string key, bool required = false)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    throw new ArgumentNullException(key, $"Не указан '{key}'.");
                else
                    return string.Empty;
            }

            return value.Replace("\\n", Environment.NewLine);
        }

        internal static string GetFileName(string key, bool required = false)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    throw new ArgumentNullException(key, $"Не указан '{key}'.");
                else
                    return string.Empty;
            }

            return Environment.ExpandEnvironmentVariables(value);
        }

        internal static string GetHtml(string key, bool required = false)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    throw new ArgumentNullException(key, $"Не указан '{key}'.");
                else
                    return string.Empty;
            }

            return GetString(value).Replace("[", "<").Replace("]", ">");
        }

        internal static string GetString(string value)
        {
            char[] sep = { '\n', '\r', '\t', ' ' };
            string[] lines = value.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", lines).Trim();
        }
    }
}
