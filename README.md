Thank you for installing Impact!
This section will go over what you need to know to use impact correctly.

== QUICKSTART GUIDE ==

1). Head over to your packages folder, which is next to your assets folder in the unity project files.
2). Open "JTools - Impact Controller"
3). Navigate to Runtime/ImpactController/Prefabs/
4). Drag either of the prefabs to your scene hierarchy (or the scene view) to deploy an Impact Controller into the world. There are currently two options: Default, and Quake (the former functions a lot like Rush, while the latter has a momentum based movement system)
5). There you go! You can now hit the play button and walk around your world.

== TROUBLESHOOTING ==

Q). My controller keeps landing in midair! What on earth?
A). This is because the grounding raycasts are hitting the player's collider. Because I can't pack layers into a package, you'll need to make your own Player layer, assign your player object to it, and then in the "grounding layers" layermask on the impact controller you'll need to disable the player layer.

Q). How can I quickly view what a given parameter does on the controller?
A). Hover your mouse over the variable's name in the inspector and a tooltip will show up to tell you what that parameter does. If that's not enough, review the impact controller modules section below.

Q). I want to write my own modules, but I'm a bit confused by the documentation, where can I go to get more info?
A). As it were, each component base class has detailed comments explaining what each thing does. Please review those for more information if the programmer's guide section isn't enough.

Q). What's up with the mouse tilting being improperly timescaled?
A). Due to how input is handled by Unity, mouse input refuses to timescale correctly. As a result, I've left it alone. This is speficially a problem on the mouse tilting feature and not the mouse sensitivity, thankfully.

Q). Can I sell a modified version of Impact?
A). No. If you want, you can give people free copies of the modified controller, but it cannot be sold for money and you must credit me for my work. Please review the LICENSE document included with the package for more information.

Okay, now we can start talking about Impact's settings and the module system. We'll start with how to configure your controller, as well as reviewing which modules are attached by default.


== IMPACT CONTROLLER MODULES ==


-- The Base Script --

While your modules can vary, the most important component of the Impact Controller is just that - The Impact Controller.
This script is what runs all of the updates for your attached modules. Let's go over all of the settings.

Assign Current [Boolean]: Impact has a singleton system set up to make it easier for you to access your current player instance from anywhere in your code. This singleton is accessed through Jtools.ImpactController.current (or just ImpactController.current if you've imported JTools at the top of your script). Assign Current determines whether or not impact controller instances will subscribe to this singleton. This is an available option in case you decide to implement local multiplayer, in which case the singleton system might break everything.

Player Camera [Reference - Camera]: This is a reference to the player's camera object. This is assigned in the base Impact Controller script so that it's easier for modules to access it without having to communicate with one another.

Player Art Root [Reference - Transform]: This refers to a child of the ImpactController object that has your art assets. If you assign this, the player art root can be accessed in other modules for various purposes. By default, the player art root will automatically rotate so that its forward vector faces where the player is moving.

Player Radius [Float]: This lets you change the lateral collision bounds of your player. This is displayed in the scene view as the thickness of the yellow cube gizmo. Despite the gizmo being a cube, the player is actually a capsule.

Player Height [Float]: This lets you change the vertical collision bounds of your player. This is displayed in the scene view as the height of the yellow cube gizmo.

Input Component [Reference - ImpactComponent_Input]: This is the input module for the player. If this is unassigned, your controls won't do anything at all. Like most of the other module slots, this is essential and must be assigned.

Camera Component [Reference - ImpactComponent_Camera]: This module controls the camera. It affects various outputs, such as the camera's relative position to the player, its FOV, and most importantly, its rotation. Like most of the other module slots, this is essential and must be assigned.

Motion Component [Reference - ImpactComponent_Motion]: This module handles moving the character around, and is arguably the most complex module in the set. It has a ton of options, and I'll cover those in its dedicated section. Like most of the other module slots, this is essential and must be assigned.

Addon Components [List of References - ImpactComponent_Addon]: Unlike the other modules where only a single one really makes sense, you can have an infinite amount of addon modules attached to Impact to add extra behavior. These are also fully optional, and you can leave the list empty if you don't want any extra functionality. This is the preferred way of adding new features to Impact without having to modify the base scripts.

Lock Framerate [Boolean]: Locks the game's framerate manually, instead of relying on VSync to do it. This is preferable if you're making a game that needs to run at a lower framerate for aesthetic reasons. Leaving this off will prevent Impact from meddling around, letting you turn on VSync.

Frame Rate [Int Range, 1 - 60]: This defines what framerate Lock Framerate will set your game to.

... And that covers the base script! Look below if you want to learn more about the controller's default modules.


-- The Input Module --

This module allows you to reconfigure your game's controls with relative ease, and was implemented to make it easier to switch to different input management solutions as Unity evolves. 
The default modules work inside of Unity's old input system, but you can also get a module designed for InControl if you get the "JTools - Impact Controller - Extras" package.
Let's go over the settings, specifically the ones that appear in the default input component, ImpactComponent_Input_Default:

Lock Input [Boolean]: This is inhereted by all input modules, but basically it just prevents inputs from being read and assigns whatever values are inside of ControlsLocked();

Look Axis X [String]: This works in tandem with Unity's input system, reading from whichever axis you've assigned in the input settings. This controls the yaw of the camera.

Look Axis Y [String]: This works in tandem with Unity's input system, reading from whichever axis you've assigned in the input settings. This controls the pitch of the camera.

Movement Strafe [String]: This works in tandem with Unity's input system, reading from whichever axis you've assigned in the input settings. This allows the player to move left and right.

Movement Walk [String]: This works in tandem with Unity's input system, reading from whichever axis you've assigned in the input settings. This allows the player to walk back and forth.

Key Crouch [KeyCode]: This controls crouching. This is assigned directly in the component, so you won't have to modify your project's input settings.

Key Jump [KeyCode]: This lets you jump. This is assigned directly in the component, so you won't have to modify your project's input settings.

Key Sprint [KeyCode]: This controls sprinting. This is assigned directly in the component, so you won't have to modify your project's input settings.

Key Menu [KeyCode]: This is a generic menu button, unused in the default controller, but you can use this input event for your menus. This is assigned directly in the component, so you won't have to modify your project's input settings.

Button Primary [Mouse Button]: This is a generic event you can use for weapons or whatever else. This is assigned directly in the component, so you won't have to modify your project's input settings.

Button Secondary [Mouse Button]: This is a generic event you can use for weapons or whatever else. This is assigned directly in the component, so you won't have to modify your project's input settings.

Key Interact [KeyCode]: Key Menu [KeyCode]: This is a generic interaction button, unused in the default controller, but you can use this input event for whatever systems you make. This is assigned directly in the component, so you won't have to modify your project's input settings.


-- The Camera Module --

This modules manages the camera. One of its unique properties is its "orientation" property, which must be assigned to by camera components so that other components can figure out where the player is facing.
Let's check the settings of ImpactComponent_Camera_Default:

Camera Sensitivity [Float]: Controls how sensitive the camera is to input.

Screenshake Decay Favor [Float]: Used in tandem with Impact's screenshake feature. Determines how quickly the camera stops shaking.

Vertical Restraint [Vector2]: Defines the lowest and highest pitch the player can look (in terms of degrees). By default this is -90 to 90 degrees, but you're welcome to change this if you want to restrain the player's view more. Going beyond the 90 degree range will mess up your view orientation.

Perspective [Float Range, 0 - 1]: This allows you to switch between first and third person! You can use this while the game's running if you want a fun transition effect.

Camera Tilting [Boolean]: A toggle for movement-based tilting effects. 

Camera Tilt Roll Power [Float]: Determines, in degrees, how much the camera will roll when the player strafes.

Camera Tilt Pitch Power [Float]: Determines, in degrees, how much the camera will pitch forward and back when the player moves back and forth.

Camera Mouse Tilting [Boolean]: Toggles effects for mouse-based tilting effects.

Camera Mouse Tilt Roll Power [Float]: Determines, in degrees, how much the camera will roll when the player looks left or right.

Camera Tilt Speed [Float Range, 0 - 1]: Sets how quickly the camera will adapt to tilting, using RLI. The higher this is, the faster the adaptation, the lower is it, the slower.

Camera Crouch Drop [Float Range, 0 - 1]: A percentage that determines how far the player's head needs to go when crouching. At 1, it'll be as far as crouching should normally take it (which is to say, it'll be proportionate to the player's capsule height), and at 0 it'll be completely at the player's feet. To manipulate how much the player's physical size changes, check out the "Crouch Percent" variable in the motion component.

Look Target Tracking Speed [Float Range, 0 - 1]: This is used for Impact's look target tracking system, this determines how quickly the player tracks the camera onto its view target.

View Bob Intensity [Float]: Determines how much vertical motion will occur for view bobbing. Set this to 0 to disable view bobbing.

View Bob Step [Float Range, 0 - 1]: Defines at which point on the view bobbing curve the system will register a step event.

View Bob X [Curve]: Determines how much motion will occur on the camera's X axis while view bobbing intensity is more than 0.

View Bob Y [Curve]: Determines how much motion will occur on the camera's Y axis while view bobbing intensity is more than 0. This is the most common curve for view bobbing.

View Bob Y [Curve]: Determines how much motion will occur on the camera's Z axis while view bobbing intensity is more than 0.

Camera Occluders [LayerMask]: Determines what layers the third person camera will count as "occluding" the player. Objects with layers not marked as a part of this mask will not cause the camera to move closer to the player to see them.

Rigidbody Occlusion [Boolean]: Determines whether or not non-kinematic rigidbodies will cause the camera to move closer to the player. You can toggle this off if you don't want dynamic objects to affect the third person camera.

Third Person Orbit Distance [Float]: This is basic, it just determines how far the camera is set from the player in third-person.

Sprint Intensity [Float]: Affects the camera's FOV when the player sprints. Set it to 0 to disable.

Zoom Intensity [Float]: Affects the camera's FOV when the player zooms in. Set this value to 0 to disable. By default this is activated by the player's "Button Secondary" in the input settings.


-- The Motion Module --

This will be the final module we'll cover in depth for this readme. After this, I'll briefly talk about the addon modules system and then we'll move on to the programming guide.
The motion module manages the player's movement, as well as various events that you can subscribe to in custom scripts (this includes onLanding, as well as onJump).
Let's Begin:

Move Speed [Float]: Determines how fast the player's base speed is.

Crouch Speed [Float]: Determines how fast the player moves while crouching.

Sprint Speed [Float]: Determines how quickly the player can sprint.

Jump Power [Float]: Determines how much force will be applied to the player to make them jump.

Grounding Layers [LayerMask]: This defaults to Everything, but it basically allows you to selectively ignore some surfaces for jumping. This should be used in tandem with the physics layer matrix if you want to ignore an entire layer completely, otherwise the player will still collide with the object, they just can't jump off it.

Crouch Mode [ImpactMotion_CrouchSetting]: This determines how the controller will manage crouching. This includes: None (crouching is disabled), Normal (crouching still allows you to sprint while on the ground), and NoSprint (as the last one hinted, this setting makes it so that you cannot sprint while crouching).

Sprint Mode [ImpactMotion_SprintSetting]: This determines how the controller handles sprinting. This includes: None (sprinting disabled), Normal (sprinting is only active while holding the button down), and Classic (sprinting will activate when the player presses the sprint button, and will stay active as long as the player is still moving).

Jump Mode [ImpactMotion_JumpSetting]: This determines how the controller will handle jumping. This includes: None (no jumping), Normal (jumping is applied normally when the jump key is pressed), Enhanced (jump height is boosted slightly when the player sprints), and Leaping (if the player sprints while jumping, they'll get launched forward as well).

Crouch Rate [Float]: This determines how fast the player will go from standing to crouching.

Walk Rate [Float]: This drives all step event data in the camera. By increasing it, it'll change how fast the internal clock counts up when walking. It's kept here because both the camera and the sound system use the motion component's "onStep" event to perform their actions.

Step Height [Float]: This determines how far the player can move vertically to walk up a stair step.

Smooth Stepping [Boolean]: Determines whether or not the motion component should use a unique smoothing system when ascending stairs. It removes the popping that naturally happens, but comes with the consequence that the camera will move lazily to its goal position.

Smooth Player Body Stepping [Boolean]: Determines whether or not the player art root should be interpolated up stairs, similar to what smooth stepping does for the camera. Naturally, it comes with similar technical issues to consider.

Anti Guttering [Boolean]: Anti-guttering is a feature that prevents the player from getting trapped in physical scenarios where they cannot necessarily ground, but cannot move either. These include, well, gutters (two 45 degree planes intersecting can replicate this effect). Enabling this will allow players to jump if the system registers that they're stuck too long somewhere.

Gravity [Float]: Determines how much downward force is applied to the player each frame.

Gravity Cap [Float]: Determines the maximum amount of gravity the player can experience before it's capped. Think of it like terminal velocity, if you dropped a penny from space and it didn't have terminal velocity it'd just build up speed until it punched a hole deep into the ground. Same principle here.

Slide Speed Cap [Float]: Determines the maximum amount of speed the player can experience when sliding down slopes. This prevents sliding from building up so much that you fire down the slope like a rocket.

Drag [Float Range, 0 - 1]: Determines how quickly the player returns to standing still. Lowering this down makes the game feel like you're on ice, and if it's too hight you'll stop instantly when you're not steering the character in a given direction.

Acceleration [Float Range, 0 - 1]: Determines how quickly the player accelerates towards the desired direction. If this is low you'll feel sluggish, and if it's high you'll move instantly in your desired direction. Find the best balance is important for making movement feel comfortable.

Air Control [Float Range, 0 - 1]: Determines how much agency the player gets while in midair. At 1 you have full control, and at 0 you're at the whims of physics.

Landing Effects [Boolean]: Determines whether or not the motion component should invoke landing events. If this is off, the camera won't drop down on landing and there won't be any sounds on impact.

Landing Sound Timer [Float]: A timer that determines how frequently landing events should be processed. While the controller already manages the frequency of landing events by checking to confirm the player is falling a certain velocity, this can help you further restrict how often landings occur.

Sliding On Slopes [Bool]: Determines whether or not the player slides down slopes that are too steep.

Slope Angle [Float Range, 0 - 90]: Determines what constitutes a "slope" below the player. At 0 everything that isn't flat ground will cause you to slide down, and with 90 absolutely everything counts as flat ground and you won't slide (much, if at all).

Crouch Percent [Float Range, 0.4 - 1]: Determines how much the player's collision capsule should be compressed when crouching. This affects the player's actual size, so if you just want to move the camera further down, please check out the camera component's "Camera Crouch Drop" setting.


-- Addon Modules --

So this section isn't a reference for specific variables in the addon modules, instead it talks about how this system works and what to expect from it.
The purpose of an addon module is to add extra behavior that the other components shouldn't be managing. This includes having weapons or interaction systems in your game.
Addon modules, like other impact components, have several methods you can override in your own scripts that'll allow you access to the base player as well as its attached modules.

Check the programming guide further down if you want to know more about how addon modules are configured and made.
Anyhow, a brief overview of the two modules that come with Impact:

Addon_Animation: Connects an animator component into impact, allowing you to have animations driven by the controller's motion. Each parameter in this component has a tooltip you can read by mousing over the parameter name, and I strongly encourage reviewing those tooltips to understand how it works.

Addon_Sound: Gives the controller sound support. It's a basic module, and I recommend making your own if you want more complex sounds for your player. As a fun tidbit, you can use PlayStepSound() with animation events (+ the override foot steps option in the animation component) to have your animations use this system for sound!

Alright, thanks for reading all that! Let's move onto the programmer's guide, which is really important if you end up modifying the controller in any way.


== PROGRAMMER'S GUIDE ==


Of course, I strongly recommend you have a good understanding of C# before trying to hammer new features onto Impact. It's not the most complex controller in the world, but if you end up having to diagnose errors because your code conflicts with Impact, you're gonna have a fun time digging through it all.
On a more important note, Impact's code is HEAVILY commented. The guide here isn't a comprehensive explanation, rather an overview to help you understand how all of this works.
If you want to actually understand what each method/variable/event does in detail, you need to open up the base classes specified below to learn.
Now then, let's go through all the important details as well as how to write your own components.

-- Impact Component Overview --

When writing an Impact Component (the in-engine name for modules), your class will need to inherit from one of four module types:

ImpactComponent_Motion: Inherit from this if you're going to modify anything involving the character's movement. This might be relevant if you're making a game that doesn't work from a conventional "humanoid" perspective (like a racing game).

ImpactComponent_Input: Inherit from this if you want to expand your input options. This is important if you have a custom input manager solution and need to add compatibility with Impact.

ImpactComponent_Camera: Inherit from this if you need a custom camera solution. This might be necessary if you're creating a game that functions from a different perspective than what's supported.

ImpactComponent_Addon: Inherit from this if you're adding unique subsystems that don't cover major parts of the controller like the above components do. This includes interaction systems, weapon systems, dialog management, etc. etc.

Once your class inherits from the above classes, you'll gain access to a bunch of built-in methods, variables and events. This might seem a bit frightening, but look at the guide below to learn the important stuff.


-- Impact Component --

All impact components you make will inherit these properties, as this is the base class that forms the backbone for all of the four variants.

public ImpactController owner: This determines which impactcontroller instance owns this component. By default the initialize method will automatically assign this to whichever controller initialized this component.

public bool initialized: This determines whether or not a component is initialized. Thankfully for you, this means you can swap components out at runtime, and impact will use this to make sure all components attached to it are initialized every update.

public virtual void ComponentInitialize(ImpactController Player): This is impact's equivalent to the "Start" method. You should put any code you want to run only once here.

public virtual void ComponentEarlyUpdate(ImpactController Player): This event runs before the standard update event in the other components. Helpful if you need to do some actions before any further processing is done elsewhere.

public virtual void ComponentUpdate(ImpactController Player): The meat and potatoes of your controller stew. Use this for most of your general behavior.

public virtual void ComponentLateUpdate(ImpactController Player): It's exactly like Unity's LateUpdate, but in component form. It runs after all other update operations.

public virtual void ComponentFixedUpdate(ImpactController Player): This allows you to perform actions that revolve around the engine's physics system. This is especially important for camera movement synchronization.


What you see here is the majority of what you'll be dealing with while writing components. The important thing to know, however, is that some of the other components have their own unqiue variables and events that they use to communicate with eachother. Check below to see those.


-- ImpactComponent_Motion Communication --

Variables:

public Quaternion orientation: This is possibly the most important communication variable the motion component has. This is assigned to by other components (namely the camera component by default) to tell the motion component which direction it needs to apply movement in. This should normally correspond with the camera's yaw alone, but you can modify the orientation if you need something more exotic.

public float walkTime: walkTime is a wrapped value that refers to the current part of the walk cycle the player is in. This drives various effects, such as viewbobbing and step sounds. This value must be added to and wrapped (using modulus or whatever you prefer) as your character moves across the ground.

public Vector3 movement: This vector determines what the player's current velocity is. While it won't break your motion component if you use a different motion vector, other components rely on this one to understand how the player is moving. Please try and use this if you can help it, or at least copy your unique motion vector to it on update.

public LandingEvent onLanding: This is a UnityEvent with a float parameter that's wrapped in a custom class to make it possible to initialize. Other components can subscribe to this event with .AddListener to receive an event when the player lands. The float parameter is how fast the player was falling when they landed. You must call onLanding.Invoke on your custom component when the player makes contact with the ground from being in midair.

public UnityEvent onJump: This event is invoked whenever the player jumps. Other components can use .AddListener to receive alerts for when the player jumps. You must use .Invoke on this event whenever your character jumps for obvious reasons.

public float crouchTime: This is a percentage between 0 and 1 that affects whether or not the player is crouched. This is irrelevant if you aren't implementing a custom crouching solution into your motion component, but keep in mind that it needs to be assigned to if you want other components to know what state the player is in.


-- ImpactComponent_Camera Communication --

Methods:

public virtual void OnStairStep(Vector3 m_smoothDirectionVector): This is used with the motion component to manage how the camera should react to stepping up a stair. The vector3 provided is the direction and distance the player moved while stepping up the stair.

public virtual void SetLookTarget(Transform target): This allows the camera to stop following the mouse and track a specific object instead. Setting the target as null disables this feature and provides control back to the player.

public virtual void DisengageCameraRotating(bool state): Allows you to stop the camera from responding to mouse input for a bit. Mainly used in case you need to take explicit control of the camera's rotation. Settings this to false will automatically adjust this component's cameraAngles to match the current camera rotation.

public virtual void DisengageCameraPositioning(bool state): Prevents the camera from being moved to its desired position. You are fully responsible for managing its position while this is true. Only use this feature if you need to navigate the camera around during like, a cutscene or something.

public virtual void ShakeScreen(float intensity): As expected, this method will set the screenshake value to the desired intensity.

Variables:

public Vector3 cameraAngles: This is the rotation, in eulerangles, that your camera is in. The default components use this to drive their rotations, so if you want to manipulate their rotation from a custom component, you'll need to use this, or lock the camera and change the transform's rotation manually.

public Vector3 cameraOrigin: This is the position in local coordinates that determines where the camera's resting position is. This is used for viewbobbing, and should be assigned if you're implementing your own viewbobbing system.

public float baseFOV: This is the camera's true FOV. Assign this to change the camera's zoom correctly. Modifying the camera component directly won't do anything.
public UnityEvent onStep: This event is invoked whenever the camera dips down during its step event. Invoke this event whenever your component should play a step sound or invoke whatever other scripts you connect to it.

public float cameraSensitivity: This is self explanatory. Change this to change how quickly you look around.

public float screenshakeDecayFactor: This is how quickly screenshake fades away. 

public Transform lookTarget: This is the current object the camera is tracking instead of looking at the mouse direction. Use SetLookTarget to modify this value.


-- ImpactComponent_Input Communication --

Methods:

public virtual void ChangeLockState(bool state): This determines whether or not the player's controls are locked. If active, then the input module calls ControlsLocked(), if inactive the input module calls Controls() instead.

public virtual void Controls(): This is the most important part of your input component, since ControlsLocked is already managed for you. This method applies input data every frame. For a more comprehensive explanation check ImpactComponent_Input and ImpactComponent_Input_Default.

public virtual void ControlsLocked(): This is managed by default, but you can override it if you need to modify how the game's controls should behave when locked.

Variables:

public ImpactInputData inputData: This is the single most important variable in your input component. All other components read this struct to understand what inputs the player is attempting for a given frame. Check out the ImpactInputData struct definition inside ImpactComponent_Input for more information.

public UnityEvent onLock; This is invoked whenever input is locked. Other components can use this if they need to invoke specific behaviors whenever locking starts.

public UnityEvent onUnlock: This is invoked whenever input is unlocked again. Other components can use this if they need to invoke specific behaviors whenever locking ends.


-- ImpactComponent_Addon Communication --

Addons don't have any communication methods/variables. They're basically the same as the default ImpactComponent, but they're designed to slot into the ImpactController's addons list. 

Please review the scripts in-editor if you need more information. The comments should do most of the work.

Thanks for reading.



- J
