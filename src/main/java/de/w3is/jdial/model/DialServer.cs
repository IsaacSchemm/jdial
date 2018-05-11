/*
 * Copyright (C) 2018 Simon Weis
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace de.w3is.jdial.model {
    public class DialServer {

        // The friendly name is only set if the device exposes it via upnp device descriptor
        private String friendlyName;

        // The url to the application rest resource
        private Uri applicationResourceUrl;

        // A unique identifier of the device
        private String uniqueServiceName;

        // The url to the upnp device descriptor
        private Uri deviceDescriptorUrl;

        // Set if the server supports wol
        private bool wakeOnLanSupport;

        // The MAC address to wake up the device
        private String wakeOnLanMAC;

        // The wake on lan timeout.
        private int wakeOnLanTimeout;

        // A technical description string of the server
        private String serverDescription;

        internal void setFriendlyName(string p) => friendlyName = p;

        internal Uri getApplicationResourceUrl() => applicationResourceUrl;

        internal void setApplicationResourceUrl(Uri p) => applicationResourceUrl = p;

        internal string getUniqueServiceName() => uniqueServiceName;

        internal void setUniqueServiceName(string v) => uniqueServiceName = v;

        internal Uri getDeviceDescriptorUrl() => deviceDescriptorUrl;

        internal void setDeviceDescriptorUrl(Uri p) => deviceDescriptorUrl = p;

        internal void setWakeOnLanMAC(string v) => wakeOnLanMAC = v;

        internal void setWakeOnLanSupport(bool v) => wakeOnLanSupport = v;

        internal void setWakeOnLanTimeout(int v) => wakeOnLanTimeout = v;

        internal void setServerDescription(string v) => serverDescription = v;
    }
}
