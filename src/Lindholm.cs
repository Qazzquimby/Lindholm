using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Serilog;

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
        while (true)
        {
            RunSafelyIfNotDebug();
        }
    }

    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    private void RunSafelyIfNotDebug()
    {
        var log = new LoggerConfiguration().WriteTo.File("Log.txt").CreateLogger();

        bool crash_on_exception = true; //This is changed manually in the source.
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
                log.Error(e.ToString());
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
