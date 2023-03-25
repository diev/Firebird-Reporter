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

#define TRACE

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using FirebirdSql.Data.FirebirdClient;

using static FireReporter.Config;

namespace FireReporter
{
    internal class Worker
    {
        private static readonly Encoding _encoding = Encoding.GetEncoding(1251);

        public Worker()
        {
            Trace.WriteLine("Чтение конфига");

            bool textRequired = TextRecipients.Length > 0;
            bool htmlRequired = HtmlRecipients.Length > 0;

            var text = new StringBuilder();
            var html = new StringBuilder();

            if (textRequired && TextHeader.Length > 0)
                text.AppendLine(TextHeader);

            if (htmlRequired)
                html.Append(HtmlHelper.Header());

            using (var connection = new FbConnection())
            {
                Trace.WriteLine("Подключение к базе");
                Trace.Indent();

                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Trace.WriteLine("Разбор строки SQL запроса");

                        using (var command = new FbCommand(SqlQuery, connection, transaction))
                        //using (var command = new FbCommand())
                        {
                            //command.Connection = connection;
                            //command.CommandText = Config.SqlQuery; //TODO: SQL with @param1
                            //command.Parameters.AddWithValue("@param1", "value...");

                            Trace.WriteLine("Выполнение SQL запроса");

                            using (var reader = command.ExecuteReader())
                            {
                                int rows = 0;
                                string separator = TextSeparator;
                                var values = new object[reader.FieldCount];

                                Trace.WriteLine("Чтение данных из базы");

                                while (reader.Read())
                                {
                                    rows++;
                                    reader.GetValues(values);

                                    if (textRequired)
                                        text.AppendLine(string.Join(separator, values));

                                    if (htmlRequired)
                                        html.Append("<tr><td>")
                                            .Append(string.Join("</td><td>", values))
                                            .AppendLine("</td></tr>");
                                }

                                Trace.WriteLine($"Считано строк: {rows}");

                                if (rows == 0)
                                {
                                    Trace.TraceWarning("Нет данных");

                                    if (textRequired)
                                        text.AppendLine("(Нет данных)");

                                    if (htmlRequired)
                                        html.Append("<tr><td colspan=")
                                            .Append(reader.FieldCount)
                                            .AppendLine(">(Нет данных)</td></tr>");
                                }
                            }
                        }
                    }
                    finally
                    {
                        transaction.Rollback(); // read-only queries!
                        Trace.Unindent();
                    }
                }

                string textReport = string.Empty;
                string htmlReport = string.Empty;

                if (textRequired)
                {
                    Trace.WriteLine("Оформление отчета TEXT");
                    Trace.Indent();

                    if (TextFooter.Length > 0)
                        text.AppendLine(TextFooter);

                    textReport = text.ToString();
                    Trace.WriteLine($"Размер: {textReport.Length}");

                    if (SaveFile)
                    {
                        Trace.WriteLine($"Файл: {TextFile}");
                        File.WriteAllText(TextFile, textReport, _encoding);
                    }

                    Trace.Unindent();
                }

                if (htmlRequired)
                {
                    Trace.WriteLine("Оформление отчета HTML");
                    Trace.Indent();

                    html.Append(HtmlHelper.Footer());

                    htmlReport = html.ToString();
                    Trace.WriteLine($"Размер: {htmlReport.Length}");

                    if (SaveFile)
                    {
                        Trace.WriteLine($"Файл: {HtmlFile}");
                        File.WriteAllText(HtmlFile, htmlReport, _encoding);
                    }

                    Trace.Unindent();
                }

                if (SendMail)
                {
                    if (textRequired)
                    {
                        Trace.WriteLine("Отправка отчета TEXT");
                        SmtpHelper.Send(TextRecipients, TextSubject, textReport);
                    }

                    if (htmlRequired)
                    {
                        Trace.WriteLine("Отправка отчета HTML");
                        SmtpHelper.HtmlSend(HtmlRecipients, HtmlSubject, htmlReport);
                    }
                }
            }
        }
    }
}
