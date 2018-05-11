using de.w3is.jdial.model;
using de.w3is.jdial.protocol;
using de.w3is.jdial.protocol.model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace de.w3is.jdial
{
    public class Discovery
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private readonly ProtocolFactory protocolFactory;

        public Discovery(ProtocolFactory protocolFactory) {

            this.protocolFactory = protocolFactory;
        }

        public Discovery() : this(new ProtocolFactoryImpl(false)) { }

        /**
            * The discover method returns all servers in the local network that support
            * discovery via udp msearch and support the upnp device descriptor.
            *
            * IOExceptions are not thrown to the user of this method. Instead an empty list
            * will be returned.
            *
            * @return Returns a list of discovered servers.
            */
        public IEnumerable<DialServer> discover() {

            IEnumerable<DialServer> dialServers;

            try {

                dialServers = protocolFactory.createMSearch().sendAndReceive();

            } catch (IOException e) {

                LOGGER.Log(LogLevel.Warn, "IOException while discovering devices:", e);
                return new List<DialServer>(0);
            }

            List<DialServer> devicesToRemove = new List<DialServer>();

            foreach (DialServer device in dialServers) {

                try {

                    DeviceDescriptor descriptor
                            = protocolFactory.createDeviceDescriptorResource().getDescriptor(device.getDeviceDescriptorUrl());

                    if (descriptor != null) {

                        device.setFriendlyName(descriptor.getFriendlyName());
                        device.setApplicationResourceUrl(descriptor.getApplicationResourceUrl());
                    } else {

                        devicesToRemove.Add(device);
                    }

                } catch (IOException e) {

                    devicesToRemove.Add(device);
                    LOGGER.Log(LogLevel.Warn, "IOException while reading device descriptor " + device.getDeviceDescriptorUrl(), e);
                }
            }
            
            return dialServers.Except(devicesToRemove);
        }
    }
}
