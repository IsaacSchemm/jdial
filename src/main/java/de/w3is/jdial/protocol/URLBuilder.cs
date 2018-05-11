using System;
using System.Collections.Generic;
using System.Text;

namespace de.w3is.jdial.protocol
{
    public class URLBuilder
    {

        private static readonly String PATH_SEPARATOR = "/";
        private static readonly String QUERY_SEPARATOR = "&";
        private static readonly String QUERY_KEY_VALUE_SEPARATOR = "=";
        private static readonly String PATH_QUERY_SEPARATOR = "?";
        private static readonly String PATH_QUERY_SPLITTER = "\\?";

        private String _protocol = "http";
        private String _host = "localhost";
        private int _port = 80;

        private StringBuilder _paths = new StringBuilder();
        private StringBuilder _query = new StringBuilder();

        private URLBuilder() { }

        public static URLBuilder of(Uri url)
        {

            URLBuilder urlBuilder = new URLBuilder()
                    .protocol(url.Scheme)
                    .host(url.Host)
                    .port(url.Port)
                    .path(url.AbsolutePath);

            if (url.Query != null)
            {
                String[] queryParts = url.Query.Split(new[] { PATH_QUERY_SPLITTER }, System.StringSplitOptions.None);

                foreach (String part in queryParts)
                {

                    urlBuilder.query(part);
                }
            }

            return urlBuilder;
        }

        URLBuilder protocol(String protocol)
        {
            this._protocol = protocol;
            return this;
        }

        private URLBuilder host(String host)
        {
            this._host = host;
            return this;
        }

        private URLBuilder port(int port)
        {
            this._port = port;
            return this;
        }

        public URLBuilder path(String path)
        {

            if (_paths.Length != 0)
            {
                this._paths.Append(PATH_SEPARATOR);
            }

            this._paths.Append(path);
            return this;
        }

        public void query(String key, String value)
        {

            appendQueryOrPathSeparator();

            this._query.Append(key).Append(QUERY_KEY_VALUE_SEPARATOR).Append(value);

        }

        public void query(String queryPart)
        {

            appendQueryOrPathSeparator();

            this._query.Append(QUERY_SEPARATOR).Append(queryPart);
        }

        public Uri build()
        {

            String joinedPath = _paths.ToString() + _query.ToString();

            return new UriBuilder(_protocol, _host, _port, joinedPath).Uri;
        }

        private void appendQueryOrPathSeparator()
        {

            if (this._query.Length == 0)
            {

                this._query.Append(PATH_QUERY_SEPARATOR);
            }
            else
            {

                this._query.Append(QUERY_SEPARATOR);
            }
        }
    }
}
