using UnityEngine;

public class MarketUIController : MonoBehaviour
{
    public GameObject infoCard;


    public void OpenInfoCard()
    {
        infoCard.SetActive(true);
    }


    public void CloseInfoCard()
    {
        infoCard.SetActive(false);
    }
}