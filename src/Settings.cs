using System.IO;
using UnityEngine;
using UnityModManagerNet;

namespace painter;

public class Settings : UnityModManager.ModSettings
{
	public bool LogToConsole = false;
	public bool EnableDebugLogs = false;
	
	public void Draw(UnityModManager.ModEntry modEntry)
	{
		if(GUILayout.Button("Dump rolling stock textures (will take LONG)"))
		{
			Dumper.DumpTextures(Path.Combine(modEntry.Path, "dump"));
		}
		LogToConsole = GUILayout.Toggle(LogToConsole, "Log messages to the in-game console as well as Player.log");
		EnableDebugLogs = GUILayout.Toggle(EnableDebugLogs, "Enable debug messages");
	}

	private void DrawIntInput(string descriptionText, ref string fieldText, ref int number)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(descriptionText);
		fieldText = GUILayout.TextField(fieldText);
		GUILayout.EndHorizontal();
		
		if (int.TryParse(fieldText, out int parsed))
		{
			number = parsed;
		}
		else
		{
			GUILayout.Label($"not a valid number");
		}
	}

	public override void Save(UnityModManager.ModEntry modEntry)
	{
		Save(this, modEntry);
	}
}
