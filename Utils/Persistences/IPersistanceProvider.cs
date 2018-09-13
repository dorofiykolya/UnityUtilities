using UnityEngine;

namespace Utils.Persistences
{
  public interface IPersistanceProvider
  {
    void SetInt(string key, int value);
    int GetInt(string key, int defaultValue);
    int GetInt(string key);
    void SetFloat(string key, float value);
    float GetFloat(string key, float defaultValue);
    float GetFloat(string key);
    void SetString(string key, string value);
    string GetString(string key, string defaultValue);
    string GetString(string key);
    bool HasKey(string key);
    void DeleteKey(string key);
    void DeleteAll();
    void Save();
  }

  public class PlayerPrefsPersistanceProvider : IPersistanceProvider
  {
    public int GetInt(string key, int defaultValue)
    {
      return PlayerPrefs.GetInt(key, defaultValue);
    }

    public int GetInt(string fullPath)
    {
      return PlayerPrefs.GetInt(fullPath);
    }

    public bool HasKey(string fullPath)
    {
      return PlayerPrefs.HasKey(fullPath);
    }

    public void DeleteKey(string key)
    {
      PlayerPrefs.DeleteKey(key);
    }

    public void DeleteAll()
    {
      PlayerPrefs.DeleteAll();
    }

    public float GetFloat(string key, float defaultValue)
    {
      return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public float GetFloat(string fullPath)
    {
      return PlayerPrefs.GetFloat(fullPath);
    }

    public string GetString(string key, string defaultValue)
    {
      return PlayerPrefs.GetString(key, defaultValue);
    }

    public string GetString(string fullPath)
    {
      return PlayerPrefs.GetString(fullPath);
    }

    public void SetInt(string fullPath, int value)
    {
      PlayerPrefs.SetInt(fullPath, value);
    }

    public void SetFloat(string fullPath, float value)
    {
      PlayerPrefs.SetFloat(fullPath, value);
    }

    public void SetString(string fullPath, string value)
    {
      PlayerPrefs.SetString(fullPath, value);
    }

    public void Save()
    {
      PlayerPrefs.Save();
    }
  }
}
