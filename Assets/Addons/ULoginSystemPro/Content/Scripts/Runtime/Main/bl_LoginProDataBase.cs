using MFPS.ULogin;
using System;
using System.Collections.Generic;
using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_LoginProDataBase : ScriptableObject
{
    public const string ObjectName = "LoginDataBasePro";

    [Header("Host Path")]
    [Tooltip("The Url of folder where your php scripts are located in your host.")]

#if !ACTK_IS_HERE
    public string PhpHostPath;
    public string SecretKey = "123456";
#else
    public ObscuredString PhpHostPath;
    public ObscuredString SecretKey = "123456";
#endif
    public string OnLoginLoadLevel = "NextLevelName";

    [Header("Settings")]
    [LovattoToogle] public bool CheckGameVersion = true;
    [LovattoToogle] public bool PeerToPeerEncryption = false;
    [LovattoToogle] public bool ForceLoginScene = true;
    [LovattoToogle] public bool allowPlayAsGuest = true;
    [LovattoToogle] public bool DetectBan = true;
    [LovattoToogle] public bool RequiredEmailVerification = true;
    [LovattoToogle] public bool usePhotonAuthentication = false;
    [LovattoToogle] public bool CanRegisterSameEmail = false;
    [LovattoToogle] public bool checkInternetConnection = true;
    [Tooltip("Check if the player has been banned in runtime?")]
    [LovattoToogle] public bool BanComprobationInMid = true; //keep checking ban status each certain time
    [LovattoToogle] public bool PlayerCanChangeNick = true; // can players change their nick name?
    [LovattoToogle] public bool UpdateIP = true;
    [Tooltip("Check that the user name doesn't contain a bad word from the black word list.")]
    [LovattoToogle] public bool FilterUserNames = true;
    [LovattoToogle] public bool showStatusPrefix = true;
    [LovattoToogle] public bool FullLogs = false;
    public int maxNickNameLenght = 16;
    [Range(3, 12)] public int MinPasswordLenght = 5;
    [Tooltip("Set 0 for unlimited attempts")]
    [Range(0, 12)] public int maxLoginAttempts = 5;
    [Tooltip("In seconds")]
    [Range(30, 3000)] public int waitTimeAfterFailAttempts = 300;
    [Range(10, 300)] public int CheckBanEach = 10;
    public ULoginBanMethod banMethod = ULoginBanMethod.IP;
    public AfterLoginBehave afterLoginBehave = AfterLoginBehave.ShowAccountResume;
    public RememberMeBehave rememberMeBehave = RememberMeBehave.RememberSession;

    public List<ULoginAccountRole> roles;

    [Header("Script Names")]
    public List<PHPClassName> pHPClassNames;

    public readonly string[] UserNameFilters = new string[] { "fuck", "fucker", "motherfucker", "nigga", "nigger", "porn", "pussy", "cock", "anus", "racist", "vih", "puto", "fagot", "shit", "bullshit", "gay", "sex", "nazi", "bitch" };


    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultName"></param>
    /// <returns></returns>
    public static string GetPHPClassName(string key, string defaultName = "")
    {
        var names = Instance.pHPClassNames;
        int index = names.FindIndex(x => x.Key == key);
        if (index == -1) return defaultName;

        return names[index].ScriptName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classKey"></param>
    /// <param name="defaultClassName"></param>
    /// <returns></returns>
    public static string GetUrl(string classKey, string defaultClassName = "")
    {
        string className = GetPHPClassName(classKey, defaultClassName);
        if (!Instance.PhpHostPath.EndsWith("/")) { Instance.PhpHostPath += "/"; }
        string url = string.Format("{0}{1}.php", Instance.PhpHostPath, className);
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) { Debug.Log("URL is not well formed, please check if your php script have the same name and have assign the host path."); }
        return url;
    }

    /// <summary>
    /// Return the url for the give endpoint
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public static string GetAPIEndpoint(string endpoint)
    {
        return Instance.PhpHostPath + endpoint;
    }

    /// <summary>
    /// Return the basic token without extra parameters
    /// </summary>
    /// <returns></returns>
    public static string GetAPIToken()
    {
        return bl_DataBaseUtils.Md5Sum(Instance.SecretKey).ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    public string GetPhpFolder
    {
        get
        {
            string folder = PhpHostPath;
            if (!folder.EndsWith("/")) { folder += "/"; }
            return folder;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int CanRegisterSameEmailInt()
    {
        return (CanRegisterSameEmail == true) ? 1 : 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int RequiereVerification()
    {
        return (RequiredEmailVerification == true) ? 0 : 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public bool FilterName(string userName)
    {
        userName = userName.ToLower();
        for (int i = 0; i < UserNameFilters.Length; i++)
        {
            if (userName.Contains(UserNameFilters[i].ToLower()))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string RememberCredentials
    {
        get
        {
            try
            {
                string data = PlayerPrefs.GetString(GetRememberMeKey(), string.Empty);
                data = bl_DataBaseUtils.Decrypt(data);
                return data;
            }
            catch
            {
                DeleteRememberCredentials();
                return string.Empty;
            }
        }
        set
        {
            string data = bl_DataBaseUtils.Encrypt(value);
            PlayerPrefs.SetString(GetRememberMeKey(), data);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string GetRememberMeKey()
    {
        return $"{Application.productName}.login.remember";
    }

    /// <summary>
    /// 
    /// </summary>
    public void DeleteRememberCredentials()
    {
        PlayerPrefs.DeleteKey(GetRememberMeKey());
    }

    /// <summary>
    /// 
    /// </summary>
    public static void SignOut()
    {
        if (!bl_DataBase.IsUserLogged) return;

        Instance.DeleteRememberCredentials();
        bl_DataBase.Instance.LocalUser = new LoginUserInfo();
        bl_DataBaseUtils.LoadLevel("Login");
    }

    /// <summary>
    /// Does the ban uses the device unique ID?
    /// </summary>
    /// <returns></returns>
    public static bool UseDeviceId()
    {
        return Instance.banMethod == ULoginBanMethod.DeviceID || Instance.banMethod == ULoginBanMethod.Both;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public static ULoginAccountRole GetRole(string roleName)
    {
        int id = Instance.roles.FindIndex(x => x.RoleName == roleName);
        if (id <= -1)
        {
            Debug.LogWarning($"Role {roleName} doesn't exist in the database, please check the roles in the ULogin database and in the configuration.");
            return null;
        }
        return GetRole(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public static ULoginAccountRole GetRole(int roleId)
    {
        return Instance.roles[roleId];
    }

    private static bl_LoginProDataBase _dataBase;
    public static bl_LoginProDataBase Instance
    {
        get
        {
            if (_dataBase == null) { _dataBase = Resources.Load("LoginDataBasePro", typeof(bl_LoginProDataBase)) as bl_LoginProDataBase; }
            return _dataBase;
        }
    }

    [Serializable]
    public class PHPClassName
    {
        public string Key;
        public string ScriptName;
    }

    [Serializable]
    public enum AfterLoginBehave
    {
        ShowAccountResume = 0,
        LoadNextScene
    }
}