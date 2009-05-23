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

            backgroundMusic = sounds.GetString("background_music");
            caveBackgroundLoop = sounds.GetString("cave_background_loop");
            lavaBackgroundLoop = sounds.GetString("lava_background_loop");
            meleeHit = sounds.GetString("melee_hit");
            flameThrowerStart = sounds.GetString("flame_thrower_start");
            flameThrowerLoop = sounds.GetString("flame_thrower_loop");
            flameThrowerEnd = sounds.GetString("flame_thrower_end");
            iceSpikeFire = sounds.GetString("ice_spike_fire");
            iceSpikeFlying = sounds.GetString("ice_spike_flying");
            iceSpikeExplosionOnPlayer = sounds.GetString("ice_spike_explosion_on_player");
            iceSpikeExplosionOnEnvironment = sounds.GetString("ice_spike_explosion_on_environment");
            takeDamage = sounds.GetString("take_damage");
            respawn = sounds.GetString("respawn");
            powerupHealthTaken = sounds.GetString("powerup_health_taken");
            powerupEnergyTaken = sounds.GetString("powerup_energy_taken");
            powerupLifeTaken = sounds.GetString("powerup_life_taken");
            islandHitsIsland = sounds.GetString("island_hits_island");
            andTheWinnerIs = sounds.GetString("and_the_winner_is");
            walk = sounds.GetString("walk");
            jumpStart = sounds.GetString("jump_start");
            jumpEnd = sounds.GetString("jump_end");
            repulsionStart = sounds.GetString("repulsion_start");
            repulsionLoop = sounds.GetString("repulsion_loop");
            repulsionEnd = sounds.GetString("repulsion_end");
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

        private string backgroundMusic;
        private string caveBackgroundLoop;
        private string lavaBackgroundLoop;

        private string meleeHit;
        private string flameThrowerStart;
        private string flameThrowerLoop;
        private string flameThrowerEnd;
        private string iceSpikeFire;
        private string iceSpikeFlying;
        private string iceSpikeExplosionOnPlayer;
        private string iceSpikeExplosionOnEnvironment;

        private string takeDamage;
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

        // menu is not maintained in this class since it is not part of 
        // the simulation!
    }
}
