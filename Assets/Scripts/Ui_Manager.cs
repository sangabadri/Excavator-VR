using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;

public class Ui_Manager : MonoBehaviour
{
    public GameObject menuUI;

    public List<Button> buttons;
    public List<string> scenes;
    public List<GameObject> ticks;
    public Button exitButton;

    public GameObject lockedUI;


    public VideoPlayer videoPlayer;
    public GameObject videoPlayerUI;

    public List<Button> tutorialButtons;
    public List<VideoClip> tutorialVideos;

    public VideoClip loadingVideo;

    bool playingVideo = false;
    bool isPaused = false;
    bool thumbstickUsed = false; // prevents holding from repeating

    void Start()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;

            buttons[i].onClick.AddListener(() => LoadScene(index));
        }

        for(int i=0;i< ticks.Count;i++)
        {
            ticks[i].SetActive(GameManager.instance.levelCompleted[i]);
        }

        exitButton.onClick.AddListener(() => ExitGame());

        for(int i = 0; i < tutorialButtons.Count; i++)
        {
            int index = i;
            tutorialButtons[i].onClick.AddListener(() => PlayTutorial(index));
        }
        bool allCompleted = true;
        for (int i = 0; i < GameManager.instance.levelCompleted.Count; i++)
        {
            if (!GameManager.instance.levelCompleted[i])
            {
                allCompleted = false;
                break;
            }
        }
        if (allCompleted)
        {
            lockedUI.SetActive(false);
            buttons[3].enabled = true;
            buttons[3].transform.GetChild(0).GetComponent<Image>().color = Color.white; // Assuming the text is the first child and has an Image component for color
        }
        else
        {
            lockedUI.SetActive(true);
            buttons[3].enabled = false;
        }
    }

    void LoadScene(int index)
    {
        StartCoroutine(PlayLoadingVideoAndLoad(index));
    }

    void ExitGame()
    {
        Application.Quit();
    }

    void PlayTutorial(int index)
    {
        videoPlayerUI.SetActive(true);
        menuUI.SetActive(false);
        playingVideo = true;
        videoPlayer.clip = tutorialVideos[index];
        videoPlayer.Play();
    }


    void Update()
    {
        if (playingVideo)
        {
            var leftVR = XRController.leftHand;
            bool seekForward = false;
            bool seekBackward = false;
            bool togglePause = false;
            bool closeVideo = false;

            if (leftVR != null)
            {
                // Thumbstick seek - one step per move
                var thumbstick = leftVR.TryGetChildControl<Vector2Control>("thumbstick");
                if (thumbstick != null)
                {
                    float x = thumbstick.ReadValue().x;
                    if (x > 0.5f && !thumbstickUsed) { seekForward = true; thumbstickUsed = true; }
                    else if (x < -0.5f && !thumbstickUsed) { seekBackward = true; thumbstickUsed = true; }
                    else if (x >= -0.5f && x <= 0.5f) { thumbstickUsed = false; } // reset when returned to center
                }

                // Trigger - pause/play
                var trigger = leftVR.TryGetChildControl<ButtonControl>("triggerPressed");
                if (trigger != null && trigger.wasPressedThisFrame) togglePause = true;

                // Menu button - close video
                var menuBtn = leftVR.TryGetChildControl<ButtonControl>("menu");
                if (menuBtn != null && menuBtn.wasPressedThisFrame) closeVideo = true;
            }
            else
            {
                // Keyboard fallback when no VR
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.rightArrowKey.wasPressedThisFrame) seekForward = true;
                    if (Keyboard.current.leftArrowKey.wasPressedThisFrame) seekBackward = true;
                    if (Keyboard.current.spaceKey.wasPressedThisFrame) togglePause = true;
                    if (Keyboard.current.eKey.wasPressedThisFrame) closeVideo = true;
                }
            }

            // Execute actions
            if (seekForward) videoPlayer.time = Mathf.Min((float)videoPlayer.time + 10f, (float)videoPlayer.length);
            if (seekBackward) videoPlayer.time = Mathf.Max((float)videoPlayer.time - 10f, 0f);
            if (togglePause)
            {
                if (isPaused) { videoPlayer.Play(); isPaused = false; }
                else { videoPlayer.Pause(); isPaused = true; }
            }
            if (closeVideo)
            {
                videoPlayer.Stop();
                videoPlayerUI.SetActive(false);
                menuUI.SetActive(true);
                playingVideo = false;
                isPaused = false;
            }
        }
    }

    IEnumerator PlayLoadingVideoAndLoad(int index)
    {
        videoPlayerUI.SetActive(true);
        menuUI.SetActive(false);
        videoPlayerUI.transform.GetChild(0).gameObject.SetActive(false); // Hide tutorial controls during loading


        videoPlayer.clip = loadingVideo;

        videoPlayer.Play();

        yield return new WaitUntil(() => videoPlayer.isPlaying);

        yield return new WaitWhile(() => videoPlayer.isPlaying);


        SceneManager.LoadScene(scenes[index]);
    }

}