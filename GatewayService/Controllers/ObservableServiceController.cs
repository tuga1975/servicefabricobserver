﻿#region Copyright

// //=======================================================================================
// // Microsoft Azure Customer Advisory Team  
// //
// // This sample is supplemental to the technical guidance published on the community
// // blog at http://blogs.msdn.com/b/paolos/. 
// // 
// // Author: Paolo Salvatori
// //=======================================================================================
// // Copyright © 2016 Microsoft Corporation. All rights reserved.
// // 
// // THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// // EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// // MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
// //=======================================================================================

#endregion

namespace Microsoft.AzureCat.Samples.ObserverPattern.GatewayService
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.AzureCat.Samples.ObserverPattern.Entities;
    using Microsoft.AzureCat.Samples.ObserverPattern.Framework;
    using Microsoft.AzureCat.Samples.ObserverPattern.Interfaces;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Remoting.Client;

    #endregion

    public class ObservableServiceController : ApiController
    {
        #region Private Static Methods

        private static IClientObservableService GetServiceProxy(long? partitionKey, Uri serviceUri)
        {
            if (serviceUri == null)
                throw new ArgumentException($"Parameter {nameof(serviceUri)} is null or invalid.", nameof(serviceUri));
            return partitionKey.HasValue
                ? ServiceProxy.Create<IClientObservableService>(serviceUri, new ServicePartitionKey(partitionKey.Value))
                : ServiceProxy.Create<IClientObservableService>(serviceUri);
        }

        #endregion

        #region Private Constants

        //************************************
        // Parameters
        //************************************

        #endregion

        #region Public Methods

        [HttpGet]
        [Route("api/observable/service")]
        public string Test()
        {
            return "TEST OBSERVABLE SERVICE CONTROLLER";
        }

        /// <summary>
        ///     Registers a service as observable for a given topic.
        ///     This method is called by a client component.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <returns>The asynchronous result of the operation.</returns>
        [HttpPost]
        [Route("api/observable/service/register")]
        public async Task RegisterObservableServiceAsync(GatewayRequest request)
        {
            try
            {
                // Validates parameter
                if (string.IsNullOrWhiteSpace(request?.Topic) ||
                    (request.ObservableEntityId?.ServiceUri == null))
                    throw new ArgumentException($"Parameter {nameof(request)} is null or invalid.", nameof(request));

                // Gets service proxy
                IClientObservableService proxy = GetServiceProxy(
                    request.ObservableEntityId.PartitionKey,
                    request.ObservableEntityId.ServiceUri);
                if (proxy == null)
                    throw new ApplicationException("The ServiceProxy cannot be null.");

                // Invokes actor using proxy
                ServiceEventSource.Current.Message(
                    $"Registering observable...\r\n[Observable]: {request.ObservableEntityId}\r\n[Publication]: Topic=[{request.Topic}]");
                await proxy.RegisterObservableServiceAsync(request.Topic);
            }
            catch (AggregateException ex)
            {
                if (!(ex.InnerExceptions?.Count > 0))
                    throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
                foreach (Exception exception in ex.InnerExceptions)
                    ServiceEventSource.Current.Message(exception.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        /// <summary>
        ///     Unregisters an actor as observable for a given topic.
        ///     This method is called by a client component.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <returns>The asynchronous result of the operation.</returns>
        [HttpPost]
        [Route("api/observable/service/unregister")]
        public async Task UnregisterObservableActorAsync(GatewayRequest request)
        {
            try
            {
                // Validates parameter
                if (string.IsNullOrWhiteSpace(request?.Topic) ||
                    (request.ObservableEntityId?.ServiceUri == null))
                    throw new ArgumentException($"Parameter {nameof(request)} is null or invalid.", nameof(request));

                // Gets service proxy
                IClientObservableService proxy = GetServiceProxy(
                    request.ObservableEntityId.PartitionKey,
                    request.ObservableEntityId.ServiceUri);
                if (proxy == null)
                    throw new ApplicationException("The ServiceProxy cannot be null.");

                // Invokes actor using proxy
                ServiceEventSource.Current.Message(
                    $"Unregistering observable...\r\n[Observable]: {request.ObservableEntityId}\r\n[Publication]: Topic=[{request.Topic}]");
                await proxy.UnregisterObservableServiceAsync(request.Topic, request.UseObserverAsProxy);
            }
            catch (AggregateException ex)
            {
                if (!(ex.InnerExceptions?.Count > 0))
                    throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
                foreach (Exception exception in ex.InnerExceptions)
                    ServiceEventSource.Current.Message(exception.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        /// <summary>
        ///     Clear all observers and publications.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <returns>The asynchronous result of the operation.</returns>
        [HttpPost]
        [Route("api/observable/service/clear")]
        public async Task ClearObserversAndPublicationsAsync(GatewayRequest request)
        {
            try
            {
                // Validates parameter
                if (request.ObservableEntityId?.ServiceUri == null)
                    throw new ArgumentException($"Parameter {nameof(request)} is null or invalid.", nameof(request));

                // Gets service proxy
                IClientObservableService proxy = GetServiceProxy(
                    request.ObservableEntityId.PartitionKey,
                    request.ObservableEntityId.ServiceUri);
                if (proxy == null)
                    throw new ApplicationException("The ServiceProxy cannot be null.");

                // Invokes actor using proxy
                ServiceEventSource.Current.Message($"Clearing observers and publications...\r\n[Observable]: {request.ObservableEntityId}");
                await proxy.ClearObserversAndPublicationsAsync(request.UseObserverAsProxy);
            }
            catch (AggregateException ex)
            {
                if (!(ex.InnerExceptions?.Count > 0))
                    throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
                foreach (Exception exception in ex.InnerExceptions)
                    ServiceEventSource.Current.Message(exception.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        /// <summary>
        ///     Sends data to observers for a given topic.
        ///     This method is called by a client component.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <returns>The asynchronous result of the operation.</returns>
        [HttpPost]
        [Route("api/observable/service/notify")]
        public async Task NotifyObserversAsync(GatewayRequest request)
        {
            try
            {
                // Validates parameter
                if (string.IsNullOrWhiteSpace(request?.Topic) ||
                    (request.Messages == null) ||
                    !request.Messages.Any() ||
                    (request.ObservableEntityId?.ServiceUri == null))
                    throw new ArgumentException($"Parameter {nameof(request)} is null or invalid.", nameof(request));

                // Gets service proxy
                IClientObservableService proxy = GetServiceProxy(
                    request.ObservableEntityId.PartitionKey,
                    request.ObservableEntityId.ServiceUri);
                if (proxy == null)
                    throw new ApplicationException("The ServiceProxy cannot be null.");

                // Invokes actor using proxy
                foreach (Message message in request.Messages.Where(message => message != null))
                {
                    ServiceEventSource.Current.Message(
                        $"Notifying observers...\r\n[Observable]: {request.ObservableEntityId}\r\n[Message]: Topic=[{request.Topic}] Body=[{message.Body ?? "NULL"}]");
                    await proxy.NotifyObserversAsync(request.Topic, message, request.UseObserverAsProxy);
                }
            }
            catch (AggregateException ex)
            {
                if (!(ex.InnerExceptions?.Count > 0))
                    throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
                foreach (Exception exception in ex.InnerExceptions)
                    ServiceEventSource.Current.Message(exception.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        #endregion
    }
}