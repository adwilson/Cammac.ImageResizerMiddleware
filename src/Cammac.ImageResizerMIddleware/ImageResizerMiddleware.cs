using ImageResizer;
using ImageResizer.Plugins.PrettyGifs;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cammac.ImageResizingMIddleware
{
	public class ImageResizerMiddleware
	{
		private readonly static string[] _imageFileTypes = { ".jpg", ".jpeg", ".png", ".gif" };

		private readonly ImageResizer.Configuration.Config _config = new ImageResizer.Configuration.Config();
		private readonly IEnumerable<string> _wwwrootRelative404Paths;
		private readonly int _numberOf404Paths;
		private readonly bool _hasMultiple404Paths;
		private readonly Random _random;

		private readonly IHostingEnvironment _hostEnv;
		private readonly RequestDelegate _next;

		public ImageResizerMiddleware(RequestDelegate next,
			IHostingEnvironment hostEnv,
			IEnumerable<string> wwwrootRelative404Paths)
		{
			_next = next;
			_hostEnv = hostEnv;
			_wwwrootRelative404Paths = wwwrootRelative404Paths;
			_numberOf404Paths = _wwwrootRelative404Paths.Count();
			_hasMultiple404Paths = _numberOf404Paths > 1;
			if (_hasMultiple404Paths)
			{
				_random = new Random();
			}
			new PrettyGifs().Install(_config);
		}

		public async Task Invoke(HttpContext httpContext)
		{
			var req = httpContext.Request;
			if (_imageFileTypes.Any(x => req.Path.Value.EndsWith(x, StringComparison.OrdinalIgnoreCase)) && req.Query.Any())
			{
				string phsyicalPath = GetPhysicalPath(req.Path.Value);
				if (!File.Exists(phsyicalPath))
				{
					if (_hasMultiple404Paths)
					{
						var useAt = _random.Next(_numberOf404Paths);
						phsyicalPath = GetPhysicalPath(_wwwrootRelative404Paths.ElementAt(useAt));
					}
					else
					{
						phsyicalPath = GetPhysicalPath(_wwwrootRelative404Paths.First());
					}
				}
				var outStream = new MemoryStream();
				try
				{
					var parsableQs = req.QueryString.Value.Trim('?').Replace('&', ';');
					var job = new ImageJob(phsyicalPath, outStream, new Instructions(parsableQs));
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
