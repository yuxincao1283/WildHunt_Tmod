using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

using Vector2=Microsoft.Xna.Framework.Vector2;

using LimbusCompanyWildHunt.Content.Projectiles;
using LimbusCompanyWildHunt.Content.Projectiles.Lightning;

namespace LimbusCompanyWildHunt.Content.Items 
{
    public class WildHunt : ModItem 
    {
        public static bool itemHeld = false;
        private int fixedAttackType = -1;
        private int attackType = 0; // keeps track of which attack it is
        private int stage = 2;
        private int stageChange = 0;
		private int comboExpireTimer = 0; // we want the attack pattern to reset if the weapon is not used for certain period of time


        public static bool coffinCaught = false;
        public static NPC caughtNpc = null;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100; // How many items need for research in Journey Mode
        }
        
        public override void SetDefaults()
        {
            Item.width = 700; // Width of an item sprite
            Item.height = 700; // Height of an item sprite
            Item.maxStack = 1; // How many items can be in one inventory slot
            Item.value = 100; // Item sell price in copper coins
            Item.rare = ItemRarityID.Master; // The color of item's name in game. Check https://terraria.wiki.gg/wiki/Rarity
        
         // Combat properties
            Item.damage = 200; // Item damage
            Item.DamageType = DamageClass.Melee; // What type of damage item is deals, Melee, Ranged, Magic, Summon, Generic (takes bonuses from all damage multipliers), Default (doesn't take bonuses from any damage multipliers)
            // useTime and useAnimation often use the same value, but we'll see examples where they don't use the same values
            Item.useTime = 20; // How long the swing lasts in ticks (60 ticks = 1 second)
            Item.useAnimation = 20; // How long the swing animation lasts in ticks (60 ticks = 1 second)
            Item.knockBack = 7; // How far the sword punches enemies, 20 is maximal value
            Item.autoReuse = true; // Can the item auto swing by holding the attack button

            // Other properties
            Item.value = 10000; // Item sell price in copper coins
            Item.useStyle = ItemUseStyleID.RaiseLamp; // This is how you're holding the weapon, visit https://terraria.wiki.gg/wiki/Use_Style_IDs for list of possible use styles
         
            Item.noMelee = true;  // This makes sure the item does not deal damage from the swinging animation
            Item.noUseGraphic = true; // This makes sure the item does not get shown when the player swings his hand

            // Projectile Properties
			Item.shoot = ModContent.ProjectileType<UpperSlash>(); // The sword as a projectile
            
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// Using the shoot function, we override the swing projectile to set ai[0] (which attack it is)
            // Projectile.NewProjectile(source, position, velocity, GenerateSwing(attackType), damage, knockback, Main.myPlayer);
            // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<UpperSlash_s1>(), damage, knockback, Main.myPlayer);  
            // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<UpperSlashVfx_s1>(), damage, knockback, Main.myPlayer);  
            // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<WolfProj>(), damage, knockback, Main.myPlayer);  
            // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BackSlot_Coffin>(), damage, knockback, Main.myPlayer);

            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CoffinHitbox>(), damage, knockback, Main.myPlayer);
            // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CoffinProjectile>(), damage, knockback, Main.myPlayer);
            
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CoffinCharge>(), damage, knockback, Main.myPlayer);
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ChainCharge>(), damage, knockback, Main.myPlayer);
           
            // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<LightningCapture>(), damage, knockback, Main.myPlayer);
            //todo:
            //purple glow in the eyes, get more vfx
            //turn charge into actual charge`
            // GenerateSwing(source, position, velocity, damage, knockback);

			comboExpireTimer = 0; // Every time the weapon is used, we reset this so the combo does not expire
            attackType = (attackType+1)%stage;

            if(stage == 3 && attackType == 0)
            {
                stageChange = 0;
                stage = 2;
            }
            else if(attackType == 0)
            {
                stageChange++;
            }

            if(stageChange == 2)
            {
                stage = 3;
            }

			return false; // return false to prevent original projectile from being shot
		}
        
        private void GenerateSwing(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            if(fixedAttackType != -1)
                attackType = fixedAttackType;

            // return ModContent.ProjectileType<WildHuntS1_1>();
            switch(stage)
            {
                case 2:
                    generateSkill_1(source, position, velocity, damage, knockback);
                    return;
                default:
                    generateSkill_2(source, position, velocity, damage, knockback);
                    return;

                                      // case 2:
                    //     return ModContent.ProjectileType<LowerSlash>();
            }
        
        }
        private void generateSkill_2(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            switch(attackType)
            {
                case 0:
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<LowerSlash_s2>(), damage, knockback, Main.myPlayer);   
                    return;  
                case 1:
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<UpperSlash_s2>(), damage, knockback, Main.myPlayer);   
   
                    return;
                default:

                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Pierce_s2>(), damage, knockback, Main.myPlayer);
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<PierceVfx_s2>(), damage, knockback, Main.myPlayer);
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<WolfProj>(), damage, knockback, Main.myPlayer);
 
                    return;
            }
        }

        private void generateSkill_1(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            switch(attackType)
            {
                case 0:
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<UpperSlash>(), damage, knockback, Main.myPlayer);  
                     
                    return;
                default:
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Pierce_s1>(), damage, knockback, Main.myPlayer); 
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<PierceVfx_s1>(), damage, knockback, Main.myPlayer);   
                    return;
            }
        }


        public override void UpdateInventory(Player player) {
            //combo
			if (comboExpireTimer++ >= 120) // after 120 ticks (== 2 seconds) in inventory, reset the attack pattern
			{
                stageChange = 0;
                attackType = 0;
                stage = 2;
            }	

            if(player.HeldItem == Item)
            {   
                if(itemHeld == false)
                {
                    IEntitySource source = null;
                    Vector2 position = player.position;
                    Vector2 velocity = new Vector2(0, 0);
                    int damage = 0;
                    float knockBack = 0;
                    
                    // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CoffinIdle>(), damage, knockBack, Main.myPlayer);
                    // Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ChainIdle>(), damage, knockBack, Main.myPlayer);
                    itemHeld = true;
                }
            }
            else
            {
                itemHeld = false;
            }
            
		}

        public override bool MeleePrefix() {
			return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)
		}

        // Creating item craft
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DirtBlock, 1); // Also, we are using vanilla material to craft, 3 Wood
            recipe.Register();
        }        
    }
}