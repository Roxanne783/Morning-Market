using UnityEngine;
using UnityEngine.Video;

public class OilCakeUIController : MonoBehaviour
{
    public GameObject infoCard;
    public VideoPlayer videoPlayer;

    public void ToggleInfoCard()
    {
        bool willOpen = !infoCard.activeSelf;

        infoCard.SetActive(willOpen);

        if (!willOpen)
        {
            videoPlayer.Stop();
            videoPlayer.frame = 0;
        }
    }

    public void PlayInterviewVideo()
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }
}
