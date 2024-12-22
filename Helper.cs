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
using ReLogic.Content;
using Vector2=Microsoft.Xna.Framework.Vector2;

namespace LimbusCompanyWildHunt
{
    class Helper
    {
        public const string resDir = "LimbusCompanyWildHunt/Content/";
        public struct textureInfo
        {
            public textureInfo(int width, int height, List<Texture2D> tex, float scal = 1f)
            {
                X = width;
                Y = height;
                texture = tex;
                Scale = scal;
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

        public static void drawChains(Color lightColor, Player Owner, float xOffset = 0, float yOffset = 0)
		{

            Vector2 pos = Owner.position;
            if(xOffset != 0 && yOffset != 0)
            {
                pos += new Vector2(xOffset, yOffset) - Main.screenPosition;
            }
            else
            {
                pos -= Main.screenPosition;
                if(Owner.direction > 0)
                {
                    pos.X += 9;
                }
                else
                {
                    pos.X += 42;
                }

                pos.Y += 40;
            }
            pos.Y += Owner.gfxOffY;
            pos = toInt(pos);   
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin = new Vector2(102, 154);
			SpriteEffects effects;         

			if (Owner.direction > 0) {
				effects = SpriteEffects.None;
			}
			else {
				effects = SpriteEffects.FlipHorizontally;
			}	

			Main.spriteBatch.Draw(
				LimbusCompanyWildHunt.ChainIdleTexture, 
				pos,
                default, 
				lightColor, 
				0f, 
				origin, 
				0.3f, 
				effects,
				0f
			);
		}

        public static void drawCoffin(Color lightColor, Player Owner, float xOffset = 0, float yOffset = 0)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			
			int projWidth = 39;
            int projHeight = 66;

			float xoff = 54 * 0.3f;
			float yoff = -88 * 0.3f;

			Vector2 origin = new Vector2(projWidth + xoff, projHeight - yoff);
			float rotationOffset;
			SpriteEffects effects;
            

            Vector2 itemPos = Owner.position;

            float angle;       
            if(Owner.direction > 0)
            {
                rotationOffset = MathHelper.ToRadians(45);
                angle = Owner.MountedCenter.ToRotation();
                angle -= MathHelper.ToRadians(51); 
                itemPos.X -= 10 - xOffset;

                effects = SpriteEffects.None;
            }
            else
            {
                rotationOffset = MathHelper.ToRadians(135);
                angle = -Owner.MountedCenter.ToRotation();
                angle += MathHelper.ToRadians(180 + 51);
                itemPos.X += 25 + xOffset;

                if(xOffset != 0)
                {
                    itemPos.X -= 32;
                }

                effects = SpriteEffects.FlipHorizontally;
            }

            itemPos.Y += 5 + yOffset;
            itemPos.Y += Owner.gfxOffY;

			itemPos = toInt(itemPos);

			Main.spriteBatch.Draw(
				LimbusCompanyWildHunt.CoffinIdleTexture, 
				itemPos - Main.screenPosition, 
				default, 
				lightColor, 
				angle + rotationOffset, 
				origin, 
				0.3f, 
				effects,
				0f
			);
		}


        public static Vector2 toInt(Vector2 vec)
        {
            vec.X = (int) vec.X;
            vec.Y = (int) vec.Y;

            return vec;
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