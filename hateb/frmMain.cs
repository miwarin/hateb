using System;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace hateb2
{

    public partial class frmMain : Form
    {
        struct PostURI
        {
            public String uri;
            public String comment;
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnHateb_Click(object sender, EventArgs e)
        {
            String username;
            String api_key;
            ArrayList uris;
            String postURI = "http://b.hatena.ne.jp/atom/post";

            username = getUsername();
            api_key = getApikey();
            uris = getURI();

            var client = new HttpClient(new WsseClientHandler { UserName = username, Password = api_key });

            foreach (PostURI uri in uris)
            {
                byte[] content;
                content = getContent(uri.uri, uri.comment);
                ByteArrayContent h = new ByteArrayContent(content);
                client.PostAsync(postURI, h);
            }

        }
        private String getUsername()
        {
            String username = txtUsername.Text;
            return username;
        }

        private String getApikey()
        {
            String apikey = txtAPIKey.Text;
            return apikey;
        }

        private ArrayList getURI()
        {
            ArrayList uris = new ArrayList();
            String text = txtInput.Text;
            String[] sp = text.Split(new String[] { "\r\n" }, StringSplitOptions.None);
            int count = sp.Count();
            int i;

            for (i = 0; i < count; )
            {
                if (sp[i] == "")
                {
                    break;
                }
                PostURI pu = new PostURI();
                pu.uri = sp[i];
                pu.comment = sp[i + 1];
                uris.Add(pu);
                i += 2;
            }
            return uris;
        }

        // タイトルはダミー
        // はてなのAPIをつかってみた / Rails4とWSSE認証 | Workabroad.jp http://www.workabroad.jp/posts/2040
        private byte[] getContent(String uri, String comment)
        {

            String entry = String.Format("<entry xmlns=\"http://purl.org/atom/ns#\"><title>dummy</title><link rel=\"related\" type=\"text/html\" href=\"{0} \" /><summary type=\"text/plain\">{1}</summary></entry>", uri, comment);

            byte[] data = Encoding.UTF8.GetBytes(entry);
            return data;
        }
    
    }
    
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
