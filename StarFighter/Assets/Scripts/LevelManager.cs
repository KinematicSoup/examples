using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Responsible for managing scenes.
 */
public static class LevelManager
{
    private static readonly string MAIN_MENU = "title";
    private static readonly string ARENA = "arena";

    private static AsyncOperation m_async = null;

    public enum Level
    {
        MAIN_MENU,
        ARENA
    }

    public static bool IsLoading()
    {
        return !(m_async == null || m_async.isDone);
    }

    public static float GetLoadProgress()
    {
        if (m_async != null)
        {
            return m_async.progress;
        }
        return -1;
    }

    public static bool IsLoadFinished()
    {
        if (m_async != null)
        {
            return m_async.isDone;
        }
        return true;
    }

    public static void AllowActivation()
    {
        if (m_async != null)
        {
            m_async.allowSceneActivation = true;
        }
    }

    public static void Load(Level level)
    {
        SceneManager.LoadScene(LevelToName(level));
    }

    public static void LoadAsync(Level level)
    {
        m_async = SceneManager.LoadSceneAsync(LevelToName(level));
        m_async.allowSceneActivation = false;
    }

    private static string LevelToName(Level level)
    {
        switch (level)
        {
            case Level.MAIN_MENU: return MAIN_MENU;
            case Level.ARENA: return ARENA;
        }
        return null;
    }
}
