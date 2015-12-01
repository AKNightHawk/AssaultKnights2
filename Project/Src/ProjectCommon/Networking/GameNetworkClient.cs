// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine;
using Engine.Networking;

namespace ProjectCommon
{
    public class GameNetworkClient : NetworkClient
    {
        private static GameNetworkClient instance;

        //services
        private UserManagementClientNetworkService userManagementService;

        private CustomMessagesClientNetworkService customMessagesService;
        private ChatClientNetworkService chatService;
        private EntitySystemClientNetworkService entitySystemService;

        ///////////////////////////////////////////

        public GameNetworkClient(bool entitySystemServiceEnabled)
        {
            if (instance != null)
                Log.Fatal("GameNetworkClient.GameNetworkClient: instance != null.");
            instance = this;

            //register network services

            //register user management service
            userManagementService = new UserManagementClientNetworkService();
            RegisterService(userManagementService);

            //register custom messages service
            customMessagesService = new CustomMessagesClientNetworkService();
            RegisterService(customMessagesService);

            //register chat service
            chatService = new ChatClientNetworkService(userManagementService);
            RegisterService(chatService);

            //register entity system service
            if (entitySystemServiceEnabled)
            {
                entitySystemService = new EntitySystemClientNetworkService(userManagementService);
                RegisterService(entitySystemService);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            instance = null;
        }

        public static GameNetworkClient Instance
        {
            get { return instance; }
        }

        public UserManagementClientNetworkService UserManagementService
        {
            get { return userManagementService; }
        }

        public CustomMessagesClientNetworkService CustomMessagesService
        {
            get { return customMessagesService; }
        }

        public ChatClientNetworkService ChatService
        {
            get { return chatService; }
        }

        public EntitySystemClientNetworkService EntitySystemService
        {
            get { return entitySystemService; }
        }

        protected override void OnConnectionStatusChanged(NetworkConnectionStatuses status)
        {
            base.OnConnectionStatusChanged(status);
        }

        protected override void OnReceiveProtocolError(string message)
        {
            base.OnReceiveProtocolError(message);

            Log.Error("GameNetworkClient: {0} from server.", message);
        }
    }
}