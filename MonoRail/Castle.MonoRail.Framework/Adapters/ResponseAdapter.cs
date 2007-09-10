// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MonoRail.Framework.Adapters
{
	using System;
	using System.IO;
	using System.Web;
	using Castle.MonoRail.Framework;

	/// <summary>
	/// Adapts the <see cref="IResponse"/> to
	/// an <see cref="HttpResponse"/> instance.
	/// </summary>
	public class ResponseAdapter : IResponse
	{
		private readonly IRailsEngineContext context;
		private readonly HttpResponse response;
		private readonly String appPath;
		private bool redirected;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseAdapter"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <param name="context">The parent context.</param>
		/// <param name="appPath">The app path.</param>
		public ResponseAdapter(HttpResponse response, IRailsEngineContext context, String appPath)
		{
			this.response = response;
			this.appPath = appPath;
			this.context = context;
		}

		/// <summary>
		/// Gets the caching policy (expiration time, privacy, 
		/// vary clauses) of a Web page.
		/// </summary>
		public HttpCachePolicy CachePolicy
		{
			get { return response.Cache; }
		}

		/// <summary>
		/// Sets the Cache-Control HTTP header to Public or Private.
		/// </summary>
		public String CacheControlHeader
		{
			get { return response.CacheControl; }
			set { response.CacheControl = value; }
		}

		/// <summary>
		/// Gets or sets the HTTP character set of the output stream.
		/// </summary>
		public String Charset
		{
			get { return response.Charset; }
			set { response.Charset = value; }
		}

		public int StatusCode
		{
			get { return response.StatusCode; }
			set { response.StatusCode = value; }
		}

		public String ContentType
		{
			get { return response.ContentType; }
			set { response.ContentType = value; }
		}

		public void AppendHeader(String name, String headerValue)
		{
			response.AppendHeader(name, headerValue);
		}

		public TextWriter Output
		{
			get { return response.Output; }
		}

		public Stream OutputStream
		{
			get { return response.OutputStream; }
		}

		public void BinaryWrite(byte[] buffer)
		{
			response.BinaryWrite(buffer);
		}

		public void BinaryWrite(Stream stream)
		{
			byte[] buffer = new byte[stream.Length];

			stream.Read(buffer, 0, buffer.Length);

			BinaryWrite(buffer);
		}

		public void Clear()
		{
			response.Clear();
		}

		public void ClearContent()
		{
			response.ClearContent();
		}

		public void Write(String s)
		{
			response.Write(s);
		}

		public void Write(object obj)
		{
			response.Write(obj);
		}

		public void Write(char ch)
		{
			response.Write(ch);
		}

		public void Write(char[] buffer, int index, int count)
		{
			response.Write(buffer, index, count);
		}

		public void WriteFile(String fileName)
		{
			response.WriteFile(fileName);
		}

		public void Redirect(String url)
		{
			redirected = true;

			response.Redirect(url, false);
		}

		public void Redirect(String url, bool endProcess)
		{
			redirected = true;

			response.Redirect(url, endProcess);
		}

		public void Redirect(String controller, String action)
		{
			redirected = true;

			IUrlBuilder builder = (IUrlBuilder) context.GetService(typeof(IUrlBuilder));

			response.Redirect(builder.BuildUrl(context.UrlInfo, controller, action), false);
		}

		public void Redirect(String area, String controller, String action)
		{
			if (area == null || area.Length == 0)
			{
				Redirect(controller, action);
			}
			else
			{
				redirected = true;

				IUrlBuilder builder = (IUrlBuilder) context.GetService(typeof(IUrlBuilder));

				response.Redirect(builder.BuildUrl(context.UrlInfo, area, controller, action), false);
			}
		}

		public bool IsClientConnected
		{
			get { return response.IsClientConnected; }
		}

		public bool WasRedirected
		{
			get { return redirected; }
		}

		/// <summary>
		/// Creates the cookie.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="cookieValue">The cookie value.</param>
		public void CreateCookie(String name, String cookieValue)
		{
			CreateCookie(new HttpCookie(name, cookieValue));
		}

		/// <summary>
		/// Creates the cookie.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="cookieValue">The cookie value.</param>
		/// <param name="expiration">The expiration.</param>
		public void CreateCookie(String name, String cookieValue, DateTime expiration)
		{
			HttpCookie cookie = new HttpCookie(name, cookieValue);

			cookie.Expires = expiration;
			cookie.Path = context.ApplicationPath;

			CreateCookie(cookie);
		}

		/// <summary>
		/// Creates the cookie.
		/// </summary>
		/// <param name="cookie">The cookie.</param>
		public void CreateCookie(HttpCookie cookie)
		{
			response.Cookies.Add(cookie);
		}

		/// <summary>
		/// Removes the cookie.
		/// </summary>
		/// <param name="name">The name.</param>
		public void RemoveCookie(string name)
		{
			HttpCookie cookie = new HttpCookie(name, "");
			
			cookie.Expires = DateTime.Now.AddYears(-10);
			cookie.Path = context.ApplicationPath;
			
			CreateCookie(cookie);
		}
	}
}