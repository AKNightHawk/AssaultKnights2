using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using GameCommon;
using Engine.Utils;

namespace GameEntities
{
    /// <summary>
    /// Defines the <see cref="Bullet"/> entity type.
    /// </summary>
    public class MissileType : BulletType
    {
        [FieldSerialize]
        float radarRamge = 250f;

        [Description("Range of Unit Detection By Missile Radar")]
        [DefaultValue(250f)]
        public float RadarRange
        {
            get { return radarRamge; }
            set
            {
                radarRamge = value;
            }
        }

        [FieldSerialize]
        float lockTime;

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
        float homingCorrection = 0.2f;

        [Description("How much the missile will correct it's path 0.01 - 1.0")]
        [DefaultValue(0.2f)]
        public float HomingCorrection
        {
            get { return homingCorrection; }
            set
            {
                homingCorrection = value;
            }
        }
        [FieldSerialize]
        float homingDelay = 0.2f;

        [Description("Delay in seconds when the missile will start homing to it's target")]
        [DefaultValue(0.2f)]
        public float HomingDelay
        {
            get { return homingDelay; }
            set
            {
                homingDelay = value;
            }
        }

        [FieldSerialize]
        float proximityDistance;

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
        MissileType _type = null; public new MissileType Type { get { return _type; } }

        private Unit target;

        float currentDelay = 0f;
        float startDistance = 0f;
        bool firstTick = true;

        [Browsable(false)]
        public Unit Target
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

            if (currentDelay < Type.HomingDelay)
            {
                currentDelay += TickDelta;
                return;
            }

            if (target == null)
               target = AquireNewTarget();

            if (target == null)
                return;

            if (target.Died)
                target = AquireNewTarget();

            if (target == null)
            {
                this.Die();
                return;
            }

            if (firstTick)
            {
                startDistance = (target.Position - Position).LengthFast();
                firstTick = false;
            }

            Vec3 direction = target.Position - Position;

            if (direction.LengthFast() < 0.5f) base.Type.Gravity = 9.81f;

            MomentaryTurnToPositionUpdate(target.Position);

            Vec3 velocity = Rotation.GetForward().GetNormalizeFast() * Type.Velocity;
            base.Velocity = velocity;

        }

        private Unit AquireNewTarget()
        {
            Unit TempTarget = null;
            Bounds volume = new Bounds(this.Position);
            volume.Expand(new Vec3(Type.RadarRange, Type.RadarRange, Type.RadarRange));

            Body[] result = PhysicsWorld.Instance.VolumeCast(volume,
                (int)ContactGroup.CastOnlyDynamic);

            foreach (Body body in result)
            {
                MapObject obj = MapSystemWorld.GetMapObjectByBody(body);
                if (obj != null)
                {
                    Unit unit = obj as Unit;

                    if (unit != null)
                    {
                        float Angletounit = CalculateAngleTo(unit);
                        //0.8f in radian is 45 Degrees and the absoulote value whould give the missile a window of 90' 
                        if (Angletounit > 0.8f)
                            continue;

                        if (TempTarget == null)
                        {
                            TempTarget = unit;
                        }
                        else
                        {
                            float PreTargetDistance = (TempTarget.Position - Position).LengthFast();
                            float newTargetDistance = (unit.Position - Position).LengthFast();

                            float DifDis = PreTargetDistance - newTargetDistance;
                            float DiftAngle = CalculateAngleTo(TempTarget) - Angletounit;

                            if (DifDis > 0 && DiftAngle > 0)
                            {
                               TempTarget = unit;
                            }
                        }
                    }
                }
            }

            return TempTarget;
        }

        private float CalculateAngleTo(Unit unit)
        {
            Vec3 dir = (unit.GetInterpolatedPosition() - Position);

            Radian unitAngle = (Rotation.ToAngles().Yaw) / -57.29578f;
            Radian needAngle = MathFunctions.ATan(dir.Y, dir.X);
            Radian diffAngle = needAngle - unitAngle;
            
           
            return Math.Abs(diffAngle);
        }

        void MomentaryTurnToPositionUpdate(Vec3 turnToPosition)
        {

            if ((turnToPosition - Position).Length() < 4.0f)
            {
                Vec3 dir = (turnToPosition - Position).GetNormalize();
                turnToPosition += dir * 10f;
            }

            Vec3 diff = (turnToPosition - Position);
            Quat collisionRotation;
            float distance = diff.LengthFast();

            Radian horizontalAngle = MathFunctions.ATan(diff.Y, diff.X);
            Radian verticalAngle = MathFunctions.ATan(diff.Z, diff.ToVec2().Length());

            collisionRotation = new Angles(0, 0, -horizontalAngle.InDegrees()).ToQuat();

            collisionRotation *= new Angles(0, verticalAngle.InDegrees(), 0).ToQuat();

            Rotation = Quat.Slerp(Rotation, collisionRotation, Type.HomingCorrection);

        }
    }
}