namespace ProjectCommon
{
    public static class GameInterface
    {
        //static Control window;

        public delegate void SendMessageToGameEventDelegate(string message, object data);

        public static event SendMessageToGameEventDelegate SendMessageToGameEvent;

        public static void SendMessageToGame(string message, object data)
        {
            if (SendMessageToGameEvent != null)
                SendMessageToGameEvent(message, data);
        }
    }
}