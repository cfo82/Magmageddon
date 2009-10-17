using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.Math.Primitives;
using System.Diagnostics;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.Simulation
{

    internal class ControllerInput
    {
        private GamePadState oldGPState;
        private KeyboardState oldKBState;

        private GamePadState gamePadState;
        private KeyboardState keyboardState;

        public void Reset()
        {
            leftStickX = leftStickY = 0;
            rightStickX = rightStickY = 0;
            flameStickX = flameStickY = 0;
            dPadX = dPadY = 0;

            moveStickMoved = flameStickMoved = dPadPressed = false;
            runButtonPressed = repulsionButtonPressed = jumpButtonPressed = flamethrowerButtonPressed = iceSpikeButtonPressed = hitButtonPressed = false;
            runButtonReleased = repulsionButtonReleased = jumpButtonReleased = flamethrowerButtonReleased = iceSpikeButtonReleased = hitButtonReleased = false;
            runButtonHold = repulsionButtonHold = jumpButtonHold = flamethrowerButtonHold = iceSpikeButtonHold = hitButtonHold = false;
        }

        // joystick
        public float leftStickX, leftStickY;
        public bool moveStickMoved;
        public float rightStickX, rightStickY;
        public bool rightStickMoved;
        public float flameStickX, flameStickY;
        public bool flameStickMoved;
        public bool dPadPressed;
        public float dPadX, dPadY;

        // buttons
        public bool runButtonPressed, repulsionButtonPressed, jumpButtonPressed, flamethrowerButtonPressed,
            iceSpikeButtonPressed, hitButtonPressed;
        public bool runButtonReleased, repulsionButtonReleased, jumpButtonReleased, flamethrowerButtonReleased,
            iceSpikeButtonReleased, hitButtonReleased;
        public bool runButtonHold, repulsionButtonHold, jumpButtonHold, flamethrowerButtonHold,
            iceSpikeButtonHold, hitButtonHold;

        // times
        public float hitButtonPressedAt = float.NegativeInfinity;
    }
  

    public abstract class InputProperty : Property
    {

        public InputProperty()
        {
        }

        internal abstract ControllerInput ControllerInput
        {
            get; 
        }

        public abstract void OnAttached(AbstractEntity entity);

        public abstract void OnDetached(AbstractEntity entity);

    }
}

