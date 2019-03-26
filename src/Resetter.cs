using System;
using Deltin.CustomGameAutomation;

public class Resetter
{
    private CustomGame _cg;

    public Resetter(CustomGame cg)
    {
        _cg = cg;
    }

    public void Reset()
    {
        try
        {
            OverwatchState state = _cg.Reset();
            if (state != OverwatchState.Ready)
            {
                ExitAndRestart();
            }
        }
        catch (UnknownOverwatchStateException)
        {
            ExitAndRestart();
        }
    }

    public void ExitAndRestart()
    {
        Console.WriteLine("Could not navigate to default viewport. Restarting.");
        _cg.OverwatchProcess.CloseMainWindow();
    }
}