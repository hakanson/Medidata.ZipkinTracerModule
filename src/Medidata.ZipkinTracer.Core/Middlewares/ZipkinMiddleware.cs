﻿using System.Threading.Tasks;
using log4net;
using Microsoft.Owin;
using Owin;

namespace Medidata.ZipkinTracer.Core.Middlewares
{
    public class ZipkinMiddleware : OwinMiddleware
    {
        private readonly ZipkinMiddlewareOptions _options;

        public ZipkinMiddleware(OwinMiddleware next, ZipkinMiddlewareOptions options) : base(next)
        {
            _options = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var traceProvider = new TraceProvider(_options, context);
            var logger = LogManager.GetLogger("ZipkinMiddleware");
            var zipkin = new ZipkinClient(traceProvider, logger, _options);
            var span = zipkin.StartServerTrace(context.Request.Uri, context.Request.Method);
            await Next.Invoke(context);
            zipkin.EndServerTrace(span);
        }
    }

    public static class AppBuilderExtensions
    {
        public static void UseZipkin(this IAppBuilder app, ZipkinMiddlewareOptions options)
        {
            if (options.Enable)
            {
                options.Validate();
                app.Use<ZipkinMiddleware>(options);
            }
        }
    }
}