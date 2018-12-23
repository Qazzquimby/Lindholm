using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Deltin.CustomGameAutomation;

internal class GameLoop
{
    private static int _serverTime = 0;
    public readonly Config Cfg;
    private readonly bool _performFirstTimeSetup;
    public CustomGame Cg;

    public GameLoop(Config cfg)
    {
        Cfg = cfg;
        Process process;
        if (OverwatchIsOpen())
        {
            _performFirstTimeSetup = false;
            process = CustomGame.GetOverwatchProcess();
        }
        else
        {
            _performFirstTimeSetup = true;
            process = CreateNewOverwatchProcess();
        }

        Cg = CreateCustomGame(process);
    }

    private Process CreateNewOverwatchProcess()
    {
        var info = new OverwatchProcessInfoAuto
        {
            BattlenetExecutableFilePath = Cfg.BattlenetExecutableFilePath,
            OverwatchSettingsFilePath = Cfg.OverwatchSettingsFilePath,
        };
        Process process = CreateNewOverwatchProcessFromInfo(info);
        return process;
    }

    private Process CreateNewOverwatchProcessFromInfo(OverwatchProcessInfoAuto info)
    {
        Process process = null;
        while (process == null)
        {
            try
            {
                process = CustomGame.CreateOverwatchProcessAutomatically(info);
            }
            catch (OverwatchStartFailedException){}
        }

        return process;

    }

    private CustomGame CreateCustomGame(Process owProcess)
    {
        CustomGameBuilder builder = new CustomGameBuilder
        {
            OpenChatIsDefault = false,
            OverwatchProcess = owProcess,
        };

        return new CustomGame(builder);
    }

    private bool OverwatchIsOpen()
    {
        bool overwatchIsOpen = CustomGame.GetOverwatchProcess() != null;
        return overwatchIsOpen;
    }

    private void FirstTimeSetup()
    {
        Cg.Settings.SetJoinSetting(Join.Everyone);

        Cg.Settings.SetGameName(Cfg.ServerName);
        Cg.Settings.SetTeamName(Team.Blue, $@"\ {Cfg.BlueName}");
        Cg.Settings.SetTeamName(Team.Red, $"* {Cfg.RedName}");

        SwapHostToSpectate();

        Cg.Settings.LoadPreset(Cfg.PresetLocation);
        Cg.Chat.SwapChannel(Channel.Match);

        Cg.StartGame();

        Program.EnterPhase(typeof(First30SecondsPhase));
    }

    private void ExistingGameSetup()
    {
        Cg.AI.RemoveAllBotsAuto();
        Program.EnterPhase(typeof(GamePhase));
    }


    public void Start()
    {
        Setup();
        Loop();
    }

    private void Setup()
    {
        if (_performFirstTimeSetup)
        {
            FirstTimeSetup();
        }
        else
        {
            ExistingGameSetup();
        }
    }

    private void SwapHostToSpectate()
    {
        Cg.Interact.Move(0, 12);
    }

    private static void Loop()
    {
        Debug.WriteLine("Starting loop");
        while (true)
        {
            foreach (int delay in Program.Phase.LoopFuncs.Keys)
            {
                if (_serverTime % delay == 0)
                {
                    try
                    {
                        foreach (Action func in Program.Phase.LoopFuncs[delay])
                        {
                            func();
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                }
            }

            _serverTime += 1;
            Program.TimeAtCurrentMap += 1;
            Thread.Sleep(500);
        }
    }
}