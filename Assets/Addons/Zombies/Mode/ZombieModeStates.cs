namespace ZombieEvents
{
    #region Enum
    /// <summary>
    /// Game Mode State
    /// </summary>
    [System.Serializable]
    public enum RoundState
    {
        Starting = 0,
        Playing = 1,
        Waiting = 2,
        Finish = 3,
    }

    /// <summary>
    /// Game Mode Event Type
    /// </summary>
    [System.Serializable]
    public enum RoundEventTypes
    {
        Preparation = 0,
        WaveStart = 1,
        PlayerDeath = 2,
        SyncData = 3,
        GameOver = 4,
        ZombieDeath = 5,
    }
    #endregion

}