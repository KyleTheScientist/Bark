# CHANGELOG.md

## 1.5.1 (2023-December-9) - The Christmas Update

### Features
- Added the **Fireflies** mod which allows you to summon a firefly that will track a player in the lobby.
- Added the **Join Bark Code** button which will let you join a private code for Bark users.
- You can now summon Bark using the **B** button on your keyboard.
- Added some Christmas lights to the menu

### Fixed 
- Fixed potions and teleport for the new update


## 1.5.0 (2023-November-11) - The Settings Update

### Features
- Added a **built-in settings menu** that will let you modify the settings for each mod
- Added the **Fly** module which is a really simple stick-driven fly mechanic
- Added the **Pearl** module which allows you to throw ender pearls and warp to them
- Added the **Nail Gun** module which allows you to fire nails into walls that you can climb with [Grip]
- Added the **Halo** module which is a secret module that only I, KyleTheScientist, can activate. If you see it, you'll know it's really me.
- Added new config options: 
    - Platforms
        - `Model` - You can now choose between **3 new platform models**:
            - `Storm Cloud` - Makes the clouds dark and adds a rain particle effect
            - `Invisible` - Invisible platform models
            - `Doug` - Two Doug the Bug's to walk on
    - Fly
        - `Speed` - The top speed of the fly module
        - `Acceleration` - The amount of easing that occurs before reaching top speed
    - Pearl
        - `Throw Force` - How much to multiply the pearl's speed by on release
    - Nail Gun
        - `Max Nail` - Maximum number of nails that can exist at one time (multiplied by 4)
        - `Nailgun Hand` - Which hand holds the nail gun
    - Rockets
        - `Thruster Volume` - How loud the thrusters are
- Added a particle effect/sound when telekinesis is being used to make it more obvious who is controlling you.
- Added a sound effect when drinking potions so you know if it's working 

### Changes
- Buttons are now a little darker to prevent blindness.
- **Platforms** is now a single mod.
- Reworked the whole interaction system *again*.
- Cloud platforms no longer fade while sticky and active.
- **Boxing** is now 5 times stronger by default.
- **Telekinesis** is now more responsive.
- **No Collide** now enables **Fly** instead of **Platforms** when it is turned on. It also turns **Fly** off now when disabled.
- **Teleport** no longer requires the triangle to be near your head to teleport.


## Fixes
- Setting [Right Stick] as an input setting no longer binds it to [Left Stick] instead.
- Fixed bug where various mods would render on other players even if they didn't have the mod active.
- Sticky platforms should no longer teleport you out of the map.
- Keyboard keys can no longer be pressed while **No Collide** is active.

## 1.4.0 (2023-October-30) - The Potions Update

Features
- Added the **Potions** module which allows players to change their player size by drinking potions.
- Added the **Rockets** module which allows you to spawn rockets that you can fly around with.
- Added new config options: 
    - Potions
        - `Show Networked Size` - Whether or not to show other players' sizes
    - Rockets
        - `Power` - How fast the rockets accelerate you

Changes
- Bubble is now networked, so other players can see your bubble
- New bubble material
- X-ray material is now significantly improved
- Boxing now plays a sound when you get punched
- Reworked the entire internal grab interaction system because Unity can eat a d***
- Zipline is now bound to trigger by default so it can be used with rockets

Fixes
- Grappling hooks no longer freak out when you move your hand while grappling
- Speed boost now always works in casual
- Teleporting should be less difficult while moving now


## 1.3.3 (2023-August-6) - The Caves Revamp Patch

Fixes
- Fixed mod so it works with this update
- Configs are now shared between the CI and non-ci versions of the mod (you will have to redo your settings one last time, in the future updates will not break configs)

## 1.3.2 (2023-July-1) - The Zipline Update (AKA The STOP BREAKING MY MODS LEMMING update)
Features
- Added the **Zipline** which gives the player a cannon that fires ziplines. Press [Grip] to spawn the cannon and [Trigger] to fire.
- Added new config options: 
    - Zipline
        - `Max Ziplines` - How many ziplines can exist on screen at once
        - `Launcher Hand` - Which hand the launcher spawns in
        - `Gravity Multiplier` - How fast you accelerate downwards while riding on the zipline
    - Bubble
        -  `Bubble Size` - The size of the bubble's hitbox
        -  `Bubble Speed` - The speed that the bubble gains when you punch it
    - Checkpoint
        -  `Charge Time` - How long it takes to set and return to the checkpoint
    - Teleport
        -  `Charge Time` - How long it takes to charge the teleport

Changes
- Going forward there will be 2 releases for Bark; One with and one without the Computer Interface dependency. This way players
can still use the mod even if CI is broken.

Fixes
- Fixed for the Summer Update
- Buttons now vibrate the correct controller
- Grappling Hooks now target more reliably when the player shrinks
- Bubble now scales with the player when they shrink
- Bubble now scales with the player when they are small

## 1.3.1 (2023-Jun-23) - The Bubble Update
Features
- Added the **Bubble** module which creates a big bubble around you that you can float in and push around.

Fixes
- Fixed the multiplayer mods (Telekinesis, Boxing, etc.) which broke in the new update.

Changes
- Added some textures to the Grappling Hooks 

Removed
- Removed **Freeze**. This mod had an issue that could cause you to get reported/banned (also no one liked it).
**Bubble** is meant to be its more interesting replacement.


## 1.3.0 (2023-Jun-18) - The Computer Interface Update
Features
- Added the **Telekinesis** module which allows players to pick you and throw you
  with The Force!
- Added configurable settings for a handful of mods
    - General
        - `Open menu` - Which button you press to open the menu
        - `Open hand` - Which hand's buttons can open the menu
    - Airplane
        - `Speed` - How fast you fly
        - `Steer with` - Whether you steer with your wrists or head
    - Boxing
        - `Punch force` - How far you'll fly when people punch you
    - Grappling Hooks
        - `Rope type` - Whether the rope will pull you or stay at the same length
        - `Springiness` - If rope type is set to elastic, how springy the ropes are
        - `Steering` - How much influence you have on your velocity while swinging
        - `Max length` - How far the grappling hooks can reach
    - Gravity
        - `Multiplier` - How strong gravity will be while the module is active
    - Platforms
        - `Sticky` - Whether or not your hands stick to the platforms
        - `Input` - Which button you press to activate the platform
    - Speed Boost
        - `Speed` - How fast you run while the module is active

Changes
- Renamed `Speed` to `Speed Boost`
- Renamed `Low Gravity` to `Gravity`

Fixes
- The menu and all the mods work again!

## 1.2.1 (2023-May-6) - Checkpoint Fix
Fixes
- Checkpoint now works again. Something in the map changed which caused it to break.


## 1.2.0 (2023-May-6) - The Physics Update

Features
- Added the **Wall Run** module
    - This module allows you to seamlessly transition between walking on the floor to walls and ceilings.
- Added the **Slippery Hands** module
    - This module makes every surface slippery like ice. You can ski anywhere!
- Added the **No Slip** module
    - This module disables all sliding (I already had the opposite made so why not just invert it?).
- Added the **Freeze** module
    - This mod freezes your player in place, essentially disabling physics for your body. Good for setting up camera angles, or just taking a breather!

Changes
- **All new look!**
    - Prettied up the menu a little and moved the help text to the side for easier reading.
    - The version is now always displayed at the bottom of the screen.
- Teleport is now much more user-friendly. 
- You can now move your head freely while using Piggyback
- Buttons now have a short cooldown after being pressed to prevent double-tapping
- Buttons now stay pressed in while you hold them down
- Refactored some of the code structure

Fixes
- Airplane now works the consistently across Steam & Oculus stores. Previously Steam users would have to bend their arms and extra
45 degrees to make the mod work. (o O o )
- Reduced lag when opening the interface for the first time

Removed
- Removed Double Jump. It just doesn't fit in with the rest of the mod very well.

## 1.1.2 - Bugs and bugs and bugs, oh my

Changes
- Platforms are less sticky
- You can no longer activate No Collide while in a tight spot, preventing the player from getting teleported under the map
- Walking through level triggers now removes your checkpoint and plays a sound effect to prevent people from unloading areas before joining public lobbies.
- Checkpoint now makes a sound when the checkpoint is successfully placed
- Piggyback now makes a sound when you get kicked off
- Buttons now turn gray when not interactable
- Buttons now vibrate when you press them
- Chest beats now expire after 1 second, so that they won't collect over time and trigger the menu on accident
- Improved Airplane tutorial
- Made Teleport a little easier to activate and improved the tutorial
- Made the Boxing collider a little smaller
- Palm colliders are on the rig's hands now instead of the player's actual hand locations. This may reduce misleading feedback.
    - This might be a bad idea because of the smoothing, and I might roll this back at some point
- Refactored a lot of the code for readability and consistency

Fixes
- Fixed NoClip and Piggyback warping you into the floor
- The page buttons can no longer be pressed on accident when you throw the menu
- Fixed Unity errors being thrown by the BarkModule class
- Fixed Double Jump tutorial

## 1.1.1 (2023-Apr-19) - No Collide Fix

Fixes
* Resolved [Issue #5](https://github.com/KyleTheScientist/Bark/issues/5)
* Fixed scaling issues with the Grappling Hooks module

Changes
* Adjusted piggyback distance
* Added back double jump

## 1.1.0 (2023-Apr-17) - Grappling Hooks

Features
* New module - Grappling Hooks. Adds some banana grapple guns to your hips that you can grab and fire to pull yourself around.

Changes
* Reduced the wind-up time on the teleport from 2 seconds to 1

Fixes
* Resolved [Issue #1](https://github.com/KyleTheScientist/Bark/issues/1)

Removed
* Temporarily removed the Double Jump module (it should be back eventually, I just want to rework it a bit).

## 1.0.1 (2023-Apr-17) - Hotfix

Fixes
* Fixed bug that allowed players to use the mod in public lobbies

## 1.0.0 (2023-Apr-14) - Initial mod release

Features

* Movement
    * Platforms (Left/Right) - Press the grip button on your controller to create a floating platform in the air that you can stand on. There's a toggle for each hand so that you can disable one if it clashes with another mod's inputs.
    * Double Jump - Press the primary button (A) on your right controller in the air to get a boost in the direction you're looking.
    * Airplane - This allows the player to fly around. Spread your arms out like and airplane to activate and use your wrists to steer!
    * Speed - Significantly increases the player's movement speed
* Physics
    * Low Gravity - Decreases the strength of gravity
    * No Collide - Allows the player to fall through solid objects. Enabling this automatically enables platforms so you don't fall to your death!
* Teleportation
    * Checkpoint - Grip the left trigger to start summoning a holographic banana above your hand. Grip the right trigger to warp back to it.
    * Teleport - Make a triangle with your thumbs and index fingers and peer through it to initiate a teleport. Use your head to aim more finely.
* Interactions
    * Boxing - Better known as "punch mod", this allows the player to be punched around by others in the lobby. You can't be punched while touching the ground.
    * Piggyback - Allows you to ride other players! To mount someone, have them give you two-thumbs-up, and then grip their shoulder. If they give you two thumbs down at any point, you'll stop riding them. Consent is important!
    * X-Ray - Allows you to see other players through walls.