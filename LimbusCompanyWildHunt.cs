using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;

namespace LimbusCompanyWildHunt
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class LimbusCompanyWildHunt : Mod
	{
		
		public static Effect appearEffect;
		public static List<Texture2D> upperSlash;
		public static List<Texture2D> lowerSlash;
		public static List<Texture2D> pierce;
		public static Texture2D normalTexture;
		public static Texture2D blackedTexture;
		public List<Texture2D> Pierce;
		public static Texture2D ChainIdleTexture;
		public static Texture2D CoffinIdleTexture;
		//load sprites
		public override void Load()
		{
			// appearEffect = ModContent.Request<Effect>("LimbusCompanyWildHunt/Effects/Content/appear", AssetRequestMode.ImmediateLoad).Value;
			// normalTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/WildHunt_Weapon", AssetRequestMode.ImmediateLoad).Value;
			
			// Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++");
			// upperSlash = Helper.loadVfxFolder("UpperSlash/", 578, 594);
			// upperSlash = loadVfxFolder("Pierce/", 369, 387);
			// appearEffect = ModContent.Request<Effect>(
			// 	"LimbusCompanyWildHunt/Content/Effects/Content/appear", 
			// 	AssetRequestMode.ImmediateLoad).Value;
			// GetEffect("Effects/GrayScale");

			ChainIdleTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/leftChain", AssetRequestMode.ImmediateLoad).Value;
			CoffinIdleTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/BackSlot_Coffin", AssetRequestMode.ImmediateLoad).Value;

			Console.WriteLine("loading vfx");
		}

        // public static List<Texture2D> loadVfxFolder(string folderName, int startingFrame, int endingFrame)
        // {
		// 	Console.WriteLine("loading vfx");
        //     List<Texture2D> textureVector = new List<Texture2D>();

		// 	string currentDirectory = resDir + "Projectiles/Vfx/" + folderName;

		// 	for(int idx = startingFrame; idx <= endingFrame; idx++)
		// 	{
		// 		textureVector.Add(ModContent.Request<Texture2D>(currentDirectory + $"frame{idx}").Value);
		// 		Console.WriteLine(currentDirectory + $"frame{idx}");
		// 	}
        //     return textureVector;
        // }
		public override void Unload()
        {
            // appearEffect = null;
        }
	}
}
