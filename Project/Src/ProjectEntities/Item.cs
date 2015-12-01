// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MathEx;
using Engine.Utils;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="Item"/> entity type.
    /// </summary>
    public class ItemType : DynamicType
    {
        [FieldSerialize]
        private float defaultRespawnTime;

        [FieldSerialize]
        private string soundTake;

        //

        public ItemType()
        {
            AllowEmptyName = true;
        }

        [DefaultValue(0.0f)]
        public float DefaultRespawnTime
        {
            get { return defaultRespawnTime; }
            set { defaultRespawnTime = value; }
        }

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundTake
        {
            get { return soundTake; }
            set { soundTake = value; }
        }

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            //preload as 2D sound
            PreloadSound(SoundTake, 0);
        }
    }

    /// <summary>
    /// Items which can be picked up by units. Med-kits, weapons, ammunition.
    /// </summary>
    public class Item : Dynamic
    {
        [FieldSerialize]
        private float respawnTime;

        private Radian rotationAngle;

        private Vec3 server_sentPositionToClients;

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            //using special method of position synchronization (not using Dynamic class features),
            //because we need only position to be synchronized (without rotation and scale)
            PositionToClient,

            SoundPlayTakeToClient
        }

        ///////////////////////////////////////////

        private ItemType _type = null; public new ItemType Type { get { return _type; } }

        public Item()
        {
            rotationAngle = World.Instance.Random.NextFloat() * MathFunctions.PI * 2;
        }

        public float RespawnTime
        {
            get { return respawnTime; }
            set { respawnTime = value; }
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            if (EntitySystemWorld.Instance.IsEditor())
                respawnTime = Type.DefaultRespawnTime;
        }

        protected override void OnPreCreate()
        {
            base.OnPreCreate();

            //using special method of position synchronization (not using Dynamic class features),
            //because we need only position to be synchronized (without rotation and scale)
            Server_EnableSynchronizationPositionsToClients = false;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            bool editor = EntitySystemWorld.Instance.IsEditor();

            if (!editor)
            {
                UpdateRotation();
                OldRotation = Rotation;
            }

            SubscribeToTickEvent();

            if (loaded && !editor && EntitySystemWorld.Instance.SerializationMode ==
                SerializationModes.Map)
            {
                ItemCreator obj = (ItemCreator)Entities.Instance.Create(
                    EntityTypes.Instance.GetByName("ItemCreator"), Parent);
                obj.Position = Position;
                obj.ItemType = Type;
                obj.CreateRemainingTime = respawnTime;
                obj.Item = this;
                obj.PostCreate();
            }

            if (EntitySystemWorld.Instance.IsServer())
            {
                if (Type.NetworkType == EntityNetworkTypes.Synchronized)
                    Server_SendPositionToAllClients();
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            rotationAngle += TickDelta;
            UpdateRotation();
        }

        protected override void Client_OnTick()
        {
            base.Client_OnTick();

            rotationAngle += TickDelta;
            UpdateRotation();
        }

        protected override void OnSetTransform(ref Vec3 pos, ref Quat rot, ref Vec3 scl)
        {
            base.OnSetTransform(ref pos, ref rot, ref scl);

            if (IsPostCreated)
            {
                if (EntitySystemWorld.Instance.IsServer())
                {
                    if (Type.NetworkType == EntityNetworkTypes.Synchronized)
                        Server_SendPositionToAllClients();
                }
            }
        }

        private void UpdateRotation()
        {
            Rotation = new Angles(0, 0, -rotationAngle.InDegrees()).ToQuat();
        }

        protected virtual bool OnTake(Unit unit)
        {
            return false;
        }

        public bool Take(Unit unit)
        {
            bool ret = OnTake(unit);
            if (ret)
            {
                string soundTakeFullPath =
                    RelativePathUtils.ConvertToFullPath(Path.GetDirectoryName(Type.FilePath), Type.SoundTake);
                unit.SoundPlay3D(soundTakeFullPath, .5f, true);

                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendSoundPlayTakeToAllClients();

                Die();
            }
            return ret;
        }

        protected override void Server_OnClientConnectedBeforePostCreate(
            RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedBeforePostCreate(remoteEntityWorld);

            Server_SendPositionToNewClient(remoteEntityWorld);
        }

        private void Server_SendSoundPlayTakeToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Item),
                (ushort)NetworkMessages.SoundPlayTakeToClient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SoundPlayTakeToClient)]
        private void Client_ReceiveSoundPlayTake(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            SoundPlay3D(Type.SoundTake, .5f, true);
        }

        private void Server_SendPositionToAllClients()
        {
            const float epsilon = .005f;

            bool updated = !Position.Equals(ref server_sentPositionToClients, epsilon);

            if (updated)
            {
                SendDataWriter writer = BeginNetworkMessage(typeof(Item),
                    (ushort)NetworkMessages.PositionToClient);
                writer.Write(Position);
                EndNetworkMessage();

                server_sentPositionToClients = Position;
            }
        }

        private void Server_SendPositionToNewClient(RemoteEntityWorld remoteEntityWorld)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorld, typeof(Item),
                (ushort)NetworkMessages.PositionToClient);
            writer.Write(Position);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.PositionToClient)]
        private void Client_ReceiveUpdatePosition(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            Vec3 value = reader.ReadVec3();
            if (!reader.Complete())
                return;
            Position = value;
        }
    }
}