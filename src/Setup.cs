using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deltin.CustomGameAutomation;

public abstract class CustomGameBuilder
{
    public abstract Type FirstPhase { get; }
    public abstract GameManager CreateGame();
    protected Config Cfg;

    protected void SetupNames(CustomGame cg)
    {
        cg.Settings.SetGameName(Cfg.ServerName);

        //        Cg.Settings.SetTeamName(Team.Blue, $@"\ {Cfg.BlueName}");
        //        Cg.Settings.SetTeamName(Team.Red, $"* {Cfg.RedName}");

        cg.Settings.SetTeamName(Team.Blue, Cfg.BlueName);
        cg.Settings.SetTeamName(Team.Red, Cfg.RedName);
    }
}


public class OverwatchIsClosedCustomGameBuilder : CustomGameBuilder
{
    private CustomGame _cg;

    public override Type FirstPhase { get; } = typeof(First30SecondsPhase);

    public OverwatchIsClosedCustomGameBuilder(Config cfg)
    {
        Cfg = cfg;
    }

    

    public override GameManager CreateGame()
    {
        Process process = CreateNewOverwatchProcess();
        CustomGame cg = new CustomGameFromProcessCreator().Run(process);
        _cg = cg;
        SetupCustomGame();
        GameManager game = new GameManager(_cg, Cfg);
        return game;
    }


    private Process CreateNewOverwatchProcess()
    {
        var info = new OverwatchInfoAuto
        {
            BattlenetExecutableFilePath = Cfg.BattlenetExecutableFilePath,
            OverwatchSettingsFilePath = Cfg.OverwatchSettingsFilePath,
            MaxBattlenetStartTime = 60*1000,
            MaxWaitForMenuTime = 60 * 1000,
            MaxOverwatchStartTime = 60 * 1000,
        };
        Process process = CreateNewOverwatchProcessFromInfo(info);
        return process;
    }

    private Process CreateNewOverwatchProcessFromInfo(OverwatchInfoAuto info)
    {
        Process process = null;
        while (process == null)
        {
            try
            {
                process = CustomGame.StartOverwatch(info);
            }
            catch (OverwatchStartFailedException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        return process;
    }

    private void SetupCustomGame()
    {
        SetupNames(_cg);
        _cg.Settings.JoinSetting = Cfg.ServerVisibility;

        SwapHostToSpectate();

//        Console.WriteLine("Waiting 15s for more stable load.");
//        Thread.Sleep(15*1000);       

        SafelyLoadPreset();

        _cg.StartGame();
        _cg.Chat.SwapChannel(Channel.Match);
    }

    private void SafelyLoadPreset()
    {
        bool success = false;
        int attempts = 0;

        while (!success)
        {
            Console.WriteLine($"Attempting to load preset named {Cfg.PresetName}");
            success = _cg.Settings.LoadPreset(Cfg.PresetName);
            attempts++;
            if (attempts > 15)
            {
                return;
            }
        }
    }

    private void SwapHostToSpectate()
    {
        _cg.Interact.Move(0, 12);
    }

}

public class OverwatchIsOpenCustomGameBuilder : CustomGameBuilder
{
    public override Type FirstPhase { get; } = typeof(GamePhase);

    public OverwatchIsOpenCustomGameBuilder(Config cfg)
    {
        Cfg = cfg;
    }

    public override GameManager CreateGame()
    {
        Process process = CustomGame.GetOverwatchProcess();
        CustomGame cg = new CustomGameFromProcessCreator().Run(process);
        CustomGameSetup(cg);
        GameManager game = new GameManager(cg, Cfg);
        return game;
    }

    private void CustomGameSetup(CustomGame cg)
    {
        SetupNames(cg);
        cg.AI.RemoveAllBotsAuto();
        cg.Chat.SwapChannel(Channel.Match);
    }
}



public class CustomGameFromProcessCreator
{
    public CustomGame Run(Process owProcess)
    {
        Deltin.CustomGameAutomation.CustomGameBuilder builder = new Deltin.CustomGameAutomation.CustomGameBuilder
        {
            OpenChatIsDefault = false,
            OverwatchProcess = owProcess,
        };

        CustomGame Cg = new CustomGame(builder);
        return Cg;
    }
}
