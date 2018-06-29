using Game.GameControl;
using Game.LevelElements;
using Game.World;
using System.Collections;
using UnityEngine;

public class AmbientBox : MonoBehaviour, IInteractable, IWorldObject
{
    //##################################################################

    [SerializeField] UnityEngine.PostProcessing.PostProcessingProfile postProcess;

    [SerializeField] bool editAmbient = true;
    [SerializeField] bool editFog = false;
    [SerializeField] bool editOverlay = false;
    [SerializeField] bool editAudio = false;

    [Header("Ambient")]
    [ConditionalHide("editAmbient")]
    [SerializeField] Color color;
    [ConditionalHide("editAmbient")]
    [SerializeField] float ambientFadeSpeed = 2;


    [Header("Gradient Fog")]
    [ConditionalHide("editFog")]
    [SerializeField] Gradient gradient;
    [ConditionalHide("editFog")]
    [SerializeField] float startDistance = 100;
    [ConditionalHide("editFog")]
    [SerializeField] float endDistance = 200;
    [ConditionalHide("editFog")]
    [SerializeField] float fogFadeSpeed = 2;

    [Header("Colour Overlay")]
    [ConditionalHide("editOverlay")]
    [SerializeField] Color overlayColour;
    [ConditionalHide("editOverlay")]
    [SerializeField] float overlayFadeSpeed = 1;

    [Header("Audio")]
    [ConditionalHide("editAudio")]
    [SerializeField] AudioClip clip;
    [ConditionalHide("editAudio")]
    [SerializeField] float audioFadeSpeed = 2f;
    [ConditionalHide("editAudio")]
    [SerializeField] AnimationCurve fadeInCurve;
    [ConditionalHide("editAudio")]
    [SerializeField] AnimationCurve fadeOutCurve;

    private GameController GameController;
    AudioClip defaultClip;
    float currentVolume, audioTimer, defaultVolume;
    AudioSource atmoSource;

    GradientFog fog;
    UnityEngine.PostProcessing.PostProcessingBehaviour postProcessStack;

    Color defaultColor;
    Gradient defaultGradient;
    ColorOverlay overlay;
    float defaultStart, defaultEnd;
    private bool IsInitialized = false;
	int isInside = 0;

    //##################################################################

    // -- INITIALIZATION

    public void Initialize(GameController game_controller)
    {
        if (IsInitialized)
        {
            return;
        }

        GameController = game_controller;

        defaultColor = RenderSettings.ambientLight;

        fog = GameController.CameraController.GradientFogComponent;
        defaultGradient = GameController.CameraController.defaultGradient;
        defaultStart = GameController.CameraController.defaultStart;
        defaultEnd = GameController.CameraController.defaultEnd;

        overlay = fog.GetComponent<ColorOverlay>();

        postProcessStack = GameController.CameraController.PostProcessingBehaviourComponent;

        atmoSource = GameController.PlayerController.AudioManager.wind;

        if (!atmoSource)
        {
            Debug.LogError("Couldn't find audiosource on P_AudioManager/Nature/Wind");
        }
        else
        {
            defaultClip = atmoSource.clip;
            defaultVolume = atmoSource.volume;
        }

        IsInitialized = true;
    }

    //##################################################################

    // -- INQUIRIES

    public Transform Transform { get { return transform; } }

    public bool IsInteractable()
    {
        return false;
    }

    //################################################################## 

    // -- OPERATIONS

    public void OnPlayerEnter()
    {
		StopAllCoroutines();
		isInside = 1;

		print("Enter " + name);

        if (editAmbient) {
            StartCoroutine(FadeAmbient(color));
        }

        if (editFog) {
			StartCoroutine(FadeFog(gradient, startDistance, endDistance));
        }

        if (postProcess) {
			postProcessStack.OverrideProfile(postProcess);
        }

		if (editOverlay) {
			StartCoroutine(FadeOverlay(overlayColour, 1));
		}

        if (editAudio) {
			StartCoroutine(FadeAudio(1f, true));
        }
    }

    public void OnPlayerExit() {
		StopAllCoroutines();
		isInside = 0;

		print("Exit " + name);

		if (editAmbient)
        {
            StartCoroutine(FadeAmbient(defaultColor));
        }

        if (editFog)
        {
            StartCoroutine(FadeFog(defaultGradient, defaultStart, defaultEnd));
        }

        if (postProcess)
        {
            postProcessStack.StopOverridingProfile();
        }

        if (editOverlay)
            StartCoroutine(FadeOverlay(Color.clear, 0));

        if (editAudio)
        {
            StartCoroutine(FadeAudio(0f, false));
        }
    }

	void OnDestroy() {
		if (isInside > 0) {
			print("Destroy " + name);

			if (editAmbient)
				RenderSettings.ambientLight = defaultColor;

			if (editFog) {
				GradientFog[] fogs = FindObjectsOfType<GradientFog>();

				foreach (GradientFog foggy in fogs) {
					foggy.gradient = defaultGradient;
					foggy.startDistance = defaultStart;
					foggy.endDistance = defaultEnd;
					foggy.gradientLerp = 0;
					foggy.GenerateTexture();
				}
			}

			if (postProcess)
				postProcessStack.StopOverridingProfile();


			if (editOverlay) {
				overlay.color = Color.clear;
				overlay.intensity = 0;
			}

			if (editAudio) {
				atmoSource.clip = defaultClip;
				atmoSource.PlayScheduled(Random.value);
				atmoSource.volume = defaultVolume;
			}
			isInside = 0;
		}

	}

    public void OnHoverBegin()
    {
    }

    public void OnHoverEnd()
    {
    }

    public void OnInteraction()
    {
    }

    private IEnumerator FadeAmbient(Color goal) {
		isInside++;
		while (RenderSettings.ambientLight != goal)
        {
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, goal, Time.deltaTime * ambientFadeSpeed);
            yield return null;
		}
		isInside--;
	}
    
    private IEnumerator FadeOverlay(Color color, float goal) {
		isInside++;
		overlay.enabled = true;
        overlay.color = color;
        while (Mathf.Abs(overlay.intensity - goal) > 0.001f)
        {
            overlay.intensity = Mathf.Lerp(overlay.intensity, goal, Time.deltaTime * overlayFadeSpeed);
            yield return null;
		}
		isInside--;
	}


    private IEnumerator FadeFog(Gradient goal, float startGoal, float endGoal) {
		isInside++;
		GradientFog[] fogs = FindObjectsOfType<GradientFog>();
        fog = fogs[0];

        foreach (GradientFog foggy in fogs)
        {
            foggy.secondaryGradient = goal;
            foggy.gradientLerp = 0;
            foggy.GenerateTexture();
        }

        float t = 0;

        while (Mathf.Abs(fog.startDistance - startGoal) > 0.1f || Mathf.Abs(fog.endDistance - endGoal) > 0.1f)
        {
            t += Time.deltaTime * fogFadeSpeed;

            //print(t);

            foreach (GradientFog foggy in fogs)
            {
                foggy.gradientLerp = t;
                foggy.GenerateTexture();
                foggy.startDistance = Mathf.Lerp(foggy.startDistance, startGoal, t);
                foggy.endDistance = Mathf.Lerp(foggy.endDistance, endGoal, t);
            }
            yield return null;
        }

        foreach (GradientFog foggy in fogs)
        {
            foggy.gradient = goal;
            foggy.gradientLerp = 0;
            foggy.GenerateTexture();
		}
		isInside--;
	}

    private IEnumerator FadeAudio(float _targetVolume, bool _entering) {
		isInside++;
		while (audioTimer <= audioFadeSpeed)
        {
            audioTimer += Time.deltaTime;
            currentVolume = fadeOutCurve.Evaluate(Mathf.Clamp01(audioTimer / audioFadeSpeed)) * defaultVolume;
            atmoSource.volume = currentVolume;
            yield return null;
        }

        audioTimer = 0f;

        atmoSource.clip = _entering ? clip : defaultClip;

        atmoSource.PlayScheduled(Random.value);

        while (audioTimer <= audioFadeSpeed)
        {
            audioTimer += Time.deltaTime;
            currentVolume = fadeInCurve.Evaluate(Mathf.Clamp01(audioTimer / audioFadeSpeed)) * defaultVolume;
            atmoSource.volume = currentVolume;
            yield return null;
        }

        audioTimer = 0f;
		isInside--;
    }
}
