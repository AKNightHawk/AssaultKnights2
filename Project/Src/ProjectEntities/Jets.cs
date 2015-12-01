using System;
using System.ComponentModel;
using Engine.MathEx;
using Engine.PhysicsSystem;

namespace ProjectEntities
{
    public class JetsType : DynamicType
    {
        public enum JetTypes
        {
            JumpJets,
            FlyJets,
            DashJets,
        }

        [FieldSerialize]
        [DefaultValue(JetsType.JetTypes.JumpJets)]
        private JetTypes jetType = JetTypes.JumpJets;

        [DefaultValue(JetsType.JetTypes.JumpJets)]
        public JetTypes JetType
        {
            get { return jetType; }
            set { jetType = value; }
        }

        [FieldSerialize]
        [DefaultValue(5f)]
        private float velocity = 5f;

        [Description("How much velocity is added every tick.")]
        public float Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        [FieldSerialize]
        [DefaultValue("")]
        private string animation = "";

        [Description("Animation played when using jets.")]
        public string Animation
        {
            get { return animation; }
            set { animation = value; }
        }

        [FieldSerialize]
        [DefaultValue(25f)]
        private float fuel = 25f;

        [Description("Amount of jet fuel in jets.")]
        public float Fuel
        {
            get { return fuel; }
            set { fuel = value; }
        }

        [FieldSerialize]
        [DefaultValue(2f)]
        private float reloadSpeed = 2f;

        [Description("Amount of jet fuel reloaded in one second.")]
        public float ReloadSpeed
        {
            get { return reloadSpeed; }
            set { reloadSpeed = value; }
        }

        [FieldSerialize]
        [DefaultValue(2f)]
        private float fuelUsage = 2f;

        [Description("Amount of jet fuel spent in one second. Only for FlyJets")]
        public float FuelUsage
        {
            get { return fuelUsage; }
            set { fuelUsage = value; }
        }
    }

    public class Jets : Dynamic
    {
        private JetsType _type = null; public new JetsType Type { get { return _type; } }

        private float secCounter = 0f;
        private float fuel = 0f;
        private float reloadAmount = 0f;

        private bool keepFlying = false;

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();

            fuel = Type.Fuel;
            reloadAmount = Type.ReloadSpeed;
        }

        protected override void OnTick()
        {
            base.OnTick();

            secCounter += TickDelta;

            if (secCounter >= 1)
            {
                secCounter = 0f;
                fuel += reloadAmount;

                if (fuel > Type.Fuel) fuel = Type.Fuel;
            }

            if (keepFlying) FireJets(false);

            TickAnimations();
        }

        public void FireJets(bool toggle)
        {
            Mech mech = GetParentUnit() as Mech;

            if (mech == null) return;

            if (Type.JetType == JetsType.JetTypes.DashJets)
            {
                FireDashJets(mech);
            }
            else if (Type.JetType == JetsType.JetTypes.JumpJets)
            {
                FireJumpJets(mech);
            }
            else if (Type.JetType == JetsType.JetTypes.FlyJets)
            {
                keepFlying = toggle == false ? keepFlying : !keepFlying;
                FireFlyJets(mech);
            }
        }

        private void TickAnimations()
        {
        }

        private void FireJumpJets(Mech mech)
        {
            //mech.PhysicsModel.Bodies[0].AddForce(ForceType.GlobalAtGlobalPos, TickDelta, Vec3.ZAxis*Type.Force, mech.Position);
            //mech.PhysicsModel.Bodies[0].AddForce(ForceType.GlobalAtGlobalPos, TickDelta, Vec3.ZAxis * 2500f, mech.Position);

            if (fuel < Type.Fuel) return;

            fuel = 0f;

            Vec3 vel = mech.PhysicsModel.Bodies[0].LinearVelocity;

            vel.Z = Type.Velocity;
            mech.PhysicsModel.Bodies[0].LinearVelocity = vel;
        }

        private void FireDashJets(Mech mech)
        {
            if (fuel < Type.FuelUsage) return;

            fuel -= Type.FuelUsage;

            Vec3 vel = mech.PhysicsModel.GetBody("mainBody").Rotation.GetForward();

            vel = vel.GetNormalize() * Type.Velocity;

            mech.PhysicsModel.GetBody("mainBody").LinearVelocity = vel;
        }

        private void FireFlyJets(Mech mech)
        {
            if (fuel < Type.FuelUsage || !keepFlying)
            {
                keepFlying = false;
                return;
            }

            fuel -= Type.FuelUsage;

            Vec3 vel = mech.MainGun.Rotation.GetForward();

            vel = vel.GetNormalize() * Type.Velocity;

            //mech.PhysicsModel.Bodies[0].LinearVelocity = vel;
            mech.PhysicsModel.Bodies[0].AddForce(ForceType.GlobalAtLocalPos, 0, 1000f * vel, Position);
        }
    }
}