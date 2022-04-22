// Copyright (c) Mihir Dilip. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AspNetCore.Authentication.Basic.Tests.Infrastructure;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Authentication.Basic.Tests.Events
{
    public class BasicAuthenticationFailedContext
    {
		private static readonly string ExpectedExceptionMessage = $"Either {nameof(BasicOptions.Events.OnValidateCredentials)} delegate on configure options {nameof(BasicOptions.Events)} should be set or use an extension method with type parameter of type {nameof(IBasicUserValidationService)} or register an implementation of type {nameof(IBasicUserValidationServiceFactory)} in the service collection.";

		[Fact]
		public async Task Exception_result_null()
		{
			using var server = TestServerBuilder.BuildTestServer(options =>
			{
				options.Realm = TestServerBuilder.Realm;
				options.Events.OnValidateCredentials = context =>
				{
					Assert.Null(context.Result);

					return Task.CompletedTask;
				};

				options.Events.OnAuthenticationFailed = context =>
				{
					Assert.Null(context.Result);
					Assert.NotNull(context.Exception);
					Assert.IsType<InvalidOperationException>(context.Exception);
					Assert.Equal(ExpectedExceptionMessage, context.Exception.Message);

					return Task.CompletedTask;
				};
			});
			using var client = server.CreateClient();
			using var request = new HttpRequestMessage(HttpMethod.Get, TestServerBuilder.BaseUrl);
			request.Headers.Authorization = FakeUsers.FakeUser.ToAuthenticationHeaderValue();

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			{
				using var response = await client.SendAsync(request);

				Assert.False(response.IsSuccessStatusCode);
				Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
			});

			Assert.Equal(ExpectedExceptionMessage, exception.Message);
		}


		[Fact]
		public async Task Exception_result_not_null()
		{
			using var server = TestServerBuilder.BuildTestServer(options =>
			{
				options.Realm = TestServerBuilder.Realm;
				options.Events.OnValidateCredentials = context =>
				{
					Assert.Null(context.Result);

					return Task.CompletedTask;
				};

				options.Events.OnAuthenticationFailed = context =>
				{
					Assert.Null(context.Result);
					Assert.NotNull(context.Exception);
					Assert.IsType<InvalidOperationException>(context.Exception);
					Assert.Equal(ExpectedExceptionMessage, context.Exception.Message);

					context.NoResult();

					Assert.NotNull(context.Result);

					return Task.CompletedTask;
				};
			});
			using var client = server.CreateClient();
			using var request = new HttpRequestMessage(HttpMethod.Get, TestServerBuilder.BaseUrl);
			request.Headers.Authorization = FakeUsers.FakeUser.ToAuthenticationHeaderValue();
			using var response = await client.SendAsync(request);

			Assert.False(response.IsSuccessStatusCode);
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
	}
}
