using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.WebGateway.ContentFormatters
{
    public class PlainTextOutputFormatter : TextOutputFormatter
    {
        public PlainTextOutputFormatter()
        {
            MediaTypeHeaderValue header = new MediaTypeHeaderValue("text/plain");
            SupportedMediaTypes.Add(header);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) ||
                contentType == "text/plain")
                return true;

            return false;
        }

        public override Task WriteAsync(OutputFormatterWriteContext context)
        {
            return base.WriteAsync(context);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            HttpResponseMessage response = (HttpResponseMessage)context.Object;
            if (response.Content != null)
            {
                byte[] content = await response.Content.ReadAsByteArrayAsync();
                context.HttpContext.Response.ContentLength = content.Length;
                await context.HttpContext.Response.Body.WriteAsync(content);
            }
            else
            {
                return;
            }
        }
    }

}
