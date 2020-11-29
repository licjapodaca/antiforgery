using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace jmam.api.csrf.middlewares
{
	public class AntiForgeryMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IAntiforgery _csrf;

		public AntiForgeryMiddleware(
			RequestDelegate next,
			IAntiforgery csrf
		)
		{
			_next = next;
			_csrf = csrf;
		}

		public async Task Invoke(HttpContext ctx)
		{
			try
			{
				if (ctx.Request.Method.ToUpper() == "GET")
				{
					var tokens = _csrf.GetAndStoreTokens(ctx);

					ctx.Response.Cookies.Append(
						key: "x-csrf-token",
						value: tokens.RequestToken,
						options: new CookieOptions
						{
							HttpOnly = true,
							Secure = true,
							SameSite = SameSiteMode.Strict
						}
					);

					await _next(ctx);
				}
				else
				{
					ctx.Request.Headers.Append("x-csrf-token", ctx.Request.Cookies["x-csrf-token"]);
					await _csrf.ValidateRequestAsync(ctx);
					await _next(ctx);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
	}

	public static class AntiForgeryMiddlewareExtensions
	{
		public static IApplicationBuilder UseAntiForgeryMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<AntiForgeryMiddleware>();
		}
	}
}