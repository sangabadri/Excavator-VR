using GLTFast.Schema;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UI_Interaction : MonoBehaviour
{
    public static UI_Interaction Instance { get; private set; }


    public GameObject tutorialUI;

    [Header("Inside Camera UI")]

    public GameObject insideMenuUI;
    public GameObject insideCamControlsUI;

    public Button insideResumeButton;
    public Button insideMainControlsButton;
    public Button insideCamControlsButton;
    public Button insideExitButton;
    public GameObject insideLeftController;


    [Header("Outside Camera UI")]

    public GameObject outsideMenuUI;
    public GameObject outsideCamControlsUI;

    public Button outsideResumeButton;
    public Button outsideMainControlsButton;
    public Button outsideCamControlsButton;
    public Button outsideExitButton;
    public GameObject outsideLeftController;


    bool isMenuOpen = false;
    public bool isTutorialOpen = true;
    bool isCamControlsOpen = false;
    public bool isInsideCameraActive = true;
    ExcavatorController controller;

    [Header("Cameras")]
    public GameObject insideXRCamera;
    public GameObject outsideXRCamera;

    public GameObject taskInfo;
    public GameObject exitNotification;

    bool isFirstTime = true;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        controller = transform.root.GetComponent<ExcavatorController>();

        insideResumeButton.onClick.AddListener(ResumeGame);
        insideMainControlsButton.onClick.AddListener(OpenTutorial);
        insideCamControlsButton.onClick.AddListener(OpenCamController);
        insideExitButton.onClick.AddListener(ExitGame);

        insideMenuUI.SetActive(false);
        insideCamControlsUI.SetActive(false);
        tutorialUI.SetActive(isTutorialOpen);

        insideLeftController.SetActive(false);

        outsideResumeButton.onClick.AddListener(ResumeGame);
        outsideMainControlsButton.onClick.AddListener(OpenTutorial);
        outsideCamControlsButton.onClick.AddListener(OpenCamController);
        outsideExitButton.onClick.AddListener(ExitGame);

        outsideMenuUI.SetActive(false);
        outsideCamControlsUI.SetActive(false);

        outsideLeftController.SetActive(false);

        isInsideCameraActive = false;
        ToggleCamera();
    }

    void Update()
    {
        bool menuPressedThisFrame = false;

        // 1. Keyboard Fallback (Escape Key)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            menuPressedThisFrame = true;
        }

        // 2. Gamepad Menu/Start Button
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            menuPressedThisFrame = true;
        }

        // 3. VR Left Controller Menu Button
        var leftVR = XRController.leftHand;
        if (leftVR != null)
        {
            var leftMenuBtn = leftVR.TryGetChildControl<ButtonControl>("menu");
            if (leftMenuBtn != null && leftMenuBtn.wasPressedThisFrame)
            {
                menuPressedThisFrame = true;
            }
        }

        // Execute the UI Logic if the menu button was pressed
        if (menuPressedThisFrame)
        {
            if (isTutorialOpen)
            {
                tutorialUI.SetActive(false);
                isTutorialOpen = false;
                if (isFirstTime)
                {
                    isFirstTime = false;
                    StartCoroutine(PLayInfoText());
                }
            }
            else if (isCamControlsOpen)
            {
                if (isInsideCameraActive)
                {
                    insideCamControlsUI.SetActive(false);
                }
                else
                {
                    outsideCamControlsUI.SetActive(false);
                }
                isCamControlsOpen = false;
            }
            else if (isMenuOpen)
            {
                if (isInsideCameraActive)
                {
                    insideMenuUI.SetActive(false); // Hide the menu
                    insideLeftController.SetActive(false);
                }
                else
                {
                    outsideMenuUI.SetActive(false); // Hide the menu
                    outsideLeftController.SetActive(false);
                }
                isMenuOpen = false;
                Cursor.lockState = CursorLockMode.Locked;
                controller.enabled = true; // Re-enable the controller
            }
            else if (!isMenuOpen)
            {
                if (isInsideCameraActive)
                {
                    insideMenuUI.SetActive(true); // Show the inside menu
                    insideLeftController.SetActive(true);
                }
                else
                {
                    outsideMenuUI.SetActive(true); // Show the outside menu
                    outsideLeftController.SetActive(true);
                }
                isMenuOpen = true;
                Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                controller.enabled = false;
            }
        }

        var rightVR = XRController.rightHand;
        bool toggleCamera = Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame;

        if (rightVR != null)
        {
            var btn = rightVR.TryGetChildControl<ButtonControl>("triggerPressed");
            if (btn != null && btn.wasPressedThisFrame) toggleCamera = true;
        }

        if (toggleCamera && !isMenuOpen && !isTutorialOpen && !isCamControlsOpen)
            ToggleCamera();
    }
    void ResumeGame()
    {
        if (isInsideCameraActive)
        {
            insideMenuUI.SetActive(false); // Hide the inside menu
            insideLeftController.SetActive(false);
            controller.enabled = true;
        }
        else
        {
            outsideMenuUI.SetActive(false); // Hide the outside menu
            outsideLeftController.SetActive(false);
        }
        isMenuOpen = false;
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
    }

    void OpenTutorial()
    {
        isTutorialOpen = true;
        isMenuOpen = false;
        tutorialUI.SetActive(true); // Show the tutorial UI
        if (isInsideCameraActive)
        {
            insideMenuUI.SetActive(false);
            insideLeftController.SetActive(false);
            controller.enabled = true;
        }
        else
        {
            outsideMenuUI.SetActive(false);
            outsideLeftController.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OpenCamController()
    {
        if (isInsideCameraActive)
        {
            insideMenuUI.SetActive(false);
            insideCamControlsUI.SetActive(true);
            insideLeftController.SetActive(false);
            controller.enabled = true;
        }
        else
        {
            outsideMenuUI.SetActive(false);
            outsideCamControlsUI.SetActive(true);
            outsideLeftController.SetActive(false);
        }
        isCamControlsOpen = true;
        isMenuOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void ExitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void ToggleCamera()
    {
        isInsideCameraActive = !isInsideCameraActive;
        insideXRCamera.SetActive(isInsideCameraActive);
        outsideXRCamera.SetActive(!isInsideCameraActive);
        controller.enabled = isInsideCameraActive;
    }

    IEnumerator  PLayInfoText()
    {
        if(taskInfo != null)
        {
            taskInfo.SetActive(true);
            yield return new WaitForSeconds(5f);
            taskInfo.SetActive(false);

        }
    }

    public IEnumerator PlayExitText()
    {
        exitNotification.SetActive(true);
        yield return new WaitForSeconds(5f);
        exitNotification.SetActive(false);
        SceneManager.LoadScene("MainMenu");
        Cursor.lockState = CursorLockMode.None; // Unlocks the cursor
        Cursor.visible = true;
    }
}
