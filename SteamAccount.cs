using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamAuth;

namespace Ultimate_Steam_Acount_Manager
{
    class SteamAccount
    {

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("shared_secret")]
        public string SharedSecret { get; set; }

        [JsonProperty("last_login")]
        public long LastLogin { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("SteamID64")]
        public long SteamID64 { get; set; }

        private SteamGuardAccount SteamGuardAccount;

        public ManifestEntry ManifestEntry;

        public SteamAccount() { }

        public SteamAccount(string login, string password)
        {
            Login = login ?? throw new ArgumentNullException(nameof(login));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public SteamAccount(string login, string password, string sharedSecret, string comment = "") : this(login, password)
        {
            SharedSecret = sharedSecret ?? throw new ArgumentNullException(nameof(sharedSecret));
            Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        }

        public string Get2FACode()
        {
            if (string.IsNullOrEmpty(SharedSecret)) return null;
            if (SteamGuardAccount == null)
                SteamGuardAccount = new SteamGuardAccount() { SharedSecret = SharedSecret };
            return SteamGuardAccount.GenerateSteamGuardCode();
        }

    }
}
