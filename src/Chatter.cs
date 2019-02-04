using Deltin.CustomGameAutomation;

public class Chatter
{
    private Config _cfg;
    private Chat _chat;

    private SlotObservation _observation;
    private BotManager _bots;

    public Chatter(Chat chat, Config cfg, SlotObservation observation, BotManager bots)
    {
        _chat = chat;
        _cfg = cfg;
        _observation = observation;
        _bots = bots;
    }

    public void SendChatMessage(string message)
    {
        _chat.SendChatMessage(message);
    }

    public void JoinMatchChat()
    {
        _chat.JoinChannel(Channel.Match);
    }

    public void ChatStartMessages()
    {
        _chat.SendChatMessage(_cfg.StartMessage1st);
        _chat.SendChatMessage(_cfg.StartMessage2nd);
    }

    public void ChatEndMessages()
    {
        _chat.SendChatMessage(_cfg.EndMessage1st);
        _chat.SendChatMessage(_cfg.EndMessage2nd);
    }

    public void ChatFewPlayersMessageIfFewPlayers()
    {
        if (_observation.GetPlayerCount() - _bots.NumBots < 3)
        {
            ChatFewerPlayersMessage();
        }
    }

    public void ChatFewerPlayersMessage()
    {
        _chat.SendChatMessage(_cfg.FewPlayersMessage);
    }
}