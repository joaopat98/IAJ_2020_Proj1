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
        public float NumSamples { get; set; }
        public float CharWeight { get; set; }

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<GameObject> obs)
        {
            this.DesiredMovement = goalMovement;
            base.Target = new KinematicData();
            Characters = movingCharacters;
            Obstacles = obs;
        }

        public override MovementOutput GetMovement()
        {
            var desiredMovementOutput = this.DesiredMovement.GetMovement();
            //if movementOutput is acceleration we need to convert it to velocity
            var desiredVelocity = Character.velocity + desiredMovementOutput.linear;
            //trim velocity if bigger than max
            if (desiredVelocity.magnitude > MaxSpeed)
            {
                desiredVelocity.Normalize();
                desiredVelocity *= MaxSpeed;
            }
            //2) generate samples
            //always consider the desired velocity as a sample
            var samples = new List<Vector3>();
            samples.Add(desiredVelocity);
            for (int i = 0; i < NumSamples; i++)
            {
                float angle = Random.value * 2 * Mathf.PI; //random angle between 0 and 2PI
                float magnitude = Random.value * MaxSpeed; // random magnitude between 0 and maxSpeed
                Vector3 velocitySample = MathHelper.ConvertOrientationToVector(angle) * magnitude;
                samples.Add(velocitySample);
            }
            //3) evaluate and get best sample
            base.Target.velocity = GetBestSample(desiredVelocity, samples);
            Debug.DrawRay(base.Character.Position, base.Target.velocity);
            //4) let the base class take care of achieving the final velocity
            return base.GetMovement();

        }

        Vector3 GetBestSample(Vector3 desiredVelocity, List<Vector3> samples)
        {
            Vector3 bestSample = Vector3.zero; //default velocity if all samples suck
            float minimumPenalty = Mathf.Infinity;
            foreach (var sample in samples)
            {
                //penalty based on the distance to desired velocity
                float distancePenalty = (desiredVelocity - sample).magnitude;
                float maximumTimePenalty = 0;
                foreach (var b in Characters)
                {
                    if (b != Character)
                    {

                        Vector3 deltaP = b.Position - Character.Position;
                        if (deltaP.magnitude > IgnoreCharDistance) //we can safely ignore this character
                            continue;
                        //test the collision of the ray λ(pA,2vA’-vA-vB) with the circle
                        Vector3 rayVector = 2 * sample - Character.velocity - b.velocity;
                        float tc = MathHelper.TimeToCollisionBetweenRayAndCircle(Character.Position, rayVector, b.Position, CharacterSize * 2);
                        float timePenalty = 0;
                        if (tc > 0) //future collision
                            timePenalty = CharWeight / tc;
                        else if (tc == 0) //immediate collision
                        {
                            timePenalty = Mathf.Infinity;
                            maximumTimePenalty = timePenalty;
                            break;
                        }
                        if (timePenalty > maximumTimePenalty) //opportunity for optimization here
                            maximumTimePenalty = timePenalty;
                    }
                }
                if (maximumTimePenalty != Mathf.Infinity)
                {
                    foreach (var b in Obstacles)
                    {
                        Vector3 deltaP = b.transform.position - Character.Position;
                        //test the collision of the ray λ(pA,2vA’-vA-vB) with the circle
                        //Vector3 rayVector = sample;
                        RaycastHit hit;
                        float timePenalty = 0;
                        if (sample.sqrMagnitude != 0)
                        {
                            //float tc = MathHelper.TimeToCollisionBetweenRayAndCircle(Character.Position, rayVector, b.GetComponent<Collider>().ClosestPoint(Character.Position), ObstacleSize + CharacterSize);
                            bool collided = b.GetComponent<Collider>().Raycast(new Ray(Character.Position, sample.normalized), out hit, IgnoreObsDistance);
                            float dist = collided ? Vector3.Distance(hit.point, Character.Position) : Mathf.Infinity;
                            float tc = collided ? (dist - CharacterSize) / MaxSpeed : -1;
                            if (tc > 0) //future collision
                                timePenalty = ObstacleWeight / tc;
                            else if (dist <= CharacterSize) //immediate collision
                            {
                                timePenalty = Mathf.Infinity;
                                maximumTimePenalty = timePenalty;
                                break;
                            }
                            if (timePenalty > maximumTimePenalty) //opportunity for optimization here
                                maximumTimePenalty = timePenalty;
                        }
                    }
                }
                float penalty = distancePenalty + maximumTimePenalty;
                //opportunity for optimization here
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
