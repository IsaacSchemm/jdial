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
using System.Xml;

namespace de.w3is.jdial.model {
    public class Application {

        public static readonly String NETFLIX = "Netflix";
        public static readonly String YOUTUBE = "YouTube";
        public static readonly String AMAZON_INSTANT_VIDEO = "AmazonInstantVideo";

        // The name of the application
        private String name;

        // The state of the application
        private State state;

        // True if the client is allowed to stop the app
        private bool allowStop;

        // The installUrl can be used to issue an installation of the app.
        private Uri installUrl;

        /*
         * The url of a running instance.
         * The installUrl is null when no instance is running.
         */
        private Uri instanceUrl;

        // Additional data defined by the app author.
        private XmlNode additionalData;

        internal string getName() => this.name;

        internal void setName(string v) => this.name = v;

        internal State getState() => this.state;

        internal void setState(State state) => this.state = state;

        internal bool isAllowStop() => allowStop;

        internal void setAllowStop(bool v) => this.allowStop = v;

        internal Uri getInstallUrl() => installUrl;

        internal void setInstallUrl(Uri uri) => this.installUrl = uri;

        internal Uri getInstanceUrl() => instanceUrl;

        internal void setInstanceUrl(Uri uri) => this.installUrl = uri;

        internal XmlNode getAdditionalData() => additionalData;

        internal void setAdditionalData(XmlNode xmlNode) => this.additionalData = xmlNode;
    }
}
