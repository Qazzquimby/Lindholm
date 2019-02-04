using System;
using System.Threading;

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

    private void RunSafelyIfNotDebug()
    {
        if (_cfg.Debug)
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
