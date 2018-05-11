using System;
using System.Collections.Generic;
using System.Text;

namespace de.w3is.jdial.protocol
{
    class ProtocolFactoryImpl : ProtocolFactory
    {
        private bool legacyCompatibility;
        private int httpClientReadTimeoutMs = 1500;
        private int httpClientConnectionTimeoutMs = 1500;
        private int socketTimeoutMs = 1500;
        private int mSearchResponseDelay = 0;

        public ProtocolFactoryImpl(bool legacyCompatibility) {

            this.legacyCompatibility = legacyCompatibility;
        }
        
        public MSearch createMSearch() {

            return new MSearchImpl(mSearchResponseDelay, socketTimeoutMs);
        }
        
        public DeviceDescriptorResource createDeviceDescriptorResource() {

            return new DeviceDescriptorResourceImpl();
        }
        
        public ApplicationResource createApplicationResource(String clientFriendlyName, Uri applicationResourceUrl) {

            ApplicationResourceImpl applicationResource = new ApplicationResourceImpl(clientFriendlyName, applicationResourceUrl);
            applicationResource.setSendQueryParameter(!legacyCompatibility);
            applicationResource.setConnectionTimeout(httpClientConnectionTimeoutMs);
            applicationResource.setReadTimeout(httpClientReadTimeoutMs);

            return applicationResource;
        }
    }
}
