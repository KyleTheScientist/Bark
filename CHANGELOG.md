# CHANGELOG.md

## 1.2.0 - Wall Running

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