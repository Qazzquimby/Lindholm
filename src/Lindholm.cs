using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Deltin.CustomGameAutomation;

//using Serilog;

public class Lindholm
{
    private readonly Config _cfg;
    private GameLoop _loop;

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
            catch (OverwatchClosedException)
            {
                Thread.Sleep(15*1000);
                _loop = new GameLoop(_cfg);
                RunForever();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e);
                FileLogger.Log(e.ToString());
                DebugUtils.Screenshot(DateTime.Now.ToString());
                Thread.Sleep(15000);
            }
        } 
    }

    private void Run()
    {
        Console.WriteLine("Starting Up");
        _loop.Run();
    }

}