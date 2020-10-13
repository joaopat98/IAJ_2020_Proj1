using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidObstacle : DynamicSeek
    {
        public override string Name
        {
            get { return "Avoid Obstacle"; }
        }

        private GameObject obstacle;

        public GameObject Obstacle
        {
            get { return this.obstacle; }
            set
            {
                this.obstacle = value;
                this.ObstacleCollider = value.GetComponent<Collider>();
            }
        }

        private Collider ObstacleCollider { get; set; }
        public float MaxLookAhead { get; set; }
        public float MaxLookAheadWhiskers { get; set; }
        public float AvoidMargin { get; set; }

        public float FanAngle { get; set; }

        public DynamicAvoidObstacle(GameObject obstacle)
        {
            this.Obstacle = obstacle;
            this.Target = new KinematicData();
        }

        public override MovementOutput GetMovement()
        {
            int numHits = 0;
            Vector3 acumTarget = Vector3.zero;

            var ray_front = new Ray(Character.Position, Character.velocity.normalized);
            var ray_left = new Ray(Character.Position, MathHelper.Rotate2D(Character.velocity, FanAngle).normalized);
            var ray_right = new Ray(Character.Position, MathHelper.Rotate2D(Character.velocity, -FanAngle).normalized);
            if (Character.velocity.sqrMagnitude != 0)
            {
                AddTarget(ref numHits, ref acumTarget, ray_front, MaxLookAhead);
                AddTarget(ref numHits, ref acumTarget, ray_left, MaxLookAhead * 2 / 3);
                AddTarget(ref numHits, ref acumTarget, ray_right, MaxLookAhead * 2 / 3);
            }


            Debug.DrawRay(ray_front.origin, ray_front.direction * MaxLookAhead);
            Debug.DrawRay(ray_left.origin, ray_left.direction * MaxLookAhead * 2 / 3);
            Debug.DrawRay(ray_right.origin, ray_right.direction * MaxLookAhead * 2 / 3);
            if (numHits == 0)
                return new MovementOutput();

            base.Target.Position = acumTarget / numHits;
            return base.GetMovement();
        }

        private RaycastHit AddTarget(ref int numHits, ref Vector3 acumTarget, Ray ray, float length)
        {
            RaycastHit hit;
            if (ObstacleCollider.Raycast(ray, out hit, length))
            {
                numHits++;
                acumTarget += hit.point + hit.normal * AvoidMargin;
            }

            return hit;
        }
    }
}
