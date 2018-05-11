using de.w3is.jdial.model;
using de.w3is.jdial.protocol;
using de.w3is.jdial.protocol.model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace de.w3is.jdial
{
    public class DialClientConnection {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private ApplicationResource applicationResource;

        internal DialClientConnection(ApplicationResource applicationResource) {
            this.applicationResource = applicationResource;
        }

        /**
         * Tests if the server supports the application.
         *
         * @param applicationName The name of the application.
         * @return True if the server supports the application.
         */
        public bool supportsApplication(String applicationName) {

            return getApplication(applicationName) != null;
        }

        /**
         * Returns an Application instance if the app is supported.
         *
         * @param applicationName The name of the application
         * @return An instance of the Application
         */
        public Application getApplication(String applicationName) {

            try {

                return applicationResource.getApplication(applicationName);
            } catch (IOException e) {

                LOGGER.Log(LogLevel.Warn, "IOException while getting application", e);
                return null;
            }
        }

        /**
         * Start an application
         *
         * @param application An application instance
         * @return An url to the started instance if the server provides one
         * @throws DialClientException In case of an network or protocol error
         */
        public Uri startApplication(Application application) {

            return startApplication(application.getName());
        }

        /**
         * Starts an application and provide additional data to send to the server
         * @param application The application to start
         * @param dialContent The additional data to send
         * @return An url to the started instance if the server provides one
         * @throws DialClientException In case of an network or protocol error
         */
        public Uri startApplication(Application application, DialContent dialContent) {

            return startApplication(application.getName(), dialContent);
        }

        /**
         * Start an application by name
         *
         * @param applicationName The name of the application
         * @return An url to the started instance if the server provides one
         * @throws DialClientException In case of an network or protocol error
         */
        public Uri startApplication(String applicationName) {

            try {
                return applicationResource.startApplication(applicationName);

            } catch (IOException e) {

                LOGGER.Log(LogLevel.Warn, "Exception while starting application", e);
                throw new DialClientException(e);
            } catch (ApplicationResourceException e) {

                LOGGER.Log(LogLevel.Warn, "Exception while starting application", e);
                throw new DialClientException(e);
            }
        }

        /**
         * Start an application by name and provide additional data to send to the server
         *
         * @param applicationName The name of the application
         * @param dialContent The additional data to send
         * @return An url to the started instance if the server provides one
         * @throws DialClientException In case of an network or protocol error
         */
        public Uri startApplication(String applicationName, DialContent dialContent) {

            try {

                return applicationResource.startApplication(applicationName, dialContent);

            } catch (IOException e) {

                LOGGER.Log(LogLevel.Warn, "Exception while starting application", e);
                throw new DialClientException(e);
            } catch (ApplicationResourceException e) {

                LOGGER.Log(LogLevel.Warn, "Exception while starting application", e);
                throw new DialClientException(e);
            }
        }

        /**
         * Stop an application
         *
         * @param instanceUrl An url to the app instance
         * @throws DialClientException In case of an network or protocol error
         */
        public void stopApplication(Uri instanceUrl) {

            if (instanceUrl == null) {
                return;
            }

            try {

                applicationResource.stopApplication(instanceUrl);

            } catch (IOException e) {

                LOGGER.Log(LogLevel.Warn, "Exception while stopping the application", e);
                throw new DialClientException(e);
            } catch (ApplicationResourceException e) {

                LOGGER.Log(LogLevel.Warn, "Exception while stopping the application", e);
                throw new DialClientException(e);
            }
        }

        /**
         * Stop an application that is not in the stopped state and supports stopping
         *
         * @param application The application to stop
         * @throws DialClientException In case of an network or protocol error or when the application
         * does not support stopping
         */
        public void stopApplication(Application application) {

            if (application.isAllowStop()) {
                throw new DialClientException("The application doesn't support stopping");
            }

            if (application.getState() == State.STOPPED) {

                return;
            }

            stopApplication(application.getInstanceUrl());
        }

        /**
         * Hide an application
         *
         * @param application An application instance
         * @throws DialClientException In case of an network or protocol error
         */
        public void hideApplication(Application application) {

            if (application.getState() == State.STOPPED || application.getState() == State.HIDDEN) {

                return;
            }

            hideApplication(application.getInstanceUrl());
        }

        /**
         * Hide an application by url
         * @param instanceUrl The url of the app instance
         * @throws DialClientException In case of an network or protocol error
         */
        private void hideApplication(Uri instanceUrl) {

            if (instanceUrl == null) {
                return;
            }

            try {

                applicationResource.hideApplication(instanceUrl);

            } catch (IOException  e) {

                LOGGER.Log(LogLevel.Warn, "Exception while hiding the application", e);
                throw new DialClientException(e);
            } catch (ApplicationResourceException e) {

                LOGGER.Log(LogLevel.Warn, "Exception while hiding the application", e);
                throw new DialClientException(e);
            }
        }
    }
}
