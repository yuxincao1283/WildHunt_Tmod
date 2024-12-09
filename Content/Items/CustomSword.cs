using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LimbusCompanyWildHunt.Content.Items
{
    public class customSword : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Reverse Sword");
            // Tooltip.SetDefault("A sword that swings backwards!");
        }

        public override void SetDefaults()
        {
            Item.damage = 50; // Set sword damage
            Item.DamageType = DamageClass.Melee;
            Item.width = 256; // Sword's hitbox width
            Item.height = 256; // Sword's hitbox height
            Item.useTime = 20; // The use time (speed)
            Item.useAnimation = 20; // The animation time
            Item.useStyle = ItemUseStyleID.Swing; // Swinging use style
            Item.knockBack = 6; // Knockback
            Item.value = 10000; // Value in copper coins
            Item.rare = ItemRarityID.Blue; // Rarity
            Item.UseSound = SoundID.Item1; // Use sound
            Item.autoReuse = true; // Automatically swing again when held
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // Reverse the swing by flipping the player's arm rotation
            float reverseRotation = (float)(Math.PI * (player.direction == 1 ? -1 : 1));
            player.itemRotation = reverseRotation * (player.itemAnimation / (float)player.itemAnimationMax);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddIngredient(ItemID.IronBar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}