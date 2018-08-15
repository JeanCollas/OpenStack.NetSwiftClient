namespace NetSwiftClient
{
    public class SwiftClientConfig
    {
        public string AuthUrl { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; } = "Default";
        public string Token { get; set; }
        public string ObjectStoreUrl { get; set; }
    }
}