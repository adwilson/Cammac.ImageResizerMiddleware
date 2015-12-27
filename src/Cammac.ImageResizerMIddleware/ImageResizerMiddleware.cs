using ImageResizer;
using ImageResizer.Plugins.PrettyGifs;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace Cammac.ImageResizingMIddleware
{
	public class ImageResizerMiddleware
	{
		private readonly static string[] _imageFileTypes = { ".jpg", ".jpeg", ".png", ".gif" };

		private readonly ImageResizer.Configuration.Config _config = new ImageResizer.Configuration.Config();

		private readonly IHostingEnvironment _hostEnv;
		private readonly RequestDelegate _next;
		private readonly Action<ImageResizerOptions> _options;
		private readonly IUrlHelper _urlHelper;

		public ImageResizerMiddleware(RequestDelegate next,
			IHostingEnvironment hostEnv,
			IUrlHelper urlHelper,
			Action<ImageResizerOptions> options)
		{
			_next = next;
			_hostEnv = hostEnv;
			_urlHelper = urlHelper;
			_options = options;
			new PrettyGifs().Install(_config);
		}

		public async Task Invoke(HttpContext httpContext)
		{
			ImageResizerOptions irOpts = new ImageResizerOptions();
			_options(irOpts);
			var req = httpContext.Request;
			if (_imageFileTypes.Any(x => req.Path.Value.EndsWith(x, StringComparison.OrdinalIgnoreCase)) && req.Query.Any())
			{
				string physicalPath = GetPhysicalPath(req.Path.Value);
				if (!File.Exists(physicalPath))
				{
					if (!string.IsNullOrEmpty(irOpts.Image404))
					{
						physicalPath = GetPhysicalPath(_urlHelper.Content(irOpts.Image404));
					}
					else
					{
						httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
						return;
					}
				}
				var outStream = new MemoryStream();
				try
				{
					var parsableQs = req.QueryString.Value.Trim('?').Replace('&', ';');
					var job = new ImageJob(physicalPath, outStream, new Instructions(parsableQs));
					_config.Build(job);
					string ext = req.Path.Value.Substring(req.Path.Value.LastIndexOf('.')).ToLowerInvariant();
					string mimeType = "application/octet-stream";
					switch (ext)
					{
						case ".jpg":
						case ".jpeg":
							mimeType = "image/jpeg";
							break;
						case ".png":
							mimeType = "image/png";
							break;
						case ".gif":
							mimeType = "image/gif";
							break;
					}

					httpContext.Response.ContentLength = outStream.Length;
					httpContext.Response.StatusCode = 200;
					httpContext.Response.ContentType = mimeType;

					long bytesReamining = outStream.Length;
					const int defaultBufferSize = 1024 * 16;
					byte[] buffer = new byte[defaultBufferSize];
					outStream.Seek(0, SeekOrigin.Begin);
					while (true)
					{
						if (bytesReamining <= 0)
						{
							return;
						}
						int readLength = (int)Math.Min(bytesReamining, buffer.Length);
						int count = await outStream.ReadAsync(buffer, 0, readLength);
						bytesReamining -= count;
						if (count == 0)
						{
							return;
						}
						await httpContext.Response.Body.WriteAsync(buffer, 0, count);
					}
				}
				finally
				{
					outStream.Dispose();
				}
			}

			await _next(httpContext);

		}

		private string GetPhysicalPath(string wwwrootRelativePath)
		{
			return Path.Combine(_hostEnv.WebRootPath, wwwrootRelativePath.Substring(1).Replace('/', '\\'));
		}
	}
}
