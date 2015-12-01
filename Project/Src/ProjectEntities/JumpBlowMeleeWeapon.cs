// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.ComponentModel;
using System.Drawing.Design;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.SoundSystem;
using Engine.Utils;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="JumpBlowMeleeWeapon"/> entity type.
    /// </summary>
    public class JumpBlowMeleeWeaponType : MeleeWeaponType
    {
        [FieldSerialize]
        private Vec3 jumpVelocity;

        [FieldSerialize]
        private string soundBlowKick;

        //

        [DefaultValue(typeof(Vec3), "0 0 0")]
        public Vec3 JumpVelocity
        {
            get { return jumpVelocity; }
            set { jumpVelocity = value; }
        }

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundBlowKick
        {
            get { return soundBlowKick; }
            set { soundBlowKick = value; }
        }

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            PreloadSound(SoundBlowKick, SoundMode.Mode3D);
        }
    }

    public class JumpBlowMeleeWeapon : MeleeWeapon
    {
        private JumpBlowMeleeWeaponType _type = null; public new JumpBlowMeleeWeaponType Type { get { return _type; } }

        private bool firstTick = true;

        [FieldSerialize]
        private float lastJumpTime;

        private bool collisionEventInitialized;

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            SoundPlayBlowKickToClient,
            SoundPlayFireToClient,
        }

        ///////////////////////////////////////////

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            if (firstTick)
            {
                Character unit = (Character)AttachedMapObjectParent;

                foreach (Body body in unit.PhysicsModel.Bodies)
                    body.Collision += new Body.CollisionDelegate(attachedParentBody_Collision);
                collisionEventInitialized = true;

                firstTick = false;
            }

            if (lastJumpTime != 0)
                lastJumpTime += TickDelta;
        }

        protected override void OnDestroy()
        {
            if (collisionEventInitialized)
            {
                Character unit = (Character)AttachedMapObjectParent;
                if (unit != null)
                {
                    foreach (Body body in unit.PhysicsModel.Bodies)
                        body.Collision -= new Body.CollisionDelegate(attachedParentBody_Collision);
                }

                collisionEventInitialized = false;
            }

            base.OnDestroy();
        }

        private void attachedParentBody_Collision(ref CollisionEvent collisionEvent)
        {
            if (lastJumpTime == 0)
                return;

            lastJumpTime = 0;

            Dynamic objDynamic = MapSystemWorld.GetMapObjectByBody(
                collisionEvent.OtherShape.Body) as Dynamic;

            if (objDynamic == null)
                return;

            Character unit = (Character)AttachedMapObjectParent;
            if (unit == null || unit.Intellect == null)
                return;

            //Not kick allies
            Unit objUnit = objDynamic.GetParentUnitHavingIntellect();
            if (objUnit == null)
                return;
            if (objUnit.Intellect.Faction == unit.Intellect.Faction)
                return;

            objUnit.DoDamage(unit, unit.Position, collisionEvent.OtherShape,
                Type.NormalMode.Damage, true);

            SoundPlay3D(Type.SoundBlowKick, .5f, false);

            if (EntitySystemWorld.Instance.IsServer() &&
                Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                Server_SendSoundPlayBlowKick();
            }
        }

        protected override void Blow()
        {
            Character unit = (Character)AttachedMapObjectParent;

            if (!unit.IsOnGround())
                return;

            //jump
            lastJumpTime = .001f;

            unit.TryJump();
            unit.MainBody.Position += new Vec3(0, 0, .1f);//for no collision event on ground
            unit.MainBody.AngularVelocity = Vec3.Zero;
            unit.MainBody.LinearVelocity = unit.Rotation * Type.JumpVelocity;

            SoundPlay3D(Type.NormalMode.SoundFire, .5f, true);

            if (EntitySystemWorld.Instance.IsServer() &&
                Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                Server_SendSoundPlayFire();
            }
        }

        private void Server_SendSoundPlayBlowKick()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(JumpBlowMeleeWeapon),
                (ushort)NetworkMessages.SoundPlayBlowKickToClient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SoundPlayBlowKickToClient)]
        private void Client_ReceiveSoundPlayBlowKick(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            SoundPlay3D(Type.SoundBlowKick, .5f, false);
        }

        private void Server_SendSoundPlayFire()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(JumpBlowMeleeWeapon),
                (ushort)NetworkMessages.SoundPlayFireToClient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SoundPlayFireToClient)]
        private void Client_ReceiveSoundPlayFire(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            SoundPlay3D(Type.NormalMode.SoundFire, .5f, true);
        }
    }
}