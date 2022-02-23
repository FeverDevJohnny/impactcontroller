using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTools
{
    public class ImpactComponent_Motion_Default : ImpactComponent_Motion
    {
        [Header("Default - General")]
        [Space]
        [Tooltip("A system that gives the player the ability to jump if they're technically \"midair\" but can't move. This isn't terribly common, but four slopes leading inward can cause it, along with poorly designed collision geometry.")] public bool antiGuttering = true;
        [Space]
        [Tooltip("The amount of gravity the player experiences per frame.")] public float gravity = 30f;
        [Tooltip("The maximum fall speed possible.")] public float gravityCap = 100f;
        [Tooltip("The maximum speed the player will slide down slopes.")] public float slideSpeedCap = 45f;
        [Space]
        [Tooltip("0.01 means that the player will control like they're on ice, and 1 means the player slows down instantly to a stop. This is modified by air control.")] [Range(0.01f, 1f)] public float drag = 0.2f;
        [Tooltip("0.01 means that the player will slowly accelerate, and 1 means the player will instantly start moving in the desired direction. This is modified by air control.")] [Range(0.01f, 1f)] public float acceleration = 0.2f;
        [Tooltip("The amount of control a player gets in the air. 0 means absolutely none, 1 means same amount as on the ground.")] [Range(0f, 1f)] public float airControl = 1f;
        [Space]
        [Tooltip("Whether or not the camera should respond to landing.")] public bool landingEffects;
        [Tooltip("How long the player will wait before allowing the landing sound to play again. Check m_landingTimer in the code for more details.")] public float landingSoundTimer = 0.2f;
        [Space]
        [Tooltip("Whether or not the player is affected by slopes.")] public bool slidingOnSlopes = true;
        [Tooltip("Determines at what normal value the player will slide down a sloped surface (assuming slidingOnSlopes is enabled). The higher this is, the steeper a surface a player can walk up.")] [Range(0f, 90f)] public float slopeAngle = 30f;
        [Space]
        [Tooltip("A precentage of the player's current height that represents how low they can crouch. This specfically affects the player's collider height.")] [Range(0.4f, 1f)] public float crouchPercent = 0.4f;


        //Private Variables
        private bool m_canJump; //Decided based on the angle of the ground below the player.

        private float m_lastGrav; //The gravity as of last frame. Used for a handful of calculations.
        private float m_jumpTimer; //Prevents key bouncing issues;

        private bool m_sliding; //Used when blocking player motion up a slope.
        private Vector3 m_slidingNormal; //The normal of the slope the player is touching.
        private float m_slideHolder; //Prevents infinite sliding when falling off slopes into air, just ignore it.

        float m_slideTimer = 0f;

        float m_stuckTimer = 0.5f;

        private Rigidbody m_rig; //Reference to the rigidbody component attached to the player object;

        Vector3 m_lastPosition;

        float m_landingTimer = 0f;

        RaycastHit m_hit; //A cached raycast for performance. The script recycles this often to avoid allocation issues.

        public override void ComponentLateUpdate(ImpactController player)
        {
            base.ComponentLateUpdate(player);

            //Slide Timer
            if (m_sliding)
                m_slideTimer = 0.2f;
            else
                m_slideTimer = Mathf.Max(m_slideTimer - Time.deltaTime, 0f); //If you're not sliding, a small debouncing value is used to prevent issues with sliding/unsliding over and over, which can cause the landing effects to fire off constantly.

            if (m_landingTimer > 0f)
            {
                if (!isGrounded)
                    m_landingTimer -= Time.deltaTime; //If we're not on the ground, we count the timer down for landing effects! This makes it so that small drops don't cause the landing sound to play.
                else
                    m_landingTimer = landingSoundTimer; //The timer is reset if we hit the ground.
            }

            if (m_lastGrav < -5f && isGrounded && m_landingTimer <= 0f) //This checks to make sure the player is falling a certain speed before using landing effects.
            {
                if (landingEffects && m_slideTimer <= 0f)
                {
                    onLanding.Invoke(-m_lastGrav);
                    m_landingTimer = landingSoundTimer;
                }
            }

            m_lastGrav = movement.y; //The gravity from the last frame is set here. This is mostly used to compare against prior frame motion.

            m_lastPosition = transform.position;

        }

        public override void ComponentInitialize(ImpactController player)
        {
            base.ComponentInitialize(player);

            //Same for the rigidbody, manually set here.
            m_rig = gameObject.AddComponent<Rigidbody>();
            m_rig.freezeRotation = true;
            m_rig.useGravity = false;
            m_rig.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            m_rig.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public override void ComponentFixedUpdate(ImpactController player)
        {
            base.ComponentFixedUpdate(player);

            if (slidingOnSlopes)
            {
                if (m_sliding)
                {
                    m_canJump = false;
                    Vector3 m_pos = m_slidingNormal;

                    movement.x += (1f - m_slidingNormal.y) * m_slidingNormal.x * 0.25f;
                    movement.z += (1f - m_slidingNormal.y) * m_slidingNormal.z * 0.25f;
                }
            }


            if (isGrounded)
            {
                if (Physics.Raycast(transform.position + Vector3.up * player.capsuleCollider.height * 0.5f, Vector3.down, out m_hit, player.playerHeight * 0.66f, groundingLayers, QueryTriggerInteraction.Ignore))
                {
                    if (new Vector3(player.inputComponent.inputData.motionInput.x, 0f, player.inputComponent.inputData.motionInput.z).magnitude > 0f && !m_sliding && Vector3.Dot(m_hit.normal, movement) > 0f)
                    {
                        if (Vector3.Angle(Vector3.up, m_hit.normal) < slopeAngle && movement.y > -2f && movement.y < 0f && m_hit.normal.y < 0.99f) //If we're not set to slide down the slope normally
                            movement.y = (m_hit.point.y - transform.position.y) * 60f;//m_rig.MovePosition(new Vector3(transform.position.x, m_hit.point.y, transform.position.z));
                    }
                }

            }




            /// This snaps the player down to a surface if the conditions are just right. Needed on slopes to prevent the player from sliding off of a surface and floating down to the ground instead of, you know, walking down the slope like a normal human.
            float snapDistance = (player.playerHeight * 0.5f) + Mathf.Clamp(m_rig.velocity.y, -1f, 0f); //How far down the player will search for snappable terrain.
            if (!isGrounded && movement.y < 0f) //If we're midair and we're also falling down.
            {
                if (Physics.Raycast(transform.position + Vector3.up * player.capsuleCollider.height * 0.5f, Vector3.down, out m_hit, snapDistance, groundingLayers, QueryTriggerInteraction.Ignore)) //A raycast is fired below the player.
                {
                    if (slidingOnSlopes)
                    {
                        if (Vector3.Angle(Vector3.up, m_hit.normal) < slopeAngle)
                        {
                            if ((1f - m_hit.normal.y) < Mathf.Acos(slopeAngle)) //If the surface is flat enough, 
                                movement.y = (m_hit.point.y - transform.position.y) * 60f;//m_rig.MovePosition(new Vector3(transform.position.x, m_hit.point.y, transform.position.z)); //The player is shifted down to the surface for landing.

                            if (landingEffects && m_slideTimer <= 0f && m_landingTimer <= 0f)
                            {
                                onLanding.Invoke(-m_lastGrav);
                                m_landingTimer = landingSoundTimer;
                            }

                            isGrounded = true;
                        }
                    }
                    else
                    {
                        if (landingEffects && m_slideTimer <= 0f && m_landingTimer <= 0f)
                        {
                            onLanding.Invoke(-m_lastGrav);
                            m_landingTimer = landingSoundTimer;
                        }

                        isGrounded = true;
                    }
                }
            }



            Debug.DrawLine(transform.position + stepHeight * Vector3.up, (transform.position + stepHeight * Vector3.up) + player.motionComponent.movement.normalized * player.playerRadius * 1.2f, Color.red);


            if (!Physics.Raycast(transform.position + stepHeight * Vector3.up * (player.capsuleCollider.height / player.playerHeight), player.motionComponent.movement.normalized, player.playerRadius * 1.2f, player.motionComponent.groundingLayers))
            {
                Vector3 mx = transform.position + ((player.motionComponent.movement * Time.fixedDeltaTime).normalized * player.playerRadius * 1.2f) + stepHeight * Vector3.up;
                Debug.DrawLine(mx, mx + Vector3.down * stepHeight, Color.blue);
                if (Physics.Raycast(transform.position + ((player.motionComponent.movement * Time.fixedDeltaTime).normalized * player.playerRadius * 1.2f) + stepHeight * Vector3.up * (player.capsuleCollider.height / player.playerHeight), Vector3.down, out RaycastHit m_step, stepHeight, player.motionComponent.groundingLayers))
                {
                    if (Vector3.Dot(m_step.normal, Vector3.up) > 0.99f && m_step.point.y > transform.position.y + Mathf.Max(player.motionComponent.movement.y, 0f))
                    {
                        Vector3 cameraPos = owner.transform.position;
                        Debug.DrawLine(m_step.point, m_step.point + Vector3.down * 0.4f, Color.green);
                        movement.y = Mathf.Max((m_step.point.y - transform.position.y) * 15f, movement.y);//transform.position = new Vector3(transform.position.x, m_step.point.y, transform.position.z);

                        if (smoothStepping)
                            player.cameraComponent.OnStairStep(Vector3.up * (m_step.point.y - transform.position.y) * Time.fixedDeltaTime); //m_camPosTracer += (cameraPos - transform.position);

                    }
                }
            }



            m_rig.velocity = movement * Time.fixedDeltaTime * 60f;

            ///
        }

        public override void ComponentUpdate(ImpactController player)
        {
            base.ComponentUpdate(player);

            if (player.playerArtRoot != null)
            {
                if (player.inputComponent.inputData.holdingSecondary)
                {
                    player.playerArtRoot.rotation = Quaternion.Slerp(player.playerArtRoot.rotation, Quaternion.Euler(0f, player.cameraComponent.cameraAngles.y, 0f), 0.2f * Time.deltaTime * 60f);
                }
                else
                {
                    if (new Vector3(movement.x, 0f, movement.z).magnitude > 0.1f)
                    {
                        player.playerArtRoot.rotation = Quaternion.Slerp(player.playerArtRoot.rotation, Quaternion.LookRotation(new Vector3(movement.x, 0f, movement.z)), 0.1f * Time.deltaTime * 60f);
                    }
                }
            }

            /// MOVEMENT MANAGEMENT
            //Crouching control/logic (check just a bit further down for the part where the height is actually affected!)
            if (crouchMode != ImpactMotion_CrouchSetting.none) //Assuming we're actually letting the player crouch
            {
                if (!Physics.SphereCast(new Ray(transform.position + (Vector3.up * player.capsuleCollider.height * 0.5f), Vector3.up), player.playerRadius * 0.9f, player.playerHeight * 0.52f, groundingLayers, QueryTriggerInteraction.Ignore)) //We fire a check above the player to make sure they're not trying to stand up while a ceiling is present.
                    isCrouching = ((!player.inputComponent.lockInput) ? player.inputComponent.inputData.holdingCrouch : false);
                else
                    isCrouching = ((player.inputComponent.inputData.pressedCrouch) || (crouchTime >= 0.1f));
            }
            else
                isCrouching = false;


            //The actual physical crouching.
            crouchTime = Mathf.Clamp01(crouchTime + ((isCrouching) ? 1f : -1f) * (Time.deltaTime * ((crouchRate > 0f) ? (1f / crouchRate) : 1f / 0.01f)));

            //Collider adjustments based on the crouching.
            player.capsuleCollider.height = player.playerHeight + ((player.playerHeight * crouchPercent) - player.playerHeight) * ImpactController.SmoothUp(crouchTime);
            player.capsuleCollider.center = Vector3.up * (player.capsuleCollider.height * 0.5f);

            /// Sprinting. Code for managing the camera's FOV, the ways sprinting can be enabled, and the player's top speed.
            switch (sprintMode)
            {
                case (ImpactMotion_SprintSetting.normal):
                    isSprinting = player.inputComponent.inputData.holdingSprint;
                    break;

                case (ImpactMotion_SprintSetting.classic):
                    //Player's intended movement is averaged on intensity and analyzed. If it falls below a threshold, sprinting turns off.
                    isSprinting = ((Mathf.Abs(player.inputComponent.inputData.motionInput.x) + Mathf.Abs(player.inputComponent.inputData.motionInput.z)) * 0.5f < 0.5f) ? false : player.inputComponent.inputData.holdingSprint;
                    break;
            }

            if (crouchMode == ImpactMotion_CrouchSetting.noSprint && isCrouching)
                isSprinting = false;



            if (!isCrouching)
                topSpeed = (isSprinting) ? sprintSpeed : moveSpeed;
            else
                topSpeed = (isSprinting) ? (crouchSpeed * (sprintSpeed / moveSpeed)) : crouchSpeed;
            ///

            /// General Motion.
            if (!player.inputComponent.lockInput) //If the movement isn't locked, manage player controls to figure out where they want to go.
            {
                if (!isGrounded)
                {
                    if (m_slideHolder > 0f)
                        m_slideHolder -= Time.deltaTime;
                    else
                        m_sliding = false;
                }

                if (m_sliding)
                    m_canJump = false;

                movement = ImpactController.RLerp(movement, orientation * new Vector3(player.inputComponent.inputData.motionInput.x * topSpeed, movement.y, player.inputComponent.inputData.motionInput.z * topSpeed), (player.inputComponent.inputData.motionInput.magnitude > 0.2f) ? acceleration : drag * ((!isGrounded) ? airControl : 1f));

            }
            else
                movement = ImpactController.RLerp(movement, new Vector3(0f, movement.y, 0f), drag); //Assuming movement's locked, the player is recursively slowed down to zero.

            ///

            walkTime = (walkTime + (Time.deltaTime * walkRate * new Vector3(movement.x, 0f, movement.z).magnitude)) % 1f;

            /// Ceiling bumping. Prevents that obnoxious "sticky" feel you get whenever you jump into something above you.

            if (Physics.Raycast(transform.position + Vector3.up * player.capsuleCollider.height * 0.5f, Vector3.up, out m_hit, player.capsuleCollider.height * 0.55f, groundingLayers, QueryTriggerInteraction.Ignore))
            {
                if (m_hit.collider.GetComponent<Rigidbody>() != null)
                {
                    if (m_hit.collider.GetComponent<Rigidbody>().isKinematic)
                        if (movement.y > 0f) movement.y = 0f;
                }
                else
                    if (movement.y > 0f) movement.y = 0f;
            }


            /*
             * Gravity is affected here. We clamp it so if there's a bug, the gravity won't grow so great that players just fly through geometry.
             * Think of it like terminal velocity, if you drop an object it doesn't just keep accelerating until it punches a hole through the Earth, right?
            */

            movement.y = Mathf.Clamp(movement.y - gravity * Time.deltaTime, (!m_sliding) ? -gravityCap : -slideSpeedCap, Mathf.Infinity);

            if (m_sliding)
                isGrounded = false;


            ///


            bool m_gqueue = false; //A queued boolean so the grounding code can run first, but the snapping code is guaranteed a chance to check things out.

            //Checking for ground.
            if (Physics.SphereCast(transform.position + Vector3.up * player.capsuleCollider.height * 0.5f, player.playerRadius * 0.99f, Vector3.down, out m_hit, (player.capsuleCollider.height * 0.51f) - (player.playerRadius * 0.8f), groundingLayers, QueryTriggerInteraction.Ignore))
            {
                if (slidingOnSlopes)
                {
                    if (Vector3.Angle(Vector3.up, m_hit.normal) < slopeAngle)
                        m_gqueue = true;
                    else
                        isGrounded = false;
                }
                else
                {
                    m_gqueue = true;
                }

                Vector3 normal = m_hit.normal;

                if (Vector3.Dot(normal, Vector3.up) > 0.1f) //We only need to make the following decisions if the object we're colliding with is beneath us.
                {
                    if (slidingOnSlopes)
                    {
                        if (movement.y < 0f)
                        {
                            if (Vector3.Angle(Vector3.up, normal) > slopeAngle) //If sliding on slopes is enabled, we check to see if the current surface is too steep. If so, the player can no longer jump, and they are shunted down the slope. If the slope isn't too steep, the player is then allowed to jump.
                            {
                                m_sliding = true;
                                m_slideHolder = 0.1f;
                                m_slidingNormal = normal;
                            }
                            else
                            {
                                m_sliding = false;
                                m_slidingNormal = Vector3.zero;
                                m_canJump = true;
                            }
                        }
                    }
                    else
                        m_canJump = true;
                }
            }
            else
                isGrounded = false;


            if (m_gqueue)
                isGrounded = true;


            /*
             * We use this in case jump controls get rapid inputs, whether this is a hardware fault or a deliberate action.
             * If this weren't here, it'd be possible to chain enough jumps in the frames before the player is no longer considered "grounded" to fly into the air.
            */
            if (m_jumpTimer > 0f)
                m_jumpTimer -= Time.deltaTime;
            /// Jumping management.
            if (!isGrounded)
            {
                if (!player.inputComponent.lockInput)
                {
                    if (movement.y > 0f && player.inputComponent.inputData.releasedJump) //Whenever the player is in midair and going up, we allow them to halve their vertical speed by releasing the jump button.
                        movement.y -= movement.y * 0.5f;
                }

                if (antiGuttering)
                {
                    if ((m_lastPosition - transform.position).magnitude < 0.05f && movement.y <= 0f)
                    {
                        if (m_stuckTimer <= 0f)
                        {
                            m_canJump = true;
                            JumpDetection(player);
                        }
                        else
                            m_stuckTimer -= Time.deltaTime;
                    }
                    else
                        m_stuckTimer = 1f;
                }
            }
            else
            {

                JumpDetection(player);

                /*
                 * If the surface below us is flat enough to count as ground, we clamp the player's gravity to prevent it 
                 * from stacking up and making the player fall through terrain!
                 * 
                 * We leave some gravity so when the player walks down a slope, they move to meet the terrain. Otherwise they'd walk off slopes like they were flat
                 * ground, and this looks extremely ugly.
                */

                movement.y = Mathf.Clamp(movement.y, (new Vector3(player.inputComponent.inputData.motionInput.x, 0f, player.inputComponent.inputData.motionInput.z).magnitude > 0f) ? -0.5f : 0f, Mathf.Infinity);
            }
            ///

            isSliding = m_sliding;

        }

        void JumpDetection(ImpactController player)
        {

            if (!player.inputComponent.lockInput)
            {
                if (jumpMode != ImpactMotion_JumpSetting.none)
                {
                    if (m_canJump)
                    {
                        if (player.inputComponent.inputData.pressedJump)
                        {
                            if (m_jumpTimer <= 0f)
                            {

                                onJump.Invoke();

                                switch (jumpMode)
                                {
                                    case (ImpactMotion_JumpSetting.normal):
                                        movement.y = jumpPower; //No matter what, you will always jump with consistent power.
                                        break;

                                    case (ImpactMotion_JumpSetting.enhanced):
                                        movement.y = jumpPower + ((isSprinting) ? jumpPower * 0.15f : 0f); //Jumppower is scaled up if enhanced jumping is enabled.
                                        break;

                                    case (ImpactMotion_JumpSetting.leaping):
                                        movement.y = jumpPower; //Jumping is normal here, but the next line flings the player if they're sprinting.

                                        if (isSprinting)
                                        {
                                            Vector3 m_applied = Vector3.zero;

                                            m_applied = new Vector3(player.inputComponent.inputData.motionInput.x * sprintSpeed * 2f, 0f, player.inputComponent.inputData.motionInput.z * sprintSpeed * 2f);
                                            m_applied.y = jumpPower * 0.5f;

                                            movement += orientation * m_applied;
                                        }
                                        break;
                                }

                                m_jumpTimer = 0.05f;
                            }
                        }
                    }
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {

            //If the object we're standing on happens to be a rigidbody that's active, sliding calculations will be inaccurate. As a result, we stop here if it's a moving rigidbody.
            if (collision.collider.GetComponent<Rigidbody>() != null)
                if (!collision.collider.GetComponent<Rigidbody>().isKinematic)
                    return;

            /// This entire section focuses on preventing the player from climbing or jumping up slopes. While sliding can be disabled, for the sake of gameplay, you cannot change settings that prevent the player from jumping up hills. If it's too steep, it ain't happening.
            Vector3 normal = Vector3.zero;

            if (Physics.Raycast(transform.position + Vector3.up * owner.capsuleCollider.height * 0.5f, Vector3.down, out m_hit))
                normal = m_hit.normal;

            if (Vector3.Dot(normal, Vector3.up) > 0.1f) //We only need to make the following decisions if the object we're colliding with is beneath us.
            {
                if (slidingOnSlopes)
                {
                    if (movement.y < 0f)
                    {
                        if (Vector3.Angle(Vector3.up, normal) > slopeAngle) //If sliding on slopes is enabled, we check to see if the current surface is too steep. If so, the player can no longer jump, and they are shunted down the slope. If the slope isn't too steep, the player is then allowed to jump.
                        {
                            m_sliding = true;
                            m_slideHolder = 0.1f;
                            m_slidingNormal = normal;
                        }
                        else
                        {
                            m_sliding = false;
                            m_slidingNormal = Vector3.zero;
                            m_canJump = true;
                        }
                    }
                }
                else
                    m_canJump = true;
            }
            ///
        }

    }
}