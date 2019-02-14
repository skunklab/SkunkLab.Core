using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.WebGateway.ContentFormatters
{
    public class BinaryOutputFormatter : OutputFormatter
    {
        public BinaryOutputFormatter()
        {
            MediaTypeHeaderValue header = new MediaTypeHeaderValue("application/octet-stream");
            SupportedMediaTypes.Add(header);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) ||
                contentType == "application/octet-stream")
                return true;

            return false;
        }

        public override Task WriteAsync(OutputFormatterWriteContext context)
        {
            return base.WriteAsync(context);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            byte[] msg = null;

            if(context.Object is string)
            {
                msg = Encoding.UTF8.GetBytes((string)context.Object);
                
            }
            else if(context.Object is byte[])
            {
                msg = (byte[])context.Object;
            }
            else
            {
                throw new InvalidCastException("Unsupported content type.");
            }

            await context.HttpContext.Response.Body.WriteAsync(msg);
            //HttpResponseMessage response = (HttpResponseMessage)context.Object;
            //if (response.Content != null)
            //{
            //    byte[] content = await response.Content.ReadAsByteArrayAsync();
            //    context.HttpContext.Response.ContentLength = content.Length;
            //    await context.HttpContext.Response.Body.WriteAsync(content);
            //}
            //else
            //{
            //    return;
            //}
        }

        private Exception InvalidOperationException(string v)
        {
            throw new NotImplementedException();
        }
    }
}
