using System;
using System.Collections.Generic;
using System.Threading;

public class PhaseRunner
{
    private readonly GameManager _game;
    private Phase _currentPhase;
    private int _timeCounter = 0;

    public PhaseRunner(Type firstPhase, GameManager game)
    {
        _game = game;

        _game.Match.OnNextMap += EnterFirst30SecondsPhase;
        _game.Cg.OnGameOver += EnterRunningGameEndingPhase;

        EnterPhase(firstPhase);
    }

    public void RunForever()
    {
        while (true)
        {
            Run();
        }
    }

    public void EnterPhase(Type newPhase)
    {
        _currentPhase?.Exit();

        var paramArray = new object[] {_game, (Action<Type>) EnterPhase};
        _currentPhase = (Phase) Activator.CreateInstance(newPhase, paramArray);
        _currentPhase.Enter();
    }


    private void EnterFirst30SecondsPhase(object sender, EventArgs e)
    {
        EnterPhase(typeof(First30SecondsPhase));
    }


    private void EnterRunningGameEndingPhase(object sender, EventArgs e)
    {
        EnterPhase(typeof(RunningGameEndingPhase));
    }


    private void Run()
    {
        foreach (int delay in _currentPhase.LoopFuncs.Keys)
        {
            if (_timeCounter % delay == 0)
            {
                try
                {
                    foreach (Action func in _currentPhase.LoopFuncs[delay])
                    {
                        func();
                    }
                }
                catch (KeyNotFoundException)
                {
                }
            }
        }

        _timeCounter += 1;
        Thread.Sleep(500);
    }
}

public abstract class Phase
{
    protected GameManager _game;
    protected Action<Type> EnterPhase;

    protected Phase(GameManager game, Action<Type> enterPhase)
    {
        _game = game;
        EnterPhase = enterPhase;
    }

    public abstract Dictionary<int, List<Action>> LoopFuncs { get; }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }
}

internal class First30SecondsPhase : Phase
{
    private int _timer = 0;

    public First30SecondsPhase(GameManager game, Action<Type> enterPhase) : base(game, enterPhase)
    {
        LoopFuncs = new Dictionary<int, List<Action>>
        {
            {
                1, new List<Action>
                {
                    IncrementTimer
                }
            },
            {
                5, new List<Action>
                {
                    ScrambleIfImbalance
                }
            }
        };
    }

    public override Dictionary<int, List<Action>> LoopFuncs { get; }


    private void IncrementTimer()
    {
        _timer++;
        if (_timer >= 30)
        {
            EnterPhase(typeof(GamePhase));
        }
    }

    private void ScrambleIfImbalance()
    {
    }

    public override void Enter()
    {
        Console.WriteLine("First30Seconds phase");
        _timer = 0;
        _game.Scrambler.ScrambleTeams();
        _game.Chatter.ChatStartMessages();
    }
}

internal class GamePhase : Phase
{
    public GamePhase(GameManager game, Action<Type> enterPhase) : base(game, enterPhase)
    {
        LoopFuncs = new Dictionary<int, List<Action>>
        {
            {
                600, new List<Action>
                {
                    _game.Chatter.ChatFewPlayersMessageIfFewPlayers
                }
            },
            {
                30, new List<Action>
                {
                    _game.PrintRunningTrace,
                    _game.Bots.HandleBots
                }
            },
            {
                15, new List<Action>
                {
                    _game.Autobalancer.HandleAutoBalance
                }
            },
            {
                1, new List<Action>
                {
                    _game.Match.HandleMissedGameOver,
                    _game.Match.PreventMapTimeout,
                    _game.Autobalancer.PerformAutoBalance
                }
            }
        };

        }

    public override Dictionary<int, List<Action>> LoopFuncs { get; }





    public override void Enter()
    {
        Console.WriteLine("game phase");
        _game.Chatter.JoinMatchChat();
    }

    public override void Exit()
    {
        Console.WriteLine("leaving game phase");
    }
}

internal class RunningGameEndingPhase : Phase
{
    public RunningGameEndingPhase(GameManager game, Action<Type> enterPhase) : base(game, enterPhase)
    {
        LoopFuncs = new Dictionary<int, List<Action>>
        {
            {
                1, new List<Action>
                {
                    _game.Match.SkipPostGame
                }
            }
        };
    }

    public override Dictionary<int, List<Action>> LoopFuncs { get; }

    public override void Enter()
    {
        Console.WriteLine("game ending phase");
    }


    public override void Exit()
    {
    }
}