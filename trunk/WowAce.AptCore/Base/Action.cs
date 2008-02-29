/*
    This file is part of WowAce.AptCore.
    Copyright (C) 2008  Sairén of EU-Malfurion

    WowAce.AptCore is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    WowAce.AptCore is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;

namespace WowAce.AptCore
{
    public class AptAction : AptCommon
    {
        public delegate void StatusMessageEventHandler(string[] message);
        protected event StatusMessageEventHandler eStatusMessage;

        public void AddStatusListener(StatusMessageEventHandler func)
        {
            eStatusMessage += func;
        }

        protected void SendStatus(string str1)
        {
            StatusMessageEventHandler copy = eStatusMessage;

            if (copy != null) { copy(new string[] { str1 }); }
        }

        protected void SendStatus(string str1, string str2)
        {
            StatusMessageEventHandler copy = eStatusMessage;

            if (copy != null) { copy(new string[] { str1, str2 }); }
        }

        protected void SendStatus(string str1, string str2, string str3)
        {
            StatusMessageEventHandler copy = eStatusMessage;

            if (copy != null) { copy(new string[] { str1, str2, str3 }); }
        }
    }
}
