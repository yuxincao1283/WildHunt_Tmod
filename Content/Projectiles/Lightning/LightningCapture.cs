
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace LimbusCompanyWildHunt.Content.Projectiles.Lightning
{
	public class LightningCapture : ModProjectile
	{
		// These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.

		// Variables to keep track of during runtime
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
		private ref float Size => ref Projectile.localAI[2]; // Size of sword

		// We define timing functions for each stage, taking into account melee attack speed
		// Note that you can change this to suit the need of your projectile
		// private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

		private float DisplayTime => 18f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		public override string Texture => "LimbusCompanyWildHunt/Content/Items/WildHunt"; // Use texture of item as projectile texture
        private Player Owner => Main.player[Projectile.owner];
        private Helper.textureInfo[] projectileInfo = new Helper.textureInfo[1];
		private int frameIdx = 0;
		private int maxFrame = 0;
		private int basetime = 0;
		private int remainder = 0;
        public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

        public override void SetDefaults() {
			Projectile.width = 200; // Hitbox width of projectile
			Projectile.height = 200; // Hitbox height of projectile
			
			Projectile.timeLeft = 100000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee 
			
			Projectile.friendly = false; // cannot damage during charge time
			Projectile.scale = 0.9f;
            Projectile.hide = true;

            projectileInfo[0] = new Helper.textureInfo(413, 731, Helper.loadVfxFolder("Lighting_bw/", 267, 275));
		}

		public override void AI() {
			// Extend use animation until projectile is killed
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;

			// Kill the projectile if the player dies or gets crowd controlled
			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed) {
				Projectile.Kill();
				return;
			}

			// AI depends on stage and attack
			// Note that these stages are to facilitate the scaling effect at the beginning and end
			// If this is not desirable for you, feel free to simplify

			Timer++;

			int allocatedTime = basetime;
			if(frameIdx >= remainder)
				allocatedTime++;

			if(Timer%allocatedTime == 0 && frameIdx+1 < maxFrame)
			{
				frameIdx++;
			}


			if(Timer >= DisplayTime)
			{
				Projectile.Kill();
			}
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
			setFrameInfo();
		}

		public override void SendExtraAI(BinaryWriter writer) {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
			writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.spriteDirection = reader.ReadSByte();
		}
        public override bool PreDraw(ref Color lightColor) {
            //400 diff
			drawSprite(Color.White, 0, 0, 0, 0);

			// drawSingle(lightColor, 0, 0, 0, 2);
			// Since we are doing a custom draw, prevent it from normally drawing
			return false;
		}

		private void drawSprite(Color lightColor, int xOffset, int yOffset, int angleOffset, int renderIndex)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;
            
            int projWidth = projectileInfo[renderIndex].X;
            int projHeight = projectileInfo[renderIndex].Y;

			if (Projectile.spriteDirection > 0) {
				origin = new Vector2(xOffset, projHeight - yOffset);
				rotationOffset = MathHelper.ToRadians(45 + angleOffset);
				effects = SpriteEffects.None;
			}
			else {
				origin = new Vector2(projWidth - xOffset, projHeight - yOffset);
				rotationOffset = MathHelper.ToRadians(135 - angleOffset);
				effects = SpriteEffects.FlipHorizontally;
			}	
			// Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			//draw slash effect 
			Main.spriteBatch.Draw(
				projectileInfo[renderIndex].texture[frameIdx], 
				Projectile.Center - Main.screenPosition, 
				default, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation + rotationOffset, 
				origin, 
				Projectile.scale, 
				effects,
				0f
			);
		}
		//CHANGE THIS FOR HITBOX
	// // // Find the start and end of the sword and use a line collider to check for collision with enemies
        // public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
		// 	Vector2 start = Owner.MountedCenter;
		// 	Vector2 end = start + Projectile.rotation.ToRotationVector2() * ((Projectile.Size.Length()) * Projectile.scale);
		// 	float collisionPoint = 0f;
		// 	return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		// }

		// Do a similar collision check for tiles
		public override void CutTiles() {
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
		}

		// Function to easily set projectile and arm position
		public void SetSwordPosition() {   
			// Owner.front
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
			
			armPosition.Y += Owner.gfxOffY;
		
			Projectile.Center = armPosition; // Set projectile to arm position

			Projectile.scale = 1.7f * 0.7f * Size * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers
		}
		
		private void setFrameInfo()
		{   
            maxFrame = projectileInfo[0].texture.Count;
            basetime = (int) Math.Floor(DisplayTime/maxFrame);
            remainder = maxFrame - (int) Math.Round(DisplayTime % maxFrame);
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }
}