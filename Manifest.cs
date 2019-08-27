using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Ultimate_Steam_Acount_Manager
{
    class Manifest
    {

        [JsonProperty("encrypted")]
        public bool encrypted;

        [JsonProperty("accounts")]
        public List<ManifestEntry> accounts;

        [JsonProperty("loginMethod")]
        public LoginMethod loginMethod;

        [JsonProperty("steamArguments")]
        public string steamArguments;

        private static Manifest _manifest = null;

        private static readonly string filename = "manifest.json";
        private static readonly string fileExtention = "usam";

        public Manifest()
        {
            accounts = new List<ManifestEntry>();
            encrypted = false;
            loginMethod = LoginMethod.Parameter;
            steamArguments = "";
        }

        public static Manifest GetManifest()
        {
            if (_manifest != null) return _manifest;
            _manifest = Load();
            return _manifest;
        }

        private bool Save()
        {
            string data = JsonConvert.SerializeObject(this);
            try
            {
                string folder = Util.GetDataFolder();
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(folder + filename, data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Manifest Load()
        {
            string file = Util.GetDataFolder() + filename;
            if (File.Exists(file))
            {
                string data = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<Manifest>(data);
            }
            Manifest manifest = new Manifest();
            manifest.Save();
            return manifest;
        }

        public bool AddAccount(SteamAccount account, string password = null)
        {
            if (string.IsNullOrEmpty(password) && encrypted) return false;
            string data = JsonConvert.SerializeObject(account);
            long date = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            string filename = string.Format("{0:X}." + fileExtention, date);
            ManifestEntry entry = new ManifestEntry()
            {
                Filename = filename,
                IV = null,
                Salt = null
            };
            if (!string.IsNullOrEmpty(password))
            {
                entry.IV = Steam_Desktop_Authenticator.FileEncryptor.GetInitializationVector();
                entry.Salt = Steam_Desktop_Authenticator.FileEncryptor.GetRandomSalt();
                data = Steam_Desktop_Authenticator.FileEncryptor.EncryptData(password, entry.Salt, entry.IV, data);
            }
            try
            {
                string folder = Util.GetDataFolder();
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(folder + entry.Filename, data);
            }
            catch (Exception)
            {
                return false;
            }
            accounts.Add(entry);
            return Save();
        }

        public bool DeleteAccount(ManifestEntry entry)
        {
            if (entry == null) return false;
            if (!string.IsNullOrEmpty(entry.Filename))
                try
                {
                    File.Delete(Util.GetDataFolder() + entry.Filename);
                }
                catch (Exception)
                {
                }
            if (accounts.Contains(entry)) accounts.Remove(entry);
            if (accounts.Count == 0) encrypted = false;
            return Save();
        }

        public bool UpdateAccount(ManifestEntry entry, SteamAccount account, string password = null)
        {
            if (entry == null || account == null) return false;
            if (encrypted && string.IsNullOrEmpty(password)) return false;
            if (!accounts.Contains(entry)) return AddAccount(account, password);
            string data = JsonConvert.SerializeObject(account);
            if (!string.IsNullOrWhiteSpace(password))
            {
                if(string.IsNullOrEmpty(entry.IV))
                    entry.IV = Steam_Desktop_Authenticator.FileEncryptor.GetInitializationVector();
                if (string.IsNullOrEmpty(entry.Salt))
                    entry.Salt = Steam_Desktop_Authenticator.FileEncryptor.GetRandomSalt();
                data = Steam_Desktop_Authenticator.FileEncryptor.EncryptData(password, entry.Salt, entry.IV, data);
            }
            try
            {
                string folder = Util.GetDataFolder();
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(folder + entry.Filename, data);
            }
            catch (Exception)
            {
                return false;
            }
            return Save();
        }

        public List<SteamAccount> GetAllAccounts(string password = null, int limit = -1, EncryptionProgressForm progressForm = null)
        {
            if (encrypted && string.IsNullOrEmpty(password)) return null;
            string dir = Util.GetDataFolder();
            List<SteamAccount> ret = new List<SteamAccount>(accounts.Count);
            int done = 0;
            if (progressForm != null) progressForm.SetProgress(done, accounts.Count);
            foreach (ManifestEntry entry in accounts)
            {
                try
                {
                    string data = File.ReadAllText(dir + entry.Filename);
                    if (encrypted)
                    {
                        string decrypted = Steam_Desktop_Authenticator.FileEncryptor.DecryptData(password, entry.Salt, entry.IV, data);
                        if (string.IsNullOrEmpty(decrypted)) return null;
                        data = decrypted;
                    }
                    SteamAccount account = JsonConvert.DeserializeObject<SteamAccount>(data);
                    if (account == null) continue;
                    account.ManifestEntry = entry;
                    ret.Add(account);
                    if (progressForm != null) progressForm.SetProgress(++done);
                    if (ret.Count == limit) break;//count never goes below 0
                }
                catch (Exception)
                {
                }
            }
            return ret;
        }

        public bool ChangeEncryptionKey(string oldPass, string newPass, EncryptionProgressForm progressForm = null)
        {
            if (string.IsNullOrEmpty(oldPass) && encrypted) return false;
            if (!encrypted) oldPass = null;
            List<SteamAccount> accounts = GetAllAccounts(oldPass, -1, progressForm);
            if (accounts == null || accounts.Count == 0) return false;
            encrypted = !string.IsNullOrEmpty(newPass);
            bool sucess = true;
            int done = 0;
            if(progressForm != null) progressForm.SetProgress(done, accounts.Count);
            foreach (SteamAccount account in accounts)
            {
                sucess &= UpdateAccount(account.ManifestEntry, account, newPass);
                if (progressForm != null) progressForm.SetProgress(++done);
            }
            
            return sucess;
        }

    }

    public class ManifestEntry
    {
        [JsonProperty("encryption_iv")]
        public string IV { get; set; }

        [JsonProperty("encryption_salt")]
        public string Salt { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }
    }

}
