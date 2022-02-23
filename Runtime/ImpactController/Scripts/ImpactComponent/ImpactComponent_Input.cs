using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JTools
{
    public abstract class ImpactComponent_Input : ImpactComponent
    {
        //This is an abstract class you can use to implement your own input systems for Impact.
        //It explicitly exists because of how many different input solutions exist for Unity, so I wanted to create a more universal approach for Impact so you can have whatever input manager you need to operate the controller.

        //OUTPUTS
        public ImpactInputData inputData = new ImpactInputData(Vector3.zero); //A generalized struct containing input data for Impact.

        [HideInInspector]
        public UnityEvent onLock; //Runs when ChangeLockState is called and set to true.
        [HideInInspector]
        public UnityEvent onUnlock; //Runs when ChangeLockState is called and set to false.

        //INPUTS
        public bool lockInput = false; //Allows you to lock input. You can choose whether or not you want to opt into this system in your own components.


        public void Reset()
        {
            if(GetComponent<ImpactController>())
            {
                if(GetComponent<ImpactController>().inputComponent == null)
                {
                    GetComponent<ImpactController>().inputComponent = this;
                }
            }
        }

        public virtual void ChangeLockState(bool lockState)
        {
            if (lockState != lockInput)
            {
                if (lockState)
                    onLock.Invoke();
                else
                    onUnlock.Invoke();
            }

            lockInput = lockState;
        }

        public override void ComponentUpdate(ImpactController player)
        {
            base.ComponentUpdate(player);

            if (!lockInput)
                Controls();
            else
                ControlsLocked();
        }
    
        //This is used to read the inputs for all controls related to movement.
        public virtual void Controls()
        {
            //You can write whatever you need in here, if you're implementing a new input layout.
        }

        //This is commonly used to determine what the locking values should be when the player's movement is locked.
        //By default this'll disable everything. Feel free to override if you need non-zero default values for stuff.
        public virtual void ControlsLocked()
        {
            inputData.motionInput = Vector3.zero;
            inputData.mouseInput = Vector2.zero;

            inputData.pressedCrouch = false;
            if (inputData.holdingCrouch)
                inputData.releasedCrouch = true;
            else
                inputData.releasedCrouch = false;
            inputData.holdingCrouch = false;

            inputData.pressedSprint = false;
            if (inputData.holdingSprint)
                inputData.releasedSprint = true;
            else
                inputData.releasedSprint = false;
            inputData.holdingSprint = false;

            inputData.pressedJump = false;
            if (inputData.holdingJump)
                inputData.releasedJump = true;
            else
                inputData.releasedJump = false;
            inputData.holdingJump = false;

            inputData.pressedPrimary = false;
            if (inputData.holdingPrimary)
                inputData.releasedPrimary = true;
            else
                inputData.releasedPrimary = false;
            inputData.holdingPrimary = false;

            inputData.pressedSecondary = false;
            if (inputData.holdingSecondary)
                inputData.releasedSecondary = true;
            else
                inputData.releasedSecondary = false;
            inputData.holdingSecondary = false;

            inputData.pressedMenu = false;
            if (inputData.holdingMenu)
                inputData.releasedMenu = true;
            else
                inputData.releasedMenu = false;
            inputData.holdingMenu = false;
        }
    }

    public struct ImpactInputData
    {
        public Vector3 motionInput;
        public Vector2 mouseInput;

        public bool pressedJump;
        public bool holdingJump;
        public bool releasedJump;

        public bool pressedSprint;
        public bool holdingSprint;
        public bool releasedSprint;

        public bool pressedCrouch;
        public bool holdingCrouch;
        public bool releasedCrouch;

        public bool pressedPrimary;
        public bool holdingPrimary;
        public bool releasedPrimary;

        public bool pressedSecondary;
        public bool holdingSecondary;
        public bool releasedSecondary;

        public bool pressedMenu;
        public bool holdingMenu;
        public bool releasedMenu;

        public bool pressedInteract;
        public bool holdingInteract;
        public bool releasedInteract;

        public ImpactInputData(Vector3 initialMotion)
        {
            motionInput = initialMotion;
            mouseInput = Vector2.zero;

            pressedJump = false;
            holdingJump = false;
            releasedJump = false;

            pressedSprint = false;
            holdingSprint = false;
            releasedSprint = false;

            pressedCrouch = false;
            holdingCrouch = false;
            releasedCrouch = false;

            pressedPrimary = false;
            holdingPrimary = false;
            releasedPrimary = false;

            pressedSecondary = false;
            holdingSecondary = false;
            releasedSecondary = false;

            pressedMenu = false;
            holdingMenu = false;
            releasedMenu = false;

            pressedInteract = false;
            holdingInteract = false;
            releasedInteract = false;

        }
    }

    public enum ImpactInput_MouseSetting
    { //Enumerator for mouse buttons.

        leftMouse = 0,
        rightMouse = 1,
        middleMouse = 2,
        extraMouse1 = 3,
        extraMouse2 = 4,
        none = 5
    }

}