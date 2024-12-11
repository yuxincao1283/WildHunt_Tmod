using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using SteelSeries.GameSense.DeviceZone;
using System.Numerics;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace LimbusCompanyWildHunt
{
    class Helper
    {
        public const string resDir = "LimbusCompanyWildHunt/Content/";
        public struct textureInfo
        {
            public textureInfo(int width, int height, List<Texture2D> tex, float scale=1f)
            {
                X = width;
                Y = height;
                texture = tex;
                Scale = scale;
            }

            public int X { get; }
            public int Y { get; }
            public List<Texture2D> texture {get; }
            public float Scale {get; }
        }

        public static void playSound(string soundName)
        {
			SoundEngine.PlaySound(new SoundStyle(resDir + $"Projectiles/Sound/" + soundName));
        }

        public static List<Texture2D> loadVfxFolder(string folderName, int startingFrame, int endingFrame)
        {
            List<Texture2D> textureVector = new List<Texture2D>();

			string currentDirectory = resDir + "Projectiles/Vfx/" + folderName;

			for(int idx = startingFrame; idx <= endingFrame; idx++)
			{
				textureVector.Add(ModContent.Request<Texture2D>(currentDirectory + $"frame{idx}").Value);

			}
            return textureVector;
        }
        
        //lights
        //Lighting.AddLight(projectile.Center, 1f, 1f, 0f)

        // public static void setHandPos(float frontHandOffset, float offHandOffset, float rotation, Player Owner, int spriteDirection)
        // {
        //     if(spriteDirection > 0)
        //     {
        //         Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.ToRadians(90f) - frontHandOffset); // set arm position (90 degree offset since arm starts lowered)
        //         Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter , rotation - MathHelper.ToRadians(75f) - offHandOffset);
        //     }
        //     else
        //     {
        //         Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.ToRadians(90f) + frontHandOffset); // set arm position (90 degree offset since arm starts lowered)
        //         Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter , rotation - MathHelper.ToRadians(75f) + offHandOffset);
        //     }
                        
        // }
      
    }
}