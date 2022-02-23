using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JTools
{
    [System.Serializable]
    public sealed class LandingEvent : UnityEvent<float> { }

    public abstract class ImpactComponent_Motion : ImpactComponent
    {
        //OUTPUTS
        [HideInInspector]
        public bool isGrounded = false; //Whether or not the player is on the ground.
        [HideInInspector]
        public bool isCrouching = false;
        [HideInInspector]
        public bool isSprinting = false;
        [HideInInspector]
        public bool isSliding = false;

        [HideInInspector]
        public float topSpeed; //The maximum amount of speed the player may move at any time. Doesn't include vertical speed.

        [HideInInspector]
        public float walkTime = 0f; //Should be wrapped using a modulus function. Increases as the player moves. Used for view bobbing among other effects.

        [HideInInspector]
        public Vector3 movement; //A movement vector representing the current motion the player is making.

        [HideInInspector]
        public float currentSpeed = 5f;

        [HideInInspector]
        public LandingEvent onLanding = new LandingEvent(); //Runs whenever the player lands on the ground. Used in other components for various effects.

        [HideInInspector]
        public UnityEvent onJump = new UnityEvent(); //Runs whenever the player is able to successfully jump.

        [HideInInspector] public float crouchTime; //The transition for crouching, used for calculating curves.

        //INPUTS
        [HideInInspector]
        public Quaternion orientation; //This quaternion helps motion components figure out which direction they need to move relative to.

        [Header("Motion Component - General")]
        [Tooltip("How fast the player is when walking normally.")] public float moveSpeed = 5f;
        [Tooltip("How fast the player is when crouch-walking like a creepy little crab.")] public float crouchSpeed = 2f;
        [Tooltip("How fast the player moves when sprinting.")] public float sprintSpeed = 9f;
        [Space]
        [Tooltip("The amount of force the player jumps with.")] public float jumpPower = 12f;
        [Space]
        [Tooltip("Sometimes you don't want to be able to walk on anything! You can use this to disable grounding on different layers if you need to.")] public LayerMask groundingLayers = ~0;
        [Space]
        [Tooltip("Check the internal code of ImpactComponent_Motion for more information on how these work. They'll be at the bottom of the script.")] public ImpactMotion_CrouchSetting crouchMode = ImpactMotion_CrouchSetting.normal;
        [Tooltip("Check the internal code of ImpactComponent_Motion for more information on how these work. They'll be at the bottom of the script.")] public ImpactMotion_SprintSetting sprintMode = ImpactMotion_SprintSetting.normal;
        [Tooltip("Check the internal code of ImpactComponent_Motion for more information on how these work. They'll be at the bottom of the script.")] public ImpactMotion_JumpSetting jumpMode = ImpactMotion_JumpSetting.normal;
        [Space]
        [Tooltip("The speed the crouch transtition occurs at. Measured in seconds, so the smaller this is the faster the transition.")] public float crouchRate = 0.2f; //The speed the crouch transtition occurs at. Measured in seconds, so the smaller this is the faster the transition.
        [Tooltip("A multiplier that determines how quickly the walkTime increases for Impact. This affects the viewbob rate, as well as the stepping frequency.")] public float walkRate = 0.2f;


        [Header("Motion Component - Stairs")]
        [Tooltip("This defines the tallest step a player can climb when dealing with stair-related stuff.")] public float stepHeight = 0.5f;
        [Tooltip("Enables smooth stepping, allowing the player to climb stairs with very little visual jittering.")] public bool smoothStepping = true;
        [Tooltip("If the ImpactController has a playerArtRoot assigned, this will allow it to subscribe to the same smoothing system as the camera uses to reduce jittering when climbing stairs.")] public bool smoothPlayerBodyStepping = true;



        //Motion components, as expected, manage player motion.
        //Barring a handful of general variables that the components are expected to share for the sake of modularity, motion components are typically self-contained and don't rely on outside sources as much as other components might.

        //Motion components have some input they expect from outside sources, such as readings from a valid Input component to control motion (which are read from the attached player), and rotation data for defining a movement direction.

        //Exposed variables that motion components need to utilize include details about grounding conditions, walk time and more.

        public void Reset()
        {
            if (GetComponent<ImpactController>())
            {
                if (GetComponent<ImpactController>().motionComponent == null)
                {
                    GetComponent<ImpactController>().motionComponent = this;
                }
            }
        }
    }


    public enum ImpactMotion_JumpSetting
    { //Determines how jumping is calculated.
        none = 0,
        normal = 1,
        enhanced = 2,
        leaping = 3
    }

    public enum ImpactMotion_SprintSetting
    { //Changes how sprint behaves.

        none = 0, //Self explanatory.
        normal = 1, //Hold sprint to sprint. When released, you will return to normal speed.
        classic = 2 //While you're running, pressing the sprint key will engage sprinting. Sprinting will only stop when you do.
    }

    public enum ImpactMotion_CrouchSetting
    { //Changes how crouching behaves.
        none = 0,
        normal = 1, //Crouching doesn't affect whether or not the player can sprint. Get a move on!
        noSprint = 2 //Crouching disables the ability to sprint. Good if you're practical or whatever.
    }

}