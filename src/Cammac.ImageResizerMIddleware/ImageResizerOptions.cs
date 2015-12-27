using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cammac.ImageResizingMIddleware
{
    public class ImageResizerOptions
    {
		/// <summary>
		/// The virtual path to the 404 image
		/// </summary>
		public string Image404 { get; set; }
	}
}
