using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace jmam.api.csrf.middlewares
{
	public class GlobalExceptionMiddleware
	{
		private readonly RequestDelegate _next;

		public GlobalExceptionMiddleware(
			RequestDelegate next
		)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				// Logic to perform on request

				await _next(context);

				// Logic to perform on response
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
		{
			try
			{
				ctx.Response.ContentType = "application/json";
				ctx.Response.StatusCode = (ex is AntiforgeryValidationException) ? (int)HttpStatusCode.Conflict : (int)HttpStatusCode.InternalServerError;
				var result = JsonConvert.SerializeObject(GetCompleteError(ctx, ex));
				await ctx.Response.WriteAsync(result);
			}
			catch (Exception)
			{
				throw;
			}
		}

		private ErrorDetails GetCompleteError(HttpContext ctx, Exception ex)
		{
			return new ErrorDetails
			{
				StatusCode = ctx.Response.StatusCode,
				Message = ex.Message,
				StackTrace = ex.StackTrace,
				InnerException = ex.InnerException != null ? GetCompleteError(ctx, ex.InnerException) : null
			};
		}
	}

	public static class GlobalExceptionMiddlewareExtensions
	{
		public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<GlobalExceptionMiddleware>();
		}
	}

	public class ErrorDetails
	{
		public int StatusCode { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public ErrorDetails InnerException { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

}