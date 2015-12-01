using System;
using Lidgren.Network;

namespace Network
{
	public class NetworkCore
	{
		public enum MessagesType
		{
			AddServer,
			AskServers,
		}

		protected NetPeer instance;
		private Func<NetIncomingMessage, bool>[] methods;

		public NetworkCore()
		{
			methods = new Func<NetIncomingMessage, bool>[ 10 ];
			methods[ (uint)MessagesType.AddServer ] = AddServer;
			methods[ (uint)MessagesType.AskServers ] = AskServers;
		}

		protected virtual bool AddServer( NetIncomingMessage netIncomingMessage )
		{
			throw new NotImplementedException();
		}
		protected virtual bool AskServers( NetIncomingMessage netIncomingMessage )
		{
			throw new NotImplementedException();
		}
		protected void Update()
		{
			NetIncomingMessage msg;
			while( ( msg = instance.ReadMessage() ) != null )
			{
				switch( msg.MessageType )
				{
				case NetIncomingMessageType.VerboseDebugMessage:
				case NetIncomingMessageType.DebugMessage:
				case NetIncomingMessageType.WarningMessage:
				case NetIncomingMessageType.ErrorMessage:
					Console.WriteLine( msg.ReadString() );
					break;
				case NetIncomingMessageType.Data:
					var index = msg.ReadUInt32();
					methods[ index ].Invoke( msg );
					break;
				default:
					Console.WriteLine( "Unhandled type: " + msg.MessageType );
					break;
				}
				instance.Recycle( msg );
			}
		}

		protected void Send( NetOutgoingMessage sendMsg, NetConnection recipient )
		{
			instance.SendMessage( sendMsg, recipient, NetDeliveryMethod.ReliableOrdered );
		}

		protected NetOutgoingMessage CreateMessage( MessagesType type )
		{
			var msg = instance.CreateMessage();
			msg.Write( (uint)type );
			return msg;
		}
	}
}