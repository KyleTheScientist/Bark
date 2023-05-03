# Manual Testing Procedures

## Public Lobbies
- Verify the menu cannot be opened offline
- Verify the menu cannot be opened offline after disabling/enabling the mod
- Verify the menu cannot be opened in a standard lobby 
- Verify the menu cannot be opened in a standard lobby after disabling/enabling the mod

## Documentation
- Grammar check the help text for each module 
- Verify the CHANGELOG

## Logs
- Validate no debug messages are being printed to the log

## Modules

### Airplane
- The user should be able to fly and steer by t-posing and rotating palms
- The user should not be able to fly while holding any triggers
- The user should not be able to fly after Airplane is disabled

### Double Jump
- The user should be launched toward their look-vector when A is pressed
- The user should not be able to jump twice in the air without touching the ground in-between
- The user should not be able to double jump after Double Jump is disabled

### Grappling Hooks
- The user should be given bananas when Grappling Hooks is enabled
- The user should be able to grab the bananas
- The user should be able to fire the bananas when in range
- The user should be able to steer in the air while firing the bananas
- The laser should disappear when out of range
- The user should not be able to fire the bananas when out of range
- The laser should disappear when the banana is holstered
- The bananas should be deleted when Grappling Hooks is disabled

### Platforms
- The user should be able to summon just one platform if only one platform button is active
- The user should not stick to the platforms
- The platforms should disappear after 2 seconds
- The platforms should "feel good"
- The platforms should be deleted after Platforms is disabled

### Speed Boost
- The user should move faster while Speed Boost is active
- The user's speed should reset after Speed Boost is disabled

### Wall Run

- The user should be able to walk on walls and ceilings
- Gravity should reset if the user is too far from the last placed they touched
- The direction gravity should reset if Wall Run is disabled

### Freeze 
- The user should not be affected by physics while Freeze is enabled
- The user should be affected by physics after Freeze is disabled

### No Collide
- The user should be able to walk through objects while No Collide is enabled
- The user should not be able to load maps while No Collide is enabled
- The user should return to the location they were initially standing when they hit the "Quit Trigger" while No Collide is enabled
- The user should return to the location they were initially standing when No Collide is disabled
- The user should be able to touch things when No Collide is disabled

### Low Gravity
- The strength of gravity should be reduced after Low Gravity is enabled
- The strength of gravity should be reset after Low Gravity is disabled

### Slippery Hands
- The user should slide on all materials while Slippery Hands is enabled
- The user should not slide on normal materials while Slippery Hands is disabled
- The user should slide on ice after Slippery Hands is disabled

### Boxing
- All players should get boxing gloves while Boxing is enabled
- Players should be able to move the user while Boxing is enabled
- Players should not be able to move the user after Boxing is disabled
- Players that join the lobby after boxing is enabled should receive boxing gloves
- All players should have their boxing gloves removed after Boxing is disabled

### Piggyback
- The user should not be able to mount a non-consenting player
- The user should be able to mount a consenting player
- The user should not be able to mount a player while near a wall
- Mounted players should be able to dismount the user with thumbs down
- Non-mounted players should not be able to dismount the user with thumbs down
- The user should return to the location they were initially standing when they let go 
- The user should return to the location they were initially standing when rejected

### X-Ray
- The user should be able to see all players through walls while X-Ray is enabled
- Players that join after X-Ray is enabled should receive X-Ray shader
- The user should not be able to see players through walls after X-Ray is disabled

### Checkpoint
- The user should be able to set a checkpoint
- The user should be able to return to a checkpoint
- The user should not be able to return to a checkpoint after it disappears
- The checkpoint should disappear when the user walks through a level trigger
- The user should not be able to set a checkpoint while in a level trigger
- The checkpoint should disappear after Checkpoint is disabled
- The user should not be able to set a checkpoint after Checkpoint is disabled
- The user should not be able to return to a checkpoint after Checkpoint is disabled

### Teleport
- The user should be able to teleport by making a triangle and holding it to their head while Teleport is enabled
- A rainbow triangle should appear when the user is doing the symbol while Teleport is enabled
- A banana should appear when the user is teleporting while Teleport is enabled
- The banana should disappear if the user breaks the triangle
- The banana should disappear if Teleport is disabled while the user is teleporting
- The rainbow triangle should disappear if Teleport is disabled while the user is teleporting
- The user should not be able to teleport after Teleport is disabled
- The user

## Cohesion (Validate that mods that cover similar ground can be used together)

### Platforms & No Collide
- Platforms should automatically enable when No Collide is active
- The player should be able to touch the platforms while No Collide is active

### Platforms & Freeze
- The user should be able to move around with Platforms while Freeze is enabled

### Checkpoint & No Collide
- Checkpoint should not be settable while using no collide
- Checkpoint should be returnable while using no collide

### Piggyback & No Collide
- No Collide should automatically enable when Piggyback is active
- No Collide should not be disable-able while Piggyback is 

### Low Gravity & Wall Run
- The scale of gravity should not change if Wall Run is enabled after Low Gravity
- The scale of gravity should not change if Wall Run is disabled while Low Gravity is active
- The scale of gravity should change if Low Gravity is enabled while Wall Run is active
- The scale of gravity should change if Low Gravity is disabled while Wall Run is active

### Teleport & Piggyback
- The player should not be able to teleport while Piggyback is active


