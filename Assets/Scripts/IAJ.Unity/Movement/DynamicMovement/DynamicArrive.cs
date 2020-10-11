using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicArrive : DynamicVelocityMatch
    {

        public float MaxSpeed { get; set; }
        public float StopRadius { get; set; }
        public float SlowRadius { get; set; }
        public KinematicData ArriveTarget { get; set; }

        //public float TurnAngle { get; set; }

        public DynamicArrive()
        {
            this.Target = new KinematicData();
            this.MaxSpeed = 3.0f;
            this.StopRadius = 1.0f;
            this.SlowRadius = 5.0f;
        }

        public override string Name
        {
            get { return "Arrive"; }
        }

        public override MovementOutput GetMovement()
        {
            var direction = this.ArriveTarget.Position - this.Character.Position;
            var distance = direction.magnitude;
            float targetSpeed;

            if (distance < StopRadius)
            {
                targetSpeed = 0.0f;
            }
            else
            {
                targetSpeed = MaxSpeed * (distance / SlowRadius);
            }

            Target.velocity = direction.normalized * targetSpeed;

            return base.GetMovement();
        }

    }
}
