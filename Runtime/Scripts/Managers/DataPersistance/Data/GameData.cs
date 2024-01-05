
[System.Serializable]
public class GameData
{
    #region Settings Data
    public int resolutionIndex;
    public bool isFullscreen;
    public bool isVSync;
    public int qualityIndex;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;
    #endregion

    public GameData()
    {
        #region Settings Data
        isFullscreen = true;
        isVSync = true;
        resolutionIndex = -1;
        qualityIndex = 4;
        masterVolume = 1f;
        musicVolume = 1f;
        soundVolume = 1f;
        #endregion
    }
}
