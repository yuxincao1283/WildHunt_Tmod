
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
	public class CoffinProjectile : ModProjectile
	{
		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        public override string Texture => "LimbusCompanyWildHunt/Content/Projectiles/Texture/BackSlot_Coffin"; // Use texture of item as projectile textureE
		private List<Texture2D> CoffinProj;
		private enum AttackStage
		{
			Execute,
			Retract,
			OnHit
		}

		private int frameIdx = 0;
		private int maxFrame = 0;
		private int basetime = 0;
		private int remainder = 0;

        private AttackStage CurrentStage {
			get => (AttackStage)Projectile.localAI[0];
			set {
				Projectile.localAI[0] = (float)value;
				Timer = 0; // reset the timer when the projectile switches states
				setFrameInfo();
			}
		}
		
        private Player Owner => Main.player[Projectile.owner];
		
		private List<Texture2D> onHitTexture = new List<Texture2D>();


        public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

        public override void SetDefaults() {
			Projectile.width = 1235; // Hitbox width of projectile
			Projectile.height = 255; // Hitbox height of projectile
			
			Projectile.timeLeft = 100000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee 
			
			CoffinProj = Helper.loadVfxFolder("CoffinThrow/", 3, 10);
			
			onHitTexture.Add(ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/onhit_2").Value);
			onHitTexture.Add(ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/onhit_1").Value);
			
            Projectile.scale = 0.7f;
            Projectile.friendly = false; // cannot damage
		}

        public override void OnSpawn(IEntitySource source) {
			setFrameInfo();
			

		
			Owner.ChangeDir(-Owner.direction);
			if(Owner.direction > 0)
			{
				InitialAngle =  -Owner.MountedCenter.ToRotation();
			}
			else
			{
				InitialAngle =  Owner.MountedCenter.ToRotation();

			}
			CurrentStage = AttackStage.OnHit;
		
			Projectile.rotation = InitialAngle;
		}


		public override void AI() {
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

					int allocatedTime = basetime;
					if(frameIdx >= remainder)
						allocatedTime++;

					if((Timer+1)%allocatedTime == 0 && frameIdx+1 < maxFrame)
					{
						frameIdx++;
					}					
					break;
				case AttackStage.Retract:
					retractStrike();

					allocatedTime = basetime;
					if(frameIdx >= remainder)
						allocatedTime++;

					if((Timer+1)%allocatedTime == 0 && frameIdx-1 >= 0)
					{
						frameIdx--;
					}					
					break;
				case AttackStage.OnHit:
					onhitStrike();
					break;
			}

			Owner.heldProj = Projectile.whoAmI;
			Timer++;
		}
		private float execTime => 30f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float retractTime => 7f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float onhitTime => 40f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float offHandAngle1 = MathHelper.ToRadians(110f);
		private float offHandAngle2 = MathHelper.ToRadians(70);

		private void executeStrike()
		{	
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
			
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
			
			armPosition.Y += Owner.gfxOffY;

			Projectile.Center = armPosition;
	
			if(Owner.direction > 0)
			{
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - offHandAngle1); // set arm position (90 degree offset since arm starts lowered)
			}
			else
			{
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - offHandAngle2); // set arm position (90 degree offset since arm starts lowered)
			}

			if(Timer >= execTime)
			{
				CurrentStage = AttackStage.Retract;
			}
		}
		
		private void retractStrike()
		{
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
			
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
			
			armPosition.Y += Owner.gfxOffY;

			Projectile.Center = armPosition;
	
			if(Owner.direction > 0)
			{
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - offHandAngle1); // set arm position (90 degree offset since arm starts lowered)
			}
			else
			{
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - offHandAngle2); // set arm position (90 degree offset since arm starts lowered)
			}

			if(Timer >= retractTime)
			{
				// if(CoffinHitbox.caught == true)
				// {
					CurrentStage = AttackStage.OnHit;

					
					return;
				// }

				Projectile.Kill();
			}
		}
		
		private int curOnhit = 0;

		private void onhitStrike()
		{
			Vector2 armPos;
			if(Owner.direction > 0)
			{
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(135)); 
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(45));

				armPos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(135));

				armPos.X += 22;
			}
			else
			{
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(135));
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(45)); 

				armPos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(135));

				armPos.X -= 8;
			}

			
			armPos.Y += 25;

			armPos.Y += Owner.gfxOffY;
			Projectile.Center = armPos;


			
			if(Timer >= onhitTime/2)
			{
				curOnhit = 1;
			}
			

			if(Timer >= onhitTime)
			{
				Projectile.Kill();
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
			
			if(CurrentStage != AttackStage.OnHit)
			{
				drawSprite(Color.White);
			}
			else
			{
				drawSpriteRetract(Color.White, 0, 0, curOnhit);
			}


			return false;
		}

		private void drawSprite(Color lightColor)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;
			
			if (Owner.direction > 0) {
				origin = new Vector2(0, Projectile.height);
				rotationOffset = MathHelper.ToRadians(0);
				effects = SpriteEffects.None;
			}
			else {
				origin = new Vector2(Projectile.width + 375, Projectile.height);
				rotationOffset = MathHelper.ToRadians(180);
				effects = SpriteEffects.FlipHorizontally;
			}	

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
			
			// Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			pos = Helper.toInt(pos);

			//draw slash effect 
			Main.spriteBatch.Draw(
				CoffinProj[frameIdx],
				pos, 
				default, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation + rotationOffset, 
				origin, 
				Projectile.scale, 
				effects,
				0f
			);
		}
		private void drawSpriteRetract(Color lightColor, int xOffset, int yOffset, int idx)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;
			
			if (Owner.direction > 0) {
				origin = new Vector2(353, 372 - yOffset);
				rotationOffset = MathHelper.ToRadians(0);
				effects = SpriteEffects.FlipHorizontally;
			}
			else {
				origin = new Vector2(xOffset, 372 - yOffset);
				rotationOffset = MathHelper.ToRadians(0);
				effects = SpriteEffects.None;
			}	
			
			// Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			//draw slash effect 
			Main.spriteBatch.Draw(
				onHitTexture[idx],
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
	
		private void setFrameInfo()
		{   
			switch(CurrentStage)
			{
				case AttackStage.Execute:
					maxFrame = CoffinProj.Count;
					basetime = (int) Math.Floor(execTime/maxFrame);
					remainder = maxFrame - (int) Math.Round(execTime % maxFrame);

					break;
				case AttackStage.Retract:
					maxFrame = CoffinProj.Count;
					basetime = (int) Math.Floor(retractTime/maxFrame);
					remainder = maxFrame - (int) Math.Round(retractTime % maxFrame);

					break;
			}
		}
    }
}