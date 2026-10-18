using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

namespace ElementalAnomaly.Prototype
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(PrototypePlayerHealth), typeof(PrototypePlayerController))]
    public sealed class PrototypePlayerFailure : MonoBehaviour
    {
        public GameObject failureScreen;
        public TMP_Text failureReason;
        public UnityEngine.UI.Button restartButton;
        public GameObject[] gameplayHudRoots;
        public PrototypeCinemachineExploration cameraInput;
        public PrototypeReactorCinematic reactorCinematic;
        public float fallHeight = -8f;

        [Header("Stage Completion")]
        public GameObject completionScreen;
        public CanvasGroup completionFade;
        public CanvasGroup completionPanel;
        public TMP_Text completionMessage;
        public UnityEngine.UI.Button replayButton;
        public UnityEngine.UI.Button quitButton;
        [Min(0.1f)] public float completionFadeSeconds = .75f;

        public bool HasFailed { get; private set; }
        public bool HasCompleted { get; private set; }
        private PrototypePlayerHealth health;
        private PrototypePlayerController player;
        private PrototypeFireCombat combat;
        private PrototypeMeleeWeapon melee;
        private PrototypeFireInteractor interactor;
        private PrototypeBurnBright burnBright;
        private string scenePath;
        private bool fell, restarting, globalsChanged;
        private float previousTimeScale;
        private bool previousAudioPause, previousCursorVisible;
        private CursorLockMode previousCursorLock;
        private bool completionReady;

        private void Awake()
        {
            health = GetComponent<PrototypePlayerHealth>();
            player = GetComponent<PrototypePlayerController>();
            combat = GetComponent<PrototypeFireCombat>();
            melee = GetComponent<PrototypeMeleeWeapon>();
            interactor = GetComponent<PrototypeFireInteractor>();
            burnBright = GetComponent<PrototypeBurnBright>();
            scenePath = gameObject.scene.path;
            if (failureScreen != null) failureScreen.SetActive(false);
            if (completionScreen != null) completionScreen.SetActive(false);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (replayButton != null) replayButton.onClick.AddListener(Restart);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
        }

        private void OnEnable()
        {
            if (health != null) health.Died += ShowFailure;
        }

        private void Update()
        {
            if (HasCompleted && !completionReady) return;
            if (!HasFailed && !HasCompleted)
            {
                // Polling also handles an already-depleted player if this component is enabled later.
                if (health.CurrentHealth <= 0) ShowFailure();
                else if (transform.position.y < fallHeight)
                {
                    fell = true;
                    health.TakeDamage(health.CurrentHealth);
                }
                return;
            }
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.rKey.wasPressedThisFrame || (!HasCompleted &&
                (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)))) Restart();
            if (HasCompleted && keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Quit();
        }

        private void ShowFailure()
        {
            if (HasFailed || HasCompleted) return;
            HasFailed = true;
            FreezeGameplay();
            if (failureReason != null)
                failureReason.text = fell ? "You fell beyond the facility.\nRestart from Observation C-01."
                    : "Your health has been depleted.\nRestart from Observation C-01.";
            if (failureScreen != null) failureScreen.SetActive(true);
        }

        public bool CompleteStage()
        {
            if (HasFailed || HasCompleted || health.CurrentHealth <= 0 || completionScreen == null ||
                completionFade == null || completionPanel == null) return false;
            HasCompleted = true;
            FreezeGameplay();
            completionScreen.SetActive(true);
            completionFade.alpha = 0f;
            completionFade.blocksRaycasts = true;
            completionPanel.gameObject.SetActive(false);
            if (replayButton != null) replayButton.interactable = false;
            if (quitButton != null) quitButton.interactable = false;
            StartCoroutine(CompletionSequence());
            return true;
        }

        private System.Collections.IEnumerator CompletionSequence()
        {
            float elapsed = 0f;
            while (elapsed < completionFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                completionFade.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / completionFadeSeconds));
                yield return null;
            }
            completionFade.alpha = 1f;
            completionPanel.alpha = 0f;
            completionPanel.gameObject.SetActive(true);
            elapsed = 0f;
            while (elapsed < .3f)
            {
                elapsed += Time.unscaledDeltaTime;
                completionPanel.alpha = Mathf.Clamp01(elapsed / .3f);
                yield return null;
            }
            completionPanel.alpha = 1f;
            completionReady = true;
            if (replayButton != null) replayButton.interactable = true;
            if (quitButton != null) quitButton.interactable = true;
            if (UnityEngine.EventSystems.EventSystem.current != null && replayButton != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(replayButton.gameObject);
        }

        private void FreezeGameplay()
        {
            // Finish/cancel any cinematic before taking permanent ownership of the input lock.
            if (reactorCinematic != null && reactorCinematic.IsPlaying) reactorCinematic.enabled = false;
            player.SetInputLocked(true);
            player.enabled = false;
            if (combat != null) combat.enabled = false;
            if (melee != null) melee.enabled = false;
            if (interactor != null) interactor.enabled = false;
            if (burnBright != null) burnBright.enabled = false;
            if (cameraInput != null) cameraInput.enabled = false;
            if (gameplayHudRoots != null)
                foreach (var hud in gameplayHudRoots)
                    if (hud != null) hud.SetActive(false);

            previousTimeScale = Time.timeScale;
            previousAudioPause = AudioListener.pause;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            globalsChanged = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Restart()
        {
            if ((!HasFailed && !HasCompleted) || (HasCompleted && !completionReady) || restarting) return;
            restarting = true;
            if (restartButton != null) restartButton.interactable = false;
            if (replayButton != null) replayButton.interactable = false;
            if (quitButton != null) quitButton.interactable = false;
            TMP_Text message = HasCompleted ? completionMessage : failureReason;
            if (message != null) message.text = "Restarting...";
            try
            {
                // Reloading resets every scene-owned system, including consumed/deactivated objects.
                var operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
                if (operation == null) throw new System.InvalidOperationException("Scene reload could not start.");
            }
            catch (System.Exception exception)
            {
                restarting = false;
                if (restartButton != null) restartButton.interactable = true;
                if (replayButton != null) replayButton.interactable = true;
                if (quitButton != null) quitButton.interactable = true;
                if (message != null) message.text = "Restart could not load the scene. Please try again.";
                Debug.LogException(exception, this);
            }
        }

        public void Quit()
        {
            if (!HasCompleted || !completionReady || restarting) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RestoreGlobals()
        {
            if (!globalsChanged) return;
            globalsChanged = false;
            Time.timeScale = previousTimeScale;
            AudioListener.pause = previousAudioPause;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }

        private void OnDisable()
        {
            if (health != null) health.Died -= ShowFailure;
            RestoreGlobals();
        }

        private void OnDestroy()
        {
            if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
            if (replayButton != null) replayButton.onClick.RemoveListener(Restart);
            if (quitButton != null) quitButton.onClick.RemoveListener(Quit);
            RestoreGlobals();
        }
    }
}
