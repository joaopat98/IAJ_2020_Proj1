//adapted to IAJ classes by João Dias and Manuel Guimarães

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Movement.VO
{
    public class RVOMovement : DynamicMovement.DynamicVelocityMatch
    {
        public override string Name
        {
            get { return "RVO"; }
        }

        protected List<KinematicData> Characters { get; set; }
        protected List<GameObject> Obstacles { get; set; }
        public float CharacterSize { get; set; }
        public float ObstacleSize { get; set; }
        public float ObstacleWeight { get; set; }
        public float IgnoreCharDistance { get; set; }
        public float IgnoreObsDistance { get; set; }
        public float MaxSpeed { get; set; }
        public int NumSamples { get; set; }
        public float CharWeight { get; set; }
        private Collider[] colliders;

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<GameObject> obs)
        {
            this.DesiredMovement = goalMovement;
            base.Target = new KinematicData();
            Characters = movingCharacters;
            Obstacles = obs;
            //Cache obstacle colliders on initialization
            colliders = Obstacles.Select(o => o.GetComponent<Collider>()).ToArray();
        }

        public override MovementOutput GetMovement()
        {
            var desiredMovementOutput = this.DesiredMovement.GetMovement();
            var desiredVelocity = Character.velocity + desiredMovementOutput.linear;
            if (desiredVelocity.magnitude > MaxSpeed)
            {
                desiredVelocity.Normalize();
                desiredVelocity *= MaxSpeed;
            }
            var samples = new Vector3[NumSamples];
            samples[0] = desiredVelocity;
            for (int i = 1; i < NumSamples; i++)
            {
                float angle = Random.value * 2 * Mathf.PI;
                float magnitude = Random.value * MaxSpeed;
                Vector3 velocitySample = MathHelper.ConvertOrientationToVector(angle) * magnitude;
                samples[i] = velocitySample;
            }
            base.Target.velocity = GetBestSample(desiredVelocity, samples);
            Debug.DrawRay(base.Character.Position, base.Target.velocity);
            return base.GetMovement();

        }

        Vector3 GetBestSample(Vector3 desiredVelocity, Vector3[] samples)
        {
            var charPos = Character.Position;
            Vector3 bestSample = Vector3.zero;
            float minimumPenalty = Mathf.Infinity;
            foreach (var sample in samples)
            {
                float distancePenalty = (desiredVelocity - sample).magnitude;
                float maximumTimePenalty = 0;

                foreach (var b in Characters)
                {
                    if (b != Character)
                    {
                        var otherPos = b.Position;

                        Vector3 deltaP = otherPos - charPos;
                        if (deltaP.magnitude > IgnoreCharDistance)
                            continue;
                        Vector3 rayVector = 2 * sample - Character.velocity - b.velocity;
                        float tc = MathHelper.TimeToCollisionBetweenRayAndCircle(charPos, rayVector, otherPos, CharacterSize * 2);
                        float timePenalty = 0;
                        if (tc > 0.0001)
                            timePenalty = CharWeight / tc;
                        else if (tc >= 0)
                        {
                            timePenalty = Mathf.Infinity;
                            maximumTimePenalty = timePenalty;
                            break;
                        }
                        if (timePenalty > maximumTimePenalty)
                            maximumTimePenalty = timePenalty;
                    }
                }
                if (maximumTimePenalty != Mathf.Infinity)
                {
                    var sampleRay = new Ray(charPos, sample.normalized);
                    RaycastHit hit;
                    foreach (var b in colliders)
                    {
                        float timePenalty = 0;
                        if (sample.sqrMagnitude != 0)
                        {
                            // Cast a ray from the character to each obstacle's collider
                            // Calculate the time to collision by dividing the distance from the character to the collision point by the velocity's magnitude
                            bool collided = b.Raycast(sampleRay, out hit, IgnoreObsDistance);
                            float dist = collided ? Vector3.Distance(hit.point, charPos) : Mathf.Infinity;
                            float tc = collided ? (dist - CharacterSize) / MaxSpeed : -1;
                            if (tc > 0)
                                timePenalty = ObstacleWeight / tc;
                            else if (dist <= CharacterSize)
                            {
                                timePenalty = Mathf.Infinity;
                                maximumTimePenalty = timePenalty;
                                break;
                            }
                            if (timePenalty > maximumTimePenalty)
                                maximumTimePenalty = timePenalty;
                        }
                    }
                }
                float penalty = distancePenalty + maximumTimePenalty;
                if (penalty < 0.001)
                {
                    minimumPenalty = penalty;
                    bestSample = sample;
                    break;
                }
                if (penalty < minimumPenalty)
                {
                    minimumPenalty = penalty;
                    bestSample = sample;
                }
            }
            return bestSample;
        }
    }
}
