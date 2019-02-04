using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Deltin.CustomGameAutomation;


public class Program
{
    public static bool GameEnded = false;
    public static double TimeAtCurrentMap = 0;


    private static Lindholm _lindholm;

    private static void Main()
    {
        _lindholm = new Lindholm();
    }
}