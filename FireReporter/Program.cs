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

namespace FireReporter
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    Usage();
                    return 1;
                }

                Trace.WriteLine($"Старт: {DateTime.Now}");
                var worker = new Worker();
                Trace.WriteLine("Завершено без ошибок.");
                return 0;
            }
            #if !DEBUG
            catch (Exception ex)
            {
                string msg = ex.Message;

                if (ex.InnerException != null)
                    msg += Environment.NewLine + ex.InnerException.Message;

                Trace.IndentLevel = 0;
                Trace.WriteLine("Завершено из-за ошибки:");
                Trace.WriteLine(msg);

                SmtpHelper.FailSend(msg);

                return 2;
            }
            #endif
            finally
            {
                Trace.WriteLine($"Финиш: {DateTime.Now}{Environment.NewLine}");
                Trace.Flush();
                Trace.Close();

                #if DEBUG
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
                #endif
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Sorry: no help, no args, check .config!");
        }
    }
}
