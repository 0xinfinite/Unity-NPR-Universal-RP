using UnityEngine;

[ExecuteInEditMode()]
public class MainCharacter : MonoBehaviour
{

    private void OnEnable()
    {
        MainCharacterManager.manager?.mainCharacterList.Add(this);
    }

    private void OnDisable()
    {
        MainCharacterManager.manager?.mainCharacterList.Remove(this);
    }
}
