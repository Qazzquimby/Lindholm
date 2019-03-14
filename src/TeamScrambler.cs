using System;
using System.Collections.Generic;
using System.Threading;
using Deltin.CustomGameAutomation;

public class TeamScrambler
{
    private readonly BotManager _bots;
    private readonly Config _cfg;
    private readonly CustomGame _cg;
    private readonly SlotManipulation _manipulation;
    private readonly SlotObservation _observation;

    public TeamScrambler(CustomGame cg, Config cfg, BotManager bots, SlotObservation observation,
        SlotManipulation manipulation)
    {
        _cg = cg;
        _cfg = cfg;
        _bots = bots;
        _observation = observation;
        _manipulation = manipulation;
    }

    public void ScrambleTeams()
    {
        Thread.Sleep(5000);
        _bots.RemoveBots();
        Thread.Sleep(2000);

        int redCount  = _cg.RedCount;
        int blueCount = _cg.BlueCount;
        int playerCount = redCount + blueCount;

        // List the players randomly
        var distribution = new List<Tuple<float, int>>(playerCount);
        Random rnd = new Random();
        for (var player = 0; player < playerCount; player++)
        {
            distribution[player] = new Tuple<float, int>(rnd.Next(), player);
        }
        distribution.Sort(Comparer<Tuple<float, int>>.Default);

        // First half is team red, second half is team blue
        var swapToRed = new Stack<int>();
        var swapToBlue = new Stack<int>();
        int halfCount = (playerCount - playerCount % 2) / 2;
        for (var i = 0; i < playerCount - playerCount % 2; i++)
        {
            int player = distribution[i].Item2;
            if (i < halfCount)
            { // Team Red
                if (player >= redCount)
                    swapToRed.Push(player);
            }
            else
            { // Team Blue
                if (player < redCount)
                    swapToBlue.Push(player);
            }
        }

        // Swap players between teams
        while (swapToRed.Count > 1 && swapToBlue.Count > 1)
        {
            _cg.Interact.Move(swapToRed.Pop(), swapToBlue.Pop());
        }

        // Swap to team Red first to keep Red player's indexes in order
        while (swapToRed.Count > 1)
            _manipulation.SwapWithEmpty(swapToRed.Pop(), Team.Red);

        // Swap to team Blue starting with the largest indexed Red player to preserve ordering
        List<int> swapToBlueList = new List<int>(swapToBlue.ToArray());
        swapToBlueList.Sort((a, b) => -1*a.CompareTo(b));
        while (swapToBlueList.Count > 1)
        {
            int index = swapToBlue.Count - 1;
            int player = swapToBlueList[index];
            swapToBlueList.RemoveAt(index);
            _manipulation.SwapWithEmpty(player, Team.Blue);
        }

        // Check if teams were balanced
        if (playerCount % 2 == 0)
            return;

        // Exchange the odd player out on the list to the team previously with less players
        int oddPlayer = distribution[playerCount - 1].Item2;
        Team smallerTeam;
        if (redCount > blueCount)
            smallerTeam = Team.Blue;
        else
            smallerTeam = Team.Red;

        _manipulation.SwapWithEmpty(oddPlayer, smallerTeam);
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