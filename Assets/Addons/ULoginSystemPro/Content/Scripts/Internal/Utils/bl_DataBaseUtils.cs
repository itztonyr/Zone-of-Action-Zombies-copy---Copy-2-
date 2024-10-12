using MFPS.ULogin;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public static class bl_DataBaseUtils
{
    public const string LOCK_TIME_KEY = "ulsp.ll.time";
    public static string SHA256_SECRET_KEY;

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        SHA256_SECRET_KEY = "";
    }

    /// <summary>
    /// Generate a random string with the given lenght
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string GenerateKey(int length = 7)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghlmnopqrustowuvwxyz";
        string key = "";
        for (int i = 0; i < length; i++)
        {
            key += chars[Random.Range(0, chars.Length)];
        }
        return key;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int ToInt(this string str)
    {
        int i = 0;
        if (!int.TryParse(str, out i))
        {
            Debug.LogWarning("Can't parse this string: " + str);
        }
        return i;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Md5Sum(string input)
    {
        MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++) { sb.Append(hash[i].ToString("X2")); }
        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static string CreateSecretHash(string parameters)
    {
        return Md5Sum(parameters + bl_LoginProDataBase.Instance.SecretKey).ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns></returns>
    public static string TimeFormat(float minutes)
    {
        TimeSpan t = TimeSpan.FromMinutes((double)minutes);
        return t.TotalHours >= 24
            ? string.Format("{0}d {1:D2}h:{2:D2}m", t.Days, t.Hours, t.Minutes)
            : string.Format("{0:D2}h:{1:D2}m", t.Hours, t.Minutes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="haystack"></param>
    /// <param name="needles"></param>
    /// <returns></returns>
    public static bool ContainsAny(this string haystack, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (haystack.Contains(needle))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hashParamenter"></param>
    /// <param name="addSID"></param>
    /// <returns></returns>
    public static WWWForm CreateWWWForm(FormHashParm hashParamenter = FormHashParm.ID, bool addSID = false)
    {
        WWWForm wf = new WWWForm();
        if (hashParamenter != FormHashParm.None)
        {
            string parm = "0";
            if (bl_DataBase.Instance != null && bl_DataBase.Instance.LocalUser != null)
            {
                parm = hashParamenter == FormHashParm.ID ? bl_DataBase.Instance.LocalUser.ID.ToString() : (string)bl_DataBase.Instance.LocalUser.LoginName;
            }
            if (hashParamenter == FormHashParm.Name)
            {
                wf.AddSecureField("name", parm);
            }
            string hash = Md5Sum(parm + bl_LoginProDataBase.Instance.SecretKey).ToLower();
            wf.AddSecureField("hash", hash);
        }
        if (addSID && bl_DataBase.Instance != null)
        {
            wf.AddField("sid", AnalyticsSessionInfo.sessionId.ToString());
        }
        return wf;
    }

    public static WWWForm CreateWWWForm(bool addBasicHash = true, bool addSID = false) => CreateWWWForm(addBasicHash ? FormHashParm.ID : FormHashParm.None, addSID);

    /// <summary>
    /// Simple AES Encryption
    /// </summary>
    /// <param name="encryptString"></param>
    /// <returns></returns>
    public static string Encrypt(string encryptString)
    {
        if (string.IsNullOrEmpty(encryptString))
        {
            return string.Empty;
        }
        string EncryptionKey = bl_LoginProDataBase.Instance.SecretKey;
        byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
             0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                encryptString = Convert.ToBase64String(ms.ToArray());
            }
        }
        return encryptString;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plaintext"></param>
    /// <returns></returns>
    public static string EncryptForServer(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(SHA256_SECRET_KEY))
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(bl_LoginProDataBase.Instance.SecretKey));
                byte[] key = new byte[32];
                Array.Copy(hash, 0, key, 0, 32);
                SHA256_SECRET_KEY = Convert.ToBase64String(key);
                SHA256_SECRET_KEY = SHA256_SECRET_KEY.Substring(0, 32);
            }
        }

        using (Aes encryptor = Aes.Create())
        {
            encryptor.KeySize = 256;
            encryptor.BlockSize = 128;
            encryptor.Mode = CipherMode.CBC;

            byte[] iv = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }

            encryptor.Key = Encoding.UTF8.GetBytes(SHA256_SECRET_KEY);
            encryptor.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                    cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                    cs.Close();
                }
                byte[] ciphertext = ms.ToArray();
                byte[] ivAndCiphertext = new byte[iv.Length + ciphertext.Length];
                Array.Copy(iv, ivAndCiphertext, iv.Length);
                Array.Copy(ciphertext, 0, ivAndCiphertext, iv.Length, ciphertext.Length);
                return Convert.ToBase64String(ivAndCiphertext);
            }
        }
    }


    /// <summary>
    /// Simple AES decryption
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }
        string EncryptionKey = bl_LoginProDataBase.Instance.SecretKey;
        cipherText = cipherText.Replace(" ", "+");
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static bool IsUsername(string username)
    {
        string pattern;
        // start with a letter, allow letter or number, length between 6 to 12.
        pattern = @"^[a-zA-Z0-9_ ]+$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(username);
    }

    /// <summary>
    /// Sanitize input from the client before send to the server
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string SanitazeString(string input)
    {
        input = input.Replace("|", "");
        input = input.Replace("&&", "");
        return input;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void AddSecureField(this WWWForm wf, string key, int value) => AddSecureField(wf, key, value.ToString());

    /// <summary>
    /// 
    /// </summary>
    public static void AddSecureField(this WWWForm wf, string key, string value, string prefix = "")
    {
        if (bl_LoginProDataBase.Instance.PeerToPeerEncryption && bl_DataBase.Instance != null && !string.IsNullOrEmpty(bl_DataBase.Instance.RSAPublicKey))
        {
            if (!string.IsNullOrEmpty(value) && value.Length >= 244)
            {
                string encrypt = EncryptForServer(value);
                wf.AddField(key, $"seae#{encrypt}");
            }
            else
            {
                wf.AddField(key, prefix + bl_RSA.Encrypter(value, bl_DataBase.Instance.RSAPublicKey));
            }
        }
        else
        {
            wf.AddField(key, value);
        }
    }

    /// <summary>
    /// Get the basic auth string for the given username and password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string GetBasicAuth(string username, string password)
    {
        string auth = username + ":" + password;
        auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
        return "Basic " + auth;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetUnitySessionID() => AnalyticsSessionInfo.sessionId.ToString();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsLoginBlocked()
    {
        string key = Encrypt(LOCK_TIME_KEY);
        string date = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrEmpty(date)) return false;

        date = Decrypt(date);
        var gl = new CultureInfo("en-US");
        var lockDate = DateTime.Parse(date, gl);
        var ct = lockDate.Subtract(DateTime.Now.ToUniversalTime());

        return (ct.Minutes > 0 || ct.Seconds > 0);
    }

    /// <summary>
    /// A unique device identifier. It is guaranteed to be unique for every device
    /// </summary>
    public static string DeviceIdentifier => SystemInfo.deviceUniqueIdentifier;

    public static void LoadLevel(int levelID) => SceneManager.LoadScene(levelID);
    public static void LoadLevel(string levelName) => SceneManager.LoadScene(levelName);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsNetworkError(UnityWebRequest w)
    {
#if UNITY_2020_1_OR_NEWER
        return w.result != UnityWebRequest.Result.Success && w.result != UnityWebRequest.Result.InProgress;
#else
        return w.isNetworkError || w.isHttpError;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputStr"></param>
    /// <returns></returns>
    public static string ReverseChars(string inputStr)
    {
        char[] charArray = inputStr.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Check if a flag is selected in the give flag enum property
    /// </summary>
    /// <returns></returns>
    public static bool IsEnumFlagPresent<T>(T value, T lookingForFlag) where T : Enum
    {
        int intValue = (int)(object)value;
        int intLookingForFlag = (int)(object)lookingForFlag;
        return ((intValue & intLookingForFlag) == intLookingForFlag);
    }
}