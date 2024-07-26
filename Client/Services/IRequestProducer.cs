using Client.Model;

namespace Client.Services
{
    public interface IRequestProducer
    {
        public Task<string> SendRequest(string request); 
        //Task<IEnumerable<Order>> SendGetRequest(); 
    }
}
