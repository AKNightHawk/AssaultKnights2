using System.ComponentModel;
using Engine.MathEx;
using Engine.PhysicsSystem;

namespace ProjectEntities.Community
{
    public class ElevatorType : DynamicType
    {
    }

    public class Elevator : Dynamic
    {
        [FieldSerialize]
        public bool cycle = false;

        [FieldSerialize]
        private float maxElevation = 1;

        [FieldSerialize]
        public bool platformMoving = false;

        [FieldSerialize]
        private float velocity = 1f;

        private float initialZ;
        private float direction = 1;

        private ElevatorType _type = null; public new ElevatorType Type { get { return _type; } }

        [Description("The highest point above the start position that the elevator will reach.")]
        [DefaultValue(10f)]
        public float TopZ
        {
            get { return maxElevation; }
            set { maxElevation = value; }
        }

        [Description("The speed at which to move the elevator.")]
        [DefaultValue(1f)]
        public float Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        [Description("Determines if the elevator is in motion or not.")]
        [DefaultValue(false)]
        public bool PlatformMoving
        {
            get { return platformMoving; }
            set { platformMoving = value; }
        }

        [Description("Determines if the elevator should go back to the bottom, and repeat the process after reaching the top.")]
        [DefaultValue(false)]
        public bool Cycle
        {
            get { return cycle; }
            set { cycle = value; }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            initialZ = PhysicsModel.Bodies[0].Position.Z;
            SubscribeToTickEvent();
        }

        protected override void OnTick()
        {
            base.OnTick();
            if (PlatformMoving == true)
            {
                PhysicsModel.Bodies[0].LinearVelocity = new Vec3(0, 0, velocity * direction);

                if ((PhysicsModel.Bodies[0].Position.Z >= (initialZ + maxElevation) && direction == 1) ||
                    (PhysicsModel.Bodies[0].Position.Z <= initialZ && direction == -1))
                {
                    if (cycle == false)
                        PlatformMoving = false;
                    PhysicsModel.Bodies[0].LinearVelocity = Vec3.Zero;
                    direction = -direction;
                }
            }
        }
    }
}