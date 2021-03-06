﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Thinktecture.IdentityModel.Hawk.Core;
using Thinktecture.IdentityModel.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Hawk.WebApi;

namespace Thinktecture.IdentityModel.Hawk.Samples.WebApi.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new HttpSelfHostConfiguration("http://localhost:12345");

            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var credentialStorage = new List<Credential>()
            {
                new Credential()
                {
                    Id = "dh37fgj492je",
                    Algorithm = SupportedAlgorithms.SHA256,
                    User = "Steve",
                    Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn"
                }
            };

            var options = new Options()
            {
                ClockSkewSeconds = 60,
                LocalTimeOffsetMillis = 0,
                CredentialsCallback = (id) => credentialStorage.FirstOrDefault(c => c.Id == id),
                ResponsePayloadHashabilityCallback = (r) => true,
                VerificationCallback = (request, ext) =>
                {
                    if (String.IsNullOrEmpty(ext))
                        return true;

                    string name = "X-Request-Header-To-Protect";
                    return ext.Equals(name + ":" + request.Headers[name].First());
                }
            };

            configuration.MessageHandlers.Add(new HawkAuthenticationHandler(options));

            using (var server = new HttpSelfHostServer(configuration))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("[SelfHost] Press Enter to terminate the server...");
                Console.ReadLine();
            }
        }
    }

    [Authorize]
    public class ValuesController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse<string>(HttpStatusCode.OK, "Hello, " + User.Identity.Name);
        }

        public HttpResponseMessage Post([FromBody]string name)
        {
            string message = String.Format("Hello, {0}. Thanks for flying Hawk", name);
            return Request.CreateResponse<string>(HttpStatusCode.OK, message);
        }
    }
}
