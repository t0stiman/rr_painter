using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace painter;

public static class SkinFinder
{
	private const string SKIN_INFO_FILE = "skin.json";
	private static List<Skin> loadedSkins = new();
	private static Random random = new();
	
	public static void Setup()
	{
		// todo is this necessary?
		// foreach (var entry in UnityModManager.modEntries)
		// {
		// 	FindSkinsInMod(entry);
		// }
		
		UnityModManager.toggleModsListen += ToggleModsListen;
	}

	private static void ToggleModsListen(UnityModManager.ModEntry aModEntry, bool modEnabled)
	{
		if (modEnabled)
		{
			Main.Info($"New mod enabled ({aModEntry.Info.DisplayName}), checking for skins...");
			FindSkinsInMod(aModEntry);
		}
	}
	
	private static void FindSkinsInMod(UnityModManager.ModEntry aModEntry)
	{
		// search the subdirectories of the mod directory
		foreach (string modSubDirectory in Directory.GetDirectories(aModEntry.Path))
		{
			var skinInfoPath = Path.Combine(modSubDirectory, SKIN_INFO_FILE);
			if (!File.Exists(skinInfoPath)) {
				continue;
			}

			try
			{
				AddSkin(modSubDirectory, skinInfoPath);
			}
			catch (Exception anException)
			{
				Main.Error($"Error while loading skin from {modSubDirectory}: {anException}");
			}
		}
	}

	private static void AddSkin(string skinDirectory, string skinInfoPath)
	{
		Main.Info($"Trying to add skin from {skinDirectory}");
		
		var skin = JsonUtility.FromJson<Skin>(File.ReadAllText(skinInfoPath));

		foreach (var loadedSkin in loadedSkins)
		{
			if (skin.Name == loadedSkin.Name &&
			    skin.ModelID == loadedSkin.ModelID)
			{
				Main.Info($"skipping skin {skin.Name} from {skinDirectory} due to duplicate skin {loadedSkin.Name} already loaded");
				return;
			}
		}

		if (!LoadSkin(skin, skinDirectory))
		{
			Main.Error($"Failed to load skin from '{skinDirectory}'");
			return;
		}

		Main.Info($"Loaded skin '{skin.Name}' from '{skinDirectory}'");
			
		loadedSkins.Add(skin);
	}

	private static bool LoadSkin(Skin aSkin, string skinDirectory)
	{
		if (aSkin.Name == "" ||
		    aSkin.ModelID == "")
		{
			Main.Error($"Invalid {SKIN_INFO_FILE} at {skinDirectory}");
			return false;
		}
		
		var texturePaths = Directory.EnumerateFiles(skinDirectory)
			.Where(name => name.EndsWith(".png") || name.EndsWith(".jpg") || name.EndsWith(".dds"))
			.ToList();

		if (texturePaths.Count == 0)
		{
			Main.Error($"Skin at {skinDirectory} has no textures");
			return false;
		}

		aSkin.Textures = new List<Texture2D>();

		for (var i = 0; i < texturePaths.Count; i++)
		{
			var newTexture = new Texture2D(2, 2);
			var loadedSuccessfully = newTexture.LoadImage(File.ReadAllBytes(texturePaths[i]));
			if (!loadedSuccessfully)
			{
				Main.Error($"image at {texturePaths[i]} failed to load");
				Object.Destroy(newTexture);
				return false;
			}
			newTexture.name = Path.GetFileNameWithoutExtension(texturePaths[i]);
			aSkin.Textures.Add(newTexture);
		}
		
		return true;
	}

	public static bool GetRandomSkin(out Skin outSkin, string aModelID)
	{
		var suitableSkins = loadedSkins
			.Where(skin => 
				skin.ModelID == aModelID || 
				skin.ModelID == "everything")
			.ToList();

		if (suitableSkins.Count == 0)
		{
			Main.Debug($"model {aModelID} has no skins");
			outSkin = new Skin();
			return false;
		}
		
		//return a random suitable skin
		var i = random.Next(suitableSkins.Count);
		outSkin = suitableSkins[i];
		return true;
	}
}