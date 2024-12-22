
using Humanizer;
using LimbusCompanyWildHunt.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace LimbusCompanyWildHunt.Content.Projectiles
{
	public class CoffinIdle : ModProjectile
	{
	

		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        public override string Texture => "LimbusCompanyWildHunt/Content/Projectiles/Texture/BackSlot_Coffin"; // Use texture of item as projectile textureE
        private Texture2D normalTexture = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/BackSlot_Coffin").Value;
		// We define timing functions for each stage, taking into account melee attack speed
		// Note that you can change this to suit the need of your projectile
		// private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private Player Owner => Main.player[Projectile.owner];
        public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

        public override void SetDefaults() {
			Projectile.width = 133; // Hitbox width of projectile
			Projectile.height = 221; // Hitbox height of projectile
			
			Projectile.timeLeft = 3600; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee 
			
            Projectile.scale = 0.3f;
            Projectile.friendly = false; // cannot damage during charge time
		}

        public override void OnSpawn(IEntitySource source) {
			InitialAngle = (-Owner.MountedCenter).ToRotation();

            if(Owner.direction > 0)
            {
                InitialAngle -= MathHelper.ToRadians(angleOffset); 
            }
            else
            {
                InitialAngle += MathHelper.ToRadians(angleOffset);
            }
			
			Projectile.rotation = InitialAngle;
		}


		public override void AI() {
			// Extend use animation until projectile is killed
			if(Projectile.timeLeft+120 >= int.MaxValue)
			{
				Projectile.timeLeft = 3600;
			}
			else
			{
				Projectile.timeLeft += 120;
			}
            
            if(WildHunt.itemHeld == false)
            {
                Projectile.Kill();
                return;
            }

			// Kill the projectile if the player dies or gets crowd controlled
			if (!Owner.active || Owner.dead) {
				Projectile.Kill();
				return;
			}

			// AI depends on stage and attack
			// Note that these stages are to facilitate the scaling effect at the beginning and end
			// If this is not desirable for you, feel free to simplify

            idle();
		}

        private int angleOffset = 45;
        private void idle()
        {
            int xOffset = 10;
            int yOffset = 5;
            Vector2 itemPos = Owner.position;
            if(Owner.direction > 0)
            {
                InitialAngle = Owner.MountedCenter.ToRotation();
                InitialAngle -= MathHelper.ToRadians(51); 
                itemPos.X -= xOffset;
            }
            else
            {
                InitialAngle = -Owner.MountedCenter.ToRotation();
                InitialAngle += MathHelper.ToRadians(180 + 51);
                itemPos.X += xOffset + 15;
            }

            itemPos.Y += yOffset;
			itemPos.Y += Owner.gfxOffY;


			Projectile.rotation = InitialAngle;
            Projectile.Center = itemPos;
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
			Helper.drawCoffin(lightColor, Owner);
			return false;
		}
    }
}