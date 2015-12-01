using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Network
{
	public class Client : NetworkCore
	{
		public Client()
		{
			NetPeerConfiguration config = new NetPeerConfiguration( "Client" );
			config.Port = 14242;

			instance = new NetClient( config );

			instance.Connect("192.168.1.1", 14242);
		}
	}
}

