using System.Web;
using System.Xml.Linq;

namespace BlazorMedia.Client
{
    public partial class Reviewer
    {
        public string Url => $"https://letmebingthatforyou.com/?q={HttpUtility.UrlEncode(Name)}";
    }
}
