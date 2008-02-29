/*
    This file is part of WowAce.AptGet.
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
using System.Collections.Generic;
using System.Text;

namespace WowAce.AptGet
{
    static class Output
    {
        public static void Error(string message)
        {
            Console.Write("\n[e] " + message);
        }

        public static void Info(string message)
        {
            Console.Write("\n[i] " + message);
        }

        public static void Append(string message)
        {
            Console.Write(message);
        }

        public static void NewLine()
        {
            Console.Write("\n");
        }
    }
}
