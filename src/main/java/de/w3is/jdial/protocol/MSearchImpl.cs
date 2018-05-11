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

using de.w3is.jdial.model;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace de.w3is.jdial.protocol {
    class MSearchImpl : MSearch {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private const String MULTICAST_IP = "239.255.255.250";
        private const int MULTICAST_PORT = 1900;

        private const String SEARCH_TARGET_HEADER_VALUE = "urn:dial-multiscreen-org:service:dial:1";
        private const String SEARCH_TARGET_HEADER = "ST";
        private const String LOCATION_HEADER = "LOCATION";
        private const String USN_HEADER = "USN";
        private const String WAKEUP_HEADER = "WAKEUP";
        private const String SERVER_HEADER = "SERVER";
        private const String WOL_MAC = "MAC";
        private const String WOL_TIMEOUT = "TIMEOUT";

        private readonly String msearchRequest;
        private readonly int socketTimeoutMs;

        internal MSearchImpl(int responseDelay, int socketTimeoutMs) {

            this.msearchRequest = "M-SEARCH * HTTP/1.1\r\n" +
                    "HOST: " + MULTICAST_IP + ":" + MULTICAST_PORT + "\r\n" +
                    "MAN: \"ssdp:discover\"\r\n" +
                    "MX: " + responseDelay + "\r\n" +
                    SEARCH_TARGET_HEADER + ": " + SEARCH_TARGET_HEADER_VALUE + "\r\n" +
                    "USER-AGENT: OS/version product/version\r\n";

            this.socketTimeoutMs = socketTimeoutMs;
        }
        
        public IEnumerable<DialServer> sendAndReceive() {
            
            byte[] requestBuffer = Encoding.UTF8.GetBytes(msearchRequest);

            UdpClient udpClient = new UdpClient();
            udpClient.Client.SendTimeout = socketTimeoutMs;
            udpClient.Client.ReceiveTimeout = socketTimeoutMs;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            LOGGER.Log(LogLevel.Debug, "Send M-SEARCH request");
            udpClient.Send(requestBuffer, 0, MULTICAST_IP, MULTICAST_PORT);

            Dictionary<String, DialServer> discoveredDevicesByNames = new Dictionary<String, DialServer>();

            try {
                while (true) {
                    
                    IPEndPoint remoteEP = null;
                    byte[] responseBuffer = udpClient.Receive(ref remoteEP);

                    DialServer dialServer = toServer(responseBuffer);

                    if (dialServer != null) {
                        if (!discoveredDevicesByNames.ContainsKey(dialServer.getUniqueServiceName())) {
                            LOGGER.Log(LogLevel.Debug, "Found device: " + dialServer.ToString());
                            discoveredDevicesByNames.Add(dialServer.getUniqueServiceName(), dialServer);
                        }
                    }

                }
            } catch (SocketException e) {

                LOGGER.Log(LogLevel.Trace, "Socket timed out: ", e);
            }

            return discoveredDevicesByNames.Values;
        }

        private DialServer toServer(byte[] packet) {

            String data = Encoding.UTF8.GetString(packet);

            if (!data.Contains(SEARCH_TARGET_HEADER_VALUE)) {

                LOGGER.Log(LogLevel.Trace, "Ignore response for unrelated search target: " + data);
                return null;
            }

            String[] dataRows = data.Split('\n');
            DialServer dialServer = new DialServer();

            foreach (String row in dataRows) {

                String[] headerParts = row.Split(new[] { ": " }, StringSplitOptions.None);

                if (headerParts.Length == 2) {

                    String headerName = headerParts[0].ToUpperInvariant();

                    switch (headerName) {
                        case LOCATION_HEADER:
                            parseDeviceDescriptorUrl(dialServer, headerParts[1]);
                            break;
                        case USN_HEADER:
                            dialServer.setUniqueServiceName(headerParts[1]);
                            break;
                        case WAKEUP_HEADER:
                            parseWolHeader(dialServer, headerParts[1]);
                            break;
                        case SERVER_HEADER:
                            dialServer.setServerDescription(headerParts[1]);
                            break;
                        default:
                            LOGGER.Log(LogLevel.Debug, "Ignoring unknown header: " + headerName);
                            break;
                    }
                }
            }

            if (dialServer.getDeviceDescriptorUrl() != null
                    && dialServer.getUniqueServiceName() != null && dialServer.getUniqueServiceName().Length > 0) {

                return dialServer;
            } else {

                LOGGER.Log(LogLevel.Trace, "Ignore package with incomplete data: " + data);
                return null;
            }
        }

        private void parseDeviceDescriptorUrl(DialServer dialServer, String headerPart) {
            try {
                dialServer.setDeviceDescriptorUrl(new Uri(headerPart));
            } catch (UriFormatException e) {
                LOGGER.Log(LogLevel.Warn, "Server provided malformed device descriptor url: ", e);
            }
        }

        private void parseWolHeader(DialServer dialServer, String headerValue) {

            String[] wolParts = headerValue.Split(';');

            foreach (String wolPart in wolParts) {

                String[] wolHeader = wolPart.Split('=');

                if (wolHeader.Length == 2) {

                    switch (wolHeader[0].ToUpperInvariant()) {
                        case WOL_MAC:
                            dialServer.setWakeOnLanMAC(wolHeader[1]);
                            dialServer.setWakeOnLanSupport(true);
                            break;
                        case WOL_TIMEOUT:
                            dialServer.setWakeOnLanTimeout(int.Parse(wolHeader[1]));
                            break;
                        default:
                            LOGGER.Log(LogLevel.Debug, "Ignore unknown wol header: " + wolHeader[0]);
                            break;
                    }
                }
            }
        }
    }
}
