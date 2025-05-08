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

        foreach (GameObject c in commonCharms)
        {
            if (name == c.GetComponent<BaseCharm>().charmName)
            {
                GameObject instance = Instantiate(pickup, position, transform.rotation);
                instance.GetComponent<PickUp>().SetCharm(c);
                Instantiate(commonCharmVFX, instance.transform);
                return;
            }
        }

        foreach (GameObject c in rareCharms)
        {
            if (name == c.GetComponent<BaseCharm>().charmName)
            {
                GameObject instance = Instantiate(pickup, position, transform.rotation);
                instance.GetComponent<PickUp>().SetCharm(c);
                Instantiate(rareCharmVFX, instance.transform);
                return;
            }
        }

        foreach (GameObject c in legendaryCharms)
        {
            if (name == c.GetComponent<BaseCharm>().charmName)
            {
                GameObject instance = Instantiate(pickup, position, transform.rotation);
                instance.GetComponent<PickUp>().SetCharm(c);
                Instantiate(legendaryCharmVFX, instance.transform);
                return;
            }
        }
    }

    public void DropRandomCommonCharm(Vector3 position)
    {
        int rand = Random.Range(0, commonCharms.Count);

        GameObject instance = Instantiate(pickup, position, transform.rotation);
        instance.GetComponent<PickUp>().SetCharm(commonCharms[rand]);
        Instantiate(commonCharmVFX, instance.transform);
    }

    public void DropRandomRareCharm(Vector3 position)
    {
        int rand = Random.Range(0, rareCharms.Count);

        GameObject instance = Instantiate(pickup, position, transform.rotation);
        instance.GetComponent<PickUp>().SetCharm(rareCharms[rand]);
        Instantiate(rareCharmVFX, instance.transform);
    }

    public void DropRandomLegendaryCharm(Vector3 position)
    {
        int rand = Random.Range(0, legendaryCharms.Count);

        GameObject instance = Instantiate(pickup, position, transform.rotation);
        instance.GetComponent<PickUp>().SetCharm(legendaryCharms[rand]);
        Instantiate(legendaryCharmVFX, instance.transform);
    }

    public void DropCoins(int amount, Vector3 position)
    {
        GameObject coininstance = Instantiate(coins, position, transform.rotation);
        coininstance.GetComponent<Coins>().amount = amount;
    }
}
