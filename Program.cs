﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Deltin.CustomGameAutomation;

public class Program
{
    public static int NumBots = 0;

    public static Phase Phase;
    public static bool GameEnded = false;
    public static double TimeAtCurrentMap = 0;

    private static DateTime _autobalanceStartTime = DateTime.Parse("2017-02-16T00:00:00-0:00");
    private static readonly Random Rnd = new Random();
    private static readonly int ticks_per_second = 2;
    private static int _blueTeamSizeAdvantage = 0;
    private static int _prevBlueTeamSizeAdvantage = 0;
    private static bool _autobalance = false;
    private static DateTime _lastGameOver = DateTime.Parse("2017-02-16T00:00:00-0:00");
    private static GameLoop _loop;

    private static void Main()
    {
        Config cfg = new Config();
        _loop = new GameLoop(cfg);
        _loop.Cg.OnGameOver += SetGameEnded;
        _loop.Start();
    }


    public static void EnterPhase(Type newPhase)
    {
        Phase?.Exit();

        Phase = (Phase) Activator.CreateInstance(newPhase, _loop.Cg, _loop.Cfg);
        Phase.Enter();
    }

    private static void SetGameEnded(object sender, GameOverArgs e)
    {
        double passedSeconds = DateTime.Now.Subtract(_lastGameOver).TotalSeconds;

        if (passedSeconds > 60)
        {
            //Cant gameover twice in quick succession.
            Debug.WriteLine("game ended");
            _lastGameOver = DateTime.Now;

//            if (current_game_log != null)
//            {
//                Debug.WriteLine(string.Format("Winning team is {0}.", e.GetWinningTeam()));
//                current_game_log.winning_team = e.GetWinningTeam();
//            }

            GameEnded = true;
        }
    }

    public static void GameOver()
    {
        GameEnded = false;
        Debug.WriteLine("running game over");

        _loop.Cg.Chat.SendChatMessage(_loop.Cfg.EndMessage1st);
        _loop.Cg.Chat.SendChatMessage(_loop.Cfg.EndMessage2nd);
    }

    public static void PrintRunningTrace()
    {
        Debug.WriteLine("...Running...");
    }

    public static void PreventMapTimeout()
    {
        TimeAtCurrentMap++;
        if (TimeAtCurrentMap > 28 * 60 * ticks_per_second && NumBots > 0)
        {
            _loop.Cg.Chat.SendChatMessage("Sever may timeout soon. Changing map to prevent timeout.");
            RandomAndNextMap();
        }
    }

    public static void RandomAndNextMap()
    {
        Debug.WriteLine("Random next map");
        //map_chooser.SetRandomMap();

        NextMap();
    }

    public static void NextMap()
    {
        Debug.WriteLine("Next map");
        TimeAtCurrentMap = 5;
        _loop.Cg.RestartGame();
        EnterPhase(typeof(First30SecondsPhase));
    }

    public static void ScrambleTeams()
    {
        if (_loop.Cg.PlayerCount > 0)
        {
            _loop.Cg.Chat.SendChatMessage(">>Scrambling teams.");
            SettleBlueTeam();
            SettleRedTeam();

            ShuffleBlueTeam();

            int totalRows = Math.Max(_loop.Cg.RedCount, _loop.Cg.BlueCount);
            int numSwaps = (totalRows + 1) / 2;
            Debug.WriteLine($"num_swaps {numSwaps}.");

            List<int> swappableRows = new List<int>();
            for (int i = 0; i < totalRows; i++)
            {
                swappableRows.Add(i);
            }

            List<int> rowsToSwap = new List<int>();
            for (int i = 0; i < numSwaps; i++)
            {
                int swappableRowIndex = Rnd.Next(swappableRows.Count);
                int row = swappableRows[swappableRowIndex];
                rowsToSwap.Add(row);
                swappableRows.RemoveAt(swappableRowIndex);
            }

            for (int i = 0; i < rowsToSwap.Count; i++)
            {
                int row = rowsToSwap[i];
                _loop.Cg.Interact.Move(row, row + 6);
            }
        }
    }


    private static List<int> EmptyRedSlots()
    {
        List<int> emptySlots = new List<int>();
        for (int i = 6; i < 12; i++)
        {
            if (!_loop.Cg.RedSlots.Contains(i))
            {
                emptySlots.Add(i);
            }
        }

        return emptySlots;
    }

    private static List<int> EmptyBlueSlots()
    {
        List<int> emptySlots = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            if (!_loop.Cg.BlueSlots.Contains(i))
            {
                emptySlots.Add(i);
            }
        }

        return emptySlots;
    }

    private static void SettleBlueTeam()
    {
        Debug.WriteLine("Settling blue team");
        List<int> filledSlots = _loop.Cg.BlueSlots;
        Debug.WriteLine($"blue filled slots {filledSlots}.");
        int swaps = 0;
        for (int i = 0; i < filledSlots.Count; i++)
        {
            if (!filledSlots.Contains(i))
            {
                _loop.Cg.Interact.Move(filledSlots[filledSlots.Count - swaps - 1], i);
                swaps++;
            }
        }
    }

    private static void SettleRedTeam()
    {
        Debug.WriteLine("Settling red team");
        List<int> filledSlots = _loop.Cg.RedSlots;
        Debug.WriteLine($"red filled slots {filledSlots}.");
        for (int i = 0; i < filledSlots.Count; i++)
        {
            if (filledSlots[i] != i + 6)
            {
                _loop.Cg.Interact.Move(filledSlots[i], i + 6);
            }
        }
    }

    private static void ShuffleBlueTeam()
    {
        List<int> slots = _loop.Cg.BlueSlots;
        if (slots.Count > 1)
        {
            Debug.WriteLine($"Shuffling team starting at {slots[0]}");
            for (int i = 0; i < slots.Count - 1; i++)
            {
                int randomIndex = Rnd.Next(i + 1, slots.Count);
                _loop.Cg.Interact.Move(i, randomIndex);
            }
        }
    }

    public static int GetBlueTeamSizeAdvantage()
    {
        return _loop.Cg.BlueCount - _loop.Cg.RedCount;
    }

    public static int GetImbalanceAmount()
    {
        return Math.Abs(GetBlueTeamSizeAdvantage());
    }

    public static bool GetTeamsAreBalanced()
    {
        return GetImbalanceAmount() < 2;
    }

    public static void HandleAutoBalance()
    {
        _prevBlueTeamSizeAdvantage = _blueTeamSizeAdvantage;
        _blueTeamSizeAdvantage = GetBlueTeamSizeAdvantage();
        if (GetTeamsAreBalanced())
        {
            EndAutoBalance();
        }
        else if (GetImbalanceAmount() == 2)
        {
            Debug.WriteLine($"blue team size advantage {_loop.Cg.BlueCount} vs {_loop.Cg.RedCount}");
            BeginAutoBalance();
        }
        else if (GetImbalanceAmount() >= 3)
        {
            Debug.WriteLine($"blue team size advantage {_loop.Cg.BlueCount} vs {_loop.Cg.RedCount}");
            BeginAutoBalance();
        }
    }

    public static void PerformAutoBalance()
    {
        _blueTeamSizeAdvantage = GetBlueTeamSizeAdvantage();
        if (_autobalance)
        {
            if (GetTeamsAreBalanced())
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
                        slots = _loop.Cg.BlueSlots;
                        empties = EmptyRedSlots();
                    }
                    else
                    {
                        slots = _loop.Cg.RedSlots;
                        empties = EmptyBlueSlots();
                    }

                    List<int> dead = _loop.Cg.PlayerInfo.PlayersDead();
                    foreach (int deadSlot in dead)
                    {
                        if (slots.Contains(deadSlot))
                        {
                            if (empties.Count > 0)
                            {
                                int lastEmpty = empties[empties.Count - 1];
                                _loop.Cg.Interact.Move(deadSlot, lastEmpty);
                            }
                        }
                    }
                }
                else
                {
                    //Swap anyone
                    SwapToBalance();
                }
            }
        }
    }

    public static void SwapToBalance()
    {
        _blueTeamSizeAdvantage = GetBlueTeamSizeAdvantage();
        if (!GetTeamsAreBalanced())
        {
            List<int> slots;
            List<int> empties;
            if (_blueTeamSizeAdvantage > 0)
            {
                slots = _loop.Cg.BlueSlots;
                empties = EmptyRedSlots();
            }
            else
            {
                slots = _loop.Cg.RedSlots;
                empties = EmptyBlueSlots();
            }

            int randomSlot = slots[Rnd.Next(slots.Count)];
            if (empties.Count > 0)
            {
                int lastEmpty = empties[empties.Count - 1];
                _loop.Cg.Interact.Move(randomSlot, lastEmpty);
            }
        }
    }


    private static void BeginAutoBalance()
    {
        if (!_autobalance)
        {
            _autobalanceStartTime = DateTime.Now;
            _loop.Cg.Chat.SendChatMessage(">>Team size imbalance detected. Swapping someone from the larger team.");
            _autobalance = true;
            RemoveBotsIfAny();
        }
    }

    private static void EndAutoBalance()
    {
        if (_autobalance)
        {
            _loop.Cg.Chat.SendChatMessage(">>Team sizes balanced.");
            _autobalance = false;
        }
    }

    public static void HandleBots()
    {
        //Remove bots if there are more than there should be.
        if (NumBots > _loop.Cfg.GetNumTotalBots())
        {
            Debug.WriteLine($"Bot count {NumBots} > {_loop.Cfg.GetNumTotalBots()}. Removing bots.");
            RemoveBotsIfAny();
        }

        //Remove bots if many players
        int humanCount = _loop.Cg.PlayerCount - NumBots;
        if (humanCount >= _loop.Cfg.NumberPlayersWhenBotsAreRemoved && NumBots > 0)
        {
            Debug.WriteLine($"Human count {humanCount} >= {_loop.Cfg.NumberPlayersWhenBotsAreRemoved}. Removing bots.");
            RemoveBotsIfAny();
        }

        //Add bots if few players
        if (humanCount < _loop.Cfg.NumberPlayersWhenBotsAreRemoved 
            && NumBots < _loop.Cfg.GetNumTotalBots()
            && GetTeamsAreBalanced())
        {
            Debug.WriteLine($"Human count {humanCount} < {_loop.Cfg.NumberPlayersWhenBotsAreRemoved} and BotCount {NumBots} < 4. Adding bots.");
            foreach (var bot in _loop.Cfg.Bots)
            {
                _loop.Cg.AI.AddAI(bot.Item1, bot.Item2, Team.Blue, 1);
                _loop.Cg.AI.AddAI(bot.Item1, bot.Item2, Team.Red, 1);
            }
            NumBots += _loop.Cfg.GetNumTotalBots();
        }
    }

    

    public static void RemoveBotsIfAny()
    {
        if (NumBots > 0)
        {
            RemoveBots();
        }
    }

    public static void RemoveBots()
    {
        _loop.Cg.AI.RemoveAllBotsAuto();
        NumBots = 0;
        Debug.WriteLine("All bots removed.");
    }
}