using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Hybs;

namespace Hybs.Central
{
    public class Client : HttpClient
    {
        public Client(Region region, Environment environment): base()
        {
            Timeout = TimeSpan.FromSeconds(5);
            _region = region;
            _env = environment;
        }

        public IEnumerator<Realtime.ServerConnectDescriptor> GetLobbyServer(string appId, params string[] options)
        {
            string url = string.Format("{0}/v1/realtime/{1}", BaseUrl(), appId);
            if (options.Length > 0)
            {
                url += string.Format("?{0}", options[0]);
            }
            for (int i = 1; i < options.Length; i++)
            {
                url += "&" + options[i];
            }

            HttpResponseMessage response = GetAsync(url).Result;
            
            if (!response.IsSuccessStatusCode)
            {
                yield return null;
            } 
            else
            {
                System.IO.Stream s = response.Content.ReadAsStreamAsync().Result;
                object server = new DataContractJsonSerializer(typeof(Realtime.ServerConnectDescriptor)).ReadObject(s);
                yield return server as Realtime.ServerConnectDescriptor;
            } 
        }

        protected string BaseUrl()
        {
            switch (_env) 
            {
                case Environment.Playground:
                    return "http://central.playground.hayabusa-cloud.link";
                case Environment.Production:
                    switch (_region)
                    {
                        case Region.JP:
                            return "https://central.jp.prd.hayabusa-cloud.link";
                        default:
                            return "https://central.jp.prd.hayabusa-cloud.link";
                    }
                default:
                    return "http://localhost";
            };
        }

        protected Region _region;
        protected Environment _env;
    }
}
