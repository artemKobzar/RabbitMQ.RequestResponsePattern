namespace Client.Services
{
    public interface IRequestSender
    {
        public string SendRequest(string message);
        public Task<string> GetResponse();

    }
}
