using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Threading.Tasks;

namespace Piraeus.HttpGateway.Formatters
{
    public class BinaryOutputFormatter : OutputFormatter
    {
        private const string CONTENT_TYPE = "application/octet-stream";

        public BinaryOutputFormatter()
        {
            SupportedMediaTypes.Add(CONTENT_TYPE);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.HttpContext.Request.ContentType == CONTENT_TYPE;
        }

        public override Task WriteAsync(OutputFormatterWriteContext context)
        {
            return base.WriteAsync(context);
        }


        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            await Task.CompletedTask;
        }

        
    }
}
