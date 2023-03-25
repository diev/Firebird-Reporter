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
            Trace.WriteLine("������ �������");

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
                Trace.WriteLine("����������� � ����");
                Trace.Indent();

                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Trace.WriteLine("������ ������ SQL �������");

                        using (var command = new FbCommand(SqlQuery, connection, transaction))
                        //using (var command = new FbCommand())
                        {
                            //command.Connection = connection;
                            //command.CommandText = Config.SqlQuery; //TODO: SQL with @param1
                            //command.Parameters.AddWithValue("@param1", "value...");

                            Trace.WriteLine("���������� SQL �������");

                            using (var reader = command.ExecuteReader())
                            {
                                int rows = 0;
                                string separator = TextSeparator;
                                var values = new object[reader.FieldCount];

                                Trace.WriteLine("������ ������ �� ����");

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

                                Trace.WriteLine($"������� �����: {rows}");

                                if (rows == 0)
                                {
                                    Trace.TraceWarning("��� ������");

                                    if (textRequired)
                                        text.AppendLine("(��� ������)");

                                    if (htmlRequired)
                                        html.Append("<tr><td colspan=")
                                            .Append(reader.FieldCount)
                                            .AppendLine(">(��� ������)</td></tr>");
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
                    Trace.WriteLine("���������� ������ TEXT");
                    Trace.Indent();

                    if (TextFooter.Length > 0)
                        text.AppendLine(TextFooter);

                    textReport = text.ToString();
                    Trace.WriteLine($"������: {textReport.Length}");

                    if (SaveFile)
                    {
                        Trace.WriteLine($"����: {TextFile}");
                        File.WriteAllText(TextFile, textReport, _encoding);
                    }

                    Trace.Unindent();
                }

                if (htmlRequired)
                {
                    Trace.WriteLine("���������� ������ HTML");
                    Trace.Indent();

                    html.Append(HtmlHelper.Footer());

                    htmlReport = html.ToString();
                    Trace.WriteLine($"������: {htmlReport.Length}");

                    if (SaveFile)
                    {
                        Trace.WriteLine($"����: {HtmlFile}");
                        File.WriteAllText(HtmlFile, htmlReport, _encoding);
                    }

                    Trace.Unindent();
                }

                if (SendMail)
                {
                    if (textRequired)
                    {
                        Trace.WriteLine("�������� ������ TEXT");
                        SmtpHelper.Send(TextRecipients, TextSubject, textReport);
                    }

                    if (htmlRequired)
                    {
                        Trace.WriteLine("�������� ������ HTML");
                        SmtpHelper.HtmlSend(HtmlRecipients, HtmlSubject, htmlReport);
                    }
                }
            }
        }
    }
}
