using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("References From This Scene")]

    [Tooltip(
        "The parent containing this scene's environment, UI, " +
        "teleport areas and other scene content."
    )]
    [SerializeField]
    private GameObject sceneContentRoot;

    [Tooltip(
        "The complete XR System belonging to this scene."
    )]
    [SerializeField]
    private GameObject xrSystemRoot;

    [Tooltip(
        "The Input Action Asset used by this scene's XR rig, " +
        "normally XRI Default Input Actions."
    )]
    [SerializeField]
    private InputActionAsset inputActions;

    private static SceneLoader originalLoader;
    private static SceneLoader secondLoader;

    private static Scene originalScene;
    private static Scene secondScene;

    private static bool originalContentWasActive;
    private static bool originalXRWasActive;

    private static bool secondSceneIsOpen;
    private static bool transitionInProgress;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetStaticState()
    {
        originalLoader = null;
        secondLoader = null;

        originalScene = default(Scene);
        secondScene = default(Scene);

        originalContentWasActive = false;
        originalXRWasActive = false;

        secondSceneIsOpen = false;
        transitionInProgress = false;
    }

    // Keep this function name unchanged.
    public void LoadSceneAsync(string sceneName)
    {
        if (transitionInProgress)
        {
            Debug.LogWarning(
                "SceneLoader: A transition is already running."
            );

            return;
        }

        // Calling this from the second scene returns to the first.
        if (secondSceneIsOpen)
        {
            BeginReturnToOriginalScene();
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "SceneLoader: The requested scene name is empty."
            );

            return;
        }

        if (IsSceneLoaded(sceneName))
        {
            Debug.LogWarning(
                "SceneLoader: Scene '" +
                sceneName +
                "' is already loaded. " +
                "It will not be loaded additively again."
            );

            return;
        }

        StartCoroutine(LoadSecondScene(sceneName));
    }

    // Keep this function name unchanged.
    public void LoadSceneByIndexAsync(int sceneIndex)
    {
        if (transitionInProgress)
        {
            Debug.LogWarning(
                "SceneLoader: A transition is already running."
            );

            return;
        }

        // Calling this from the second scene returns to the first.
        if (secondSceneIsOpen)
        {
            BeginReturnToOriginalScene();
            return;
        }

        if (IsSceneLoaded(sceneIndex))
        {
            Debug.LogWarning(
                "SceneLoader: Scene index " +
                sceneIndex +
                " is already loaded."
            );

            return;
        }

        StartCoroutine(LoadSecondScene(sceneIndex));
    }

    private IEnumerator LoadSecondScene(string sceneName)
    {
        transitionInProgress = true;

        if (!ValidateConfiguration())
        {
            transitionInProgress = false;
            yield break;
        }

        SaveOriginalSceneState();

        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Additive
            );

        if (loadOperation == null)
        {
            Debug.LogError(
                "SceneLoader: Could not start loading scene: " +
                sceneName
            );

            ClearState();
            transitionInProgress = false;
            yield break;
        }

        /*
         * Prepare the second scene, but do not activate it yet.
         * This prevents the two XR rigs from becoming active together.
         */
        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        /*
         * Disable the first XR System before activating the second.
         * Its Input Action Manager may disable the shared action asset.
         */
        originalLoader.xrSystemRoot.SetActive(false);

        yield return null;

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        Scene loadedScene =
            SceneManager.GetSceneByName(sceneName);

        yield return StartCoroutine(
            FinishLoadingSecondScene(loadedScene)
        );
    }

    private IEnumerator LoadSecondScene(int sceneIndex)
    {
        transitionInProgress = true;

        if (!ValidateConfiguration())
        {
            transitionInProgress = false;
            yield break;
        }

        SaveOriginalSceneState();

        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(
                sceneIndex,
                LoadSceneMode.Additive
            );

        if (loadOperation == null)
        {
            Debug.LogError(
                "SceneLoader: Could not start loading scene index: " +
                sceneIndex
            );

            ClearState();
            transitionInProgress = false;
            yield break;
        }

        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        /*
         * Disable the first XR System before activating the second.
         */
        originalLoader.xrSystemRoot.SetActive(false);

        yield return null;

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        Scene loadedScene =
            SceneManager.GetSceneByBuildIndex(sceneIndex);

        yield return StartCoroutine(
            FinishLoadingSecondScene(loadedScene)
        );
    }

    private IEnumerator FinishLoadingSecondScene(
        Scene loadedScene
    )
    {
        if (
            !loadedScene.IsValid() ||
            !loadedScene.isLoaded
        )
        {
            Debug.LogError(
                "SceneLoader: The second scene did not load correctly."
            );

            RestoreOriginalScene();

            ClearState();
            transitionInProgress = false;
            yield break;
        }

        SceneLoader foundLoader =
            FindRootSceneLoader(loadedScene);

        if (foundLoader == null)
        {
            Debug.LogError(
                "SceneLoader: No root-level SceneLoader was found in " +
                loadedScene.name
            );

            yield return StartCoroutine(
                RollbackFailedLoad(loadedScene)
            );

            yield break;
        }

        if (!foundLoader.ValidateConfiguration())
        {
            Debug.LogError(
                "SceneLoader: The second scene references are invalid."
            );

            yield return StartCoroutine(
                RollbackFailedLoad(loadedScene)
            );

            yield break;
        }

        secondLoader = foundLoader;
        secondScene = loadedScene;

        bool sceneChanged =
            SceneManager.SetActiveScene(secondScene);

        if (!sceneChanged)
        {
            Debug.LogError(
                "SceneLoader: Could not make the second scene active."
            );

            yield return StartCoroutine(
                RollbackFailedLoad(loadedScene)
            );

            yield break;
        }

        /*
         * Ensure the second scene is active.
         */
        secondLoader.sceneContentRoot.SetActive(true);
        secondLoader.xrSystemRoot.SetActive(true);

        /*
         * Allow Awake, Start and OnEnable to run.
         */
        yield return null;
        yield return null;

        /*
         * Hide the first environment only after the second environment
         * and XR rig have become active.
         */
        originalLoader.sceneContentRoot.SetActive(false);

        /*
         * CRITICAL FIX:
         *
         * The first rig's Input Action Manager may have disabled this
         * shared asset when the first XR System was deactivated.
         *
         * Explicitly enable all action maps again after every other
         * enable/disable operation has completed.
         */
        secondLoader.inputActions.Enable();

        yield return null;

        /*
         * Re-enable once more after the final initialization frame.
         */
        secondLoader.inputActions.Enable();

        secondSceneIsOpen = true;
        transitionInProgress = false;

        Debug.Log(
            "SceneLoader: Second scene ready. XR actions enabled: " +
            secondScene.name
        );
    }

    private void BeginReturnToOriginalScene()
    {
        if (originalLoader == null)
        {
            Debug.LogError(
                "SceneLoader: The original SceneLoader is missing."
            );

            return;
        }

        /*
         * Run the return coroutine on the original Loader because
         * the second Loader will be destroyed during unloading.
         */
        originalLoader.StartCoroutine(
            originalLoader.ReturnToOriginalScene()
        );
    }

    private IEnumerator ReturnToOriginalScene()
    {
        if (transitionInProgress)
        {
            yield break;
        }

        transitionInProgress = true;

        if (
            !originalScene.IsValid() ||
            !originalScene.isLoaded
        )
        {
            Debug.LogError(
                "SceneLoader: The original scene is unavailable."
            );

            ClearState();
            transitionInProgress = false;
            yield break;
        }

        if (
            !secondScene.IsValid() ||
            !secondScene.isLoaded ||
            secondLoader == null
        )
        {
            Debug.LogError(
                "SceneLoader: The second scene is unavailable."
            );

            RestoreOriginalScene();

            ClearState();
            transitionInProgress = false;
            yield break;
        }

        Scene sceneToUnload = secondScene;

        /*
         * Restore the original environment before removing the
         * second environment. This avoids an empty or blue frame.
         */
        originalLoader.sceneContentRoot.SetActive(
            originalContentWasActive
        );

        /*
         * Disable the second XR rig first.
         * Its Input Action Manager can disable the shared action asset.
         */
        secondLoader.xrSystemRoot.SetActive(false);

        yield return null;

        bool sceneChanged =
            SceneManager.SetActiveScene(originalScene);

        if (!sceneChanged)
        {
            Debug.LogError(
                "SceneLoader: Could not restore the original scene."
            );

            transitionInProgress = false;
            yield break;
        }

        /*
         * Activate the original XR rig.
         */
        originalLoader.xrSystemRoot.SetActive(
            originalXRWasActive
        );

        yield return null;
        yield return null;

        /*
         * CRITICAL FIX:
         * Explicitly restore the first scene's input actions.
         */
        originalLoader.inputActions.Enable();

        yield return null;

        originalLoader.inputActions.Enable();

        /*
         * The original scene is now fully visible and interactive.
         * It is safe to unload the second scene.
         */
        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(sceneToUnload);

        if (unloadOperation == null)
        {
            Debug.LogError(
                "SceneLoader: Could not unload the second scene."
            );

            transitionInProgress = false;
            yield break;
        }

        while (!unloadOperation.isDone)
        {
            yield return null;
        }

        Debug.Log(
            "SceneLoader: Returned successfully to " +
            originalScene.name
        );

        ClearState();
        transitionInProgress = false;
    }

    private void SaveOriginalSceneState()
    {
        originalLoader = this;
        originalScene = gameObject.scene;

        originalContentWasActive =
            sceneContentRoot.activeSelf;

        originalXRWasActive =
            xrSystemRoot.activeSelf;
    }

    private bool ValidateConfiguration()
    {
        if (sceneContentRoot == null)
        {
            Debug.LogError(
                "SceneLoader: Scene Content Root is missing in " +
                gameObject.scene.name
            );

            return false;
        }

        if (xrSystemRoot == null)
        {
            Debug.LogError(
                "SceneLoader: XR System Root is missing in " +
                gameObject.scene.name
            );

            return false;
        }

        if (inputActions == null)
        {
            Debug.LogError(
                "SceneLoader: Input Actions is missing in " +
                gameObject.scene.name +
                ". Assign XRI Default Input Actions."
            );

            return false;
        }

        if (
            sceneContentRoot.scene.handle !=
            gameObject.scene.handle
        )
        {
            Debug.LogError(
                "SceneLoader: Scene Content Root belongs to another scene."
            );

            return false;
        }

        if (
            xrSystemRoot.scene.handle !=
            gameObject.scene.handle
        )
        {
            Debug.LogError(
                "SceneLoader: XR System Root belongs to another scene."
            );

            return false;
        }

        if (sceneContentRoot == xrSystemRoot)
        {
            Debug.LogError(
                "SceneLoader: Scene Content Root and XR System Root " +
                "cannot be the same object."
            );

            return false;
        }

        if (
            xrSystemRoot.transform.IsChildOf(
                sceneContentRoot.transform
            )
        )
        {
            Debug.LogError(
                "SceneLoader: XR System must be outside Scene Content Root."
            );

            return false;
        }

        if (
            transform == sceneContentRoot.transform ||
            transform.IsChildOf(sceneContentRoot.transform)
        )
        {
            Debug.LogError(
                "SceneLoader: Loader must be outside Scene Content Root."
            );

            return false;
        }

        if (
            transform == xrSystemRoot.transform ||
            transform.IsChildOf(xrSystemRoot.transform)
        )
        {
            Debug.LogError(
                "SceneLoader: Loader must be outside XR System Root."
            );

            return false;
        }

        return true;
    }

    private static SceneLoader FindRootSceneLoader(
        Scene scene
    )
    {
        GameObject[] roots =
            scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            SceneLoader loader =
                root.GetComponent<SceneLoader>();

            if (loader != null)
            {
                return loader;
            }
        }

        return null;
    }

    private IEnumerator RollbackFailedLoad(
        Scene failedScene
    )
    {
        SceneLoader failedLoader =
            FindRootSceneLoader(failedScene);

        if (
            failedLoader != null &&
            failedLoader.xrSystemRoot != null
        )
        {
            failedLoader.xrSystemRoot.SetActive(false);
        }

        RestoreOriginalScene();

        if (
            originalScene.IsValid() &&
            originalScene.isLoaded
        )
        {
            SceneManager.SetActiveScene(originalScene);
        }

        if (
            originalLoader != null &&
            originalLoader.inputActions != null
        )
        {
            originalLoader.inputActions.Enable();
        }

        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(failedScene);

        if (unloadOperation != null)
        {
            while (!unloadOperation.isDone)
            {
                yield return null;
            }
        }

        ClearState();
        transitionInProgress = false;
    }

    private static void RestoreOriginalScene()
    {
        if (originalLoader == null)
        {
            return;
        }

        if (originalLoader.sceneContentRoot != null)
        {
            originalLoader.sceneContentRoot.SetActive(
                originalContentWasActive
            );
        }

        if (originalLoader.xrSystemRoot != null)
        {
            originalLoader.xrSystemRoot.SetActive(
                originalXRWasActive
            );
        }

        if (originalLoader.inputActions != null)
        {
            originalLoader.inputActions.Enable();
        }
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        for (
            int index = 0;
            index < SceneManager.sceneCount;
            index++
        )
        {
            Scene scene =
                SceneManager.GetSceneAt(index);

            if (
                scene.isLoaded &&
                scene.name == sceneName
            )
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSceneLoaded(int buildIndex)
    {
        for (
            int index = 0;
            index < SceneManager.sceneCount;
            index++
        )
        {
            Scene scene =
                SceneManager.GetSceneAt(index);

            if (
                scene.isLoaded &&
                scene.buildIndex == buildIndex
            )
            {
                return true;
            }
        }

        return false;
    }

    private static void ClearState()
    {
        originalLoader = null;
        secondLoader = null;

        originalScene = default(Scene);
        secondScene = default(Scene);

        originalContentWasActive = false;
        originalXRWasActive = false;

        secondSceneIsOpen = false;
    }
}