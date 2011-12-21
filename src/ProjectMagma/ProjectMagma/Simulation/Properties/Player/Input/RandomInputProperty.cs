using System;
using ProjectMagma.Framework;

namespace ProjectMagma.Simulation
{

    public class RandomInputProperty : InputProperty
    {
        private RandomControllerInput controllerInput = new RandomControllerInput();

        override internal ControllerInput ControllerInput
        {
            get { return controllerInput; }
        }

        public RandomInputProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            (player as Entity).OnUpdate += OnUpdate;
        }

        public override void OnDetached(AbstractEntity player)
        {
            (player as Entity).OnUpdate -= OnUpdate;
        }

        private void OnUpdate(Entity player, SimulationTime simTime)
        {
            controllerInput.Update(simTime);
        }

        private class RandomControllerInput : ControllerInput
        {
            public void Update(SimulationTime simTime)
            {
                Random rand = new Random();
                if (simTime.At < iceSpikeShotAt + 2000 + rand.Next(500))
                {
                    iceSpikeButtonPressed = true;
                    iceSpikeShotAt = simTime.At;
                }
                if (simTime.At < flameThrowerActivatedAt + 200 + rand.Next(100))
                {
                    flamethrowerButtonHold = true;
                    flameThrowerActivatedAt = simTime.At;
                }
                if (simTime.At < jumpButtonPressedAt + 1000 + rand.Next(1000))
                {
                    jumpButtonPressed = true;
                    jumpButtonPressedAt = simTime.At;
                }
                moveStickMoved = true;
                leftStickX = (float)rand.NextDouble();
                leftStickY = (float)rand.NextDouble();
            }

            private float iceSpikeShotAt, flameThrowerActivatedAt, jumpButtonPressedAt = float.NegativeInfinity;
        }

    }
}

