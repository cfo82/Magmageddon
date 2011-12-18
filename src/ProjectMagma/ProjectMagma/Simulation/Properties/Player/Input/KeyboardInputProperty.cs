using System;
using ProjectMagma.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation
{

    public class KeyboardInputProperty: InputProperty
    {
        #region button assignments
        // eps for registering move
        private static readonly float StickMovementEps = 0.1f;

        private static float gamepadEmulationValue = -1f;
        private static readonly float HitButtonTimeout = 400;

        // keyboard keys
        private static readonly Keys JetpackKey = Keys.Space;
        private static readonly Keys IceSpikeKey = Keys.Q;
        private static readonly Keys HitKey = Keys.E;
        private static readonly Keys FlamethrowerKey = Keys.R;
        private static readonly Keys RunKey = Keys.LeftControl;
        #endregion

        private KeyboardControllerInput controllerInput;

        override internal ControllerInput ControllerInput
        {
            get { return controllerInput; }
        }

        public KeyboardInputProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            (player as Entity).Update += OnUpdate;
            this.controllerInput = new KeyboardControllerInput((PlayerIndex)player.GetInt(CommonNames.GamePadIndex));
        }

        public override void OnDetached(AbstractEntity player)
        {
            (player as Entity).Update -= OnUpdate;
        }

        private void OnUpdate(Entity player, SimulationTime simTime)
        {
            controllerInput.Update(simTime);
        }

        private class KeyboardControllerInput: ControllerInput
        {
            public KeyboardControllerInput(PlayerIndex playerIndex)
            {
                this.playerIndex = playerIndex;
            }

            private readonly PlayerIndex playerIndex;

            private KeyboardState oldKBState;

            private KeyboardState keyboardState;

            public void Update(SimulationTime simTime)
            {
                keyboardState = Keyboard.GetState(playerIndex);

                if (keyboardState.IsKeyDown(Keys.A))
                {
                    leftStickX = gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.D))
                    {
                        leftStickX = -gamepadEmulationValue;
                    }

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    leftStickY = gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.S))
                    {
                        leftStickY = -gamepadEmulationValue;
                    }

                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    rightStickX = gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        rightStickX = -gamepadEmulationValue;
                    }

                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    rightStickY = -gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        rightStickY = gamepadEmulationValue;
                    }

                moveStickMoved = leftStickX > StickMovementEps || leftStickX < -StickMovementEps
                    || leftStickY > StickMovementEps || leftStickY < -StickMovementEps;
                rightStickMoved = rightStickX > StickMovementEps || rightStickX < -StickMovementEps
                    || rightStickY > StickMovementEps || rightStickY < -StickMovementEps;
                flameStickMoved = flameStickX > StickMovementEps || flameStickX < -StickMovementEps
                    || flameStickY > StickMovementEps || flameStickY < -StickMovementEps;


                SetStates(JetpackKey, out repulsionButtonPressed, out repulsionButtonHold, out repulsionButtonReleased);
                SetStates(JetpackKey, out jumpButtonPressed, out jumpButtonHold, out jumpButtonReleased);
                SetStates(IceSpikeKey, out iceSpikeButtonPressed, out iceSpikeButtonHold, out iceSpikeButtonReleased);
                SetStates(HitKey, out hitButtonPressed, out hitButtonHold, out hitButtonReleased);
                SetStates(FlamethrowerKey, out flamethrowerButtonPressed, out flamethrowerButtonHold, out flamethrowerButtonReleased);
                SetStates(RunKey, out runButtonPressed, out runButtonHold, out runButtonReleased);

                if (hitButtonPressed)
                {
                    // filter pressed events
                    if (simTime.At < hitButtonPressedAt + HitButtonTimeout)
                    {
                        hitButtonPressed = false;
                    }
                    else
                    {
                        hitButtonPressedAt = simTime.At;
                    }
                }

                oldKBState = keyboardState;
            }

            private void SetStates(Keys key,
                out bool pressedIndicator,
                out bool holdIndicator,
                out bool releasedIndicator)
            {
                pressedIndicator = false;
                releasedIndicator = false;
                holdIndicator = false;
            }

            private bool GetPressed(Keys key)
            {
                return keyboardState.IsKeyDown(key)
                    && oldKBState.IsKeyUp(key);
            }

            private bool GetReleased(Keys key)
            {
                return keyboardState.IsKeyUp(key)
                    && oldKBState.IsKeyDown(key);
            }

            private bool GetHold(Keys key)
            {
                return keyboardState.IsKeyDown(key)
                    && oldKBState.IsKeyDown(key);
            }

        }


    }
}

