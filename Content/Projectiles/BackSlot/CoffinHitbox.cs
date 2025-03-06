
using LimbusCompanyWildHunt.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace LimbusCompanyWildHunt.Content.Projectiles
{
	public class CoffinHitbox : ModProjectile
	{
		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        public override string Texture => "LimbusCompanyWildHunt/Content/Projectiles/Texture/BackSlot_Coffin"; // Use texture of item as projectile textureE
		private enum AttackStage
		{
			Execute,
			Retract,
			OnHit,
			Smash,
			Throw
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
			Projectile.width = 150; // Hitbox width of projectile
			Projectile.height = 250; // Hitbox height of projectile
			
			Projectile.timeLeft = 100000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee 
			
			Projectile.scale = 0.7f;
            Projectile.friendly = true; // can damage
		}

        public override void OnSpawn(IEntitySource source) {
			InitialAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

			Projectile.rotation = InitialAngle;
		}

		private float execTime => 30f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float retractTime => 7f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float onhitTime => 40f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float smashTime => 40f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float throwTime => 40f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

		public override void AI() {
			// Extend use animation until projectile is killed
			// 
			// Projectile.timeLeft = 4;
			// 
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;
            
			// Kill the projectile if the player dies or gets crowd controlled
			if (!Owner.active || Owner.dead) {
				Projectile.Kill();
				return;
			}

			switch(CurrentStage)
			{
				case AttackStage.Execute:
					executeStrike();			
					break;
				case AttackStage.Retract:
					Projectile.friendly = true;
					if(Timer >= retractTime)
					{
						CurrentStage = AttackStage.OnHit;
					}
					break;
				case AttackStage.OnHit:
					if(Timer >= onhitTime)
					{
						CurrentStage = AttackStage.Smash;
					}
					break;
				case AttackStage.Smash:
					if(Timer >= smashTime)
					{
						CurrentStage = AttackStage.Throw;
					}
					break;
				case AttackStage.Throw:
					if(Timer >= throwTime)
					{
						WildHunt.coffinCaught = false;
						WildHunt.caughtNpc = null;
						Projectile.Kill();
					}
					break;
			}


			Timer++;
		}
		private float xPosOffset = 65;
		private float xMaxPosOffset = 745;
		private void executeStrike()
		{	
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
		
			armPosition.Y += Owner.gfxOffY;

			if(Owner.direction > 0)
			{
				armPosition.X += MathHelper.SmoothStep(xPosOffset, xMaxPosOffset, Timer/execTime);
			}
			else
			{
				armPosition.X -= MathHelper.SmoothStep(70, 750, Timer/execTime);;
			}

			armPosition.Y += -30;

			Projectile.Center = armPosition;

			if(Timer >= execTime)
			{
				CurrentStage = AttackStage.Retract;
			}
		}

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			if(CurrentStage == AttackStage.Execute)
			{
				if(WildHunt.coffinCaught)
					return;
				WildHunt.caughtNpc = target;
				WildHunt.coffinCaught = true;
			}
			else
			{
				//kill projectile after damaging

			}           
        }


        public override void SendExtraAI(BinaryWriter writer) {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
			// writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			// Projectile.spriteDirection = reader.ReadSByte();
		}

		public override bool PreDraw(ref Color lightColor) {
            // drawSingle(lightColor, 54, -88, 0);

			// Redraw player and projectile at the top most layer instead
			// Main.spriteBatch.End();
			// Main.spriteBatch.Begin(SpriteSortMode.Immediate, )
			if(CurrentStage != AttackStage.Smash)
				return false;





			return false;
		}
    }
}