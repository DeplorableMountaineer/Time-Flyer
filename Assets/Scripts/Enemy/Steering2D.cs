using System.Collections.Generic;
using UnityEngine;

namespace Enemy {
    /*Method names are from Craig Reynolds, Steering Behaviors for Autonomous Characters,
     *  https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf
     *  Algorithms modified from Ian Millington, AI for Games, 3rd Ed.*/

    [RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
    public class Steering2D : MonoBehaviour {
        private Rigidbody2D _rigidbody;

        #region Update

        /**
         * Accelerate the body by the given amount, but do not exceed speed limit.
         */
        public void UpdateSteering(Vector2 acceleration, float maxSpeed) {
            _rigidbody.velocity =
                Vector2.ClampMagnitude(_rigidbody.velocity + acceleration*Time.smoothDeltaTime, maxSpeed);
        }

        /**
         * Rotate to face given orientation.  All values are in degrees.  0 is up and positive is clockwise.
         * It's opposite just uses orientation + 180.  targetRadius is number of degrees it is allowed to miss by.
         * slowRadius is number of degrees from target before it starts braking to prevent overshooting.  Time to target
         * is how long it aims to reach target orientation, subject to speed limits.  
         */
        public void Align(float targetOrientation, float rotationSpeed,
            float targetRadius, float slowRadius) {
            float delta = (targetOrientation - _rigidbody.rotation)%360;
            if(delta > 180) delta -= 360;
            else if(delta < -180) delta += 360;
            float mag = Mathf.Abs(delta);
            if(mag < targetRadius) return;
            float targetRotation = rotationSpeed;
            if(mag <= slowRadius) targetRotation *= mag/slowRadius;
            targetRotation *= Mathf.Sign(delta);
            _rigidbody.rotation += targetRotation*Time.smoothDeltaTime;
        }

        #endregion


        #region Basic Steering

        /**
         * Return the orientation to face a given direction.  Direction need not be normalized.
         */
        public float Face(Vector2 direction) {
            return direction.magnitude < .0001f
                ? _rigidbody.rotation
                : Mathf.Atan2(-direction.x, direction.y)*Mathf.Rad2Deg;
        }


        /**
         * Return seek acceleration.  Seek accelerates toward target's current position
         * (no prediction) as fast as allowed.  It's opposite steering is Flee.
         */
        public Vector2 Seek(Vector2 targetPosition, float maxAcceleration) {
            return (targetPosition - _rigidbody.position).normalized*maxAcceleration;
        }

        /**
         * Return flee acceleration.  Flee accelerates away from target's current position
         * as fast as allowed.  This is the opposite of Seek, and is used as the opposite of Arrive.  
         */
        public Vector2 Flee(Vector2 targetPosition, float maxAcceleration) {
            return (_rigidbody.position - targetPosition).normalized*maxAcceleration;
        }

        /**
         * Return arrive acceleration.  Arrive is similar to Seek, except as it gets close to the target
         * position, it slows and stops.  The opposite of Arrive is just Flee, nothing more complex needed.
         * targetRadius is how close it needs to be to consider the body has having successfully arrived.
         * slowRadius is how close it needs to be before braking to prevent overshooting.  Time to target
         * is how long it aims to reach target, subject to speed limits.  
         */
        public Vector2 Arrive(Vector2 targetPosition, float maxAcceleration, float maxSpeed, float targetRadius,
            float slowRadius, float timeToTarget = .1f) {
            Vector2 direction = targetPosition - _rigidbody.position;
            float distance = direction.magnitude;
            if(distance < targetRadius) return default;
            float targetSpeed = maxSpeed;
            if(distance <= slowRadius) targetSpeed = maxSpeed*distance/slowRadius;
            Vector2 targetVelocity = direction.normalized*targetSpeed;
            return Vector2.ClampMagnitude(targetVelocity - _rigidbody.velocity, maxAcceleration);
        }

        /**
         * Returns acceleration to match the specified velocity in the given time, subject to the acceleration
         * limit.
         */
        public Vector2 MatchVelocity(Vector2 targetVelocity, float maxAcceleration, float timeToTarget = .1f) {
            return Vector2.ClampMagnitude((targetVelocity - _rigidbody.velocity)/timeToTarget,
                maxAcceleration);
        }

        /**
         * Pursue is a smarter Seek.  It accelerates to the predicted location of the target, assuming
         * the target maintains a constant velocity.  maxPrediction sets how many seconds into the future it looks.
         * Keeping it small means it reacts better to sudden velocity changes at the expense of
         * heading in the optimal direction if velocity doesn't change.  The opposite of Pursue is Evade.
         */
        public Vector2 Pursue(Vector2 targetPosition, Vector2 targetVelocity, float maxAcceleration,
            float maxPrediction = 1) {
            Vector2 direction = targetPosition - _rigidbody.position;
            float distance = direction.magnitude;
            float speed = _rigidbody.velocity.magnitude;
            float prediction = maxPrediction;
            if(speed > distance/maxPrediction) prediction = distance/speed;
            Vector2 target = targetPosition + targetVelocity*prediction;
            return Seek(target, maxAcceleration);
        }

        /**
         * Evade is a smarter Flee.  It is the opposite of Pursue. It accelerates away from the predicted location
         * of the target, assuming the target maintains a constant velocity.  maxPrediction sets how many seconds
         * into the future it looks. Keeping it small means it reacts better to sudden velocity changes at the
         * expense of heading in the optimal direction if velocity doesn't change.
         */
        public Vector2 Evade(Vector2 targetPosition, Vector2 targetVelocity, float maxAcceleration,
            float maxPrediction = 1) {
            Vector2 direction = targetPosition - _rigidbody.position;
            float distance = direction.magnitude;
            float speed = _rigidbody.velocity.magnitude;
            float prediction = maxPrediction;
            if(speed > distance/maxPrediction) prediction = distance/speed;
            Vector2 target = targetPosition + targetVelocity*prediction;
            return Flee(target, maxAcceleration);
        }

        #endregion


        #region Flocking Behaviors

        /**
         * Return acceleration for separation.  Separation keeps flock members (teammates) from getting too close to
         * each other.  threshold is how close they come before the separation behavior kicks in.  Decay coef
         * scales how separation strength decays with distance (inverse square law times the coef).
         */
        public Vector2 Separation(float maxAcceleration, IEnumerable<Rigidbody2D> flock, float threshold,
            float decayCoef = 1) {
            Vector2 result = default;
            foreach(Rigidbody2D target in flock) {
                if(target == _rigidbody) continue; //don't avoid self
                Vector2 direction = target.position - _rigidbody.position;
                float distance = direction.magnitude;
                if(!(distance < threshold)) continue;
                float strength = Mathf.Min(decayCoef/(distance*distance), maxAcceleration);
                result += direction*strength/distance;
            }

            return result;
        }

        /**
         * Return acceleration from cohesion.  Cohesion keeps the flock together as a flock by moving each
         * member toward the center of mass of the other members.  Use a wide targetRadius and slowRadius
         * to keep them from trying to get too close.
         */
        public Vector2 Cohesion(float maxAcceleration, IEnumerable<Rigidbody2D> flock, float maxSpeed,
            float targetRadius, float slowRadius, float timeToTarget = .1f) {
            Vector2 centerOfMass = default;
            float totalMass = 0;
            foreach(Rigidbody2D target in flock) {
                if(target == _rigidbody) continue; //don't consider self
                float mass = _rigidbody.mass;
                centerOfMass += target.position*mass;
                totalMass += mass;
            }

            centerOfMass /= totalMass;
            return Arrive(centerOfMass, maxAcceleration, maxSpeed, targetRadius, slowRadius, timeToTarget);
        }

        /**
         * Return the velocity of the center of mass of the flock
         */
        public Vector2 ComputeFlockVelocity(IEnumerable<Rigidbody2D> flock) {
            Vector2 velocity = default;
            float totalMass = 0;
            foreach(Rigidbody2D target in flock) {
                float mass = _rigidbody.mass;
                velocity += target.position*mass;
                totalMass += mass;
            }

            return velocity/totalMass;
        }

        #endregion


        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
        }
    }
}
