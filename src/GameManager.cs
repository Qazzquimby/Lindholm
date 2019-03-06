using System;
using System.Collections.Generic;
using Deltin.CustomGameAutomation;


public class BotManager
{
    public int NumBots { get; private set; } = 0;
    private readonly Config _cfg;
    private readonly AI _ai;
    private readonly SlotObservation _observation;
    private readonly Resetter _resetter;

    public BotManager(Config cfg, AI ai, SlotObservation observation, Resetter resetter)
    {
        _cfg = cfg;
        _ai = ai;
        _observation = observation;
        _resetter = resetter;
    }


    public void HandleBots()
    {
        //Remove bots if there are more than there should be.
        if (NumBots > _cfg.GetNumTotalBots())
        {
            Console.WriteLine($"Bot count {NumBots} > {_cfg.GetNumTotalBots()}. Removing bots.");
            RemoveBotsIfAny();
        }

        //Remove bots if many players
        int humanCount = _observation.GetPlayerCount() - NumBots;
        if (humanCount >= _cfg.NumberPlayersWhenBotsAreRemoved && NumBots > 0)
        {
            Console.WriteLine($"Human count {humanCount} >= {_cfg.NumberPlayersWhenBotsAreRemoved}. Removing bots.");
            RemoveBotsIfAny();
        }

        //Add bots if few players
        if (humanCount < _cfg.NumberPlayersWhenBotsAreRemoved
            && NumBots < _cfg.GetNumTotalBots()
            && _observation.GetTeamsAreBalanced())
        {
            Console.WriteLine($"Human count {humanCount} < {_cfg.NumberPlayersWhenBotsAreRemoved} and BotCount {NumBots} < 4. Adding bots.");
            foreach (var bot in _cfg.Bots)
            {
                _ai.AddAI(bot.Item1, bot.Item2, Team.Blue, 1);
                _ai.AddAI(bot.Item1, bot.Item2, Team.Red, 1);
            }
            NumBots += _cfg.GetNumTotalBots();
        }
        _resetter.Reset();
    }

    public void RemoveBotsIfAny()
    {
        if (NumBots > 0)
        {
            RemoveBots();
        }
    }

    public void RemoveBots()
    {
        _ai.RemoveAllBotsAuto();
        NumBots = 0;
        Console.WriteLine("All bots removed.");
        _resetter.Reset();
    }
}





public class SlotObservation
{
    private CustomGame _cg;

    public SlotObservation(CustomGame cg)
    {
        _cg = cg;
    }

    public int GetPlayerCount()
    {
        return _cg.PlayerCount;
    }

    public List<int> GetRedSlots()
    {
        return _cg.RedSlots;
    }

    public List<int> GetBlueSlots()
    {
        return _cg.BlueSlots;
    }

    public List<int> EmptyRedSlots()
    {
        List<int> emptySlots = new List<int>();
        for (int i = 6; i < 12; i++)
        {
            if (!_cg.RedSlots.Contains(i))
            {
                emptySlots.Add(i);
            }
        }

        return emptySlots;
    }

    public List<int> EmptyBlueSlots()
    {
        List<int> emptySlots = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            if (!_cg.BlueSlots.Contains(i))
            {
                emptySlots.Add(i);
            }
        }

        return emptySlots;
    }

    public int GetBlueTeamSizeAdvantage()
    {
        return _cg.BlueCount - _cg.RedCount;
    }

    public int GetImbalanceAmount()
    {
        return Math.Abs(GetBlueTeamSizeAdvantage());
    }

    public bool GetTeamsAreBalanced()
    {
        return GetImbalanceAmount() < 2;
    }


}


public class SlotManipulation
{
    private readonly SlotObservation _observation;
    private readonly Interact _interact;

    public SlotManipulation(SlotObservation observation, Interact interact)
    {
        _observation = observation;
        _interact = interact;
    }

    public void SwapWithEmpty(int slot, Team toSwapTo)
    {
        List<int> empties;
        if (toSwapTo == Team.Blue)
        {
            empties = _observation.EmptyBlueSlots();
        }
        else if (toSwapTo == Team.Red)
        {
            empties = _observation.EmptyRedSlots();
        }
        else
        {
            throw new ArgumentException("toSwapTo must be Red or Blue");
        }

        if (empties.Count > 0)
        {
            int lastEmpty = empties[empties.Count - 1];
            _interact.Move(slot, lastEmpty);
        }
    }

    public void SwapToBalance()
    {
        int blueTeamSizeAdvantage = _observation.GetBlueTeamSizeAdvantage();
        if (!_observation.GetTeamsAreBalanced())
        {
            List<int> slots;

            Team emptyTeam;
            if (blueTeamSizeAdvantage > 0)
            {
                slots = _observation.GetBlueSlots();
                emptyTeam = Team.Red;
            }
            else
            {
                slots = _observation.GetRedSlots();
                emptyTeam = Team.Blue;
            }

            Random rnd = new Random();
            int randomSlot = slots[rnd.Next(slots.Count)];
            SwapWithEmpty(randomSlot, emptyTeam);
        }
    }

}



public class Autobalancer
{
    private DateTime _autobalanceStartTime = DateTime.Parse("2017-02-16T00:00:00-0:00");
    private int _blueTeamSizeAdvantage = 0;
//    private int _prevBlueTeamSizeAdvantage = 0;
    private bool _autobalance = false;

    private readonly CustomGame _cg;
    private readonly SlotObservation _observation;
    private readonly SlotManipulation _manipulation;
    private readonly BotManager _bots;

    public Autobalancer(CustomGame cg, SlotObservation observation, BotManager bots, SlotManipulation manipulation)
    {
        _cg = cg;
        _observation = observation;
        _bots = bots;
        _manipulation = manipulation;
    }

    public void HandleAutoBalance()
    {
//        _prevBlueTeamSizeAdvantage = _blueTeamSizeAdvantage;
        _blueTeamSizeAdvantage = _observation.GetBlueTeamSizeAdvantage();
        if (_observation.GetTeamsAreBalanced())
        {
            EndAutoBalance();
        }
        else if (_observation.GetImbalanceAmount() == 2)
        {
            Console.WriteLine($"blue team size advantage {_cg.BlueCount} vs {_cg.RedCount}");
            BeginAutoBalance();
        }
        else if (_observation.GetImbalanceAmount() >= 3 || _cg.BlueCount == 0 || _cg.RedCount == 0)
        {
            Console.WriteLine($"blue team size advantage {_cg.BlueCount} vs {_cg.RedCount}");
            BeginAutoBalance();
        }
    }

    public void PerformAutoBalance()
    {
        _blueTeamSizeAdvantage = _observation.GetBlueTeamSizeAdvantage();
        if (_autobalance)
        {
            if (_observation.GetTeamsAreBalanced())
            {
                EndAutoBalance();
            }
            else
            {
                //Wait for death
                if (DateTime.Now.Subtract(_autobalanceStartTime).TotalSeconds < 15)
                {
                    List<int> slots;
                    List<int> empties;
                    if (_blueTeamSizeAdvantage > 0)
                    {
                        slots = _cg.BlueSlots;
                        empties = _observation.EmptyRedSlots();
                    }
                    else
                    {
                        slots = _cg.RedSlots;
                        empties = _observation.EmptyBlueSlots();
                    }


                    List<int> dead = _cg.PlayerInfo.GetDeadSlots();
                    foreach (int deadSlot in dead)
                    {
                        if (slots.Contains(deadSlot))
                        {
                            if (empties.Count > 0)
                            {
                                int lastEmpty = empties[empties.Count - 1];
                                _cg.Interact.Move(deadSlot, lastEmpty);
                            }
                        }
                    }
                }
                else
                {
                    //Swap anyone
                    _manipulation.SwapToBalance();
                }
            }
        }
    }

    private void BeginAutoBalance()
    {
        if (!_autobalance)
        {
            _autobalanceStartTime = DateTime.Now;
            _cg.Chat.SendChatMessage(">>Team size imbalance detected. Swapping someone from the larger team.");
            _autobalance = true;
            _bots.RemoveBotsIfAny();
        }
    }

    private void EndAutoBalance()
    {
        if (_autobalance)
        {
            _cg.Chat.SendChatMessage(">>Team sizes balanced.");
            _autobalance = false;
        }
    }


}


public class Resetter
{
    private CustomGame _cg;

    public Resetter(CustomGame cg)
    {
        _cg = cg;
    }

    public void Reset()
    {
        _cg.Reset();
    }
}

public class GameManager
{
    public CustomGame Cg;
    public Config Cfg;
    public BotManager Bots;
    public SlotObservation Observation;
    public SlotManipulation Manipulation;
    public TeamScrambler Scrambler;
    public Autobalancer Autobalancer;
    public Chatter Chatter;
    public MatchTracker Match;

    public GameManager(CustomGame cg, Config cfg)
    {
        Cg = cg;
        Cfg = cfg;

        DebugUtils.Debug = Cfg.Debug;
        DebugUtils.Cg = Cg;

        Observation = new SlotObservation(cg);
        Manipulation = new SlotManipulation(Observation, Cg.Interact);

        Resetter resetter = new Resetter(Cg);

        Bots = new BotManager(cfg, cg.AI, Observation, resetter);
        Scrambler = new TeamScrambler(Cg, Cfg, Bots, Observation, Manipulation);
        Autobalancer = new Autobalancer(Cg, Observation, Bots, Manipulation);

        Chatter = new Chatter(Cg.Chat, Cfg, Observation, Bots);
        Match = new MatchTracker(Chatter, Cg);
    }

    public void PrintRunningTrace()
    {
        //        Console.WriteLine("...Running...");
        if (Cfg.Debug)
        {
            Console.WriteLine($"DEBUG: Blue size: {Cg.BlueCount}, Red size {Cg.RedCount}");
        }
    }
}