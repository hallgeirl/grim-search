using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GrimSearch.Utils.CharacterFiles;

public static class CharacterLoader
{
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
                continue;

            var characterFile = Path.Combine(d, "player.gdc");
            if (!File.Exists(characterFile))
                continue;

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
                Trace.TraceError(ex.ToString());
            }
        }

        LoadTransferStashAsCharacter(grimDawnSavesDirectory, stateChangeCallback, characters);
        LoadBlueprintsAsCharacter(grimDawnSavesDirectory, stateChangeCallback, characters, formulasFilename);
        return characters;
    }

    private static void LoadTransferStashAsCharacter(string grimDawnSavesDirectory, Action<string> stateChangeCallback, List<CharacterFile> characters)
    {
        var transferStashFile = Path.Combine(grimDawnSavesDirectory, "transfer.gst");
        stateChangeCallback("Loading " + transferStashFile);
        var transferStash = new TransferStashFile();
        using (var s = File.OpenRead(transferStashFile))
        {
            transferStash.Read(s);
        }

        characters.Add(transferStash.ToCharacterFile());
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