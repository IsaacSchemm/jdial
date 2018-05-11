using de.w3is.jdial.model;
using de.w3is.jdial.protocol;
using System;

namespace de.w3is.jdial
{
    public class DialClient
    {
        private readonly ProtocolFactory protocolFactory;

        private String clientFriendlyName = "jdial";

        public DialClient(ProtocolFactory protocolFactory)
        {

            this.protocolFactory = protocolFactory;
        }

        public DialClient() : this(new ProtocolFactoryImpl(false)) { }

        /**
         * Creates a connection to a dial server.
         *
         * @param dialServer The server to connect to.
         * @return A new connection.
         */
        public DialClientConnection connectTo(DialServer dialServer)
        {

            return new DialClientConnection(protocolFactory.createApplicationResource(clientFriendlyName,
                    dialServer.getApplicationResourceUrl()));
        }
    }
}
