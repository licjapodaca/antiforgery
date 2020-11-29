using System;
using jmam.api.csrf.middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace jmam.api.csrf
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
			// Agregando CSRF
			services.AddAntiforgery(options =>
			{
				options.Cookie.Expiration = TimeSpan.FromSeconds(20);
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				options.Cookie.SameSite = SameSiteMode.Strict;
				options.Cookie.Name = "x-csrf-token-server";
				
				options.HeaderName = $"x-csrf-token";
				
				options.SuppressXFrameOptionsHeader = true;
			});

			// Activando CORS
			services.AddCors(options =>
			{
				options.AddPolicy("MyCors", opts =>
				{
					opts.AllowAnyHeader();
					opts.AllowAnyMethod();
					opts.AllowAnyOrigin();
				});
			});

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
			
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

			app.UseGlobalExceptionMiddleware();

            app.UseHttpsRedirection();

            app.UseRouting();

			app.UseCors("MyCors");

			app.UseAntiForgeryMiddleware();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
