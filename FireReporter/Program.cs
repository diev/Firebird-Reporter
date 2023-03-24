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
using System.IO;
using System.Text;

using FirebirdSql.Data.FirebirdClient;

using static FireReporter.Config;

namespace FireReporter
{
    internal class Program
    {
        private static readonly TraceSource _trace = new TraceSource("FireReporter");
        private static readonly Encoding _encoding = Encoding.GetEncoding(1251);

        private static int Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    Usage();
                    return 1;
                }

                _trace.TraceInformation("{0} ======", DateTime.Now);
                Process();
                _trace.TraceInformation("{0} Завершено без ошибок.", DateTime.Now);
                return 0;
            }
            //catch (Exception ex)
            //{
            //    _trace.TraceInformation("{0} Завершено из-за ошибки.", DateTime.Now);
            //    _trace.TraceEvent(TraceEventType.Error, 2, ex.Message);
            //    TextHelper.FailSend(ex.ToString());
            //    return 2;
            //}
            finally
            {
                _trace.Flush();
                _trace.Close();

                //Console.ReadLine();
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Sorry: no help, no args, check .config!");
        }

        private static void Process()
        {
            _trace.TraceInformation("{0} Чтение конфига.", DateTime.Now);

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
                _trace.TraceInformation("{0} Подключение к базе.", DateTime.Now);

                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        _trace.TraceInformation("{0} Разбор строки SQL запроса.", DateTime.Now);

                        using (var command = new FbCommand(SqlQuery, connection, transaction))
                        //using (var command = new FbCommand())
                        {
                            //command.Connection = connection;
                            //command.CommandText = Config.SqlQuery; //TODO: SQL with @param1
                            //command.Parameters.AddWithValue("@param1", "value...");

                            _trace.TraceInformation("{0} Выполнение SQL запроса.", DateTime.Now);

                            using (var reader = command.ExecuteReader())
                            {
                                int rows = 0;
                                string separator = TextSeparator;
                                var values = new object[reader.FieldCount];

                                _trace.TraceInformation("{0} Чтение данных из базы.", DateTime.Now);

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

                                _trace.TraceInformation("{0} Считано строк: {1}", rows, DateTime.Now);

                                if (rows == 0)
                                {
                                    _trace.TraceEvent(TraceEventType.Warning, 0, "{0} Нет данных.", DateTime.Now);

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
                    }
                }

                if (textRequired)
                {
                    _trace.TraceInformation("{0} Оформление отчета TEXT.", DateTime.Now);

                    if (TextFooter.Length > 0)
                        text.AppendLine(TextFooter);

                    string textReport = text.ToString();

                    if (SaveFile)
                        File.WriteAllText(TextFile, textReport, _encoding);

                    if (SendMail)
                        TextHelper.Send(textReport);
                }

                if (htmlRequired)
                {
                    _trace.TraceInformation("{0} Оформление отчета HTML.", DateTime.Now);

                    html.Append(HtmlHelper.Footer());

                    string htmlReport = html.ToString();

                    if (SaveFile)
                        File.WriteAllText(HtmlFile, htmlReport, _encoding);

                    if (SendMail)
                        HtmlHelper.Send(htmlReport);
                }
            }
        }
    }
}
