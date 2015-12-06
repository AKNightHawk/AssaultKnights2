// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine.Networking;
using Engine.Utils;

namespace ProjectCommon
{
    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class CustomMessagesServerNetworkService : ServerNetworkService
    {
        private MessageType transferMessageType;

        ///////////////////////////////////////////

        public delegate void ReceiveMessageDelegate(CustomMessagesServerNetworkService sender,
            NetworkNode.ConnectedNode source, string message, string data);

        public event ReceiveMessageDelegate ReceiveMessage;

        ///////////////////////////////////////////

        public CustomMessagesServerNetworkService()
            : base("CustomMessages", 2)
        {
            //register message types
            transferMessageType = RegisterMessageType("transferMessage", 1,
                ReceiveMessage_TransferMessageToServer);
        }

        private bool ReceiveMessage_TransferMessageToServer(NetworkNode.ConnectedNode sender,
            MessageType messageType, ReceiveDataReader reader, ref string error)
        {
            string message = reader.ReadString();
            string data = reader.ReadString();
            if (!reader.Complete())
                return false;

            if (ReceiveMessage != null)
                ReceiveMessage(this, sender, message, data);

            return true;
        }

        public void SendToClient(NetworkNode.ConnectedNode connectedNode, string message,
            string data)
        {
            SendDataWriter writer = BeginMessage(connectedNode, transferMessageType);
            writer.Write(message);
            writer.Write(data);
            EndMessage();
        }

        public void SendToAllClients(string message, string data)
        {
            foreach (NetworkNode.ConnectedNode connectedNode in Owner.ConnectedNodes)
                SendToClient(connectedNode, message, data);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class CustomMessagesClientNetworkService : ClientNetworkService
    {
        private MessageType transferMessageType;
        private MessageType spawnInfoToClient;

        ///////////////////////////////////////////

        public delegate void ReceiveMessageDelegate(CustomMessagesClientNetworkService sender,
            string message, string data);

        public event ReceiveMessageDelegate ReceiveMessage;

        ///////////////////////////////////////////

        public CustomMessagesClientNetworkService()
            : base("CustomMessages", 2)
        {
            //register message types
            transferMessageType = RegisterMessageType("transferMessage", 1,
                ReceiveMessage_TransferMessageToClient);

            spawnInfoToClient = RegisterMessageType("SpawnInfoToClient", 4,
                ReceiveMessage_SpawnInfoToClient);
			RegisterMessageType( "SpawnInfoToServer", 5,
				ReceiveMessage_SpawnInfoToServer );
		}

        public void SendToServer(string message, string data)
        {
            SendDataWriter writer = BeginMessage(transferMessageType);
            writer.Write(message);
            writer.Write(data);
            EndMessage();
        }

        private bool ReceiveMessage_TransferMessageToClient(NetworkNode.ConnectedNode sender,
            MessageType messageType, ReceiveDataReader reader, ref string additionalErrorMessage)
        {
            string message = reader.ReadString();
            string data = reader.ReadString();
            if (!reader.Complete())
                return false;

            if (ReceiveMessage != null)
                ReceiveMessage(this, message, data);

            return true;
        }

        private bool ReceiveMessage_SpawnInfoToClient(NetworkNode.ConnectedNode sender,
            MessageType messageType, ReceiveDataReader reader, ref string additionalErrorMessage)
        {
            string message = reader.ReadString();
            string data = reader.ReadString();
            if (!reader.Complete())
                return false;

            if (ReceiveMessage != null)
                ReceiveMessage(this, message, data);

            return true;
        }

	    private bool ReceiveMessage_SpawnInfoToServer(NetworkNode.ConnectedNode sender, MessageType messageType,
		    ReceiveDataReader reader, ref string additionalErrorMessage)
	    {
		    return ReceiveMessage_SpawnInfoToClient(sender, messageType, reader, ref additionalErrorMessage);
	    }
	}
}