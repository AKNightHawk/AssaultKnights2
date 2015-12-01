using System;
using System.ComponentModel;
using Engine.EntitySystem;
using Engine.MathEx;
using Engine.PhysicsSystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="Bullet"/> entity type.
    /// </summary>
    public class MissileType : BulletType
    {
        [FieldSerialize]
        private float lockTime;

        [Description("Seconds needed to acquire targetlock")]
        public float LockingTime
        {
            get { return lockTime; }
            set
            {
                lockTime = value;
            }
        }

        [FieldSerialize]
        private Degree homingCorrection = 35.0f; // NH & KEEN - added default value (to fix a potential bug)

        [Description("Degrees per second the missile can turn, should be between 0 and 360.")]
        [DefaultValue(35.0f)] // NH & KEEN - changed to degrees to make it easier to predict
        public Degree HomingCorrection
        {
            get { return homingCorrection; }
            set
            {
                homingCorrection = value;
            }
        }

        [FieldSerialize]
        private float homingDelay;

        [Description("Delay in seconds when the missile will start homing to it's target")]
        [DefaultValue(0f)]
        public float HomingDelay
        {
            get { return homingDelay; }
            set
            {
                homingDelay = value;
            }
        }

        // NH & KEEN - makes the missiles predict target movement, is a float because setting it to more or less than 1 can be used for useful tweaking
        [FieldSerialize]
        private float homingPrediction = 0.0f; // default is no prediction, just like the old missiles

        [Description("How much does the missile predict targt movement. 1.0 is ideal trajectory.")]
        [DefaultValue(0.0f)]
        public float HomingPrediction
        {
            get { return homingPrediction; }
            set
            {
                homingPrediction = value;
            }
        }

        // NH & KEEN - the angle beyond which missiles lose their lock
        [FieldSerialize]
        private float homingAngle = 180.0f;

        [Description("The angle in which the missile can see it's target. 360 is all around vision.")]
        [DefaultValue(180.0f)]
        public float HomingAngle
        {
            get { return homingAngle; }
            set
            {
                homingAngle = value;
            }
        }

        [FieldSerialize]
        private float proximityDistance;

        [Description("Distance from target to trigger detonation. 0 distance will wait for hit.")]
        [DefaultValue(0f)]
        public float ProximityDistance
        {
            get { return proximityDistance; }
            set
            {
                proximityDistance = value;
            }
        }
    }

    public class Missile : Bullet
    {
        private MissileType _type = null; public new MissileType Type { get { return _type; } }

        private Dynamic target;

        private float currentDelay = 0f;
        private float startDistance = 0f;
        private bool firstTick = true;

        private Body targetBody;
        private Shape targetShape;

        [Browsable(false)]
        public Dynamic Target
        {
            get { return target; }
            set
            {
                target = value;
            }
        }

        protected override void OnTick()
        {
            base.OnTick();

            if (target == null) return;

            if (firstTick)
            {
                startDistance = (target.Position - Position).Length();
                firstTick = false;

                // NH & KEEN - if our target is a unit, select a random bodypart to target, otherwise check if there is a physics body to follow
                AKunit targetUnit = target as AKunit;

                if (targetUnit != null)
                {
                    if (targetUnit.PhysicsModel != null && targetUnit.PhysicsModel.Bodies.Length != 0)
                    {
                        string targetBodyName = targetUnit.Type.BodyParts[World.Instance.Random.Next(0, targetUnit.Type.BodyParts.Count)].PhysicsShape;

                        FindTargetShape(targetUnit, targetBodyName, out targetBody, out targetShape);
                    }
                }
                else
                {
                    if (target.PhysicsModel != null && target.PhysicsModel.Bodies.Length != 0)
                    {
                        targetBody = target.PhysicsModel.Bodies[0];
                    }
                }
            }

            if (currentDelay < Type.HomingDelay)
            {
                currentDelay += TickDelta;
                return;
            }

            // NH & KEEN - lose target if it is out of our view angle (stops missiles from doing endless circles around the target)
            if (Type.HomingAngle < 360.0f)
            {
                float homingAngle = Type.HomingAngle / 2;

                if (CalculateAngleToTarget(target) > new Degree(homingAngle).InRadians())
                {
                    target = null;
                    return;
                }
            }

            // NH & KEEN - explode if we get closer than proximityDistance to the target (used for Bluestreak Missiles)
            if ((target.Position - Position).Length() < Type.ProximityDistance)
            {
                HitObjects_Create();
                Die();
            }

            if (Type.HomingCorrection > 0)
            {
                MomentaryTurnToPositionUpdate(target.Position);
            }

            Vec3 velocity = Rotation.GetForward().GetNormalize() * Type.Velocity;
            base.Velocity = velocity;
        }

        // NH & KEEN - reworked the homing function for better accuracy
        private void MomentaryTurnToPositionUpdate(Vec3 turnToPosition)
        {
            if (targetBody != null)
            {
                turnToPosition = targetBody.Position;

                if (targetShape != null)
                {
                    turnToPosition += (targetShape.Position * targetBody.Rotation);
                }

                if (Type.HomingPrediction > 0.0f)
                {
                    float flyTime = (turnToPosition - Position).Length() / Type.Velocity;

                    turnToPosition += targetBody.LinearVelocity * flyTime * Type.HomingPrediction;
                }
            }

            Vec3 diff = (turnToPosition - Position);

            Degree horizontalAngle = new Radian(-MathFunctions.ATan(diff.Y, diff.X)).InDegrees();
            Degree verticalAngle = new Radian(MathFunctions.ATan(diff.Z, diff.ToVec2().Length())).InDegrees();

            Quat collisionRotation = new Angles(0, 0, horizontalAngle).ToQuat();
            collisionRotation *= new Angles(0, verticalAngle, 0).ToQuat();

            Radian targetAngle = Quat.GetAngle(Rotation, collisionRotation);

            float PI = (float)Math.PI;

            if (targetAngle > PI)
            {
                targetAngle = (2 * PI) - targetAngle;
            }

            Radian maxTurnAbility = Type.HomingCorrection.InRadians() * TickDelta;

            if (targetAngle > maxTurnAbility)
            {
                float relativeCorrection = maxTurnAbility / targetAngle;

                Rotation = Quat.Slerp(Rotation, collisionRotation, relativeCorrection);
            }
            else
            {
                Rotation = collisionRotation;
            }
        }

        // NH & KEEN - a function to calculate the angle to target
        private Radian CalculateAngleToTarget(Dynamic target)
        {
            Vec3 needDirection = (target.GetInterpolatedPosition() - Position).GetNormalize();
            Vec3 weaponDirection = Rotation.GetForward();

            Radian angle = Math.Abs(MathFunctions.ACos(Vec3.Dot(needDirection, weaponDirection)));
            return angle;
        }

        // NH & KEEN - searches all bodies of the target for a certain shape (used for bodypart targetting)
        private void FindTargetShape(Dynamic target, string targetShapeName, out Body targetBody, out Shape targetShape)
        {
            foreach (Body body in target.PhysicsModel.Bodies)
            {
                Shape shape = body.GetShape(targetShapeName);

                if (shape != null)
                {
                    targetBody = body;
                    targetShape = shape;

                    return;
                }
            }

            targetBody = null;
            targetShape = null;

            return;
        }
    }
}