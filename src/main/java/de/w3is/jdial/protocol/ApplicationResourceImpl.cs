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
using de.w3is.jdial.protocol.model;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Xml;

namespace de.w3is.jdial.protocol {
    public class ApplicationResourceImpl : ApplicationResource {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static readonly String APPLICATION_DIAL_VERSION_QUERY = "clientDialVersion=2.1";
        private static readonly String CLIENT_FRIENDLY_NAME_QUERY = "friendlyName";
        private static readonly String CONTENT_LENGTH_HEADER = "Content-Length";
        private static readonly String CONTENT_TYPE_HEADER = "Content-Type";

        private static readonly DialContent NO_CONTENT = new DialNoContent();
        
        private class DialNoContent : DialContent {
            public String getContentType() {
                return null;
            }
            
            public byte[] getData() {
                return null;
            }
        };

        private readonly String clientFriendlyName;

        private readonly Uri rootUrl;
        private bool sendQueryParameter;
        private int? connectionTimeout;
        private int? readTimeout;

        internal void setSendQueryParameter(bool v) => sendQueryParameter = v;

        internal void setConnectionTimeout(int httpClientConnectionTimeoutMs) => connectionTimeout = httpClientConnectionTimeoutMs;

        internal void setReadTimeout(int httpClientReadTimeoutMs) => readTimeout = httpClientReadTimeoutMs;

        internal ApplicationResourceImpl(String clientFriendlyName, Uri rootUrl) {

            this.clientFriendlyName = clientFriendlyName;
            this.rootUrl = rootUrl;
            this.sendQueryParameter = true;
        }
        
        public Application getApplication(String applicationName) {

            URLBuilder applicationUrl = URLBuilder.of(rootUrl).path(applicationName);

            if (sendQueryParameter) {

                applicationUrl.query(APPLICATION_DIAL_VERSION_QUERY);
            }

            HttpWebRequest httpUrlConnection = WebRequest.CreateHttp(applicationUrl.build());
            addTimeoutParameter(httpUrlConnection);

            try {
                using (HttpWebResponse response = (HttpWebResponse)httpUrlConnection.GetResponse())
                using (Stream inputStream = response.GetResponseStream()) try {

                    XmlDocument serviceDocument = getServiceDocument(inputStream);

                    Application application = new Application();
                    application.setName(serviceDocument.getTextFromSub("name"));
                    application.setInstanceUrl(getInstanceUrl(serviceDocument, application.getName()));
                    application.setAllowStop(getIsAllowStopFromOption(serviceDocument));
                    application.setAdditionalData(extractAdditionalData(serviceDocument));

                    extractState(serviceDocument, application);

                    return application;

                } catch (XmlException e) {

                    LOGGER.Log(LogLevel.Warn, "Can't parse body xml", e);
                    return null;
                }
            } catch (WebException ex) {
                LOGGER.Log(LogLevel.Trace, "Application not found: " + (ex.Response as HttpWebResponse)?.StatusCode);
                return null;
            }
        }
            
        public Uri startApplication(String applicationName) {

            return startApplication(applicationName, NO_CONTENT);
        }
            
        public Uri startApplication(String applicationName, DialContent dialContent) {

            URLBuilder applicationUrl = URLBuilder.of(rootUrl).path(applicationName);

            if (clientFriendlyName != null && sendQueryParameter) {
                applicationUrl.query(CLIENT_FRIENDLY_NAME_QUERY, clientFriendlyName);
            }

            HttpWebRequest httpURLConnection = WebRequest.CreateHttp(applicationUrl.build());
            httpURLConnection.Method = ("POST");
            httpURLConnection.AllowAutoRedirect = false;

            addTimeoutParameter(httpURLConnection);

            if (dialContent.getData() == null) {

                httpURLConnection.ContentLength = 0;

                // HttpURLConnection will not send headers if the outputStream not getting opened.
                using (Stream outputStream = httpURLConnection.GetRequestStream()) { }
            } else {

                httpURLConnection.ContentLength = dialContent.getData().Length;
                httpURLConnection.ContentType = dialContent.getContentType();
                
                using (Stream outputStream = httpURLConnection.GetRequestStream()) {
                    outputStream.Write(dialContent.getData(), 0, dialContent.getData().Length);
                }
            }

            try {
                using (HttpWebResponse response = (HttpWebResponse)httpURLConnection.GetResponse()) {
                    String instanceLocation = httpURLConnection.Headers["Location"];

                    if (instanceLocation != null) {

                        return new Uri(instanceLocation);
                    } else {

                        return null;
                    }
                }
            } catch (WebException ex) {

                throw new ApplicationResourceException("Could not start application. Status: " + (ex.Response as HttpWebResponse)?.StatusCode);
            }
        }
        
        public void stopApplication(Uri instanceUrl) {

            HttpWebRequest httpURLConnection = WebRequest.CreateHttp(instanceUrl);
            addTimeoutParameter(httpURLConnection);
            httpURLConnection.Method = ("DELETE");

            try {
                using (HttpWebResponse response = (HttpWebResponse)httpURLConnection.GetResponse()) { }
            } catch (WebException ex) {
                throw new ApplicationResourceException("Could not stop the application. Status: " +
                        (ex.Response as HttpWebResponse)?.StatusCode);
            }
        }

        public void hideApplication(Uri instanceURL) {

            Uri hidingUrl = URLBuilder.of(instanceURL).path("hide").build();
            HttpWebRequest httpURLConnection = WebRequest.CreateHttp(hidingUrl);
            addTimeoutParameter(httpURLConnection);
            httpURLConnection.Method = ("POST");

            try {
                using (HttpWebResponse response = (HttpWebResponse)httpURLConnection.GetResponse()) { }
            } catch (WebException ex) {
                throw new ApplicationResourceException("Could not hide the application. Status: " +
                        (ex.Response as HttpWebResponse)?.StatusCode);
            }
        }

        private XmlDocument getServiceDocument(Stream inputStream) {
            
            XmlDocument document = new XmlDocument();
            document.Load(inputStream);

            document.DocumentElement.Normalize();

            return document;
        }

        private XmlNode extractAdditionalData(XmlDocument document) {

            XmlNodeList nodes = document.GetElementsByTagName("additionalData");

            if (nodes.Count >= 1) {

                return nodes.Item(0);
            }

            return null;
        }

        private bool getIsAllowStopFromOption(XmlDocument document) {

            XmlNodeList nodes = document.GetElementsByTagName("options");

            if (nodes.Count < 1) {
                return false;
            }

            XmlNamedNodeMap optionAttributes = nodes.Item(0).Attributes;
            XmlNode allowStop = optionAttributes.GetNamedItem("allowStop");

            return allowStop != null && bool.Parse(allowStop.InnerText);
        }

        private Uri getInstanceUrl(XmlDocument document, String applicationName) {

            XmlNodeList nodes = document.GetElementsByTagName("link");

            if (nodes.Count < 1) {
                throw new ApplicationResourceException("Document has no link element");
            }

            XmlNamedNodeMap linkAttributes = nodes.Item(0).Attributes;
            XmlNode href = linkAttributes.GetNamedItem("href");
            XmlNode rel = linkAttributes.GetNamedItem("rel");

            if (rel == null || href == null || !rel.InnerText.Equals("run")) {

                throw new ApplicationResourceException("Unknown link type on service");
            }

            return URLBuilder.of(rootUrl).path(applicationName).path(href.InnerText).build();
        }

        private void extractState(XmlDocument document, Application application) {

            String stateText = document.getTextFromSub("state");

            State state = mapToState(stateText);
            application.setState(state);

            if (state == State.INSTALLABLE) {
                application.setInstallUrl(getInstallUrl(stateText));
            }
        }

        private Uri getInstallUrl(String state) {

            String[] stateParts = state.Split('=');

            if (stateParts.Length < 2) {
                return null;
            }

            return new Uri(stateParts[1]);
        }

        private State mapToState(String value) {

            if (value == null) {
                throw new ApplicationResourceException("App exists but has no state");
            }

            String lowercaseStatus = value.ToLowerInvariant();
            if (lowercaseStatus.StartsWith("installable")) {

                return State.INSTALLABLE;
            }

            switch (lowercaseStatus) {

                case "running":
                    return State.RUNNING;
                case "stopped":
                    return State.STOPPED;
                case "hidden":
                    return State.HIDDEN;
                default:
                    throw new ApplicationResourceException("Unknown state: " + value);
            }
        }

        private void addTimeoutParameter(HttpWebRequest httpUrlConnection) {

            if (connectionTimeout != null) {
                httpUrlConnection.Timeout = (connectionTimeout).Value;
            }

            if (readTimeout != null) {
                httpUrlConnection.ReadWriteTimeout = (readTimeout).Value;
            }
        }
    }
}
