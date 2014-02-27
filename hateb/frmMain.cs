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
        class PostURI
        {
            public PostURI(String uri, String comment)
            {
                this.uri = uri;
                this.comment = comment;
            }
            public String uri { get; set; }
            public String comment { get; set; }
        }

        public frmMain()
        {
            InitializeComponent();
            loadUsername();
            loadAPIKey();
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

        private void loadUsername()
        {
            txtUsername.Text = Properties.Settings.Default.ID;
        }

        private void loadAPIKey()
        {
            txtAPIKey.Text = Properties.Settings.Default.APIKEY;
        }

        private String getApikey()
        {
            String apikey = txtAPIKey.Text;
            return apikey;
        }

        enum ST
        {
            URI,
            COMMENT,
        };

        // フォーマット
        //
        //   空白行 0 行以上
        //   URI
        //   コメント0 or 1行
        //   空白行 1 行以上
        //   URI
        //   コメント0 or 1行
        //   空白行 1 行以上
        //     :
        private ArrayList getURI()
        {
            ArrayList uris = new ArrayList();
            String text = txtInput.Text;
            String[] sp = text.Split(new String[] { "\r\n" }, StringSplitOptions.None);
            ST st = ST.URI;
            String tmp_uri = "";

            foreach(String line in sp)
            {
                switch(st)
                {
                    case ST.URI:

                        // URI として正しいかどうかを例外でチェック
                        try
                        {
                            Uri uri = new Uri(line);
                            tmp_uri = uri.AbsoluteUri;
                            st = ST.COMMENT;
                        }
                        catch (UriFormatException ex)
                        {
                            Console.WriteLine(ex);
                        }
                        break;

                    case ST.COMMENT:
                        uris.Add(new PostURI(tmp_uri, line));
                        st = ST.URI;
                        break;

                    default:
                        break;
                }
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

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ID = txtUsername.Text;
            Properties.Settings.Default.Save();
        }

        private void txtAPIKey_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.APIKEY = txtAPIKey.Text;
            Properties.Settings.Default.Save();
        }
    
    }
    
}
