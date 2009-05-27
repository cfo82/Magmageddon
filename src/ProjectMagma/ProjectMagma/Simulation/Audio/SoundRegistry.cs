using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Simulation
{
    public class SoundRegistry
    {
        public SoundRegistry()
        {
        }

        public void Load()
        {
            Entity sounds = Game.Instance.Simulation.EntityManager["sound"];

            backgroundMusic = sounds.HasString(BackgroundMusicKey) ? sounds.GetString(BackgroundMusicKey) : "";
            caveBackgroundLoop = sounds.HasString(CaveBackgroundLoopKey) ? sounds.GetString(CaveBackgroundLoopKey) : "";
            lavaBackgroundLoop = sounds.HasString(LavaBackgroundLoopKey) ? sounds.GetString(LavaBackgroundLoopKey) : "";
            meleeHit = sounds.HasString(MeleeHitKey) ? sounds.GetString(MeleeHitKey) : "";
            meleeNotHit = sounds.HasString(MeleeNotHitKey) ? sounds.GetString(MeleeNotHitKey) : "";
            flameThrowerStart = sounds.HasString(FlameThrowerStartKey) ? sounds.GetString(FlameThrowerStartKey) : "";
            flameThrowerLoop = sounds.HasString(FlameThrowerLoopKey) ? sounds.GetString(FlameThrowerLoopKey) : "";
            flameThrowerEnd = sounds.HasString(FlameThrowerEndKey) ? sounds.GetString(FlameThrowerEndKey) : "";
            iceSpikeFire = sounds.HasString(IceSpikeFireKey) ? sounds.GetString(IceSpikeFireKey) : "";
            iceSpikeFlying = sounds.HasString(IceSpikeFlyingKey) ? sounds.GetString(IceSpikeFlyingKey) : "";
            iceSpikeExplosionOnPlayer = sounds.HasString(IceSpikeExplosionOnPlayerKey) ? sounds.GetString(IceSpikeExplosionOnPlayerKey) : "";
            iceSpikeExplosionOnEnvironment = sounds.HasString(IceSpikeExplosionOnEnvironmentKey) ? sounds.GetString(IceSpikeExplosionOnEnvironmentKey) : "";
            takeDamage = sounds.HasString(TakeDamageKey) ? sounds.GetString(TakeDamageKey) : "";
            playerDies = sounds.HasString(PlayerDiesKey) ? sounds.GetString(PlayerDiesKey) : "";
            respawn = sounds.HasString(RespawnKey) ? sounds.GetString(RespawnKey) : "";
            powerupHealthTaken = sounds.HasString(PowerupHealthTakenKey) ? sounds.GetString(PowerupHealthTakenKey) : "";
            powerupEnergyTaken = sounds.HasString(PowerupEnergyTakenKey) ? sounds.GetString(PowerupEnergyTakenKey) : "";
            powerupLifeTaken = sounds.HasString(PowerupLifeTakenKey) ? sounds.GetString(PowerupLifeTakenKey) : "";
            islandHitsIsland = sounds.HasString(IslandHitsIslandKey) ? sounds.GetString(IslandHitsIslandKey) : "";
            andTheWinnerIs = sounds.HasString(AndTheWinnerIsKey) ? sounds.GetString(AndTheWinnerIsKey) : "";
            walk = sounds.HasString(WalkKey) ? sounds.GetString(WalkKey) : "";
            jumpStart = sounds.HasString(JumpStartKey) ? sounds.GetString(JumpStartKey) : "";
            jumpEnd = sounds.HasString(JumpEndKey) ? sounds.GetString(JumpEndKey) : "";
            repulsionStart = sounds.HasString(RepulsionStartKey) ? sounds.GetString(RepulsionStartKey) : "";
            repulsionLoop = sounds.HasString(RepulsionLoopKey) ? sounds.GetString(RepulsionLoopKey) : "";
            repulsionEnd = sounds.HasString(RepulsionEndKey) ? sounds.GetString(RepulsionEndKey) : "";
            jetpackStart = sounds.HasString(JetpackStartKey) ? sounds.GetString(JetpackStartKey) : "";
            jetpackLoop = sounds.HasString(JetpackLoopKey) ? sounds.GetString(JetpackLoopKey) : "";
            jetpackEnd = sounds.HasString(JetpackEndKey) ? sounds.GetString(JetpackEndKey) : "";
        }

        public string BackgroundMusic
        {
            get { return backgroundMusic; }
        }

        public string CaveBackgroundLoop
        {
            get { return caveBackgroundLoop; }
        }

        public string LavaBackgroundLoop
        {
            get { return lavaBackgroundLoop; }
        }

        public string MeleeHit
        {
            get { return meleeHit; }
        }

        public string MeleeNotHit
        {
            get { return meleeNotHit; }
        }

        public string FlameThrowerStart
        {
            get { return flameThrowerStart; }
        }

        public string FlameThrowerLoop
        {
            get { return flameThrowerLoop; }
        }

        public string FlameThrowerEnd
        {
            get { return flameThrowerEnd; }
        }

        public string IceSpikeFire
        {
            get { return iceSpikeFire; }
        }

        public string IceSpikeFlying
        {
            get { return iceSpikeFlying; }
        }

        public string IceSpikeExplosionOnPlayer
        {
            get { return iceSpikeExplosionOnPlayer; }
        }

        public string IceSpikeExplosionOnEnvironment
        {
            get { return iceSpikeExplosionOnEnvironment; }
        }

        public string TakeDamage
        {
            get { return takeDamage; }
        }

        public string PlayerDies
        {
            get { return playerDies; }
        }

        public string Respawn
        {
            get { return respawn; }
        }

        public string PowerupHealthTaken
        {
            get { return powerupHealthTaken; }
        }

        public string PowerupEnergyTaken
        {
            get { return powerupEnergyTaken; }
        }

        public string PowerupLifeTaken
        {
            get { return powerupLifeTaken; }
        }

        public string IslandHitsIsland
        {
            get { return islandHitsIsland; }
        }

        public string AndTheWinnerIs
        {
            get { return andTheWinnerIs; }
        }

        public string Walk
        {
            get { return walk; }
        }

        public string JumpStart
        {
            get { return jumpStart; }
        }

        public string JumpEnd
        {
            get { return jumpEnd; }
        }

        public string RepulsionStart
        {
            get { return repulsionStart; }
        }

        public string RepulsionLoop
        {
            get { return repulsionLoop; }
        }

        public string RepulsionEnd
        {
            get { return repulsionEnd; }
        }

        public string JetpackStart
        {
            get { return jetpackStart; }
        }

        public string JetpackLoop
        {
            get { return jetpackLoop; }
        }

        public string JetpackEnd
        {
            get { return jetpackEnd; }
        }

        private string backgroundMusic;
        private string caveBackgroundLoop;
        private string lavaBackgroundLoop;

        private string meleeHit;
        private string meleeNotHit;
        private string flameThrowerStart;
        private string flameThrowerLoop;
        private string flameThrowerEnd;
        private string iceSpikeFire;
        private string iceSpikeFlying;
        private string iceSpikeExplosionOnPlayer;
        private string iceSpikeExplosionOnEnvironment;

        private string takeDamage;
        private string playerDies;
        private string respawn;
        private string powerupHealthTaken;
        private string powerupEnergyTaken;
        private string powerupLifeTaken;
        private string islandHitsIsland;
        private string andTheWinnerIs;

        private string walk;
        private string jumpStart;
        private string jumpEnd;
        private string repulsionStart;
        private string repulsionLoop;
        private string repulsionEnd;
        private string jetpackStart;
        private string jetpackLoop;
        private string jetpackEnd;

        // menu is not maintained in this class since it is not part of 
        // the simulation!

        private static readonly string BackgroundMusicKey = "background_music";
        private static readonly string CaveBackgroundLoopKey = "cave_background_loop";
        private static readonly string LavaBackgroundLoopKey = "lava_background_loop";
        private static readonly string MeleeHitKey = "melee_hit";
        private static readonly string MeleeNotHitKey = "melee_not_hit";
        private static readonly string FlameThrowerStartKey = "flame_thrower_start";
        private static readonly string FlameThrowerLoopKey = "flame_thrower_loop";
        private static readonly string FlameThrowerEndKey = "flame_thrower_end";
        private static readonly string IceSpikeFireKey = "ice_spike_fire";
        private static readonly string IceSpikeFlyingKey = "ice_spike_flying";
        private static readonly string IceSpikeExplosionOnPlayerKey = "ice_spike_explosion_on_player";
        private static readonly string IceSpikeExplosionOnEnvironmentKey = "ice_spike_explosion_on_environment";
        private static readonly string TakeDamageKey = "take_damage";
        private static readonly string PlayerDiesKey = "player_dies";
        private static readonly string RespawnKey = "respawn";
        private static readonly string PowerupHealthTakenKey = "powerup_health_taken";
        private static readonly string PowerupEnergyTakenKey = "powerup_energy_taken";
        private static readonly string PowerupLifeTakenKey = "powerup_life_taken";
        private static readonly string IslandHitsIslandKey = "island_hits_island";
        private static readonly string AndTheWinnerIsKey = "and_the_winner_is";
        private static readonly string WalkKey = "walk";
        private static readonly string JumpStartKey = "jump_start";
        private static readonly string JumpEndKey = "jump_end";
        private static readonly string RepulsionStartKey = "repulsion_start";
        private static readonly string RepulsionLoopKey = "repulsion_loop";
        private static readonly string RepulsionEndKey = "repulsion_end";
        private static readonly string JetpackStartKey = "jetpack_start";
        private static readonly string JetpackLoopKey = "jetpack_loop";
        private static readonly string JetpackEndKey = "jetpack_end";
    }
}
