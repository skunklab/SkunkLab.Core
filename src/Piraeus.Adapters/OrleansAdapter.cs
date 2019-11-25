using Capl.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piraeus.Adapters.Utilities;
using Piraeus.Auditing;
using Piraeus.Core;
using Piraeus.Core.Logging;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class OrleansAdapter
    {
        public OrleansAdapter(string identity, string channelType, string protocolType, GraphManager graphManager, ILog logger = null)
        {

            auditor = AuditFactory.CreateSingleton().GetAuditor(AuditType.Message);
            this.identity = identity;
            this.channelType = channelType;
            this.protocolType = protocolType;
            this.graphManager = graphManager;
            this.logger = logger;
            //this.context = context;

            container = new Dictionary<string, Tuple<string, string>>();
            ephemeralObservers = new Dictionary<string, IMessageObserver>();
            durableObservers = new Dictionary<string, IMessageObserver>();
        }

        public event EventHandler<ObserveMessageEventArgs> OnObserve;   //signal protocol adapter

        private readonly GraphManager graphManager;
        //private readonly HttpContext context;
        private readonly IAuditor auditor;
        private string identity;
        private readonly string channelType;
        private readonly string protocolType;
        private readonly Dictionary<string, Tuple<string, string>> container;  //resource, subscription + leaseKey
        private readonly Dictionary<string, IMessageObserver> ephemeralObservers; //subscription, observer
        private readonly Dictionary<string, IMessageObserver> durableObservers;   //subscription, observer
        private System.Timers.Timer leaseTimer; //timer for leases
        private bool disposedValue = false; // To detect redundant calls
        private readonly ILog logger;



        public string Identity
        {
            set { identity = value; }
        }

        public async Task<List<string>> LoadDurableSubscriptionsAsync(string identity)
        {
            List<string> list = new List<string>();

            IEnumerable<string> subscriptionUriStrings = await graphManager.GetSubscriberSubscriptionsListAsync(identity);

            if (subscriptionUriStrings == null || subscriptionUriStrings.Count() == 0)
            {
                return null;
            }

            foreach (var item in subscriptionUriStrings)
            {
                if (!durableObservers.ContainsKey(item))
                {
                    MessageObserver observer = new MessageObserver();
                    observer.OnNotify += Observer_OnNotify;

                    //set the observer in the subscription with the lease lifetime
                    TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);

                    string leaseKey = await graphManager.AddSubscriptionObserverAsync(item, leaseTime, observer);

                    //add the lease key to the list of ephemeral observers
                    durableObservers.Add(item, observer);
                    Console.WriteLine($"Durable observer added - '{item}' - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");

                    //get the resource from the subscription
                    Uri uri = new Uri(item);
                    string resourceUriString = item.Replace(uri.Segments[^1], "");

                    list.Add(resourceUriString); //add to list to return

                    //add the resource, subscription, and lease key the container

                    if (!container.ContainsKey(resourceUriString))
                    {
                        container.Add(resourceUriString, new Tuple<string, string>(item, leaseKey));
                        Console.WriteLine($"Resource, subscriptioon and lease key added to container {resourceUriString}, {item}, and {leaseKey} - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
                    }
                }
            }

            if (subscriptionUriStrings.Count() > 0)
            {
                EnsureLeaseTimer();
            }

            return list.Count == 0 ? null : list;
        }

        //public async Task<bool> CanPublishAsync(EventMetadata metadata, bool channelEncrypted)
        //{
        //    if (metadata == null)
        //    {
        //        await logger?.LogWarningAsync($"Cannot publish to Orleans resource with null metadata");
                
        //        return false;
        //    }

        //    if (!metadata.Enabled)
        //    {
        //        Console.WriteLine($"Publish resource '{metadata.ResourceUriString}' is disabled - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Publish resource '{1}' is disabled.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
        //        return false;
        //    }

        //    if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
        //    {
        //        Console.WriteLine($"Publish resource '{metadata.ResourceUriString}' has expired - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Publish resource '{1}' has expired.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
        //        return false;
        //    }

        //    if (metadata.RequireEncryptedChannel && !channelEncrypted)
        //    {
        //        Console.WriteLine($"Publish resource '{metadata.ResourceUriString}' required encrypted channel - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Publish resource '{1}' requires an encrypted channel.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
        //        return false;
        //    }

        //    AuthorizationPolicy policy = await graphManager.GetAccessControlPolicyAsync(metadata.PublishPolicyUriString);

        //    if (policy == null)
        //    {
        //        Console.WriteLine($"Publish resource '{metadata.ResourceUriString}' has no publish authorization policy - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Publish policy URI {1} did not return an authorization policy.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.PublishPolicyUriString);
        //        return false;
        //    }

        //    ClaimsIdentity identity = context == null ? Thread.CurrentPrincipal.Identity as ClaimsIdentity : new ClaimsIdentity(context.User.Claims);

        //    bool authz = policy.Evaluate(identity);

        //    if (!authz)
        //    {
        //        Console.WriteLine($"Publish resource '{metadata.ResourceUriString}' authorization is denied for {this.identity} - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Identity '{1}' is not authorized to publish to resource '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), this.identity, metadata.ResourceUriString);
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Publish resource '{metadata.ResourceUriString}' authorization is allowed for {this.identity} - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //    }

        //    return authz;
        //}

        //public async Task<bool> CanSubscribeAsync(string resourceUriString, bool channelEncrypted)
        //{
        //    EventMetadata metadata = await graphManager.GetPiSystemMetadataAsync(resourceUriString);
            

        //    if (metadata == null)
        //    {
        //        Console.WriteLine($"Cannot subscribe to Orleans resource '{resourceUriString}' with null metadata - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Cannot subscribe to Orleans resource will null metadata.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
        //        return false;
        //    }

        //    if (!metadata.Enabled)
        //    {
        //        Console.WriteLine($"Subscrbe resource '{resourceUriString}' is disabled - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Subscribe resource '{1}' is disabled.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
        //        return false;
        //    }

        //    if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
        //    {
        //        Console.WriteLine($"Subscribe resource '{resourceUriString}' has expired - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Subscribe resource '{1}' has expired.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
        //        return false;
        //    }

        //    if (metadata.RequireEncryptedChannel && !channelEncrypted)
        //    {
        //        Console.WriteLine($"Subscribe resource '{resourceUriString}' required encrypted channel - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Subscribe resource '{1}' requires an encrypted channel.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
        //        return false;
        //    }

        //    AuthorizationPolicy policy = await graphManager.GetAccessControlPolicyAsync(metadata.SubscribePolicyUriString);

        //    if (policy == null)
        //    {
        //        Console.WriteLine($"Subscribe resource '{resourceUriString}' has no subscribe authorization policy - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Subscribe policy URI did not return an authorization policy", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.SubscribePolicyUriString);
        //        return false;
        //    }

        //    //ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
        //    ClaimsIdentity identity = context == null ? Thread.CurrentPrincipal.Identity as ClaimsIdentity : new ClaimsIdentity(context.User.Claims);

        //    bool authz = policy.Evaluate(identity);

        //    if (!authz)
        //    {
        //        Console.WriteLine($"Subscribe resource '{metadata.ResourceUriString}' authorization is denied for {this.identity} - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //        Trace.TraceWarning("{0} - Identity '{1}' is not authorized to subscribe/unsubcribe to resource '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), this.identity, metadata.ResourceUriString);
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Subscribe resource '{metadata.ResourceUriString}' authorization is allowed for {this.identity} - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");
        //    }

        //    return authz;
        //}


        public async Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes = null)
        {
            AuditRecord record = null;
            DateTime receiveTime = DateTime.UtcNow;

            try
            {
                record = new MessageAuditRecord(message.MessageId, identity, channelType, protocolType.ToUpperInvariant(), message.Message.Length, MessageDirectionType.In, true, receiveTime);

                if (indexes == null || indexes.Count == 0)
                {
                    await graphManager.PublishAsync(message.ResourceUri, message);
                    await logger?.LogDebugAsync($"Published to '{message.ResourceUri}' by {identity} without indexes.");
                }
                else
                {
                    await graphManager.PublishAsync(message.ResourceUri, message, indexes);
                    await logger?.LogDebugAsync($"Published to '{message.ResourceUri}' by {identity} with indexes.");
                }
            }
            catch (Exception ex)
            {
                await logger?.LogErrorAsync(ex, $"Error during publish to '{message.ResourceUri}' for {identity}");
                record = new MessageAuditRecord(message.MessageId, identity, channelType, protocolType.ToUpperInvariant(), message.Message.Length, MessageDirectionType.In, false, receiveTime, ex.Message);
            }
            finally
            {
                if (message.Audit)
                {
                    await auditor?.WriteAuditRecordAsync(record);
                }
            }
        }

        public async Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata)
        {
            try
            {
                metadata.IsEphemeral = true;
                string subscriptionUriString = await graphManager.SubscribeAsync(resourceUriString, metadata);

                //create and observer and wire up event to receive notifications
                MessageObserver observer = new MessageObserver();
                observer.OnNotify += Observer_OnNotify;

                //set the observer in the subscription with the lease lifetime
                TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);

                string leaseKey = await graphManager.AddSubscriptionObserverAsync(subscriptionUriString, leaseTime, observer);

                //add the lease key to the list of ephemeral observers
                ephemeralObservers.Add(subscriptionUriString, observer);

                //add the resource, subscription, and lease key the container
                if (!container.ContainsKey(resourceUriString))
                {
                    container.Add(resourceUriString, new Tuple<string, string>(subscriptionUriString, leaseKey));
                }

                //ensure the lease timer is running
                EnsureLeaseTimer();
                logger?.LogDebugAsync($"Subscribed to '{resourceUriString}' with '{subscriptionUriString}' for {identity}.");
                return subscriptionUriString;
            }
            catch (Exception ex)
            {
                await logger?.LogErrorAsync(ex, $"Error during subscribe to '{resourceUriString}' for {identity}");
                throw ex;
            }
        }

        public async Task UnsubscribeAsync(string resourceUriString)
        {
            try
            {

                //unsubscribe from resource
                if (container.ContainsKey(resourceUriString))
                {
                    Console.WriteLine($"Container has '{resourceUriString}' needed to unsubscribe - {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff")}");

                    if (ephemeralObservers.ContainsKey(container[resourceUriString].Item1))
                    {
                        await graphManager.RemoveSubscriptionObserverAsync(container[resourceUriString].Item1, container[resourceUriString].Item2);
                        await graphManager.UnsubscribeAsync(container[resourceUriString].Item1);
                        ephemeralObservers.Remove(container[resourceUriString].Item1);
                    }

                    container.Remove(resourceUriString);
                    await logger?.LogDebugAsync($"Unsubscribed '{resourceUriString}'.");
                }
            }
            catch (Exception ex)
            {
                await logger?.LogErrorAsync(ex, $"Error during unsubscribe to '{resourceUriString}' for {identity}.");
                throw ex;
            }
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {                
                if (disposing)
                {
                    if (leaseTimer != null)
                    {
                        leaseTimer.Stop();
                        leaseTimer.Dispose();
                    }

                    RemoveDurableObserversAsync().GetAwaiter();
                    RemoveEphemeralObserversAsync().GetAwaiter();
                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion




        #region private methods
        private void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
            //observeCount++;
            //Trace.TraceInformation("Obsever {0}", observeCount);
            //signal the protocol adapter
            OnObserve?.Invoke(this, new ObserveMessageEventArgs(e.Message));
        }

        private void EnsureLeaseTimer()
        {
            if (leaseTimer == null)
            {
                leaseTimer = new System.Timers.Timer(30000);
                leaseTimer.Elapsed += LeaseTimer_Elapsed;
                leaseTimer.Start();
            }
        }

        private void LeaseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            KeyValuePair<string, Tuple<string, string>>[] kvps = container.ToArray();

            if (kvps == null || kvps.Length == 0)
            {
                leaseTimer.Stop();
                return;
            }

            Task leaseTask = Task.Factory.StartNew(async () =>
            {
                if (kvps != null && kvps.Length > 0)
                {
                    foreach (var kvp in kvps)
                    {
                        await graphManager.RenewObserverLeaseAsync(kvp.Value.Item1, kvp.Value.Item2, TimeSpan.FromSeconds(60.0));
                    }
                }
            });

            leaseTask.LogExceptions();

        }

        private async Task RemoveDurableObserversAsync()
        {
            List<string> list = new List<string>();

            int cnt = durableObservers.Count;
            if (durableObservers.Count > 0)
            {
                Console.WriteLine($"Durable subscription observers found to remove for {identity} - {DateTime.UtcNow.ToString("yyyy - MM - ddTHH - MM - ss.fffff")}");
                List<Task> taskList = new List<Task>();
                KeyValuePair<string, IMessageObserver>[] kvps = durableObservers.ToArray();
                foreach (var item in kvps)
                {
                    IEnumerable<KeyValuePair<string, Tuple<string, string>>> items = container.Where((c) => c.Value.Item1 == item.Key);
                    foreach (var lease in items)
                    {
                        list.Add(lease.Value.Item1);

                        if (durableObservers.ContainsKey(lease.Value.Item1))
                        {
                            Task task = graphManager.RemoveSubscriptionObserverAsync(lease.Value.Item1, lease.Value.Item2);
                            taskList.Add(task);
                        }
                    }
                }

                if (taskList.Count > 0)
                {
                    await Task.WhenAll(taskList);
                }

                durableObservers.Clear();
                RemoveFromContainer(list);
                Console.WriteLine($"Durable subscription observers removed for {identity} - {DateTime.UtcNow.ToString("yyyy - MM - ddTHH - MM - ss.fffff")}");
                Trace.TraceInformation("'{0}' - Durable observers removed by Orleans Adapter for identity '{1}'", cnt, identity);
            }
            else
            {
                Console.WriteLine($"No durable subscription observers found to remove for {identity} - {DateTime.UtcNow.ToString("yyyy - MM - ddTHH - MM - ss.fffff")}");
                Trace.TraceInformation("No Durable observers found by Orleans Adapter to be removed for identity '{0}'", identity);
            }
        }

        private async Task RemoveEphemeralObserversAsync()
        {
            List<string> list = new List<string>();
            int cnt = ephemeralObservers.Count;

            if (ephemeralObservers.Count > 0)
            {
                Console.WriteLine($"Ephemeral subscription observers found to remove for {identity} - {DateTime.UtcNow.ToString("yyyy - MM - ddTHH - MM - ss.fffff")}");
                KeyValuePair<string, IMessageObserver>[] kvps = ephemeralObservers.ToArray();
                List<Task> unobserveTaskList = new List<Task>();
                foreach (var item in kvps)
                {
                    IEnumerable<KeyValuePair<string, Tuple<string, string>>> items = container.Where((c) => c.Value.Item1 == item.Key);

                    foreach (var lease in items)
                    {
                        list.Add(lease.Value.Item1);
                        if (ephemeralObservers.ContainsKey(lease.Value.Item1))
                        {
                            Task unobserveTask = graphManager.RemoveSubscriptionObserverAsync(lease.Value.Item1, lease.Value.Item2);
                            unobserveTaskList.Add(unobserveTask);
                        }
                    }
                }

                if (unobserveTaskList.Count > 0)
                {
                    await Task.WhenAll(unobserveTaskList);
                }


                ephemeralObservers.Clear();
                RemoveFromContainer(list);
                Console.WriteLine($"Ephemeral subscription observers removed for {identity} - {DateTime.UtcNow.ToString("yyyy - MM - ddTHH - MM - ss.fffff")}");
                Trace.TraceInformation("'{0}' - Ephemeral observers removed by Orleans Adapter for identity '{1}'", cnt, identity);
            }
            else
            {
                Console.WriteLine($"No ephemeral subscription observers found to remove for {identity} - {DateTime.UtcNow.ToString("yyyy - MM - ddTHH - MM - ss.fffff")}");
                Trace.TraceInformation("No Ephemeral observers found by Orleans Adapter to be removed for identity '{0}'", identity);
            }

        }

        private void RemoveFromContainer(string subscriptionUriString)
        {
            List<string> list = new List<string>();
            var query = container.Where((c) => c.Value.Item1 == subscriptionUriString);

            foreach (var item in query)
            {
                list.Add(item.Key);
            }

            foreach (string item in list)
            {
                container.Remove(item);
            }
        }

        private void RemoveFromContainer(List<string> subscriptionUriStrings)
        {
            foreach (var item in subscriptionUriStrings)
            {
                RemoveFromContainer(item);
            }
        }




        #endregion
    }
}
