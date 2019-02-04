using System;
using Deltin.CustomGameAutomation;

public class DebugUtils
{
    public static bool Debug;
    public static CustomGame Cg { private get; set; }

    public DebugUtils(bool debug, CustomGame cg)
    {
        Debug = debug;
        Cg = cg;
    }

    public static void Print(string message)
    {
        if (Debug)
        {
            Console.WriteLine(message);
        }
    }

    public static void Screenshot(string screenshotName)
    {
        if (Debug)
        {
            Cg.SaveScreenshot($"Debug_{screenshotName}_Screenshot.png");
        }
    }
}