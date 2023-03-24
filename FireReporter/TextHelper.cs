﻿#region License
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

using static FireReporter.Config;

namespace FireReporter
{
    internal static class TextHelper
    {
        public static void Send(string report)
        {
            var client = new SmtpClient();
            client.Send(Sender, TextRecipients, TextSubject, report);
        }

        public static void FailSend(string report)
        {
            var client = new SmtpClient();
            try
            {
                client.Send(Sender, FailRecipients, nameof(FireReporter) + " fail", report);
            }
            catch { }
        }
    }
}
