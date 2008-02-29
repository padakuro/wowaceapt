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
using System.Threading;

namespace WowAce.AptGet
{
    class StaticMain
    {
        private static string[] Arguments;

        /*
         * Basically this code is not needed but I tested something and forgot to
         * save the old file... so, too lazy to revert as it still functions well :)
         * (it also runs a bit faster tho)
         */
        
        static void Main(string[] args)
        {
            Arguments = args;
            
            Thread main = new Thread(new ThreadStart(Run));
            main.Start();
        }

        static void Run()
        {
            Program p = new Program(Arguments);
        }
    }
}
