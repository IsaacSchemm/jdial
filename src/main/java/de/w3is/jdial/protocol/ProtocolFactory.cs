using System;
using System.Collections.Generic;
using System.Text;

namespace de.w3is.jdial.protocol
{
    public interface ProtocolFactory
    {

        MSearch createMSearch();

        DeviceDescriptorResource createDeviceDescriptorResource();

        ApplicationResource createApplicationResource(String clientFriendlyName, Uri applicationResourceUrl);
    }
}
