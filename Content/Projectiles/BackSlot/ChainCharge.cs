
using LimbusCompanyWildHunt.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace LimbusCompanyWildHunt.Content.Projectiles
{
	public class ChainCharge : ModProjectile
	{
		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        public override string Texture => "LimbusCompanyWildHunt/Content/Projectiles/Texture/BackSlot_Coffin"; // Use texture of item as projectile textureE
		private enum AttackStage
		{
			Charge,
			Prepare
		}

        private AttackStage CurrentStage {
			get => (AttackStage)Projectile.localAI[0];
			set {
				Projectile.localAI[0] = (float)value;
				Timer = 0; // reset the timer when the projectile switches states
			}
		}
		
        private Player Owner => Main.player[Projectile.owner];
		


        public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

        public override void SetDefaults() {
			Projectile.width = 133; // Hitbox width of projectile
			Projectile.height = 221; // Hitbox height of projectile
			
			Projectile.timeLeft = 100000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee 
			

            Projectile.scale = 0.3f;
            Projectile.friendly = false; // cannot damage
		}

        public override void OnSpawn(IEntitySource source) {
			InitialAngle = 0;
			if(Owner.direction > 0)
			{
				
				InitialAngle -= MathHelper.ToRadians(90); 
			}
			else
			{
				InitialAngle += MathHelper.ToRadians(90);
			}
		}


		public override void AI() {
			// Extend use animation until projectile is killed
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;

			// Kill the projectile if the player dies or gets crowd controlled
			if (!Owner.active || Owner.dead) {
				Projectile.Kill();
				return;
			}

			switch(CurrentStage)
			{
				case AttackStage.Charge:
					chargeStrike();
					break;
				case AttackStage.Prepare:
					prepareStrike();
					break;
			}
			Owner.heldProj = Projectile.whoAmI;
			Timer++;
		}
		private float chargeTime => 40f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float prepareTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float angleChange = MathHelper.ToRadians(75);
        private float offhandOffset = MathHelper.ToRadians(30);
		private float angleOffset = 0f;
		private float xChainOffset = 0f;
		private float yChainOffset = 0f;

		private void chargeStrike()
		{
			angleOffset = MathHelper.SmoothStep(0, angleChange, Timer/chargeTime);			
			
			float radius = 6f;

			if(Owner.direction > 0)
			{
				xChainOffset = + radius * (float) Math.Cos(angleOffset) + 9;
				yChainOffset = - radius * (float) Math.Sin(angleOffset) + 40;

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, InitialAngle - angleOffset);
                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.None, -offhandOffset);
			}
			else
			{
				xChainOffset = - radius * (float) Math.Cos(angleOffset) + 42;
				yChainOffset = - radius * (float) Math.Sin(angleOffset) + 40;

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, InitialAngle + angleOffset);
                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.None, offhandOffset);

            }

			if(Timer >= chargeTime)
			{
		
				if(Owner.direction > 0)
				{
					InitialAngle -= angleChange * 2; 
				}
				else
				{
					InitialAngle += angleChange * 2;
				}
				
			
				CurrentStage = AttackStage.Prepare;
			}
		}
		
		private void prepareStrike()
		{
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, InitialAngle);
			if(Owner.direction > 0)
			{
				xChainOffset = - 5;
				yChainOffset = + 30;

                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.None, -offhandOffset);
			}
			else
			{
				xChainOffset = + 55;
				yChainOffset = + 30;

                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.None, offhandOffset);
			}

			if(Timer >= prepareTime)
			{
				Projectile.Kill();
			}
		}
		
		public override void SendExtraAI(BinaryWriter writer) {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
			writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.spriteDirection = reader.ReadSByte();
		}

		public override bool PreDraw(ref Color lightColor) {
            // drawSingle(lightColor, 54, -88, 0);
			
           	Helper.drawChains(Color.White, Owner, xChainOffset, yChainOffset);

			return false;
		}
    }
}