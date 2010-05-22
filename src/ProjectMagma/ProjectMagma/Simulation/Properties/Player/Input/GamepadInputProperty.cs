using System;
using ProjectMagma.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation
{

    public class GamePadInputProperty: InputProperty
    {

        #region button assignments
        // eps for registering move
        private static readonly float StickMovementEps = 0.1f;

        // gamepad buttons
        private static readonly float HitButtonTimeout = 400;
        private static readonly Buttons[] RepulsionButtons = { Buttons.LeftTrigger };
        private static readonly Buttons[] jumpButtons = { Buttons.A };
        private static readonly Buttons[] IceSpikeButtons = { Buttons.X };
        private static readonly Buttons[] FlamethrowerButtons = { Buttons.Y };
        private static readonly Buttons[] HitButtons = { Buttons.B };
        private static readonly Buttons[] RunButtons = { Buttons.RightTrigger };

        // keyboard keys
        private static readonly Keys JetpackKey = Keys.Space;
        private static readonly Keys IceSpikeKey = Keys.Q;
        private static readonly Keys HitKey = Keys.E;
        private static readonly Keys FlamethrowerKey = Keys.R;
        private static readonly Keys RunKey = Keys.LeftControl;
        #endregion

        private GamePadControllerInput controllerInput;

        override internal ControllerInput ControllerInput
        {
            get { return controllerInput; }
        }

        public GamePadInputProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            (player as Entity).Update += OnUpdate;
            this.controllerInput = new GamePadControllerInput((PlayerIndex)player.GetInt(CommonNames.GamePadIndex));
        }

        public override void OnDetached(AbstractEntity player)
        {
            (player as Entity).Update -= OnUpdate;
        }

        private void OnUpdate(Entity player, SimulationTime simTime)
        {
            controllerInput.Update(simTime);
        }

        private class GamePadControllerInput: ControllerInput
        {
            public GamePadControllerInput(PlayerIndex playerIndex)
            {
                this.playerIndex = playerIndex;
            }

            private readonly PlayerIndex playerIndex;

            private GamePadState oldGPState;
            private KeyboardState oldKBState;

            private GamePadState gamePadState;
            private KeyboardState keyboardState;

            public void Update(SimulationTime simTime)
            {
                gamePadState = GamePad.GetState(playerIndex);
                keyboardState = Keyboard.GetState(playerIndex);

                #region joysticks

                leftStickX = gamePadState.ThumbSticks.Left.X;
                leftStickY = -gamePadState.ThumbSticks.Left.Y;
                if (PlayerControllerProperty.LeftStickSelection)
                {
                    rightStickX = leftStickX;
                    rightStickY = leftStickY;
                }
                else
                {
                    rightStickX = gamePadState.ThumbSticks.Right.X;
                    rightStickY = -gamePadState.ThumbSticks.Right.Y;
                }

                flameStickX = gamePadState.ThumbSticks.Right.X;
                flameStickY = -gamePadState.ThumbSticks.Right.Y;

                dPadX = (gamePadState.DPad.Right == ButtonState.Pressed) ? 1.0f : 0.0f
                    - ((gamePadState.DPad.Left == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadY = (gamePadState.DPad.Down == ButtonState.Pressed) ? 1.0f : 0.0f
                    - ((gamePadState.DPad.Up == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadPressed = dPadX != 0 || dPadY != 0;

                moveStickMoved = leftStickX > StickMovementEps || leftStickX < -StickMovementEps
                    || leftStickY > StickMovementEps || leftStickY < -StickMovementEps;
                rightStickMoved = rightStickX > StickMovementEps || rightStickX < -StickMovementEps
                    || rightStickY > StickMovementEps || rightStickY < -StickMovementEps;
                flameStickMoved = flameStickX > StickMovementEps || flameStickX < -StickMovementEps
                    || flameStickY > StickMovementEps || flameStickY < -StickMovementEps;

                #endregion

                #region action buttons

                SetStates(RepulsionButtons, JetpackKey, out repulsionButtonPressed, out repulsionButtonHold, out repulsionButtonReleased);
                SetStates(jumpButtons, JetpackKey, out jumpButtonPressed, out jumpButtonHold, out jumpButtonReleased);
                SetStates(IceSpikeButtons, IceSpikeKey, out iceSpikeButtonPressed, out iceSpikeButtonHold, out iceSpikeButtonReleased);
                SetStates(HitButtons, HitKey, out hitButtonPressed, out hitButtonHold, out hitButtonReleased);
                SetStates(FlamethrowerButtons, FlamethrowerKey, out flamethrowerButtonPressed, out flamethrowerButtonHold, out flamethrowerButtonReleased);
                SetStates(RunButtons, RunKey, out runButtonPressed, out runButtonHold, out runButtonReleased);

                #endregion

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

                oldGPState = gamePadState;
                oldKBState = keyboardState;
            }

            private void SetStates(Buttons[] buttons, Keys key,
                out bool pressedIndicator,
                out bool holdIndicator,
                out bool releasedIndicator)
            {
                pressedIndicator = false;
                releasedIndicator = false;
                holdIndicator = false;

                for (int i = 0; i < buttons.Length; i++)
                {
                    pressedIndicator |= GetPressed(buttons[i]);
                    releasedIndicator |= GetReleased(buttons[i]);
                    holdIndicator |= GetHold(buttons[i]);
                }

                pressedIndicator |= GetPressed(key);
                releasedIndicator |= GetReleased(key);
                holdIndicator |= GetHold(key);
            }

            private bool GetPressed(Buttons button)
            {
                return gamePadState.IsButtonDown(button)
                    && oldGPState.IsButtonUp(button);
            }

            private bool GetReleased(Buttons button)
            {
                return gamePadState.IsButtonUp(button)
                    && oldGPState.IsButtonDown(button);
            }

            private bool GetHold(Buttons button)
            {
                return gamePadState.IsButtonDown(button)
                    && oldGPState.IsButtonDown(button);
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

