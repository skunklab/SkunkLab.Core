using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piraeus.Adapters;
using Piraeus.WebGateway.ContentFormatters;
using SkunkLab.Channels.Http;
using System;
using System.Buffers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;

using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.Core;
using Piraeus.GrainInterfaces;
using System.Diagnostics;

namespace Piraeus.WebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectController : ControllerBase
    {
        public ConnectController(PiraeusConfig pconfig, IClusterClient client)
        {
            this.config = pconfig;
            this.client = client;
        }
       
        private readonly PiraeusConfig config;
        private readonly IClusterClient client;
        private CancellationTokenSource source;
        private delegate void HttpResponseObserverHandler(object sender, SkunkLab.Channels.ChannelObserverEventArgs args);
        private event HttpResponseObserverHandler OnMessage;
        private ProtocolAdapter adapter;
        //private HttpResponseMessage response;
        private byte[] longpollValue;
        private string longpollResource;
        private readonly WaitHandle[] waitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false)
        };

        [HttpPost]
        public object Post()
        { 
            try
            {                
                adapter = ProtocolAdapterFactory.Create(config, Request.HttpContext, source.Token);
                adapter.OnClose += Adapter_OnClose;
                adapter.Init();
                return StatusCode(202);
                //return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
                //HttpRequestMessage request = HttpHelper.HttpContext.GetHttpRequestMessage();
                //return request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public IActionResult Get()
        {            
            try
            {
                SkunkLab.Security.Authentication.BasicAuthenticator authn = new SkunkLab.Security.Authentication.BasicAuthenticator();
                
                if (Request.HttpContext.WebSockets.IsWebSocketRequest)
                {
                    adapter = ProtocolAdapterFactory.Create(config, Request.HttpContext, source.Token);
                    adapter.OnClose += Adapter_OnClose;
                    adapter.OnError += Adapter_OnError;
                    adapter.Init();
                    return StatusCode(101);
                }
                else //long polling
                {
                    source = new CancellationTokenSource();
                    //adapter = ProtocolAdapterFactory.Create(config, Request.HttpContext.GetHttpRequestMessage(), source.Token);
                    adapter = ProtocolAdapterFactory.Create(config, Request.HttpContext, source.Token);
                    adapter.OnObserve += Adapter_OnObserve;
                    adapter.Init();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Listen), waitHandles[0]);
                    WaitHandle.WaitAll(waitHandles);
                    Task task = adapter.Channel.CloseAsync();
                    Task.WhenAll(task);
                    Response.Headers.Add("x-sl-resource", longpollResource);
                    return StatusCode(200, longpollValue);
                    //return response;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
                //HttpRequestMessage request = HttpHelper.HttpContext.GetHttpRequestMessage();
                //return request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            Trace.TraceError(e.Error.Message);
            //Exception ex = e.Error;
        }

        private void Adapter_OnObserve(object sender, SkunkLab.Channels.ChannelObserverEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            adapter.Dispose();
        }

        private void Listen(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            OnMessage += (o, a) => {

                //OutputFormatter formatter = null;
                //if (a.ContentType == "application/octet-stream")
                //{
                //    formatter = new BinaryOutputFormatter();
                //}
                //else if (a.ContentType == "text/plain")
                //{
                //    formatter = new PlainTextOutputFormatter();
                //}
                //else if (a.ContentType == "application/xml" || a.ContentType == "text/xml")
                //{
                //    XmlWriterSettings settings = new XmlWriterSettings();
                //    settings.OmitXmlDeclaration = true;
                //    formatter = new XmlSerializerOutputFormatter(settings);
                //}
                //else if (a.ContentType == "application/json" || a.ContentType == "text/json")
                //{
                //    JsonSerializerSettings settings = new JsonSerializerSettings();
                //    ArrayPool<char> pool = ArrayPool<char>.Shared;
                //    formatter = new JsonOutputFormatter(settings, pool);                   
                //}
                //else
                //{
                //    throw new SkunkLab.Protocols.Coap.UnsupportedMediaTypeException("Media type formatter not available.");
                //}

                //HttpRequestMessage request = HttpHelper.HttpContext.GetHttpRequestMessage();

                //if (a.ContentType != "application/octet-stream")
                //{                   
                //    response = request.CreateResponse<string>(HttpStatusCode.OK, Encoding.UTF8.GetString(a.Message), formatter);
                //}
                //else
                //{                    
                //    response = request.CreateResponse<byte[]>(HttpStatusCode.OK, a.Message, formatter);
                //}

                longpollValue = a.Message;
                longpollResource = a.ResourceUriString;
                //response.Headers.Add("x-sl-resource", a.ResourceUriString);
                are.Set();
            };
        }

        


    }
}