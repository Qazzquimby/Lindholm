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
}


public class OverwatchIsClosedCustomGameBuilder : CustomGameBuilder
{
    private readonly Config _cfg;
    private CustomGame _cg;

    public override Type FirstPhase { get; } = typeof(First30SecondsPhase);

    public OverwatchIsClosedCustomGameBuilder(Config cfg)
    {
        _cfg = cfg;
    }

    

    public override GameManager CreateGame()
    {
        Process process = CreateNewOverwatchProcess();
        CustomGame cg = new CustomGameFromProcessCreator().Run(process);
        _cg = cg;
        SetupCustomGame();
        GameManager game = new GameManager(_cg, _cfg);
        return game;
    }


    private Process CreateNewOverwatchProcess()
    {
        var info = new OverwatchInfoAuto
        {
            BattlenetExecutableFilePath = _cfg.BattlenetExecutableFilePath,
            OverwatchSettingsFilePath = _cfg.OverwatchSettingsFilePath,
            MaxOverwatchStartTime = 60000
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
        _cg.Settings.JoinSetting = Join.Everyone;
        _cg.Settings.SetGameName(_cfg.ServerName);

        //        Cg.Settings.SetTeamName(Team.Blue, $@"\ {Cfg.BlueName}");
        //        Cg.Settings.SetTeamName(Team.Red, $"* {Cfg.RedName}");

        _cg.Settings.SetTeamName(Team.Blue, _cfg.BlueName);
        _cg.Settings.SetTeamName(Team.Red, _cfg.RedName);

        SwapHostToSpectate();

        _cg.Settings.LoadPreset(_cfg.PresetName);
        _cg.Chat.SwapChannel(Channel.Match);

        _cg.StartGame();
        _cg.Chat.SwapChannel(Channel.Match);
    }

    private void SwapHostToSpectate()
    {
        _cg.Interact.Move(0, 12);
    }

}

public class OverwatchIsOpenCustomGameBuilder : CustomGameBuilder
{
    public override Type FirstPhase { get; } = typeof(GamePhase);

    private readonly Config _cfg;

    public OverwatchIsOpenCustomGameBuilder(Config cfg)
    {
        _cfg = cfg;
    }

    public override GameManager CreateGame()
    {
        Process process = CustomGame.GetOverwatchProcess();
        CustomGame cg = new CustomGameFromProcessCreator().Run(process);
        CustomGameSetup(cg);
        GameManager game = new GameManager(cg, _cfg);
        return game;
    }

    private void CustomGameSetup(CustomGame cg)
    {
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
