using Cammac.ImageResizingMIddleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Builder
{
    public static class ImageResizerExtensions
    {
		/// <summary>
		/// Starts the image resizer middleware. Place before UseStaticFiles() and UseMvc().
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseImageResizerMiddleware(this IApplicationBuilder builder, Action<ImageResizerOptions> options)
		{
			return builder.UseMiddleware<ImageResizerMiddleware>(options);
		}

		/// <summary>
		/// Starts the image resizer middleware. Place before UseStaticFiles() and UseMvc().
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseImageResizerMiddleware(this IApplicationBuilder builder)
		{
			Action<ImageResizerOptions> optsAction = (opts) => { };
			return builder.UseMiddleware<ImageResizerMiddleware>(optsAction);
		}
	}
}
