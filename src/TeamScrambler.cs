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

        // Swap pairs of players in order of listing
        for (var i = 0; i < playerCount - playerCount % 2; i += 2)
        {
            int player1 = distribution[i + 0].Item2;
            int player2 = distribution[i + 1].Item2;

            bool bothRed = player1 < redCount && player2 < redCount;
            bool bothBlue = player1 >= redCount && player2 >= redCount;
            if (bothRed || bothBlue)
                continue;

            _cg.Interact.Move(player1, player2);
        }

        // teams were already balanced
        if (redCount == blueCount)
            return;

        // exchange the last player to the team with less players
        int oddPlayer = distribution[playerCount - 1].Item2;
        Team otherTeam;
        if (redCount > blueCount)
            otherTeam = Team.Blue;
        else
            otherTeam = Team.Red;

        _manipulation.SwapWithEmpty(oddPlayer, otherTeam);
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