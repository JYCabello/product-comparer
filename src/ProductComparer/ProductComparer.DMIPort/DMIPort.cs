using System.Threading.Tasks;
using ProductComprarer.DMIPort.Productos;

namespace ProductComparer.DMIPort
{
    public static class DMIPort
    {
        public static async Task<CatalogoResponseCatalogoResult> GetAsync(string username, string password)
        {
            var client = new PRODUCTOSSoapClient(PRODUCTOSSoapClient.EndpointConfiguration.PRODUCTOSSoap12);
            return await client.CatalogoAsync(username, password);
        }
    }
}
