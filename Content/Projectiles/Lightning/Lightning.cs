
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Build.Evaluation;

namespace LimbusCompanyWildHunt.Content.Projectiles.Lightning
{
	public class Lightning : ModProjectile
	{
		public override string Texture => "LimbusCompanyWildHunt/Content/Items/WildHunt"; // Use texture of item as projectile textureE

		private Texture2D normalTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Vfx/Lightning/tmp").Value;

		public override void SetDefaults()
		{
			Projectile.width = 340;
			Projectile.height = 520;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 50;
			// base.SetDefaults();
		}

		Player player => Main.player[Projectile.owner];

		public override void AI()
		{
			player.itemTime = player.itemAnimation = 3;
			Projectile.Center = player.Center + new Vector2(0, -900);
			// Projectile.rotation += 0.1f * player.direction;
			player.heldProj = Projectile.whoAmI;
		}

		public override void SetStaticDefaults()//以下照抄
		{
			ProjectileID.Sets.TrailingMode[Type] = 2;//这一项赋值2可以记录运动轨迹和方向（用于制作拖尾）
			ProjectileID.Sets.TrailCacheLength[Type] = 12;//这一项代表记录的轨迹最多能追溯到多少帧以前
														 // base.SetStaticDefaults();
		}

		public override bool ShouldUpdatePosition()
		{
			return false;

		}

		SpriteBatch sb = Main.spriteBatch;
		GraphicsDevice gd = Main.graphics.GraphicsDevice;

		public override bool PreDraw(ref Color lightColor)
		{

			// sb.End();
			// sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			// //开始顶点绘制

			// List<Vertex> vectorList = new List<Vertex>();

			// float size = ProjectileID.Sets.TrailCacheLength[Type];
			// Color drawColor = lightColor;
			// int yoff1 = -200;
			// int yoff2 = -120;
			// for (int i = 0; i < size; i++)
			// {
				
			// 	Console.WriteLine(Projectile.Center - Main.screenPosition + new Vector2(0, -80).RotatedBy(0));
			// 	//存顶点																										从这一—————————————到这里都是乱弄的 你可以随便改改数据看看能发生什么
			// 	vectorList.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -300).RotatedBy(Projectile.oldRot[i]) * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction),
			// 			  new Vector3(i / size, 1, 0),
			// 			  drawColor));
			// 	vectorList.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -20).RotatedBy(Projectile.oldRot[i]) * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction),
			// 			  new Vector3(i / size, 0, 0),
			// 			  drawColor));
			// }

			// if (vectorList.Count >= 3)//因为顶点需要围成一个三角形才能画出来 所以需要判顶点数>=3 否则报错
			// {
			// 	gd.Textures[0] = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Vfx/Lightning/Lightning").Value;//获取刀光的拖尾贴图
			// 	gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vectorList.ToArray(), 0, vectorList.Count - 2);//画
			// }

			// //结束顶点绘制
			// sb.End();
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			// Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(0, yoff1).RotatedBy(0) * player.direction;

			Main.spriteBatch.Draw(
				normalTexture, 
				Projectile.Center - Main.screenPosition + new Vector2(0, 400), 
				default, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation, 
				new Vector2(0, 0), 
				Projectile.scale, 
				SpriteEffects.None,
				0f
			);

			//画出这把 剑 的样子

			return false;
		}
	}
}