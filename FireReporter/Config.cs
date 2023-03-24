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

using static FireReporter.ConfigHelper;

namespace FireReporter
{
    internal static class Config
    {
        public static string ConnectionString { get; } = GetConnectionString();

        public static string SqlQuery { get; } = GetSqlText(nameof(SqlQuery));

        public static bool SaveFile { get; } = GetBoolean(nameof(SaveFile));
        public static bool SendMail { get; } = GetBoolean(nameof(SendMail));

        public static string Sender { get; } = GetHtml(nameof(Sender), SendMail);

        public static string TextFile { get; } = GetFileName(nameof(TextFile), SaveFile);
        public static string TextRecipients { get; } = GetHtml(nameof(TextRecipients));
        public static string TextSubject { get; } = GetText(nameof(TextSubject));
        public static string TextHeader { get; } = GetText(nameof(TextHeader));
        public static string TextFooter { get; } = GetText(nameof(TextFooter));
        public static string TextSeparator { get; } = GetSeparator(nameof(TextSeparator));

        public static string HtmlFile { get; } = GetFileName(nameof(HtmlFile), SaveFile);
        public static string HtmlRecipients { get; } = GetHtml(nameof(HtmlRecipients));
        public static string HtmlSubject { get; } = GetText(nameof(HtmlSubject));
        public static string HtmlStyle { get; } = GetString(GetText(nameof(HtmlStyle)));
        public static string HtmlHeader { get; } = GetHtml(nameof(HtmlHeader));
        public static string HtmlTitles { get; } = GetText(nameof(HtmlTitles));
        public static string HtmlFooter { get; } = GetHtml(nameof(HtmlFooter));

        public static string FailRecipients { get; } = GetHtml(nameof(FailRecipients));
    }
}
