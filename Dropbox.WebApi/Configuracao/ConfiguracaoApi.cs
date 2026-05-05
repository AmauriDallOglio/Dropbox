using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.Dto;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Threading.RateLimiting;

namespace Dropbox.WebApi.Configuracao
{
    public static class ConfiguracaoApi
    {
        public static void Carregar(this IServiceCollection services)
        {
            // resolve AppSettingsDto já registrado
            var provider = services.BuildServiceProvider();
            var appSettings = provider.GetRequiredService<AppSettingsDto>();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100_000_000;
            });

            services.AddEndpointsApiExplorer();
            PrintaConsole.Info("Carregando configuração swager");
            services.AddSwaggerGen(options =>
            {
                options.ConfigurarSwagger();
            });
            services.AddControllers();

            services.AddHttpContextAccessor();
            PrintaConsole.Info("Carregando controllers");
            Controllers(services);
            PrintaConsole.Info("Carregando autorização");
            Autorizacao(services, appSettings);
            PrintaConsole.Info("Carregando CORS");
            Cors(services);
            PrintaConsole.Info("Carregando Prometheus");
            Prometheus(services);

            PrintaConsole.Info("Carregando ExecutarRateLimiter");
            ExecutarRateLimiter(services);
        }


        public static void ExecutarRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, cancellationToken) =>
                {
                    var resultado = ResultadoOperacao.GerarErro(
                        "Você excedeu o limite de requisições. Tente novamente mais tarde.",
                        StatusCodes.Status429TooManyRequests,
                        null
                    );

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var json = System.Text.Json.JsonSerializer.Serialize(resultado);
                    await context.HttpContext.Response.WriteAsync(json, cancellationToken);
                };

                // Políticas nomeadas
                options.AddSlidingWindowLimiter("consulta", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromSeconds(5);
                    opt.SegmentsPerWindow = 6;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                options.AddSlidingWindowLimiter("escrita", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromSeconds(5);
                    opt.SegmentsPerWindow = 3;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                options.AddSlidingWindowLimiter("login", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromSeconds(5);
                    opt.SegmentsPerWindow = 1;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                ////GlobalLimiter
                //options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                //{
                //    // Tenta recuperar o objeto salvo no middleware
                //    CacheUsuarioSessaoDto? usuarioSessao = httpContext.Items["UsuarioSessao"] as CacheUsuarioSessaoDto;

                //    string? ip = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                //    string? path = httpContext.Request.Path;
                //    string? metodo = httpContext.Request.Method;

                //    string correlationId = Guid.NewGuid().ToString();


                //    if (usuarioSessao != null)
                //    {
                //        string? ipd = ip ?? "semip";
                //        string? chave = $"usuario-{usuarioSessao.IdUsuario}-tenant-{usuarioSessao.IdTenant}-ip-{ipd}";
                //        return RateLimitPartition.GetSlidingWindowLimiter(
                //            partitionKey: chave, //  chave composta
                //            factory: _ => new SlidingWindowRateLimiterOptions
                //            {
                //                PermitLimit = 5,
                //                Window = TimeSpan.FromSeconds(5),
                //                SegmentsPerWindow = 6,
                //                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                //                QueueLimit = 0
                //            });
                //    }
                //    else
                //    {
                //        string? ipd = ip ?? "anonimo";
                //        return RateLimitPartition.GetSlidingWindowLimiter(
                //            partitionKey: ipd, //  cada IP anônimo tem seu próprio contador
                //            factory: _ => new SlidingWindowRateLimiterOptions
                //            {
                //                PermitLimit = 5,
                //                Window = TimeSpan.FromSeconds(5),
                //                SegmentsPerWindow = 3,
                //                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                //                QueueLimit = 0
                //            });
                //    }
                //});
            });
        }

        private static void Prometheus(this IServiceCollection services)
        {

            /*
             * 
                Isso vai fazer sua API expor métricas para o Prometheus coletar automaticamente.

                A API publica /metrics com dados técnicos (requisições, duração, status HTTP, etc.).
                O Prometheus (em http://localhost:9090/targets) consulta http://localhost:5135/metrics a cada 5s (seu scrape_interval).
                No Targets, o job autenticacaojwt fica UP quando a coleta funciona.
                Depois você pode criar gráficos/alertas (normalmente no Grafana) com essas métricas.
                O ajuste no middleware foi crucial porque:

                <OpenTelemetry.Exporter.Prometheus.AspNetCore Version="1.15.3-beta.1" />
                <OpenTelemetry.Extensions.Hosting Version="1.15.3" />
                <OpenTelemetry.Instrumentation.AspNetCore Version="1.15.2" />
                <OpenTelemetry.Instrumentation.Http Version="1.15.1" />

                Antes ele exigia token em /metrics, então o Prometheus travava/expirava.
                Agora /metrics passa livre, sem autenticação, só para monitoramento.
                Resumo: você ganhou observabilidade real da API, sem impactar autenticação dos endpoints de negócio. 
             * 
             */

            services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddPrometheusExporter();
                });
        }


        private static void ConfigurarSwagger(this SwaggerGenOptions c)
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Dropbox Upload API",
                Version = "v1",
                Description = "API para upload e listagem de arquivos no Dropbox"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Informe: Bearer {seu token}",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        }

        private static void Controllers(this IServiceCollection services)
        {
            services.AddControllers();
        }

        private static void Cors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });
        }

        private static void Autorizacao(this IServiceCollection services, AppSettingsDto appSettings)
        {
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.AddPolicy("dropbox.read", p => p.RequireClaim("scope", "dropbox.read"));
                options.AddPolicy("dropbox.write", p => p.RequireClaim("scope", "dropbox.write"));
            });
            AutenticacaoJwt(services, appSettings);
        }


        private static void AutenticacaoJwt(this IServiceCollection services, AppSettingsDto appSettings)
        {

            var provider = services.BuildServiceProvider();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var tokenSecret = configuration["Token:Secret"];
            var publicKeyBase64 = appSettings.Token.Secret;
            var issuer = configuration["Token:Issuer"];
            var audience = configuration["Token:Audience"];
            Microsoft.IdentityModel.Tokens.SecurityKey securityKey;
            if (!string.IsNullOrWhiteSpace(tokenSecret))
            {
                securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenSecret));
            }
            else
            {
                using var rsa = CarregarRsaPublica(publicKeyBase64);
                securityKey = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsa);
            }

            services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = securityKey,
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
        }

        private static System.Security.Cryptography.RSA CarregarRsaPublica(string publicKey)
        {
            if (string.IsNullOrWhiteSpace(publicKey))
                throw new InvalidOperationException("Token:PublicKey não configurado.");

            var rsa = System.Security.Cryptography.RSA.Create();
            var key = publicKey.Trim();

            if (key.Contains("BEGIN PUBLIC KEY") || key.Contains("BEGIN RSA PUBLIC KEY"))
            {
                rsa.ImportFromPem(key);
                return rsa;
            }

            var keyBytes = Convert.FromBase64String(key);
            try
            {
                rsa.ImportRSAPublicKey(keyBytes, out _);
                return rsa;
            }
            catch
            {
                rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                return rsa;
            }
        }

        //private static void AutenticacaoJwt(this IServiceCollection services)
        //{
        //    var provider = services.BuildServiceProvider();
        //    var configuration = provider.GetRequiredService<IConfiguration>();
        //    var publicKeyBase64 = configuration["Token:PublicKey"];
        //    var issuer = configuration["Token:Issuer"];
        //    var audience = configuration["Token:Audience"];

        //    try
        //    {
        //        using RSA rsa = RSA.Create();
        //        rsa = CarregarRsaPublica(publicKeyBase64);
        //        RsaSecurityKey? securityKey = new RsaSecurityKey(rsa);
        //        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //Microsoft.AspNetCore.Authentication.JwtBearer
        //        .AddJwtBearer(options =>
        //        {
        //            //Se houver token, valida assinatura, issuer, audience, lifetime.
        //            options.RequireHttpsMetadata = false; //Isso permite tokens em conexões não seguras. Em produção, deixar como true para exigir HTTPS.
        //            options.SaveToken = true;
        //            options.TokenValidationParameters = new TokenValidationParameters
        //            {
        //                ValidateIssuerSigningKey = false,
        //                IssuerSigningKey = securityKey,
        //                ValidateIssuer = false,
        //                ValidIssuer = issuer,
        //                ValidateAudience = false,
        //                ValidAudience = audience,
        //                ValidateLifetime = false,  //  Valida se o token ainda está dentro do prazo de validade
        //                ClockSkew = TimeSpan.Zero //  Remove tolerância de tempo (default é 5 min)
        //            };
        //            options.Events = new JwtBearerEvents
        //            {
        //                //Se não houver ou estiver malformado, cai no OnChallenge.
        //                OnChallenge = async context =>
        //                {
        //                    context.HandleResponse();
        //                    var resultado = ResultadoOperacao.GerarErro("Token inválido ou ausente", StatusCodes.Status401Unauthorized);
        //                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //                    context.Response.ContentType = "application/json";
        //                    await context.Response.WriteAsJsonAsync(resultado);
        //                },
        //                OnForbidden = async context =>
        //                {
        //                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //                    var resultado = ResultadoOperacao.GerarErro("Acesso negado", StatusCodes.Status403Forbidden);
        //                    await context.Response.WriteAsJsonAsync(resultado);
        //                }
        //            };
        //        });
        //        PrintaConsole.Sucesso("JWT Bearer configurado com sucesso");
        //    }
        //    catch (Exception ex)
        //    {
        //        PrintaConsole.Error($"Erro ao configurar JWT: {ex.Message}");
        //        throw;
        //    }
        //}


    }
}



