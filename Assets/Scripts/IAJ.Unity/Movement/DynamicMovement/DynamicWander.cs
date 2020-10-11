using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{

    public class DynamicWander : DynamicSeek
    {
        public float WanderOffset { get; set; }
        public float WanderRadius { get; set; }
        public float WanderRate { get; set; }
        public float TurnAngle { get; set; }

        public Vector3 CircleCenter { get; private set; }

        public GameObject DebugTarget { get; set; }

        protected float WanderOrientation { get; set; }


        public DynamicWander()
        {
            this.Target = new KinematicData();
            this.WanderOrientation = 0;
            this.WanderRadius = 5.0f;
        }

        public override string Name
        {
            get { return "Wander"; }
        }


        public override MovementOutput GetMovement()
        {
            this.WanderOrientation += RandomHelper.RandomBinomial() * this.WanderRate;

            var targetOrientation = this.WanderOrientation + this.Character.Orientation;

            this.CircleCenter = this.Character.Position + WanderOffset * MathHelper.ConvertOrientationToVector(Character.Orientation);

            this.Target.Position = this.CircleCenter + WanderRadius * MathHelper.ConvertOrientationToVector(targetOrientation);

            if(this.DebugTarget != null)
            {
                this.DebugTarget.transform.position = this.Target.Position;
                
            }
            return base.GetMovement();
        }
    }
}
