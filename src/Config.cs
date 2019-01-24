﻿using System;
using System.Collections.Generic;
using System.IO;
using Deltin.CustomGameAutomation;
using YamlDotNet.RepresentationModel;

public class Config
{
    public readonly string BattlenetExecutableFilePath;
    public readonly string BlueName;

    public readonly List<Tuple<AIHero, Difficulty>> Bots;

    public readonly string EndMessage1st;
    public readonly string EndMessage2nd;

    public readonly int NumberPlayersWhenBotsAreRemoved;
    public readonly string OverwatchSettingsFilePath;

    public readonly string PresetName;
    public readonly string RedName;
    public readonly string ServerName;

    private YamlMappingNode _mapping;

    public Config()
    {
        InitMapping();
        BattlenetExecutableFilePath = InitField("BattlenetExecutableFilePath");
        OverwatchSettingsFilePath = InitFieldOrFallback("OverwatchSettingsFilePath", @"C:\Users\" + Environment.UserName + @"\Documents\Overwatch\Settings\Settings_v0.ini");
        ServerName = InitField("ServerName");
        BlueName = InitField("BlueName");
        RedName = InitField("RedName");
        EndMessage1st = InitField("EndMessage1st");
        EndMessage2nd = InitField("EndMessage2nd");

        NumberPlayersWhenBotsAreRemoved = int.Parse(InitField("NumberPlayersWhenBotsAreRemoved"));

        PresetName = InitField("PresetName");
        if (PresetName == "REPLACE ME WITH THE NAME OF YOUR PRESET")
        {
            Console.WriteLine("!!! You must set the value of PresetName in cfg.yaml for Lindholm to load your preset.");
        }

        Bots = InitBots();
        if (GetNumTotalBots() + NumberPlayersWhenBotsAreRemoved > 12)
        {
            throw new BotsMayNotAllFitException(GetNumTotalBots(), NumberPlayersWhenBotsAreRemoved);
        }
    }

    public int GetNumTotalBots()
    {
        return Bots.Count * 2;
    }

    private void InitMapping()
    {
        string cfgString = File.ReadAllText("cfg.yaml");
        StringReader cfgStreamReader = new StringReader(cfgString);

        YamlStream yaml = new YamlStream();
        yaml.Load(cfgStreamReader);

        // Examine the stream
        _mapping = (YamlMappingNode) yaml.Documents[0].RootNode;
    }

    private string InitFieldOrFallback(string fieldName, string fallback)
    {
        try
        {
            return InitField(fieldName);
        }
        catch (KeyNotFoundException)
        {
            return fallback;
        }
    }

    private string InitField(string fieldName)
    {
        return (string) _mapping.Children[new YamlScalarNode(fieldName)];
    }

    private List<Tuple<AIHero, Difficulty>> InitBots()
    {
        List<Tuple<AIHero, Difficulty>> bots = new List<Tuple<AIHero, Difficulty>>();

        var botsNode = (YamlSequenceNode) _mapping.Children[new YamlScalarNode("Bots")];
        foreach (YamlMappingNode bot in botsNode)
        {
            string heroString = (string) bot.Children[new YamlScalarNode("Hero")];
            AIHero hero = StringToHero(heroString);

            string difficultyString = (string) bot.Children[new YamlScalarNode("Difficulty")];
            Difficulty difficulty = StringToDifficulty(difficultyString);

            Tuple<AIHero, Difficulty> botTuple = new Tuple<AIHero, Difficulty>(hero, difficulty);
            bots.Add(botTuple);
        }

        return bots;
    }

    private static AIHero StringToHero(string heroString)
    {
        switch (heroString)
        {
            case "Ana":
                return AIHero.Ana;
            case "Bastion":
                return AIHero.Bastion;
            case "Lucio":
                return AIHero.Lucio;
            case "McCree":
                return AIHero.McCree;
            case "Mei":
                return AIHero.Mei;
            case "Reaper":
                return AIHero.Reaper;
            case "Recommended":
                return AIHero.Recommended;
            case "Roadhog":
                return AIHero.Roadhog;
            case "Soldier76":
                return AIHero.Soldier76;
            case "Sombra":
                return AIHero.Sombra;
            case "Torbjorn":
                return AIHero.Torbjorn;
            case "Zarya":
                return AIHero.Zarya;
            case "Zenyatta":
                return AIHero.Zenyatta;
            default:
                throw new BadHeroNameException(heroString);
        }
    }

    private static Difficulty StringToDifficulty(string difficultyString)
    {
        switch (difficultyString)
        {
            case "Easy":
                return Difficulty.Easy;
            case "Medium":
                return Difficulty.Medium;
            case "Hard":
                return Difficulty.Hard;
            default:
                throw new BadDifficultyNameException(difficultyString);
        }
    }
}

public class BadHeroNameException : Exception
{
    public BadHeroNameException(string name) : base(
        $"{name} is not a valid bot hero name. See the config file for a list of valid bot hero names.")
    {
    }
}

public class BadDifficultyNameException : Exception
{
    public BadDifficultyNameException(string name) : base(
        $"{name} is not a valid bot difficulty name. See the config file for a list of valid bot difficulty names.")
    {
    }
}

public class BotsMayNotAllFitException : Exception
{
    public BotsMayNotAllFitException(int numBots, int numPlayersToRemoveBots) : base(
        $"{numBots} bots are requested. Bots are removed when there are {numPlayersToRemoveBots} players. " +
        $"{numBots} + {numPlayersToRemoveBots} is greater than the total number of slots (12)." +
        $"Note that bots are added to both teams."
    ){}
}