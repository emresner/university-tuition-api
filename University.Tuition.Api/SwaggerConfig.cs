using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace University.Tuition.Api
{
    public class SwaggerConfig : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly IConfiguration _config;

        public SwaggerConfig(IApiVersionDescriptionProvider provider, IConfiguration config)
        {
            _provider = provider;
            _config = config;
        }

        public void Configure(SwaggerGenOptions options)
        {
            // ----------- API Version Docs ---------------
            foreach (var desc in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                {
                    Title = "University Tuition API",
                    Version = desc.ApiVersion.ToString(),
                    Description = "Mobile (no auth, rate-limited), Banking (auth), and Admin (auth) endpoints."
                });
            }

            // ----------- Server (Gateway URL) ----------
            var serverUrl = _config["Swagger:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(serverUrl))
            {
                options.AddServer(new OpenApiServer
                {
                    Url = serverUrl,
                    Description = "API Gateway"
                });
            }

            // ============================================
            //                 JWT BEARER
            // ============================================
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Bearer token. Sadece token gir; 'Bearer' yazmana gerek yok.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
            options.AddSecurityDefinition("Bearer", bearerScheme);

            // ============================================
            //            APIM SUBSCRIPTION KEY
            // ============================================
            var subscriptionScheme = new OpenApiSecurityScheme
            {
                Name = "Ocp-Apim-Subscription-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "Azure API Management subscription key",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "SubscriptionKey"
                }
            };
            options.AddSecurityDefinition("SubscriptionKey", subscriptionScheme);

            // ============================================
            //            APIM TRACE (İSTEĞE BAĞLI)
            //  - Authorize penceresinde bu alanı 'true' yaparsan,
            //    isteklerde 'Ocp-Apim-Trace: true' header'ı gider
            //    ve APIM tarafında Trace üretir.
            // ============================================
            var traceScheme = new OpenApiSecurityScheme
            {
                Name = "Ocp-Apim-Trace",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "APIM trace'i açmak için 'true' gir (Ocp-Apim-Trace: true).",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApimTrace"
                }
            };
            options.AddSecurityDefinition("ApimTrace", traceScheme);

            // ============================================
            //            GLOBAL SECURITY REQUIREMENT
            //  - Üç şema da Authorize penceresinde görünür.
            //  - Trace'i boş bırakırsan header gönderilmez.
            // ============================================
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { bearerScheme, Array.Empty<string>() },
                { subscriptionScheme, Array.Empty<string>() },
                { traceScheme, Array.Empty<string>() }
            });
        }
    }
}
