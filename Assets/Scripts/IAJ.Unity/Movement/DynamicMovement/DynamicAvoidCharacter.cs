using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidCharacter : DynamicMovement
    {
        public override string Name
        {
            get { return "Avoid Character"; }
        }

        public float MaxTimeLookAhead { get; set; }
        public float CollisionRadius { get; set; }


        public DynamicAvoidCharacter(KinematicData Target)
        {
            this.Target = Target;
            this.Output = new MovementOutput();
        }

        public override MovementOutput GetMovement()
        {
            this.Output.Clear();

            var deltaPos = Target.Position - Character.Position;
            var deltaVel = Target.velocity - Character.velocity;
            var deltaSqrSpeed = deltaVel.sqrMagnitude;

            if (Mathf.Abs(deltaSqrSpeed - 0) <= 0.01) return new MovementOutput();

            var timeToClosest = -Vector3.Dot(deltaPos, deltaVel) / deltaSqrSpeed;
            if (timeToClosest > MaxTimeLookAhead) return new MovementOutput();
            //for efficiency reasons I use the deltas instead of Character and Target
            var futureDeltaPos = deltaPos + deltaVel * timeToClosest;
            var futureDistance = futureDeltaPos.magnitude;

            if (futureDistance > 2 * CollisionRadius) return new MovementOutput();
            if (futureDistance <= 0 || deltaPos.magnitude < 2 * CollisionRadius)
                //deals with exact or immediate collisions
                Output.linear = Character.Position - Target.Position;
            else
                Output.linear = -futureDeltaPos;
            Output.linear = Output.linear.normalized * MaxAcceleration;
            Output.linear.y = 0;

            return this.Output;

        }
    }
}
