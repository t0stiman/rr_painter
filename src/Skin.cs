using System;
using System.Collections.Generic;
using UnityEngine;

namespace painter;

[Serializable]
public class Skin
{
	public string Name;
	public string ModelID;
	public List<Texture2D> Textures;
}