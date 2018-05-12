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

using de.w3is.jdial.protocol.model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace de.w3is.jdial.protocol {
    class DeviceDescriptorResourceImpl : DeviceDescriptorResource {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private static readonly String APPLICATION_URL_HEADER = "Application-URL";
        
        public DeviceDescriptor getDescriptor(Uri deviceDescriptorLocation) {

            if (deviceDescriptorLocation == null) {

                throw new ArgumentException("Device descriptor can't be null");
            }

            if (!deviceDescriptorLocation.Scheme.Equals("http")) {

                LOGGER.Log(LogLevel.Warn, "Only http is supported for device descriptor resolution");
                return null;
            }

            HttpWebRequest connection = WebRequest.CreateHttp(deviceDescriptorLocation);

            try {
                using (HttpWebResponse response = (HttpWebResponse)connection.GetResponse()) {
                    String applicationUrl = connection.Headers[APPLICATION_URL_HEADER];

                    if (applicationUrl == null) {

                        LOGGER.Log(LogLevel.Warn, "Server didn't return applicationUrl");
                        return null;
                    }

                    DeviceDescriptor deviceDescriptor = new DeviceDescriptor();
                    deviceDescriptor.setApplicationResourceUrl(new Uri(applicationUrl));

                    readInfoFromBody(response, deviceDescriptor);

                    return deviceDescriptor;
                }
            } catch (WebException ex) {
                LOGGER.Log(LogLevel.Warn, "Could not get device descriptor: " + (ex.Response as HttpWebResponse)?.StatusCode);
                return null;
            }
        }

        private void readInfoFromBody(HttpWebResponse connection, DeviceDescriptor deviceDescriptor) {

            using (Stream inputStream = connection.GetResponseStream()) try {

                XmlDocument bodyDocument = new XmlDocument();
                bodyDocument.Load(inputStream);

                bodyDocument.DocumentElement.Normalize();

                deviceDescriptor.setFriendlyName(bodyDocument.getTextFromSub("friendlyName"));

            } catch (XmlException e) {

                LOGGER.Log(LogLevel.Warn, "Error while parsing device descriptor:", e);
            }
        }
    }
}
