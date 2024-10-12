using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GobbleGumLoadout", menuName = "Zombies/GobbleGumLoadout", order = 1)]
public class bl_GobbleGumLoadout : ScriptableObject
{
    public bl_GobbleGumSingle[] gobblegums;

    public IEnumerator ActivateGobbleGum(int ID)
    {
        if (ID > gobblegums.Length)
        {
            Debug.Log("Cant Spawn a Gobble Gum thats not in the ID list");
            yield return null;
        }

        gobblegums[ID].ActivateEffect();
        if (gobblegums[ID].Duration > 0)
        {
            yield return new WaitForSeconds(gobblegums[ID].Duration);
            gobblegums[ID].DisambleEffect();
        }
        else
        {
            yield return null;
        }
    }


    private static bl_GobbleGumLoadout m_instance;
    public static bl_GobbleGumLoadout Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = Resources.Load("GameData", typeof(bl_GobbleGumLoadout)) as bl_GobbleGumLoadout;
            }
            return m_instance;
        }
    }

}
