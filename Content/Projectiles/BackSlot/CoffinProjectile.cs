
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using LimbusCompanyWildHunt.Content.Items;
using ReLogic.Content;
using rail;
using System.Runtime.CompilerServices;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;
using Terraria.Graphics.Light;



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
			OnHit,
			Smash,
			Throw
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
			}
		}
		
        private Player Owner => Main.player[Projectile.owner];
		
		private List<Texture2D> onHitTexture = new List<Texture2D>();


        public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}


		private List<Texture2D>[] lightningTexture = new List<Texture2D>[2];
		private Texture2D fullwhiteTex = ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Texture/fullbright", AssetRequestMode.ImmediateLoad).Value; 

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
            Projectile.scale = 0.7f;

			CoffinProj = Helper.loadVfxFolder("CoffinThrow/", 3, 10);
			lightningTexture[0] = Helper.loadVfxFolder("Lighting_bw/", 267, 275);
			// lightningTexture[1] = );

			onHitTexture.Add(ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/onhit_1_bw").Value);
			onHitTexture.Add(ModContent.Request<Texture2D>("LimbusCompanyWildHunt/Content/Projectiles/Texture/onhit_2_bw").Value);		

            Projectile.friendly = false; // cannot damage
		}

        public override void OnSpawn(IEntitySource source) {
			setFrameInfo();
			
			//add light around the coffin position

			// Filters.Scene.Activate("greyscale:s3");
			// Filters.Scene["greyscale:s3"].GetShader().UseProgress(1.0f);
				

			InitialAngle = (Main.MouseWorld-Owner.MountedCenter).ToRotation();

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

					if((Timer+1)%allocatedTime == 0 && frameIdx-1 >= 2)
					{
						frameIdx--;
					}	

					break;
				case AttackStage.OnHit:
					onhitStrike();
					break;
				case AttackStage.Smash:
					smashStrike();
			
					break;
				case AttackStage.Throw:
					throwStrike();
					break;
			}

			Owner.heldProj = Projectile.whoAmI;
			Timer++;
		}
		private float execTime => 20f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float retractTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float onhitTime => 30f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float throwTime => 40f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
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

			if(Timer >= execTime || WildHunt.coffinCaught == true)
			{
				CurrentStage = AttackStage.Retract;

				Filters.Scene["blackfadein:s3"].GetShader().UseProgress(1.0f);
				Filters.Scene.Activate("blackfadein:s3");

				setFrameInfo();
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

			float progress = MathHelper.SmoothStep(1.0f, 0f, Timer/(retractTime+onhitTime));
			Filters.Scene["blackfadein:s3"].GetShader().UseProgress(progress);

			if(Timer >= retractTime)
			{
				
				// if(WildHunt.coffinCaught == true)
				// {
					Owner.direction = -Owner.direction;
					if(Owner.direction > 0)
					{
						InitialAngle = -Owner.MountedCenter.ToRotation();
					}
					else
					{
						InitialAngle = Owner.MountedCenter.ToRotation();
					}

					Projectile.rotation = InitialAngle;
					CurrentStage = AttackStage.OnHit;

					setFrameInfo();
					return;
				// }
			}
		}
		
		private int curOnhit = 0;
		private void onhitStrike()
		{
			Vector2 armPosition;
			if(Owner.direction > 0)
			{
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(105f)); // set arm position (90 degree offset since arm starts lowered)
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation -  MathHelper.ToRadians(30f)); // set arm position (90 degree offset since arm starts lowered)

				armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand

				armPosition.X += 10;
			}
			else
			{
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(105f)); // set arm position (90 degree offset since arm starts lowered)
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(30f)); // set arm position (90 degree offset since arm starts lowered)

				armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
				armPosition.X -= 28;
			}

			armPosition.Y += Owner.gfxOffY;
			Projectile.Center = armPosition;

			float progress = MathHelper.SmoothStep(1.0f, 0f, (Timer+retractTime)/(retractTime+onhitTime));
			Filters.Scene["blackfadein:s3"].GetShader().UseProgress(progress);
				// Filters.Scene["greyscale:s3"] = new Filter(new ScreenShaderData(greyscale_threshold, "skill3_threshold"), EffectPriority.Medium);
				// Filters.Scene["greyscale:s3"].Load();

				// Filters.Scene["blackfadein:s3"] = new Filter(new ScreenShaderData(blackfadein_s3, "skill3_fadein"), EffectPriority.Medium);
				// Filters.Scene["blackfadein:s3"].Load();

			if(Timer >= onhitTime)
			{
				

				Filters.Scene.Activate("greyscale:s3");
				Filters.Scene["greyscale:s3"].GetShader().UseProgress(1.0f);
				
				Projectile.rotation = InitialAngle;					
				CurrentStage = AttackStage.Smash;

				setFrameInfo();
				
				Filters.Scene["blackfadein:s3"].GetShader().UseProgress(1f);
				Vector2 flashOffset;

				if(Owner.direction < 0)
				{
					flashOffset = new Vector2(200, -110);
				}
				else
				{
					flashOffset = new Vector2(-200, -110);
				}

				Vector2 pos = (Projectile.Center - Main.screenPosition + flashOffset) / new Vector2(Main.screenWidth, Main.screenHeight);
				float aspect = (float)Main.screenWidth/Main.screenHeight;
				

				//midnight value
				// Filters.Scene["flash:s3"].GetShader().Shader.Parameters["x"].SetValue(pos.X);
				// Filters.Scene["flash:s3"].GetShader().Shader.Parameters["y"].SetValue(pos.Y);
				// Filters.Scene["flash:s3"].GetShader().Shader.Parameters["aspect"].SetValue(aspect);
				// Filters.Scene["flash:s3"].GetShader().Shader.Parameters["intensity"].SetValue(0.7f);
				// Filters.Scene["flash:s3"].GetShader().Shader.Parameters["baseIntensity"].SetValue(2.4f);

				//dayvalue 
				Filters.Scene["flash:s3"].GetShader().Shader.Parameters["x"].SetValue(pos.X);
				Filters.Scene["flash:s3"].GetShader().Shader.Parameters["y"].SetValue(pos.Y);
				Filters.Scene["flash:s3"].GetShader().Shader.Parameters["aspect"].SetValue(aspect);
				Filters.Scene["flash:s3"].GetShader().Shader.Parameters["intensity"].SetValue(0f);
				Filters.Scene["flash:s3"].GetShader().Shader.Parameters["baseIntensity"].SetValue(2.4f);
				//original: 0.7, 2.0
				Filters.Scene.Activate("flash:s3");
			}
		}
		
		private float appearLimit => 25f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float pauseLimit => 18f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float blackLimit => 5f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float smashTime => 2.0f * appearLimit + 2.0f * pauseLimit + 2.0f * blackLimit;

		private float appearTime = 0;
		//false for appear, true for anim;
		private int appearMode = 0;
		private void smashStrike()
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

			if(appearMode == 1)
			{
				float progress = MathHelper.SmoothStep(1.0f, 0f, appearTime/(appearLimit));
				Filters.Scene["blackfadein:s3"].GetShader().UseProgress(progress);
				if(appearTime >= appearLimit)
				{
					appearMode = (appearMode+1)%3;
					appearTime = 0;

					Filters.Scene["blackfadein:s3"].GetShader().UseProgress(1f);
					Filters.Scene.Deactivate("flash:s3");
					frameIdx = 0;
				}
			}
			else if(appearMode == 0)
			{ 
				if(frameIdx >= remainder && (Timer+1)%basetime+1 == 0 && frameIdx+1 < maxFrame)
					frameIdx++;
				else if((Timer+1)%basetime == 0 && frameIdx+1 < maxFrame)
					frameIdx++;

				if(appearTime >= pauseLimit)
				{
					frameIdx++;
					appearMode = (appearMode+1)%3;
					appearTime = 0;
				}			
				
			}
			else
			{
				if(appearTime >= blackLimit)
				{
					Filters.Scene.Activate("flash:s3");
					frameIdx++;
					appearMode = (appearMode+1)%3;
					appearTime = 0;
				}	
			}

			appearTime++;

			if(Timer >= smashTime)
			{
				//deactivate early because it takes time
				Filters.Scene["greyscale:s3"].GetShader().UseProgress(0.0f);
				Filters.Scene["greyscale:s3"].Deactivate();
				Filters.Scene["blackfadein:s3"].Deactivate();

				CurrentStage = AttackStage.Throw;
			}
		}

		private void throwStrike()
		{
			if(appearTime <= appearLimit)
			{
				float progress = MathHelper.SmoothStep(0.0f, 1.0f, appearTime/appearLimit);
				Filters.Scene["blackfadein:s3"].GetShader().UseProgress(progress);
			}
			else if(Filters.Scene["blackfadein:s3"].IsActive())
			{
				Filters.Scene["blackfadein:s3"].Deactivate();
			}

			appearTime++;
			
			
			if(Timer >= throwTime)
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

			// Filters.Scene.Activate("fullbright");


			if(CurrentStage == AttackStage.Execute || CurrentStage == AttackStage.Retract)
			{
				drawSprite(Color.White);
			} 
			else if(CurrentStage == AttackStage.OnHit)
			{
				drawSpriteOpposite(Color.White);
			}
			else if(CurrentStage == AttackStage.Smash)
			{
				//lightning animation continues when disappearing, makes it longer. 
				//also left direction of the lightning has incorrect offset
				//throw time too slow
				drawSpriteSmash(Color.White, 0, 0, curOnhit);
				if(appearMode == 0)
				{
					drawSpriteLightning(Color.White, 0);
				}
				// if(appearMode == 2)
				// {
				// 	Main.spriteBatch.Draw(
				// 		fullwhiteTex,
				// 		new Vector2(0, 0),
				// 		Color.White
				// 	);
				// }	
			}

			return false;
		}

		private void drawSpriteLightning(Color lightColor, int type)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			SpriteEffects effects;
			float rotationOffset;

			if (Owner.direction > 0) {
				
				origin = new Vector2(453, 550);
				rotationOffset = MathHelper.ToRadians(5);
				effects = SpriteEffects.FlipHorizontally;
			}
			else {
				origin = new Vector2(-40, 550);
				rotationOffset = -MathHelper.ToRadians(5);
				effects = SpriteEffects.None;
			}	
			
			// Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			//draw slash effect 
			Main.spriteBatch.Draw(
				lightningTexture[type][frameIdx],
				Projectile.Center - Main.screenPosition, 
				default, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation + rotationOffset,
				origin, 
				1f, 
				effects,
				0f
			);
		}
	
		private void drawSpriteSmash(Color lightColor, int xOffset, int yOffset, int idx)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			SpriteEffects effects;
			
			if (Owner.direction > 0) {
				origin = new Vector2(353, 372 - yOffset);
				effects = SpriteEffects.FlipHorizontally;
			}
			else {
				origin = new Vector2(xOffset, 372 - yOffset);
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
				Projectile.rotation,
				origin, 
				Projectile.scale, 
				effects,
				0f
			);
		}
	
		
		private void drawSpriteOpposite(Color lightColor)
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;
			
			if (Owner.direction < 0) {
				origin = new Vector2(0, Projectile.height);
				rotationOffset = -MathHelper.ToRadians(15);
				effects = SpriteEffects.None;
			}
			else {
				origin = new Vector2(Projectile.width + 375, Projectile.height);
				rotationOffset = MathHelper.ToRadians(15);
				effects = SpriteEffects.FlipHorizontally;
				
			}	

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
			
			// Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			pos = Helper.toInt(pos);

			//draw slash effect 
			Main.spriteBatch.Draw(
				CoffinProj[1],
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
		
		private void setFrameInfo(int time=0)
		{   
			switch(CurrentStage)
			{
				case AttackStage.Execute:
					maxFrame = CoffinProj.Count;
					basetime = (int) Math.Floor(execTime/maxFrame);
					remainder = maxFrame - (int) Math.Round(execTime % maxFrame);

					break;
				case AttackStage.Retract:
					maxFrame = frameIdx+1;
					basetime = (int) Math.Floor(retractTime/maxFrame);
					remainder = maxFrame - (int) Math.Round(retractTime % maxFrame);

					break;
				case AttackStage.Smash:
					frameIdx = 0; 
					maxFrame = lightningTexture[0].Count;
					basetime = (int) Math.Floor((pauseLimit)/maxFrame);
					remainder = maxFrame - (int) Math.Round((pauseLimit) % maxFrame);

					break;
			}
		}
    }
}