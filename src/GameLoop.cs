using System;
using Deltin.CustomGameAutomation;


internal class GameLoop
{
    private readonly Config _cfg;

    public GameManager Game;


    private PhaseRunner _phaseRunner;

    public GameLoop(Config cfg)
    {
        _cfg = cfg;
    }

    public void Run()
    {
        Setup();
        Loop();
    }



    private void Setup()
    {
        CustomGameBuilder customGameBuilder;
        bool isOverwatchOpen = IsOverwatchOpen();
        if (isOverwatchOpen)
        {
            customGameBuilder = new OverwatchIsOpenCustomGameBuilder(_cfg);
        }
        else
        {
            customGameBuilder = new OverwatchIsClosedCustomGameBuilder(_cfg);
        }

        Game = customGameBuilder.CreateGame();
        DebugUtils.Screenshot("Startup");
        _phaseRunner = new PhaseRunner(customGameBuilder.FirstPhase, Game);
    }

    private bool IsOverwatchOpen()
    {
        bool isOverwatchOpen = CustomGame.GetOverwatchProcess() != null;
        return isOverwatchOpen;
    }


    private void Loop()
    {
        Console.WriteLine("Starting loop");
        _phaseRunner.RunForever();
    }
}