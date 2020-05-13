# Time-Flyer
### Time heals all wounds!  Fly through time, evade when hurt to heal



A 2D arcade-style top-down space shooter

Made in Unity Engine 2019.3.13f1 personal

* Moving bodies are missiles and pawns

* Pawns are enemies and the player ship

* Enemies can fly solo (a flock of one) or group into a flock.

  * A flock has a leader and one or more support elements that follow the leader
  * A flock of one has a leader with no support
  * When a support element is hit, it breaks from the flock till it heals, and enters flee mode
  * When the leader is hit, if not already in flee mode, it enters flee mode

* Flocks are spawned in waves
  * A new wave begins when all flocks of the previous wave have been destroyed
  * When all waves have been destroyed, the player goes to the next time zone

* Enemy AI includes steering and flocking
  * support elements try to keep up with the flock while maintaining separation: flock support members repel each other (inverse square law) if too close, while trying to match the velocity of the center of mass and trying to maintain a constant distance from that center of mass.
  * The leader seeks the player target, attacks (fires missiles) when in range, using (linear) predictive targeting, then flees, and then seeks again
  * The support elements also attack when in range, using linear predictive targeting.
  * If the leader is destroyed, another flock member, if any remain, is immediately promoted to leader.
  * If the player is destroyed, the leader goes into wander mode
  * When a support element is hit, it breaks from the flock till it heals, and enters flee mode
  * When the leader is hit, if not already in flee mode, it enters flee mode
  * Enemy AI also includes collision avoidance from other moving bodies.

* On being hit, a pawn loses health.
  * The pawn regains health over time, unless it dies
    * thus the pawn must be shot several times quickly to kill it
  * Flames visually show the amount of damage
  * Enemy pawns flee when hit to give health time to restore

* Movement: WASD, and arrow keys for turning.  Space bar to fire.
  * The ship remains centered and pointing up (relative to the camera), while it appears that everything else moves and rotates.
  * There are two backgrounds--one moves to show the movement of the player, the other is fixed and represents a parallaxed infinite-distance background.
