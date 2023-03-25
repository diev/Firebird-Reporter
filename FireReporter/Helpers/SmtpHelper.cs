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
using System.Diagnostics;
using System.Net.Mail;

using static FireReporter.Config;

namespace FireReporter
{
    internal static class SmtpHelper
    {
        public static bool Fail { get; private set; } = false;

        public static void Send(string recipients, string subject, string body)
        {
            Send(recipients.Split(','), subject, body);
        }

        public static void Send(string[] recipients, string subject, string body)
        {
            var mail = new MailMessage()
            {
                From = new MailAddress(Sender),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            foreach (var recipient in recipients)
                mail.To.Add(recipient);

            SmtpSend(mail);
        }

        public static void HtmlSend(string[] recipients, string subject, string body)
        {
            var mail = new MailMessage()
            {
                From = new MailAddress(Sender),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var recipient in recipients)
                mail.To.Add(recipient);

            SmtpSend(mail);
        }

        private static void SmtpSend(MailMessage mail)
        {
            var client = new SmtpClient();

            try
            {
                Fail = true;
                client.Send(mail);
                Fail = false;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                Trace.WriteLine(ex.Message);
                foreach (var iex in ex.InnerExceptions)
                    Trace.WriteLine(iex.Message);
                Trace.WriteLine($"SMTP reply: {ex.StatusCode} for {ex.FailedRecipient}");
            }
            catch (SmtpFailedRecipientException ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLineIf(ex.InnerException != null, ex.InnerException.Message);
                Trace.WriteLine($"SMTP reply: {ex.StatusCode} for {ex.FailedRecipient}");
            }
            catch (SmtpException ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLineIf(ex.InnerException != null, ex.InnerException.Message);
                Trace.WriteLine($"SMTP reply: {ex.StatusCode}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLineIf(ex.InnerException != null, ex.InnerException.Message);
            }
            finally
            {
                if (Fail)
                    throw new InvalidOperationException("Отправка почты не удалась.");
            }
        }

        public static void FailSend(string body)
        {
            var client = new SmtpClient();

            try
            {
                client.Send(Sender, string.Join(",", FailRecipients), nameof(FireReporter) + " fail!", body);
                Fail = false;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                if (!Fail)
                {
                    Trace.WriteLine(ex.Message);
                    foreach (var iex in ex.InnerExceptions)
                        Trace.WriteLine(iex.Message);
                    Trace.WriteLine($"SMTP reply: {ex.StatusCode} for {ex.FailedRecipient}");
                }
            }
            catch (SmtpFailedRecipientException ex)
            {
                if (!Fail)
                {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLineIf(ex.InnerException != null, ex.InnerException.Message);
                    Trace.WriteLine($"SMTP reply: {ex.StatusCode} for {ex.FailedRecipient}");
                }
            }
            catch (SmtpException ex)
            {
                if (!Fail)
                {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLineIf(ex.InnerException != null, ex.InnerException.Message);
                    Trace.WriteLine($"SMTP reply: {ex.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (!Fail)
                {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLineIf(ex.InnerException != null, ex.InnerException.Message);
                }
            }
            finally
            {
                Trace.WriteLineIf(Fail, "Отправка почты о сбое не удалась.");
            }
        }
    }
}
