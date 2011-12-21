using System;
using ProjectMagma.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation
{

    public class GamePadInputProperty: InputProperty
    {

        #region button assignments
        private static readonly float StickMovementEps = 0.1f;

        private static readonly float HitButtonTimeout = 400;
        private static readonly Buttons[] RepulsionButtons = { Buttons.LeftTrigger };
        private static readonly Buttons[] jumpButtons = { Buttons.A };
        private static readonly Buttons[] IceSpikeButtons = { Buttons.X };
        private static readonly Buttons[] FlamethrowerButtons = { Buttons.Y };
        private static readonly Buttons[] HitButtons = { Buttons.B };
        private static readonly Buttons[] RunButtons = { Buttons.RightTrigger };
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
            (player as Entity).OnUpdate += OnUpdate;
            this.controllerInput = new GamePadControllerInput((PlayerIndex)player.GetInt(CommonNames.GamePadIndex));
        }

        public override void OnDetached(AbstractEntity player)
        {
            (player as Entity).OnUpdate -= OnUpdate;
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
            private GamePadState gamePadState;

            public void Update(SimulationTime simTime)
            {
                gamePadState = GamePad.GetState(playerIndex);

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

                SetStates(RepulsionButtons, out repulsionButtonPressed, out repulsionButtonHold, out repulsionButtonReleased);
                SetStates(jumpButtons, out jumpButtonPressed, out jumpButtonHold, out jumpButtonReleased);
                SetStates(IceSpikeButtons, out iceSpikeButtonPressed, out iceSpikeButtonHold, out iceSpikeButtonReleased);
                SetStates(HitButtons, out hitButtonPressed, out hitButtonHold, out hitButtonReleased);
                SetStates(FlamethrowerButtons, out flamethrowerButtonPressed, out flamethrowerButtonHold, out flamethrowerButtonReleased);
                SetStates(RunButtons, out runButtonPressed, out runButtonHold, out runButtonReleased);

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
            }

            private void SetStates(Buttons[] buttons,
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

        }


    }
}

