using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Network
{
	public class Server : NetworkCore
	{
		public Server()
		{
			NetPeerConfiguration config = new NetPeerConfiguration( "Server" );
			config.Port = 14242;

			instance = new NetServer( config );

			instance.Start();
		}
	}
}
