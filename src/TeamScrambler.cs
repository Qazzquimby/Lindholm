using System;
using System.Collections.Generic;
using System.Threading;
using Deltin.CustomGameAutomation;

public class TeamScrambler
{
    private Config _cfg;
    private CustomGame _cg;
    private BotManager _bots;
    private SlotObservation _observation;
    private SlotManipulation _manipulation;

    public TeamScrambler(CustomGame cg, Config cfg, BotManager bots, SlotObservation observation)
    {
        _cg = cg;
        _cfg = cfg;
        _bots = bots;
        _observation = observation;
    }

    public void ScrambleTeams()
    {
        Thread.Sleep(2000);
        _bots.RemoveBots();
        Thread.Sleep(2000);
        List<int> greaterTeamSlots;
        List<int> smallerTeamSlots;
        Team smallerTeam;
        if (_observation.GetBlueTeamSizeAdvantage() > 0)
        {
            greaterTeamSlots = _cg.BlueSlots;
            smallerTeamSlots = _cg.RedSlots;
            smallerTeam = Team.Red;
        }
        else
        {
            greaterTeamSlots = _cg.RedSlots;
            smallerTeamSlots = _cg.BlueSlots;
            smallerTeam = Team.Blue;
        }

        ScrambleEvenPortionsOfTeams(smallerTeamSlots, greaterTeamSlots);
        Thread.Sleep(3000);
        ScrambleUnevenPortionsOfTeams(smallerTeamSlots, greaterTeamSlots, smallerTeam);
    }

    private void ScrambleEvenPortionsOfTeams(List<int> smallerTeamSlots, List<int> greaterTeamSlots)
    {
        //Half of the number of slots that are present on red and on blue, round up.
        int numToSwap = (smallerTeamSlots.Count + 1) / 2;

        for (int i = 0; i < numToSwap; i++)
        {
            try
            {
                Random rnd = new Random();
                int smallerSlotToSwapIndex = rnd.Next(smallerTeamSlots.Count);
                int greaterSlotToSwapIndex = rnd.Next(greaterTeamSlots.Count);
                int smallerSlotToSwap = smallerTeamSlots[smallerSlotToSwapIndex];
                int greaterSlotToSwap = greaterTeamSlots[greaterSlotToSwapIndex];
                _cg.Interact.Move(smallerSlotToSwap, greaterSlotToSwap);

                smallerTeamSlots.RemoveAt(smallerSlotToSwapIndex);
                greaterTeamSlots.RemoveAt(greaterSlotToSwapIndex);
            }
            catch (ArgumentOutOfRangeException)
            {
                if (_cfg.Debug)
                {
                    Console.WriteLine($"DEBUG: Index error in scramble even portion of teams.");
                }
            }
        }
    }

    private void ScrambleUnevenPortionsOfTeams(List<int> smallerTeamSlots, List<int> greaterTeamSlots, Team smallerTeam)
    {
        //Half of the number of slots present that are present on only the larger team.
        int numToSwap = (greaterTeamSlots.Count - smallerTeamSlots.Count - 1) / 2 + 1; // Rounds up.

        for (int i = 0; i < numToSwap; i++)
        {
            try
            {
                Random rnd = new Random();
                int slotToSwapIndex = rnd.Next(greaterTeamSlots.Count);
                int slotToSwap = greaterTeamSlots[slotToSwapIndex];
                _manipulation.SwapWithEmpty(slotToSwap, smallerTeam);

                greaterTeamSlots.RemoveAt(slotToSwapIndex);
            }
            catch (ArgumentOutOfRangeException)
            {
                if (_cfg.Debug)
                {
                    Console.WriteLine($"DEBUG: Index error in scramble uneven portion of teams.");
                }
            }
        }
    }

    public void SwapIfImbalance()
    {
        int blueTeamSizeAdvantage = _observation.GetBlueTeamSizeAdvantage();
        if (Math.Abs(blueTeamSizeAdvantage) >= 2)
        {
            Console.WriteLine("Evening teams");
            _manipulation.SwapToBalance();
        }
    }
}