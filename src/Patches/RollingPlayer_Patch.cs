using System.IO;
using Audio;
using HarmonyLib;
using UnityEngine;

namespace painter.Patches;

//all rolling stock have this component
[HarmonyPatch(typeof(RollingPlayer))]
[HarmonyPatch(nameof(RollingPlayer.OnEnable))]
public class RollingPlayer_Patch
{
	private static void Postfix(ref RollingPlayer __instance)
	{
		var modelID = __instance.Car.Definition.ModelIdentifier;
		
		var foundSkin = SkinFinder.GetRandomSkin(out Skin skin, modelID);
		if (!foundSkin)
		{
			Main.Debug($"no suitable skin found for {modelID}");
			return;
		}

		foreach (var meshRenderer in __instance.transform.GetComponentsInChildren<MeshRenderer>())
		{
			for (var i = 0; i < meshRenderer.materials.Length; i++)
			{
				ApplySkinToMaterial(ref meshRenderer.materials[i], skin);
			}
		}
	}

	private static void ApplySkinToMaterial(ref Material material, Skin theSkin)
	{
		if (material.mainTexture == null)
		{
			return;
		}

		foreach (var texture in theSkin.Textures)
		{
			if (texture.name != material.mainTexture.name && texture.name != "everything") continue;
			
			material.mainTexture = texture;
			Main.Info($"Applied skin {theSkin.Name}");
			return;
		}
	}
}

