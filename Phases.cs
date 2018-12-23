using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Deltin.CustomGameAutomation;

public abstract class Phase
{
    public virtual Dictionary<int, List<Action>> LoopFuncs { get; set; }

    protected static CustomGame _cg;
    protected static Config _cfg;

    protected Phase(CustomGame cg, Config cfg)
    {
        _cg = cg;
        _cfg = cfg;
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }
}

internal class First30SecondsPhase : Phase
{
    private static int _timer = 0;

    public override Dictionary<int, List<Action>> LoopFuncs { get; set; } = new Dictionary<int, List<Action>>()
    {
        {
            1, new List<Action>()
            {
                IncrementTimer
            }
        },
        {
            5, new List<Action>()
            {
                ScrambleIfImbalance
            }
        },
    };

    private static void IncrementTimer()
    {
        _timer++;
        if (_timer >= 30)
        {
            Program.EnterPhase(typeof(GamePhase));
        }
    }

    private static void ScrambleIfImbalance()
    {
        int blueTeamSizeAdvantage = Program.GetBlueTeamSizeAdvantage();
        if (Math.Abs(blueTeamSizeAdvantage) >= 2)
        {
            Debug.WriteLine("Evening teams");
            Program.SwapToBalance();
        }
    }

    public override void Enter()
    {
        Debug.WriteLine("First30Seconds phase");
        _timer = 0;
        Program.RemoveBots();
        Program.ScrambleTeams();
    }

    public First30SecondsPhase(CustomGame cg, Config cfg) : base(cg, cfg)
    {
    }
}

internal class GamePhase : Phase
{
    public override Dictionary<int, List<Action>> LoopFuncs { get; set; } = new Dictionary<int, List<Action>>()
    {
        {
            30, new List<Action>()
            {
                Program.PrintRunningTrace,
                Program.HandleBots
            }
        },
        {
            15, new List<Action>()
            {
                Program.HandleAutoBalance,
            }
        },
        {
            1, new List<Action>()
            {
                HandleGameOver,
                HandleMissedGameOver,
                Program.PreventMapTimeout,
                Program.PerformAutoBalance
            }
        }
    };


    private static void HandleGameOver()
    {
        if (Program.GameEnded)
        {
            Program.EnterPhase(typeof(RunningGameEndingPhase));
        }
    }

    private static void HandleMissedGameOver()
    {
        if (_cg.GetGameState() == GameState.Ending_Commend)
        {
            Program.GameOver();
            Program.NextMap();
        }
    }

    public override void Enter()
    {
        Debug.WriteLine("game phase");
        Program.TimeAtCurrentMap = 60;
        _cg.Chat.JoinChannel(Channel.Match);
    }

    public override void Exit()
    {
        Debug.WriteLine("leaving game phase");
    }

    public GamePhase(CustomGame cg, Config cfg) : base(cg, cfg)
    {
    }
};

internal class RunningGameEndingPhase : Phase
{
    public override Dictionary<int, List<Action>> LoopFuncs { get; set; } = new Dictionary<int, List<Action>>()
    {
        {
            1, new List<Action>()
            {
                SkipPostGame
            }
        }
    };


    private static void SkipPostGame()
    {
        int waitTimer = 0;
        while (_cg.GetGameState() != GameState.Ending_Commend && waitTimer < 20)
        {
            Thread.Sleep(1000);
            waitTimer++;
            Debug.WriteLine($"{waitTimer} seconds wait");
        }

        Program.NextMap();
    }

    public override void Enter()
    {
        Debug.WriteLine("game ending phase");
        Program.GameOver();
    }

    public override void Exit()
    {
    }

    public RunningGameEndingPhase(CustomGame cg, Config cfg) : base(cg, cfg)
    {
    }
}