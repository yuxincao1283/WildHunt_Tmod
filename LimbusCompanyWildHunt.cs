using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria;
using Terraria.ID;
using Vector2 = Microsoft.Xna.Framework.Vector2;


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
			if(Main.netMode != NetmodeID.Server)
			{
				
				ChainIdleTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/leftChain", AssetRequestMode.ImmediateLoad).Value;
				CoffinIdleTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/BackSlot_Coffin", AssetRequestMode.ImmediateLoad).Value;
				
				Asset<Effect> greyscale_threshold = ModContent.Request<Effect>("LimbusCompanyWildHunt/Effects/Content/greyscale_threshold", AssetRequestMode.ImmediateLoad);
				Asset<Effect> blackfadein_s3 = ModContent.Request<Effect>("LimbusCompanyWildHunt/Effects/Content/blackfadein", AssetRequestMode.ImmediateLoad);
				Asset<Effect> flash_s3 = ModContent.Request<Effect>("LimbusCompanyWildHunt/Effects/Content/flash", AssetRequestMode.ImmediateLoad);

				Filters.Scene["greyscale:s3"] = new Filter(new ScreenShaderData(greyscale_threshold, "skill3_threshold"), EffectPriority.Medium);
				Filters.Scene["greyscale:s3"].Load();

				Filters.Scene["blackfadein:s3"] = new Filter(new ScreenShaderData(blackfadein_s3, "skill3_fadein"), EffectPriority.Medium);
				Filters.Scene["blackfadein:s3"].Load();

				Filters.Scene["flash:s3"] = new Filter(new ScreenShaderData(flash_s3, "skill3_flash"), EffectPriority.Medium);
				Filters.Scene["flash:s3"].Load();
			}
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
