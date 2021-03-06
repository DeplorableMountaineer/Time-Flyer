﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pawn {
    /**
     * Keeps track of all moving bodies: player, enemies, missiles
     */
    public class AllMovingBodies : MonoBehaviour {
        private readonly HashSet<Rigidbody2D> _bodies = new HashSet<Rigidbody2D>();

        [SerializeField] private float radius = .5f;

        public HashSet<Rigidbody2D> Bodies => _bodies;

        /**
         * Add new body to list; called by MovingBody component.
         */
        public void OnSpawn(Rigidbody2D rb) {
            Bodies.Add(rb);
        }

        /**
         * Remove destroyed body from list; called by MovingBody component
         */
        public void OnDeath(Rigidbody2D rb) {
            Bodies.Remove(rb);
        }

        /**
         * Determine next body that self will likely collide with, with linear prediction.
         * In addition to returning the rigid body, returns direction to accelerate to prevent the collision
         */
        public Rigidbody2D FindLikeliestCollision(Rigidbody2D self, float collisionAvoidanceThreshold,
            out Vector2 avoidanceDirection) {
            avoidanceDirection = default;
            float shortestTime = Mathf.Infinity;
            Rigidbody2D firstTarget = null;
            float firstMinSeparation = 0;
            float firstDistance = 0;
            Vector2 firstRelativePos = default;
            Vector2 firstRelativeVel = default;
            foreach(Rigidbody2D rb in Bodies.Where(rb => rb != self)) {
                avoidanceDirection = rb.position - self.position;
                float distance = avoidanceDirection.magnitude;
                if(distance > collisionAvoidanceThreshold) continue;
                Vector2 relativeVel = rb.velocity - self.velocity;
                float relativeSpeedSqr = relativeVel.sqrMagnitude;
                //to find time of closest approach, relativePosition dot relativeVelocity,
                //divided by square of magnitude of velocity
                float timeToCollision = Vector2.Dot(avoidanceDirection, relativeVel)/relativeSpeedSqr;
                float relativeSpeed = Mathf.Sqrt(relativeSpeedSqr);
                float minSeparation = distance - relativeSpeed*timeToCollision;
                if(minSeparation > 2*radius) continue; //a near miss, skip it
                //if something else collides sooner, or if closest approach is already in the past, skip it
                if(timeToCollision <= 0 || timeToCollision > shortestTime) continue;
                shortestTime = timeToCollision;
                firstTarget = rb;
                firstMinSeparation = minSeparation;
                firstDistance = distance;
                firstRelativePos = avoidanceDirection;
                firstRelativeVel = relativeVel;
            }

            if(!firstTarget) {
                avoidanceDirection = default;
                return null;
            }

            //if already colliding, separate from current position, else separate from predicted position
            if(firstMinSeparation <= 0 || firstDistance < 2*radius) avoidanceDirection = firstRelativePos.normalized;
            else avoidanceDirection = (firstRelativePos + firstRelativeVel*shortestTime).normalized;

            if(Mathf.Abs(avoidanceDirection.magnitude - 1) > .0001f &&
               avoidanceDirection.magnitude > .0001f) {
                Debug.Log("Bad direction: " + avoidanceDirection);
                avoidanceDirection = default;
                return null;
            }

            return firstTarget;
        }
    }
}
