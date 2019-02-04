using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

//using Serilog;

public class Lindholm
{
    private readonly Config _cfg;
    private readonly GameLoop _loop;

    public Lindholm()
    {
        _cfg = new Config();
        _loop = new GameLoop(_cfg);
        RunForever();
    }

    private void RunForever()
    {
//        while (true) //At least temporarily removed, since this can lead to general chat spam.
//        {
            RunSafelyIfNotDebug();
//        }
    }

    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    private void RunSafelyIfNotDebug()
    {
        FileLogger.FilePath = "Debug/Log.txt";

        bool crash_on_exception = false; //This is changed manually in the source.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (crash_on_exception)
        {
            Run();
        }
        else
        {
            try
            {
                Run();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e);
                FileLogger.Log(e.ToString());
                Thread.Sleep(10000);
            }
        } 
    }

    private void Run()
    {
        Console.WriteLine("Starting Up");
        _loop.Run();
    }

}

public class FileLogger
{
    public static string FilePath = @"Log.txt";
    public static void Log(string message)
    {
        using (StreamWriter streamWriter = new StreamWriter(FilePath))
        {
            streamWriter.WriteLine(message);
            streamWriter.Close();
        }
    }
}