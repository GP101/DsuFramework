using UnityEngine;
using Dsu.Framework;

public class NewRuntimeGameDataManager : RuntimeGameDataManagerBase
{
    static public NewRuntimeGameDataManager instance = null;

    private int count;

    // TODO: Add your custom fields and methods here
    //private static int count = 0;
    public int Count
    {
        get { return count; }
        set { _UpdateDataStamp(); count = value; }
    }

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
        // Initialize your variables here
        count = 0;
    }
    public void AddCount(int value)
    {
        count += value;
        _UpdateDataStamp();
    }
}
