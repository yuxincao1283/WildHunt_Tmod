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
    public class CoffinCaught : GlobalNPC
    {
        public override bool PreAI(NPC npc)
        {
            //modify ai here.
            if(WildHunt.coffinCaught == false || WildHunt.caughtNpc == null)
            {
                return true;
            }
            
            return false;
        }

    }
}