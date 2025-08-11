namespace BackendTorneosS.Entidades
{
    public class RecaptchaResponse
    {
        public bool success { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
        public List<string> errorCodes { get; set; }
}

}
