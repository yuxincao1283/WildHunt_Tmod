
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

namespace LimbusCompanyWildHunt.Content.Projectiles
{
	public class LowerSlash_s2 : ModProjectile
	{
		private int frameIdx = 0;
		private int maxFrame = 0;
		private int basetime = 0;
		private int remainder = 0;
		private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
		{
			charge,
			Execute
		}

        // These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
		private float getTime(AttackStage stage)
		{
			switch(stage)
			{
				case AttackStage.charge:
					return chargeTime;
				default:
					return execTime;
			}
		}
        private AttackStage CurrentStage {
			get => (AttackStage)Projectile.localAI[0];
			set {
				Projectile.localAI[0] = (float)value;
				Timer = 0; // reset the timer when the projectile switches states
				frameIdx = 0;
				setFrameInfo(getTime(CurrentStage));
			}
		}

		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
		private ref float Size => ref Projectile.localAI[2]; // Size of sword

		// We define timing functions for each stage, taking into account melee attack speed
		// Note that you can change this to suit the need of your projectile
		// private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float chargeTime => 20f / Owner.GetTotalAttackSpeed(Projectile.DamageType);	
		private float execTime => 35f / Owner.GetTotalAttackSpeed(Projectile.DamageType);			
         public override string Texture => "LimbusCompanyWildHunt/Content/Items/WildHunt"; // Use texture of item as projectile textureE
		private Texture2D normalTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/WildHunt_Weapon").Value;
		private Texture2D blackedTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/WildHunt_Weapon_Blacked").Value;
		private Player Owner => Main.player[Projectile.owner];
		private float chargeXOffset = 0;
		private float chargeYOffset = 0;
		private Helper.textureInfo[] projectileInfo = new Helper.textureInfo[3];

        public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}
        public override void SetDefaults() {
			Projectile.width = 700; // Hitbox width of projectile
			Projectile.height = 700; // Hitbox height of projectile
			Projectile.friendly = true; // Projectile hits enemies
			Projectile.timeLeft = 100000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee 
			
			projectileInfo[0] = new Helper.textureInfo(700, 700, new List<Texture2D>{blackedTexture}, 0.55f);
			projectileInfo[1] = new Helper.textureInfo(776, 730, Helper.loadVfxFolder("LowerSlash/", 553, 565), 2.5f);
			projectileInfo[2] = new Helper.textureInfo(700, 700, new List<Texture2D>{normalTexture}, 0.55f);
			
			// upperSlash = Helper.loadVfxFolder("UpperSlash/", 578, 594);
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
			switch(CurrentStage)
			{
				case AttackStage.charge:
					ChargeStrike();
					break;
				default: 
					ExecuteStrike();
					break;	
			} 
            	

			SetSwordPosition();
			Timer++;

			int allocatedTime = basetime;
			if(frameIdx >= remainder)
				allocatedTime++;

			if(Timer%allocatedTime == 0 && frameIdx+1 < maxFrame)
			{
				frameIdx++;
			}
		}

		public override void OnSpawn(IEntitySource source) {

			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
			
			Size = 0.65f;

			InitialAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
			
            int degreeOffset = 15;

			if(Projectile.spriteDirection == 1)
			{
				InitialAngle -= MathHelper.ToRadians(degreeOffset);
			}
			else
			{
				InitialAngle += MathHelper.ToRadians(degreeOffset);
			}
			
			Projectile.rotation = InitialAngle;

            setFrameInfo(getTime(CurrentStage));
			
		}
		public override void SendExtraAI(BinaryWriter writer) {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
			writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.spriteDirection = reader.ReadSByte();
		}

		public override bool PreDraw(ref Color lightColor) {
			if(CurrentStage == AttackStage.charge)
			{
				drawSingle(lightColor, 50, 70, -135, 0);				

				if(Projectile.spriteDirection > 0)
				{
					Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(90));
				}
				else
				{
					Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(90));
				}
				
			}
			else
			{
				drawSingle(lightColor, 50, 50, 0, 2);
				drawSprite(Color.White, 280, 295, 155, 1);

				if(Projectile.spriteDirection > 0)
				{
					Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.None, -0.4f);
				}
				else
				{
					Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.None, -0.4f);
				}
			}
				
				// drawSingleTmp(ref lightColor, 115, 130, 50, 2);
				// drawSpriteTmp(ref lightColor, 350, 270, 155, 1);

			// Since we are doing a custom draw, prevent it from normally drawing
			return false;
		}
		// private void drawSprite(ref Color lightColor, int xOffset, int yOffset, int angleOffset, int renderIndex)
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
				effects = SpriteEffects.FlipHorizontally;
			}
			else {
				origin = new Vector2(projWidth - xOffset, projHeight - yOffset);
				rotationOffset = MathHelper.ToRadians(135 - angleOffset);
				effects = SpriteEffects.None;
			}	
			// Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			// Lighting.AddLight(origin, new Vector3(67.8f, 0.8f, 93.7f));
			Lighting.AddLight(origin + Projectile.Center, Color.DarkViolet.ToVector3());

			//draw slash effect 
			Main.spriteBatch.Draw(
				projectileInfo[renderIndex].texture[frameIdx], 
				Projectile.Center - Main.screenPosition, 
				default, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation + rotationOffset, 
				origin, 
				Projectile.scale * projectileInfo[renderIndex].Scale, 
				effects,
				0f
			);
		}
		private void drawSingle(Color lightColor, int xOffset, int yOffset, int angleOffset, int renderIndex)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;
            

            int projWidth = projectileInfo[renderIndex].X;
            int projHeight = projectileInfo[renderIndex].Y;

			if (Projectile.spriteDirection > 0) {
				origin = new Vector2(xOffset, projHeight-yOffset);
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
				projectileInfo[renderIndex].texture[0], 
				Projectile.Center - Main.screenPosition, 
				default, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation + rotationOffset, 
				origin, 
				Projectile.scale * projectileInfo[renderIndex].Scale, 
				effects,
				0f
			);
		}
		// Function to easily set projectile and arm position
		public void SetSwordPosition() {
			// Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
			
			// if(CurrentStage == AttackStage.charge)
			// {
			// 	if(Projectile.spriteDirection > 0)
			// 	{
			// 		Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(90));
			// 	}
			// 	else
			// 	{
			// 		Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(90));
			// 	}
			// }

			// Owner.front
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
			
			armPosition.Y += Owner.gfxOffY;

			//change armposition by progress
			armPosition.X += chargeXOffset;
			armPosition.Y += chargeYOffset;

			Projectile.Center = armPosition; // Set projectile to arm position

			Projectile.scale = 0.7f * Size * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers
		}
		private void setFrameInfo(float time)
		{
			maxFrame = projectileInfo[1].texture.Count;
			basetime = (int) Math.Floor(time/maxFrame);
			remainder = maxFrame - (int) Math.Round(time % maxFrame);
		}

		private void ChargeStrike() {
			if(Timer >= chargeTime)
			{
				CurrentStage = AttackStage.Execute;

				int degreeOffset = -150;

				if(Projectile.spriteDirection == 1)
				{
					InitialAngle -= MathHelper.ToRadians(degreeOffset);
				}
				else
				{
					InitialAngle += MathHelper.ToRadians(degreeOffset);
				}

				Projectile.rotation = InitialAngle;

				Helper.playSound("wildheath_1_2-2");
			}
		}

		private void ExecuteStrike() {
				
			if (Timer >= execTime) {
				Projectile.Kill();
			}
		}


        // // // // Find the start and end of the sword and use a line collider to check for collision with enemies
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


		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			// Make knockback go away from player
			modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
		}
    }
}