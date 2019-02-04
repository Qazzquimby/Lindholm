using System;
using System.Threading;
using Deltin.CustomGameAutomation;

public class MatchTracker
{
    private DateTime _lastGameOver = DateTime.Parse("2017-02-16T00:00:00-0:00");

    private readonly Chatter _chat;
    private readonly CustomGame _cg;

    public MatchTracker(Chatter chat, CustomGame cg)
    {
        _chat = chat;
        _cg = cg;

        _cg.OnGameOver += GameEndedCallback;
        _cg.OnRoundOver += RoundOverCallback;
    }

    public double PassedSeconds()
    {
        return DateTime.Now.Subtract(_lastGameOver).TotalSeconds;
    }

    public void HandleMissedGameOver()
    {
        if (_cg.GetGameState() == GameState.Ending_Commend)
        {
            GameOver();
            NextMap();
        }
    }

    public void SkipPostGame()
    {
        int waitTimer = 0;
        int maxWait = 30;
        while (_cg.GetGameState() != GameState.Ending_Commend && waitTimer < maxWait)
        {
            Thread.Sleep(1000);
            waitTimer++;
            Console.WriteLine($"{maxWait - waitTimer} seconds remaining");
        }

        NextMap();
    }

    public void PreventMapTimeout()
    {
        double passedSeconds = PassedSeconds();
        if (passedSeconds > 28*60 && passedSeconds < 24*60*60) //Ignore extremely long times due to time initialization being a long ago time.
        {
            _chat.SendChatMessage("Sever may timeout soon. Changing map to prevent timeout.");
            RandomAndNextMap();
        }
    }

    public void RandomAndNextMap()
    {
        Console.WriteLine("Random next map");
        //map_chooser.SetRandomMap();
        NextMap();
    }

    public void NextMap()
    {
        Console.WriteLine("Next map");
        _cg.RestartGame();
        OnNextMap?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler OnNextMap;

    private void GameEndedCallback(object sender, GameOverArgs e)
    {
        if (PassedSeconds() > 60) //Cant gameover twice in quick succession. Probably false alarm.
        {
            Console.WriteLine("game over detected");
            GameOver();
        }
    }

    public void GameOver()
    {
        _lastGameOver = DateTime.Now;
        Console.WriteLine("running game over");
        _chat.ChatEndMessages();
    }

    private void RoundOverCallback(object sender, EventArgs e)
    {
        var customGame = (CustomGame)sender;
        using (customGame.LockHandler.Interactive)
        {
            Console.WriteLine("Round Over");
            Thread.Sleep(7000);
            Console.WriteLine("New Round");
        }
    }
}