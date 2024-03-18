using System.Collections.Generic;
using System.IO;
using System.Linq;
using Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace painter;

public static class Dumper
{
	public static void DumpTextures(string dumpFolder)
	{
		Directory.CreateDirectory(dumpFolder);

		int countLoaded = SceneManager.sceneCount;
		var loadedScenes = new Scene[countLoaded];
 
		for (int i = 0; i < countLoaded; i++)
		{
			loadedScenes[i] = SceneManager.GetSceneAt(i);
		}
		
		// SceneManager.GetAllScenes is deprecated but the suggested implementation is exactly the same as that method...
#pragma warning disable CS0618
		
		var rollingPlayers = 
			SceneManager.GetAllScenes()
				.SelectMany(scene => scene
					.GetRootGameObjects()
					.SelectMany(obj => obj
						.GetComponentsInChildren<RollingPlayer>()
					))
				.ToList();
		
#pragma warning restore CS0618

		List<string> exportedIDs = new();
		
		foreach (var roll in rollingPlayers)
		{
			var identifier = roll.Car.Definition.ModelIdentifier;
			if (exportedIDs.Contains(identifier))
			{
				continue;
			}
			
			var stockFolder = Path.Combine(dumpFolder, identifier);
			Directory.CreateDirectory(stockFolder);

			foreach (var meshRenderer in roll.GetComponentsInChildren<MeshRenderer>())
			{
				foreach (var material in meshRenderer.materials)
				{
					if (material.mainTexture == null || material.mainTexture.GetType() != typeof(Texture2D))
					{
						continue;
					}
					
					var tex = (Texture2D)material.mainTexture;
					var bytes = ImageConversion.EncodeArrayToPNG(tex.GetRawTextureData(), tex.graphicsFormat, (uint)tex.width, (uint)tex.height);
					File.WriteAllBytes(Path.Combine(stockFolder, tex.name + ".png"), bytes);
				}
			}
			
			exportedIDs.Add(identifier);
		}
	}
}