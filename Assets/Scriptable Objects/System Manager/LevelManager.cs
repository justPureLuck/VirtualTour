using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Slider progressBar;
    public GameObject transitionsContainer;

    private SceneTransition[] transitions;
    private AudioSource audioSource;
    private Canvas canvas;

    // Add a list of sound effects, which can be assigned in the Unity Inspector
    public AudioClip[] soundEffects;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Setup AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();

            // Subscribe to the sceneLoaded event to update the canvas camera after each scene loads
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
        Debug.Log(transitions.Length);
        Debug.Log(transitions.ToList());
        canvas = GetComponent<Canvas>();
    }

    // Update the render camera for the canvas when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera mainCamera = Camera.main; // Automatically find the new Main Camera in the scene
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera; // Set the new Main Camera as the canvas' Render Camera
            }
            else
            {
                Debug.LogWarning("Main Camera not found in scene: " + scene.name);
            }
        }
    }

    public void LoadScene(string sceneName, string transitionName, string soundEffectName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName, soundEffectName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName, string soundEffectName)
    {
        bool noTransition = false;
        if (transitionName == "none")
        {
            noTransition = true;
            canvas.enabled = false;
            transitionName = "CrossFade";
        }
        SceneTransition transition = transitions.First(t => t.name.Equals(transitionName));

        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        yield return transition.AnimateTransitionIn();

        do
        {
            progressBar.value = scene.progress;
            yield return null;
        } while (scene.progress < 0.9f);

        // Play sound effect (if not "none")
        PlaySoundEffect(soundEffectName);

        yield return new WaitForSeconds(2f);

        scene.allowSceneActivation = true;

        yield return transition.AnimateTransitionOut();

        if (noTransition)
        {
            canvas.enabled = true;
        }
    }

    // Play the selected sound effect if it's not "none"
    private void PlaySoundEffect(string soundEffectName)
    {
        if (soundEffectName != "none")
        {
            // Find the sound effect from the array
            AudioClip soundEffect = soundEffects.FirstOrDefault(s => s.name.Equals(soundEffectName));
            if (soundEffect != null)
            {
                audioSource.clip = soundEffect;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("Sound effect not found: " + soundEffectName);
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when this object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
