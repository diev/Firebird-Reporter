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

using System.Net.Mail;
using System.Text;

using static FireReporter.Config;

namespace FireReporter
{
    internal static class HtmlHelper
    {
        public static string Header()
        {
            var text = new StringBuilder();
            
            text.AppendLine("<!DOCTYPE html><html><head><meta charset=\"windows-1251\"><title>Message text</title>");

            if (HtmlStyle.Length > 0)
                text.Append("<style>")
                    .Append(HtmlStyle)
                    .AppendLine("</style>");

            text.AppendLine("</head><body>");

            if (HtmlHeader.Length > 0)
                text.AppendLine(HtmlHeader);

            text.AppendLine("<table><tbody>");

            if (HtmlTitles.Length > 0)
                text.Append("<tr><th>")
                    .Append(string.Join("</th><th>", HtmlTitles.Split(',')))
                    .AppendLine("</th></tr>");

            return text.ToString();
        }

        public static string Footer()
        {
            var text = new StringBuilder();

            text.AppendLine("</tbody></table>");

            if (HtmlFooter.Length > 0)
                text.AppendLine(HtmlFooter);

            text.AppendLine("</body></html>");

            return text.ToString();
        }

        public static void Send(string report)
        {
            var mailMessage = new MailMessage()
            {
                From = new MailAddress(Sender),
                Subject = HtmlSubject,
                Body = report,
                IsBodyHtml = true
            };

            foreach (var recipient in HtmlRecipients.Split(','))
                mailMessage.To.Add(recipient);

            var client = new SmtpClient();
            client.Send(mailMessage);
        }
    }
}
