using System.Threading.Tasks;
using ProductComprarer.DMIPort.Productos;

namespace ProductComparer.DMIPort
{
    public static class DMIPort
    {
        public static async Task<System.Xml.XmlNode> GetAsync(string username, string password)
        {
            var client = new PRODUCTOSSoapClient(PRODUCTOSSoapClient.EndpointConfiguration.PRODUCTOSSoap12);
            return await client.Catalogo_htmlAsync(username, password);
        }
    }
}
