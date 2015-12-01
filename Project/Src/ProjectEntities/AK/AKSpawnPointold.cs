// Copyright (C) 2006-2010 NeoAxis Group Ltd.
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;
using Engine.MathEx;
using Engine.Utils;

namespace GameEntities
{
	/// <summary>
	/// Defines the <see cref="AKSpawnPoint"/> entity type.
	/// </summary>
	public class AKSpawnPointType : AKMapObjectType
	{
	}

	public class SpawnPoint : AKMapObject
	{
        [FieldSerialize]
        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public static SpawnPoint SelectedSinglePlayerPoint = null;

        //public enum NetworkMessages
        //{
        //    TextToClient
        //}

		static List<SpawnPoint> instances = new List<SpawnPoint>();

		SpawnPointType _type = null; public new SpawnPointType Type { get { return _type; } }

		protected override void OnPostCreate( bool loaded )
		{
			base.OnPostCreate( loaded );
			instances.Add( this );
		}

		protected override void OnDestroy()
		{
			instances.Remove( this );
			base.OnDestroy();
		}

		public static SpawnPoint AKGetFreeRandomSpawnPoint(FactionType faction)
		{
            foreach (SpawnPoint sp in instances)
            {
                //if (sp.faction != faction)
                //    continue;

                bool busy = false;
                {
                    Bounds volume = new Bounds(sp.Position);
                    volume.Expand(new Vec3(1, 1, 2));

                    Body[] result = PhysicsWorld.Instance.VolumeCast(volume,
                        (int)ContactGroup.CastOnlyContact);

                    foreach (Body body in result)
                    {
                        if (body.Static)
                            continue;

                        foreach (Shape shape in body.Shapes)
                        {
                            if (PhysicsWorld.Instance.IsContactGroupsContactable(shape.ContactGroup,
                                (int)ContactGroup.Dynamic))
                            {
                                busy = true;
                                break;
                            }
                        }
                        if (busy)
                            break;
                    }
                }

                if (!busy)
                    return sp;
            }

            return null;
		}

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            IList<RemoteEntityWorld> worlds = new RemoteEntityWorld[] { remoteEntityWorld };

            if (!string.IsNullOrEmpty(text))
                Server_SendTextToClients(worlds);
        }

        private void Server_SendTextToClients(IList<RemoteEntityWorld> worlds)
        {
            SendDataWriter writer = BeginNetworkMessage(worlds, typeof(SpawnPoint), (ushort)NetworkMessages.TextToClient);

            writer.Write(text);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TextToClient)]
        void Client_ReceiveFaction(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string s = reader.ReadString();
            if (!reader.Complete())
                return;

            text = s;
        }
	}
}
