using ProjectColombo.Objects;
using ProjectColombo.Objects.Charms;
using ProjectColombo.Objects.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DropManager : MonoBehaviour
{
    public List<GameObject> commonCharms;
    public List<GameObject> rareCharms;
    public List<GameObject> legendaryCharms;
    public GameObject commonCharmVFX;
    public GameObject rareCharmVFX;
    public GameObject legendaryCharmVFX;
    public GameObject pickup;
    public GameObject coins;

    public void DropCharm(BaseCharm charmInfo, Vector3 position)
    {
        string name = charmInfo.charmName;
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        foreach (GameObject c in commonCharms)
        {
            if (name == c.GetComponent<BaseCharm>().charmName)
            {
                GameObject instance = Instantiate(pickup, position, rotation);
                instance.GetComponent<PickUp>().SetCharm(c);
                Instantiate(commonCharmVFX, instance.transform);
                return;
            }
        }

        foreach (GameObject c in rareCharms)
        {
            if (name == c.GetComponent<BaseCharm>().charmName)
            {
                GameObject instance = Instantiate(pickup, position, rotation);
                instance.GetComponent<PickUp>().SetCharm(c);
                Instantiate(rareCharmVFX, instance.transform);
                return;
            }
        }

        foreach (GameObject c in legendaryCharms)
        {
            if (name == c.GetComponent<BaseCharm>().charmName)
            {
                GameObject instance = Instantiate(pickup, position, rotation);
                instance.GetComponent<PickUp>().SetCharm(c);
                Instantiate(legendaryCharmVFX, instance.transform);
                return;
            }
        }
    }

    public void DropRandomCommonCharm(Vector3 position)
    {
        int rand = Random.Range(0, commonCharms.Count);
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject instance = Instantiate(pickup, position, rotation);
        instance.GetComponent<PickUp>().SetCharm(commonCharms[rand]);
        Instantiate(commonCharmVFX, instance.transform);
    }

    public void DropRandomRareCharm(Vector3 position)
    {
        int rand = Random.Range(0, rareCharms.Count);
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject instance = Instantiate(pickup, position, rotation);
        instance.GetComponent<PickUp>().SetCharm(rareCharms[rand]);
        Instantiate(rareCharmVFX, instance.transform);
    }

    public void DropRandomLegendaryCharm(Vector3 position)
    {
        int rand = Random.Range(0, legendaryCharms.Count);
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject instance = Instantiate(pickup, position, rotation);
        instance.GetComponent<PickUp>().SetCharm(legendaryCharms[rand]);
        Instantiate(legendaryCharmVFX, instance.transform);
    }

    public void DropCoins(int amount, Vector3 position)
    {
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject coininstance = Instantiate(coins, position, rotation);
        coininstance.GetComponent<Coins>().amount = amount;
    }
}
