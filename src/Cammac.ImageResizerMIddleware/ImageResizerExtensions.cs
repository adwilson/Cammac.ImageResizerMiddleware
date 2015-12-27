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
		/// <param name="wwwrootRelative404Paths">A list of paths to the 404 images, they must be relative to the wwwroot folder in the format "/img/404.jpg" (note the first slash).</param>
		/// <returns></returns>
		public static IApplicationBuilder UseImageResizerMiddleware(this IApplicationBuilder builder, IEnumerable<string> wwwrootRelative404Paths)
		{
			return builder.UseMiddleware<ImageResizerMiddleware>(wwwrootRelative404Paths);
		}
	}
}
