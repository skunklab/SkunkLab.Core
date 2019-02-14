using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace Piraeus.WebGateway.ContentFormatters
{
    public class PlainTextInputFormatter : TextInputFormatter
    {
        public PlainTextInputFormatter()
        {
            MediaTypeHeaderValue header = new MediaTypeHeaderValue("text/plain");
            SupportedMediaTypes.Add(header);
        }
        public override Boolean CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) ||
                contentType == "text/plain")
                return true;

            return false;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var contentType = context.HttpContext.Request.ContentType;


            if (contentType == "text/plain")
            {               
                using (var ms = new MemoryStream())
                {
                    await request.Body.CopyToAsync(ms);
                    var content = Encoding.UTF8.GetString(ms.ToArray());
                    return await InputFormatterResult.SuccessAsync(content);
                }
            }

            return await InputFormatterResult.FailureAsync();
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            return await ReadRequestBodyAsync(context);
        }
    }
}
