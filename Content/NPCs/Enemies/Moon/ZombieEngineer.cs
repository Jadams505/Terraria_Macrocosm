using Macrocosm.Common.Sets;
using Macrocosm.Content.Biomes;
using Macrocosm.Content.Projectiles.Hostile;
using System;
using System.IO;
using Macrocosm.Content.Dusts;
using Macrocosm.Common.Subworlds;
using Macrocosm.Common.Utils;
using Terraria;
using Terraria.ID;
using Macrocosm.Content.Tiles.Blocks.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using System;
namespace Macrocosm.Content.NPCs.Enemies.Moon
{
    public class ZombieEngineer : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 24;

            NPCSets.MoonNPC[Type] = true;
            NPCSets.DropsMoonstone[Type] = true;
        }
        private readonly Range HeadRaiseFrame = 0..2;
        private readonly Range IdleFrames = 3..14;
        private readonly Range AttackingFrame = 15..23;
        private readonly int AirFrame = 23;
       

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 44;
            NPC.damage = 80;
            NPC.defense = 60;
            NPC.lifeMax = 550;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.01f;
            NPC.aiStyle =-1;
      
            SpawnModBiomes = [ModContent.GetInstance<MoonUndergroundBiome>().Type];
            Banner = Item.NPCtoBanner(NPCID.Zombie);
            BannerItem = Item.BannerToItem(Banner);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return (spawnInfo.SpawnTileY > Main.rockLayer && spawnInfo.SpawnTileType == ModContent.TileType<Protolith>()) ? 0.02f : 0f;
        }

        public override void PostAI()
        {
            if (NPC.velocity.Y < 0f)
                NPC.velocity.Y += 0.1f;

        }
        public enum ActionState
        {
            Idle,
            RaiseHead,
            Attack,
            Jump
        }

        public ActionState AI_State
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        public bool RaisedHead=false;
        public ref float Timer => ref NPC.ai[1];
        public ref float ExplosionTimer => ref NPC.ai[2];
        bool GasLeak=false;
        public Vector2 IdleDirection = new Vector2(1,0);
        float RunSpeed=0.01f;
        int HeadTimer=0;
        public override void AI()
        {
            NPC.TargetClosest();
            Player target = Main.player[NPC.target];
            bool clearLineOfSight = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height);
            if (AI_State != ActionState.Jump&&AI_State!=ActionState.RaiseHead){
            if (Vector2.Distance(NPC.Center, target.Center) <600f&&clearLineOfSight)
            {
                if(RaisedHead)
                    AI_State=ActionState.Attack;
                else
                    AI_State=ActionState.RaiseHead;
            }
            else{
                AI_State=ActionState.Idle;
            }
            }
           
            switch(AI_State)
            {
                case ActionState.Idle:
                    Idle();
                    break;
                case ActionState.RaiseHead:
                    RaiseHead();
                    break;
                case ActionState.Attack:
                    Attack();
                    break;
                case ActionState.Jump:
                    Jump();
                    break;
            }
            Timer++;
            if(GasLeak)
                if(Timer%2==0&&AI_State != ActionState.Idle)
                    ExplosionTimer++;
            if(ExplosionTimer>255||(AI_State==ActionState.Jump &&Vector2.Distance(NPC.Center, target.Center) <50f)||(AI_State==ActionState.Jump &&NPC.velocity.X==0f)){
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                var entitySource = NPC.GetSource_Death();
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), 100, 0f, Main.myPlayer);
                Gore.NewGore(entitySource, NPC.position, NPC.velocity*0.1f, Mod.Find<ModGore>("ZombieEngineerGore1").Type);
                Gore.NewGore(entitySource, NPC.position, NPC.velocity*0.1f, Mod.Find<ModGore>("ZombieEngineerGore2").Type);
                Gore.NewGore(entitySource, NPC.position, NPC.velocity*0.1f, Mod.Find<ModGore>("ZombieEngineerGore3").Type);
                Gore.NewGore(entitySource, NPC.position, NPC.velocity*0.1f, Mod.Find<ModGore>("ZombieEngineerGore4").Type);
                }
                NPC.active=false;
                NPC.life=0;
            }
        }
        
        public void Idle(){
            if (Timer%120f==0f&&Main.rand.NextBool(5))
            {
                if(Main.rand.NextBool(2))
                    IdleDirection=new Vector2(1,0);
                else
                    IdleDirection=new Vector2(-1,0);
            }

            Utility.AIFighter(NPC, ref NPC.ai, NPC.Center+IdleDirection);
        }
        public void Attack()
        {
            Player player = Main.player[NPC.target];
            bool clearLineOfSight = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);
            Utility.AIFighter(NPC, ref NPC.ai, player.Center,moveInterval:RunSpeed,velMax:4f,maxJumpTilesX:3,maxJumpTilesY:1);
            if (RunSpeed<3f)
                RunSpeed+=0.01f;
            if(Vector2.Distance(NPC.Center, player.Center) <300f)
                GasLeak=true;
            if (NPC.velocity.Y==0){
            if(ExplosionTimer>150)
            {
                if(Vector2.Distance(NPC.Center, player.Center) <300f){
                NPC.velocity.X += (player.Center.X - NPC.Center.X) * 0.05f;
                NPC.velocity.Y += ((player.Center.Y - NPC.Center.Y) * 0.055f);
                AI_State=ActionState.Jump;
                }
            }   
            }
        }
        int NpcFrame=0;
        public void RaiseHead(){
            if(RaisedHead==false){
                RaisedHead=true;
                NPC.frame.Y =0;
            }
            NPC.velocity.X=0f;
            HeadTimer++;
            if (HeadTimer%25==24){
                NpcFrame++;
            }
            if(NpcFrame>3){
                AI_State=ActionState.Attack;
            }
        }
        public void Jump(){
            
            NPC.rotation=NPC.velocity.ToRotation();
        }

        public override void FindFrame(int frameHeight)//AAAAAAAAAAA
        {
            NPC.spriteDirection = NPC.velocity.X>0f ? -1:1;
            int frameIndex = NPC.frame.Y / frameHeight;
            NpcFrame=frameIndex;
            if (NPC.velocity.Y == 0f)
            {
                if (AI_State == ActionState.RaiseHead)
                {
                NPC.frame.Y = frameHeight * NpcFrame;
                       
                }
                // Walking animation 
                if (AI_State == ActionState.Idle)
                {
                    // Reset walking 
                    if (!IdleFrames.Contains(frameIndex))
                        NPC.frame.Y = frameHeight * IdleFrames.Start.Value;

                    // Walking animation frame counter, accounting for walk speed
                    NPC.frameCounter += Math.Abs(NPC.velocity.X);

                    // Update frame
                    if (NPC.frameCounter >15.0)
                    {
                        NPC.frame.Y += frameHeight;
                        NPC.frameCounter = 0.0;
                    }

                    if (frameIndex >= IdleFrames.End.Value)
                        NPC.frame.Y = frameHeight * IdleFrames.Start.Value;
                }
                if (AI_State == ActionState.Attack)
                {
            
                    if (!AttackingFrame.Contains(frameIndex))
                        NPC.frame.Y = frameHeight * AttackingFrame.Start.Value;

                    // Walking animation frame counter, accounting for walk speed
                    NPC.frameCounter += Math.Abs(NPC.velocity.X)*2f;

                    // Update frame
                    if (NPC.frameCounter >24.0)
                    {
                        NPC.frame.Y += frameHeight;
                        NPC.frameCounter = 0.0;
                    }

                    if (frameIndex >= AttackingFrame.End.Value)
                        NPC.frame.Y = frameHeight * AttackingFrame.Start.Value;
                }
                
            }
            // Air-borne frame
            else if (MathF.Abs(NPC.velocity.Y) > 1f)
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y = frameHeight * AirFrame;
            }
            
        }

        public override void ModifyNPCLoot(NPCLoot loot)
        {
        }
        public override void PostDraw(SpriteBatch spriteBatch,Vector2 screenPos,Color drawColor){
            Texture2D texture =  ModContent.Request<Texture2D>("Macrocosm/Content/NPCs/Enemies/Moon/ZombieEngineerFuelLeak").Value;
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, NPC.frame, Color.White *(float)(ExplosionTimer/255), 0f, NPC.frame.Size()*0.5f, NPC.scale, effects, 0f);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; i < 30; i++)
                { 
                    int dustType = Utils.SelectRandom<int>(Main.rand, ModContent.DustType<RegolithDust>(), DustID.Blood);

                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);
                    dust.velocity.X *= (dust.velocity.X + +Main.rand.Next(0, 100) * 0.015f) * hit.HitDirection;
                    dust.velocity.Y = 3f + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    dust.noGravity = true;
                }
            }

            if (Main.dedServ)
                return; // don't run on the server

            if (NPC.life <= 0)
            {
                var entitySource = NPC.GetSource_Death();
                Gore.NewGore(entitySource, NPC.position, NPC.velocity, Mod.Find<ModGore>("ZombieEngineerGore1").Type);
                Gore.NewGore(entitySource, NPC.position, NPC.velocity, Mod.Find<ModGore>("ZombieEngineerGore2").Type);
                Gore.NewGore(entitySource, NPC.position, NPC.velocity, Mod.Find<ModGore>("ZombieEngineerGore3").Type);
                Gore.NewGore(entitySource, NPC.position, NPC.velocity, Mod.Find<ModGore>("ZombieEngineerGore4").Type);
            }
        }
    }
}
