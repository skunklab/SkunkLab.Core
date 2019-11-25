using Capl.Authorization;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Clustering.Redis;
using Orleans.Hosting;
using Piraeus.Configuration;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Core.Utilities;
using Piraeus.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piraeus.Grains
{
    public class GraphManager
    {
        /// <summary>
        /// Creates a singleton of the GraphManager.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <remarks>This will mean that only 1 IClusterClient will be used.</remarks>
        public static GraphManager Create(IClusterClient client)
        {
            if (instance == null)
            {
                instance = new GraphManager(client);
            }

            return instance;
        }

        /// <summary>
        /// Creates a singleton of the GraphManager
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <remarks>This will mean that only 1 IClusterClient will be used.</remarks>
        public static GraphManager Create(OrleansConfig config)
        {
            if (instance == null)
            {
                instance = new GraphManager(config);
            }

            return instance;
        }

        public static bool IsInitialized
        {
            get
            {
                return instance != null && instance.client != null;
            }
        }

        public static GraphManager Instance { get { return instance; } }


        private readonly IClusterClient client;
        private static GraphManager instance;

        #region Static Resource Operations

        #region ctor
        public GraphManager(IClusterClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Creates a new instance of GraphManager
        /// </summary>
        /// <param name="config"></param>
        /// <remarks>Creates a new IClusterClient per instance of GraphManager.</remarks>
        public GraphManager(OrleansConfig config)
        : this(config.DataConnectionString, config.GetLoggerTypes(), Enum.Parse<LogLevel>(config.LogLevel, true), config.AppInsightsKey)
        {
        }

        public GraphManager(string connectionString, LoggerType loggers, LogLevel logLevel, string instrumentationKey = null)
        {
            //use storage provider
            var builder = new ClientBuilder();
            builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly));

            AddStorageProvider(builder, connectionString);
            AddAppInsighlts(builder, loggers, instrumentationKey);
            AddLoggers(builder, loggers, logLevel, instrumentationKey);
            this.client = builder.Build();
            this.client.Connect(CreateRetryFilter()).GetAwaiter().GetResult();
        }

        private Func<Exception, Task<bool>> CreateRetryFilter(int maxAttempts = 5)
        {
            var attempt = 0;
            return RetryFilter;

            async Task<bool> RetryFilter(Exception exception)
            {
                attempt++;
                Console.WriteLine($"Cluster client attempt {attempt} of {maxAttempts} failed to connect to cluster.  Exception: {exception}");
                if (attempt > maxAttempts)
                {
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(4));
                return true;
            }
        }

        private IClientBuilder AddStorageProvider(IClientBuilder builder, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                builder.UseLocalhostClustering();
            }
            else
            {
                if (connectionString.Contains("6379") || connectionString.Contains("6380"))
                {
                    builder.UseRedisGatewayListProvider(options => options.ConnectionString = connectionString);
                }
                else
                {
                    builder.UseAzureStorageClustering(options => options.ConnectionString = connectionString);
                }
            }

            return builder;
        }
        private IClientBuilder AddAppInsighlts(IClientBuilder builder, LoggerType loggers, string instrumentationKey = null)
        {
            if (string.IsNullOrEmpty(instrumentationKey))
                return builder;

            if (loggers.HasFlag(Piraeus.Configuration.LoggerType.AppInsights))
            {
                builder.AddApplicationInsightsTelemetryConsumer(instrumentationKey);
            }

            return builder;
        }

        private IClientBuilder AddLoggers(IClientBuilder builder, LoggerType loggers, LogLevel logLevel, string instrumentationKey = null)
        {
            builder.ConfigureLogging(op =>
            {
                if (loggers.HasFlag(Piraeus.Configuration.LoggerType.Console))
                {
                    op.AddConsole();
                    op.SetMinimumLevel(logLevel);
                }

                if (loggers.HasFlag(Piraeus.Configuration.LoggerType.Debug))
                {
                    op.AddDebug();
                    op.SetMinimumLevel(logLevel);
                }
            });

            return builder;
        }

        #endregion        


        /// <summary>
        /// Gets a resource from Orleans 
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns>Resource interface for grain.</returns>
        public IPiSystem GetPiSystem(string resourceUriString)
        {
            Uri uri = new Uri(resourceUriString);
            string uriString = uri.ToCanonicalString(false);
            return client.GetGrain<IPiSystem>(uriString);
        }

        /// <summary>
        /// Adds or updates a resource's metadata.
        /// </summary>
        /// <param name="metadata">Metadata that describes the resource.</param>
        /// <returns></returns>
        public async Task UpsertPiSystemMetadataAsync(EventMetadata metadata)
        {
            Uri uri = new Uri(metadata.ResourceUriString);
            metadata.ResourceUriString = uri.ToCanonicalString(false);
            IPiSystem resource = GetPiSystem(metadata.ResourceUriString);
            await resource.UpsertMetadataAsync(metadata);
        }

        /// <summary>
        /// Get a resource's metadata.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns>Resource's metadata.</returns>
        public async Task<EventMetadata> GetPiSystemMetadataAsync(string resourceUriString)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            return await resource.GetMetadataAsync();
        }




        public async Task<CommunicationMetrics> GetPiSystemMetricsAsync(string resourceUriString)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            return await resource.GetMetricsAsync();
        }
        /// <summary>
        /// Subscribes to a resource by creating a subscription and adding it to the resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="metadata">Metadata that describes the subscription.</param>
        /// <returns>Unique URI for the subscription.</returns>
        /// <remarks>The function creates a subscription, adds the subscription to the resource, then return the URI that identifies the subscription.</remarks>
        public async Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata)
        {
            Uri uri = new Uri(resourceUriString);
            string subscriptionUriString = uri.ToCanonicalString(true) + Guid.NewGuid().ToString();
            metadata.SubscriptionUriString = subscriptionUriString;


            //Add the metadata to the subscription
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.UpsertMetadataAsync(metadata);

            //Add the subscription to the resource
            IPiSystem resource = GetPiSystem(uri.ToCanonicalString(false));
            await resource.SubscribeAsync(subscription);

            return subscriptionUriString;
        }

        /// <summary>
        /// Unsubscribes a subscription from a resource.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI for the subscription.</param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(string subscriptionUriString)
        {
            //get the resource to unsubscribe
            Uri uri = new Uri(subscriptionUriString);
            string resourceUriString = uri.ToCanonicalString(false, true);
            IPiSystem resource = GetPiSystem(resourceUriString);

            //unsubscribe from the resource
            await resource.UnsubscribeAsync(subscriptionUriString);
        }

        public async Task UnsubscribeAsync(string subscriptionUriString, string identity)
        {
            Uri uri = new Uri(subscriptionUriString);
            string resourceUriString = uri.ToCanonicalString(false, true);
            IPiSystem resource = GetPiSystem(resourceUriString);

            //unsubscribe from the resource
            await resource.UnsubscribeAsync(subscriptionUriString, identity);
        }

        /// <summary>
        /// Publishes a message to a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="message">Message to publish.</param>
        /// <returns></returns>
        public async Task PublishAsync(string resourceUriString, EventMessage message)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            await resource.PublishAsync(message);
        }

        /// <summary>
        /// Publishes a message to a resources with indexes.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="message">Message to publishes.</param>
        /// <param name="indexes">Indexes used to filter subscriptions to subset.</param>
        /// <returns></returns>
        public async Task PublishAsync(string resourceUriString, EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            await resource.PublishAsync(message, indexes);
        }

        /// <summary>
        /// Gets a list of subscription URIs subscribed to the resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns>Array of subscription URIs subscribed to the resource.</returns>
        public async Task<IEnumerable<string>> GetPiSystemSubscriptionListAsync(string resourceUriString)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            return await resource.GetSubscriptionListAsync();
        }

        /// <summary>
        /// Adds a metric observer to a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="lifetime">The lifetime of the lease.</param>
        /// <param name="observer">Metric observer to receive events.</param>
        /// <returns>A unique string for the lease key, which is used to refresh the lease for the observer.</returns>
        public async Task<string> AddResourceObserverAsync(string resourceUriString, TimeSpan lifetime, MetricObserver observer)
        {
            IMetricObserver objRef = await client.CreateObjectReference<IMetricObserver>(observer);
            IPiSystem resource = GetPiSystem(resourceUriString);
            return await resource.AddObserverAsync(lifetime, objRef);
        }

        /// <summary>
        /// Add an error observer to a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="lifetime">The lifetime of the lease.</param>
        /// <param name="observer">Error observer to receive events.</param>
        /// <returns>A unique string for the lease key, whic is used to refresh the lease for the observer.</returns>
        public async Task<string> AddResourceObserverAsync(string resourceUriString, TimeSpan lifetime, ErrorObserver observer)
        {
            IErrorObserver objRef = await client.CreateObjectReference<IErrorObserver>(observer);
            IPiSystem resource = GetPiSystem(resourceUriString);
            return await resource.AddObserverAsync(lifetime, objRef);
        }

        /// <summary>
        /// Renews the lease for a resource's observer.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="leaseKey">Unique string of the observer's lease.</param>
        /// <param name="lifetime">The lifetime of the renewed lease.</param>
        /// <returns></returns>
        public async Task<bool> RenewResourceObserverLeaseAsync(string resourceUriString, string leaseKey, TimeSpan lifetime)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            return await resource.RenewObserverLeaseAsync(leaseKey, lifetime);
        }

        /// <summary>
        /// Removes an observer from a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="leaseKey">Unqiue string of the observer's lease.</param>
        /// <returns></returns>
        public async Task RemoveResourceObserverAsync(string resourceUriString, string leaseKey)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            await resource.RemoveObserverAsync(leaseKey);
        }

        /// <summary>
        /// Clears the resource; equivalent to deleting the resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns></returns>
        public async Task ClearPiSystemAsync(string resourceUriString)
        {
            IPiSystem resource = GetPiSystem(resourceUriString);
            await resource.ClearAsync();
        }

        #endregion

        #region Static Subscription Operations

        /// <summary>
        /// Get a subscription from Orleans.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns>Subscription interface for grain.</returns>
        public ISubscription GetSubscription(string subscriptionUriString)
        {
            Uri uri = new Uri(subscriptionUriString);
            return client.GetGrain<ISubscription>(uri.ToCanonicalString(false));
        }

        /// <summary>
        /// Adds or updates a subscription's metadata.
        /// </summary>
        /// <param name="metadata">Metadata that describes the subscription.</param>
        /// <returns></returns>
        public async Task UpsertSubscriptionMetadataAsync(SubscriptionMetadata metadata)
        {
            ISubscription subscription = GetSubscription(metadata.SubscriptionUriString);
            await subscription.UpsertMetadataAsync(metadata);
        }

        /// <summary>
        /// Gets a subscription's metadata.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns>Subscription metadata.</returns>
        public async Task<SubscriptionMetadata> GetSubscriptionMetadataAsync(string subscriptionUriString)
        {
            Uri uri = new Uri(subscriptionUriString);
            ISubscription subscription = GetSubscription(uri.ToCanonicalString(false));
            return await subscription.GetMetadataAsync();
        }

        public async Task<CommunicationMetrics> GetSubscriptionMetricsAsync(string subscriptionUriString)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.GetMetricsAsync();
        }

        /// <summary>
        /// Adds a message observer to the subscription.  Used to observe messages received by the subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="lifetime">Lifetime of the lease.</param>
        /// <param name="observer">Observer to receive events.</param>
        /// <returns>A unique string for the lease key, which is used to renew or delete the observer's lease.</returns>
        public async Task<string> AddSubscriptionObserverAsync(string subscriptionUriString, TimeSpan lifetime, MessageObserver observer)
        {
            IMessageObserver observerRef = await client.CreateObjectReference<IMessageObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        /// <summary>
        /// Adds a metric observer to the subscription.  Used to observe metrics in the subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="lifetime">Lifetime of the lease.</param>
        /// <param name="observer">Observer to receive events.</param>
        /// <returns>A unqiue string for the lease key, which is used to renew or delete the observer's lease.</returns>
        public async Task<string> AddSubscriptionObserverAsync(string subscriptionUriString, TimeSpan lifetime, MetricObserver observer)
        {
            IMetricObserver observerRef = await client.CreateObjectReference<IMetricObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        /// <summary>
        /// Adds an error observer to the subscription. Used to observe errors in the subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="lifetime">Lifetime of the lease.</param>
        /// <param name="observer">Observer to receive events.</param>
        /// <returns>A unique string for the lease key, which is used to renew or delete the observer's lease.</returns>
        public async Task<string> AddSubscriptionObserverAsync(string subscriptionUriString, TimeSpan lifetime, ErrorObserver observer)
        {
            IErrorObserver observerRef = await client.CreateObjectReference<IErrorObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        /// <summary>
        /// Removes an observer from a subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="leaseKey">Unqiue string of the lease to remove.</param>
        /// <returns></returns>
        public async Task RemoveSubscriptionObserverAsync(string subscriptionUriString, string leaseKey)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.RemoveObserverAsync(leaseKey);
        }

        /// <summary>
        /// Renews an observers lease for a subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="leaseKey">Unique string of the lease to renew.</param>
        /// <param name="lifetime">Lifetime of the renewed lease.</param>
        /// <returns>True if the lease is renewed; otherwise False.</returns>
        public async Task<bool> RenewObserverLeaseAsync(string subscriptionUriString, string leaseKey, TimeSpan lifetime)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.RenewObserverLeaseAsync(leaseKey, lifetime);
        }

        /// <summary>
        /// Deletes a subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns></returns>
        public async Task SubscriptionClearAsync(string subscriptionUriString)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.ClearAsync();
        }

        #endregion

        #region Static Subscriber Operations
        /// <summary>
        /// Gets subscriber from Orleans.
        /// </summary>
        /// <param name="identity">The identity of the subscriber.</param>
        /// <returns>Subscriber interface for grain.</returns>
        public ISubscriber GetSubscriber(string identity)
        {
            if (string.IsNullOrEmpty(identity))
            {
                return null;
            }

            return client.GetGrain<ISubscriber>(identity.ToLowerInvariant());
        }

        /// <summary>
        /// Adds a subscription reference to the subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns></returns>
        public async Task AddSubscriberSubscriptionAsync(string identity, string subscriptionUriString)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                await subscriber.AddSubscriptionAsync(subscriptionUriString);
            }
        }

        /// <summary>
        /// Removes a subscription reference from a subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <param name="subscriptionUriString">Unique URI that identifies the subsription.</param>
        /// <returns></returns>
        public async Task RemoveSubscriberSubscriptionAsync(string identity, string subscriptionUriString)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                await subscriber.RemoveSubscriptionAsync(subscriptionUriString);
            }
        }

        /// <summary>
        /// Returns a list of subscription URIs for the subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <returns>Array of subscription URIs for the subscriber.</returns>
        public async Task<IEnumerable<string>> GetSubscriberSubscriptionsListAsync(string identity)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                return await subscriber.GetSubscriptionsAsync();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <returns></returns>
        public async Task ClearSubscriberSubscriptionsAsync(string identity)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                await subscriber.ClearAsync();
            }
        }

        #endregion

        #region Static ResourceList

        /// <summary>
        /// Returns of list of resources in Orleans.
        /// </summary>
        /// <returns>Array of resource URIs.</returns>
        public async Task<List<string>> GetSigmaAlgebraAsync()
        {
            ISigmaAlgebra resourceList = client.GetGrain<ISigmaAlgebra>("resourcelist");
            return await resourceList.GetListAsync();
        }

        #endregion

        #region Static Access Control

        /// <summary>
        /// Gets access control grain from Orleans.
        /// </summary>
        /// <param name="policyUriString">Unique URI that identifies the access control policy.</param>
        /// <returns>AccessControl Interface from grain.</returns>
        public IAccessControl GetAccessControlPolicy(string policyUriString)
        {
            Uri uri = new Uri(policyUriString);
            string uriString = uri.ToCanonicalString(false);
            return client.GetGrain<IAccessControl>(uriString);
        }

        /// <summary>
        /// Adds or updates an CAPL access control policy.
        /// </summary>
        /// <param name="policyUriString">Unique URI that identifies the policy.</param>
        /// <param name="policy">CAPL access control policy.</param>
        /// <returns></returns>
        public async Task UpsertAcessControlPolicyAsync(string policyUriString, AuthorizationPolicy policy)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            await accessControl.UpsertPolicyAsync(policy);
        }

        /// <summary>
        /// Deletes an access control policy.
        /// </summary>
        /// <param name="policyUriString">Unique URI that identifies the policy.</param>
        /// <returns></returns>
        public async Task ClearAccessControlPolicyAsync(string policyUriString)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            await accessControl.ClearAsync();
        }

        /// <summary>
        /// Gets an access control policy.
        /// </summary>
        /// <param name="policyUriString">Unqiue URI that identifies the policy.</param>
        /// <returns>CAPL access control policy.</returns>
        public async Task<AuthorizationPolicy> GetAccessControlPolicyAsync(string policyUriString)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            return await accessControl.GetPolicyAsync();
        }

        #endregion

        #region Static Service Identity

        public IServiceIdentity GetServiceIdentity(string key)
        {
            return client.GetGrain<IServiceIdentity>(key);
        }

        public async Task AddServiceIdentityClaimsAsync(string key, List<KeyValuePair<string, string>> claims)
        {
            IServiceIdentity identity = GetServiceIdentity(key);
            await identity.AddClaimsAsync(claims);
        }

        public async Task AddServiceIdentityCertificateAsync(string key, string path, string password)
        {
            IServiceIdentity identity = GetServiceIdentity(key);
            X509Certificate2 cert = new X509Certificate2(path, password);
            if (cert != null)
            {
                byte[] certBytes = cert.Export(X509ContentType.Pfx, password);
                await identity.AddCertificateAsync(certBytes);
            }
        }

        public async Task AddServiceIdentityCertificateAsync(string key, string store, string location, string thumbprint, string password)
        {
            IServiceIdentity identity = GetServiceIdentity(key);
            SkunkLab.Storage.LocalFileStorage lfs = SkunkLab.Storage.LocalFileStorage.Create();
            X509Certificate2 cert = GetLocalCertificate(store, location, thumbprint);

            if (cert != null)
            {
                byte[] certBytes = cert.Export(X509ContentType.Pfx, password);
                await identity.AddCertificateAsync(certBytes);
            }
        }

        private static X509Certificate2 GetLocalCertificate(string store, string location, string thumbprint)
        {
            if (string.IsNullOrEmpty(store) || string.IsNullOrEmpty(location) || string.IsNullOrEmpty(thumbprint))
            {
                return null;
            }


            thumbprint = Regex.Replace(thumbprint, @"[^\da-fA-F]", string.Empty).ToUpper();


            StoreName storeName = (StoreName)Enum.Parse(typeof(StoreName), store, true);
            StoreLocation storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), location, true);


            X509Store certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection coll = certStore.Certificates;

            X509Certificate2Collection certCollection =
              certStore.Certificates.Find(X509FindType.FindByThumbprint,
                                      thumbprint.ToUpper(), false);
            X509Certificate2Enumerator enumerator = certCollection.GetEnumerator();
            X509Certificate2 cert = null;
            while (enumerator.MoveNext())
            {
                cert = enumerator.Current;
            }
            return cert;

        }




        #endregion


    }
}
