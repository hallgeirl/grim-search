using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GrimSearch.Utils.CharacterFiles;

public static class CharacterLoader
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static List<CharacterFile> LoadAllCharacters(string grimDawnSavesDirectory, Action<string> stateChangeCallback, string formulasFilename)
    {
        stateChangeCallback("Clearing index");
        var characters = new List<CharacterFile>();

        var charactersDirectory = Path.Combine(grimDawnSavesDirectory, "main");
        if (!System.IO.Directory.Exists(charactersDirectory))
            throw new InvalidOperationException("Saves directory not found: " + charactersDirectory);

        var directories = System.IO.Directory.EnumerateDirectories(charactersDirectory, "*", SearchOption.TopDirectoryOnly).OrderBy(x => x);

        foreach (var d in directories)
        {
            //Skip backup characters
            if (Path.GetFileName(d).StartsWith("__"))
            {
                Logger.Info("Skipping backup character: {Path}", d);
                continue;
            }

            var characterFile = Path.Combine(d, "player.gdc");
            if (!File.Exists(characterFile))
            {
                Logger.Info("No character file: {Path}", characterFile);
                continue;
            }

            stateChangeCallback("Loading " + characterFile);

            var character = new CharacterFile();
            try
            {
                using (var s = File.OpenRead(characterFile))
                {
                    character.Read(s);
                }
                characters.Add(character);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Skipping unreadable character: {Path}", characterFile);
            }
        }

        LoadTransferStashAsCharacter(grimDawnSavesDirectory, stateChangeCallback, characters, "transfer.gst", "reagents.gst", false);
        LoadTransferStashAsCharacter(grimDawnSavesDirectory, stateChangeCallback, characters, "transfer.gsh", "reagents.gsh", true);
        LoadBlueprintsAsCharacter(grimDawnSavesDirectory, stateChangeCallback, characters, formulasFilename);
        return characters;
    }

    private static void LoadTransferStashAsCharacter(string grimDawnSavesDirectory, Action<string> stateChangeCallback, List<CharacterFile> characters, string transferFilename, string reagentFilename, bool isHardcore)
    {
        var transferStashFile = Path.Combine(grimDawnSavesDirectory, transferFilename);
        if (!File.Exists(transferStashFile))
        {
            Logger.Info("No transfer stash: {Path}", transferStashFile);
            return;
        }

        stateChangeCallback("Loading " + transferStashFile);
        var transferStash = new TransferStashFile();
        using (var s = File.OpenRead(transferStashFile))
        {
            transferStash.Read(s);
        }

        var reagentStashFile = Path.Combine(grimDawnSavesDirectory, reagentFilename);
        if (File.Exists(reagentStashFile))
        {
            stateChangeCallback("Loading " + reagentStashFile);
            var reagentStash = new ReagentStashFile();
            using (var s = File.OpenRead(reagentStashFile))
            {
                reagentStash.Read(s);
            }
            transferStash.sacks.Add(reagentStash.ToStashPage());
        }
        else
        {
            Logger.Info("No reagent stash: {Path}", reagentStashFile);
        }

        characters.Add(transferStash.ToCharacterFile(isHardcore ? "Hardcore transfer stash" : "Transfer stash", isHardcore));
    }

    private static void LoadBlueprintsAsCharacter(string grimDawnSavesDirectory, Action<string> stateChangeCallback, List<CharacterFile> characters, string formulasFilename)
    {
        var recipesFilePath = Path.Combine(grimDawnSavesDirectory, formulasFilename);
        stateChangeCallback("Loading " + recipesFilePath);
        var recipes = new BlueprintFile();
        using (var s = File.OpenRead(recipesFilePath))
        {
            recipes.Read(s);
        }

        characters.Add(recipes.ToCharacterFile());
    }
}
