
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ReLogic.Content;

namespace LimbusCompanyWildHunt.Content.Projectiles
{
	public class WolfProj : ModProjectile
	{
		
		private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
		{
			Charge,
			Execute
		}

		// These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
		private AttackStage CurrentStage {
			get => (AttackStage)Projectile.localAI[0];
			set {
				Projectile.localAI[0] = (float)value;
				Timer = 0; // reset the timer when the projectile switches states
			}
		}

		// Variables to keep track of during runtime
        
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
		private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
		private ref float Size => ref Projectile.localAI[2]; // Size of sword

		// We define timing functions for each stage, taking into account melee attack speed
		// Note that you can change this to suit the need of your projectile
		// private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float chargeTime => 70f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float execTime => 50f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		public override string Texture => "LimbusCompanyWildHunt/Content/Items/WildHunt"; // Use texture of item as projectile textureE
		private Texture2D chargeProj = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/skill3_chargeProj").Value;
		private Texture2D execProj = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/skill3_execProj").Value;
		private Effect appearEffect = ModContent.Request<Effect>(
				"LimbusCompanyWildHunt/Effects/Content/appear", 
				AssetRequestMode.ImmediateLoad).Value;
        private Player Owner => Main.player[Projectile.owner];

        private Helper.textureInfo[] projectileInfo = new Helper.textureInfo[4];
        public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

        public override void SetDefaults() {
			Projectile.width = 500; // Hitbox width of projectile
			Projectile.height = 500; // Hitbox height of projectile
			
			Projectile.timeLeft = 100000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee 
			
			Projectile.friendly = false; // cannot damage during charge time

            Projectile.hide = true;

            projectileInfo[0] = new Helper.textureInfo(458, 385, new List<Texture2D>{chargeProj}, 0.8f);
			projectileInfo[1] = new Helper.textureInfo(529, 323, new List<Texture2D>{execProj}, 0.8f);
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
			switch (CurrentStage) {
				case AttackStage.Charge:
					ChargeStrike();
					break;
				case AttackStage.Execute:
					ExecuteStrike();
					break;
			}

			SetSwordPosition();
			Timer++;
			
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
			
			Size = 0.65f;

			InitialAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            angleRadians = InitialAngle;

            if(Projectile.spriteDirection > 0)
			{
                InitialAngle -= MathHelper.ToRadians(45);
                
            }
            else
            {
                InitialAngle += MathHelper.ToRadians(45);
            }

            Projectile.rotation = InitialAngle;


            xCenterOffset = (xOffset) * (float) Math.Cos(angleRadians);
            yCenterOffset = (yOffset) * (float) Math.Sin(angleRadians);

            Helper.playSound("wildheath_2_4");
		}

		public override void SendExtraAI(BinaryWriter writer) {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
			writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.spriteDirection = reader.ReadSByte();
		}


		public override bool PreDraw(ref Color lightColor) {
            if(CurrentStage == AttackStage.Charge){
				drawSingleCustom(Color.White, 250, -50, 0, 0); //350 ,  -50
				drawSingleCustom(Color.White, 150, -50, 0, 0); //250,  -50
				drawSingleCustom(Color.White, -100, 0, 0, 0); //0,   0
            }
            else{
				drawSingleCustom(Color.White, 250, -50, 0, 1);
				drawSingleCustom(Color.White, 150, -50, 0, 1);
				drawSingleCustom(Color.White, -100, 0, 0, 1);
            }

			// drawSingle(lightColor, 0, 0, 0, 2);
			// Since we are doing a custom draw, prevent it from normally drawing
			return false;
		}
		private float projVis = 0f;

		private void drawSingleCustom(Color lightColor, int xOffset, int yOffset, int angleOffset, int renderIndex)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;

            int projWidth = projectileInfo[renderIndex].X;
            int projHeight = projectileInfo[renderIndex].Y;

			if (Projectile.spriteDirection > 0) {
				origin = new Vector2(projWidth + xOffset, projHeight - yOffset);
				rotationOffset = MathHelper.ToRadians(45 + angleOffset);
				effects = SpriteEffects.None;
			}
			else {
				origin = new Vector2(projWidth - xOffset + 100, projHeight - yOffset);
				rotationOffset = MathHelper.ToRadians(135 - angleOffset);
				effects = SpriteEffects.FlipHorizontally;
			}	
			// Texture2D texture = TextureAssets.Projectile[Type].Value;


			Main.spriteBatch.End();

			if(renderIndex == 2)
			{
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
				
				appearEffect.Parameters["AppearFactor"].SetValue(projVis); // Example value
				appearEffect.CurrentTechnique.Passes["P0"].Apply();
	
				Main.spriteBatch.Draw(
					projectileInfo[renderIndex].texture[0], 
					Projectile.Center - Main.screenPosition, 
					default, 
					lightColor * Projectile.Opacity, 
					Projectile.rotation + rotationOffset, 
					origin, 
					Projectile.scale, 
					effects,
					0f
				);

				return;
			}
			
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			Main.spriteBatch.Draw(
				projectileInfo[renderIndex].texture[0], 
				Projectile.Center- Main.screenPosition, 
				default, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation + rotationOffset, 
				origin, 
				Projectile.scale, 
				effects,
				0f
			);


		}

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


		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			// Make knockback go away from player
			modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
		}


		// Function to easily set projectile and arm position
		public void SetSwordPosition() {   
			// Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently

			Vector2 armPosition;
			
			if(CurrentStage == AttackStage.Execute)
				armPosition = fixedArmPos;
			else
			{
			 	armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
            	armPosition.Y += Owner.gfxOffY;
			}
			
            if(CurrentStage == AttackStage.Charge)
            {
                float rotationOffset =- MathHelper.ToRadians(45);
                float xVector, yVector;

                if(Projectile.spriteDirection > 0)
                {
                    xVector = (float) Math.Cos(InitialAngle - rotationOffset);
                    yVector = (float) Math.Sin(InitialAngle - rotationOffset);
                }
                else
                {
                    xVector = (float) Math.Cos(InitialAngle + rotationOffset);
                    yVector = (float) Math.Sin(InitialAngle + rotationOffset);
                }

                //change armposition by progress
                armPosition.X -= Progress * xVector;
                armPosition.Y -= Progress * yVector;
            }

			armPosition.X += xCenterOffset;
			armPosition.Y += yCenterOffset; 

			Projectile.Center = armPosition; // Set projectile to arm position

			Projectile.scale = 0.8f * 0.7f * Size * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

            Lighting.AddLight(Projectile.Center + Projectile.rotation.ToRotationVector2(), Color.DarkViolet.ToVector3());
		}
		private int chargeOffset = 40;
        
        private float angleRadians = 0;

		private Vector2 fixedArmPos;

		public void ChargeStrike() {

            Progress = MathHelper.SmoothStep(0, chargeOffset, Timer / chargeTime);
			
			if(Timer <= chargeTime - 1 / Owner.GetTotalAttackSpeed(Projectile.DamageType))
				projVis = MathHelper.SmoothStep(0f, 1f, Timer/chargeTime);

			if (Timer >= chargeTime) {

				Projectile.friendly = true;

				fixedArmPos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2);
				fixedArmPos.Y += Owner.gfxOffY;

				CurrentStage = AttackStage.Execute;
			}
		}

		private int projMaxOffset = 3000;
		private float xCenterOffset = 0;
		private float yCenterOffset = 0;
        int xOffset = -250;
        int yOffset = -250;
		private void ExecuteStrike() {
            xCenterOffset = (xOffset + MathHelper.SmoothStep(0, projMaxOffset, Timer/execTime)) * (float) Math.Cos(angleRadians);
            yCenterOffset = (yOffset + MathHelper.SmoothStep(0, projMaxOffset, Timer/execTime)) * (float) Math.Sin(angleRadians);
                
			if (Timer >= execTime) {
				Projectile.Kill();
			}
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }
}