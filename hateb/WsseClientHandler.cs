using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;

namespace hateb2
{

    // HttpClientでWSSEしてみる - tmytのらくがき http://tmyt.hateblo.jp/entry/20130320/1363789847
    class WsseClientHandler : HttpClientHandler
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        private string GenerateCredentials()
        {
            var created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var nonce = WebUtility.UrlEncode(Guid.NewGuid().ToString());
            using (var sha1 = SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(nonce + created + Password);
                var digest = Convert.ToBase64String(sha1.ComputeHash(bytes));
                return string.Format(@"UsernameToken Username=""{0}"", PasswordDigest=""{1}"", Nonce=""{2}"", Created=""{3}""",
                UserName, digest, Convert.ToBase64String(Encoding.UTF8.GetBytes(nonce)), created);
            }
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("X-WSSE", GenerateCredentials());
            return base.SendAsync(request, cancellationToken);
        }
    }
    
}
