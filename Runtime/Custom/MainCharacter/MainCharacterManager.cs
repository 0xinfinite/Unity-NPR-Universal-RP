using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class MainCharacterManager : MonoBehaviour
{
    public static MainCharacterManager manager;

    public List<MainCharacter> mainCharacterList;

    public Vector3 offset;

    // Start is called before the first frame update
    void Awake()
    {
        if(manager == null)
        {
            manager = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        mainCharacterList = new List<MainCharacter>();
    }

    private void OnDestroy()
    {
        if(manager == this)
        manager = null;
    }
}
