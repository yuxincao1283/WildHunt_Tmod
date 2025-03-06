// using LimbusCompanyWildHunt.Content.Items;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using Terraria;
// using Terraria.DataStructures;
// using Terraria.ID;
// using Terraria.ModLoader;

// namespace LimbusCompanyWildHunt.Content.System
// {
//     public class Coffinfullwhite : ModTile
//     {
//         public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
//         {
//             var drawPosition = new Vector2(i * 16, j * 16) - Main.screenPosition;

//             // Set the shader
//             Main.spriteBatch.End();
//             YourMod.TileShader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
//             YourMod.TileShader.CurrentTechnique.Passes[0].Apply();
//             Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                
//             // Draw the tile with the shader
//             spriteBatch.Draw(
//                 ModContent.Request<Texture2D>("YourMod/Tiles/YourCustomTileTexture").Value,
//                 drawPosition,
//                 Color.White
//             );

//             Main.spriteBatch.End();
//             Main.spriteBatch.Begin(); // Restore the default sprite batch state
//         }
//     }
// }