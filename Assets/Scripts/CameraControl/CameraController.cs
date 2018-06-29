using Game.EchoSystem;
using Game.GameControl;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace Game.CameraControl
{
    public class CameraController : MonoBehaviour
    {
        //########################################################################

        // -- ATTRIBUTES

        public PoS_Camera PoS_Camera { get; private set; }
        public EchoCameraEffect EchoCameraEffect { get; private set; }
        public Eclipse EclipseEffect { get; private set; }
        public GPUIDisplayManager GPUIDisplayManager { get; private set; }
        public GradientFog GradientFogComponent { get; private set; }
        public PostProcessingBehaviour PostProcessingBehaviourComponent { get; private set; }

		public Gradient defaultGradient { get; private set; }
		public float defaultStart { get; private set; }
		public float defaultEnd { get; private set; }

		//########################################################################

		// -- INITIALIZATION

		public void InitializeCameraController(GameController gameController)
        {
            PoS_Camera = GetComponent<PoS_Camera>();
            EchoCameraEffect = GetComponent<EchoCameraEffect>();
            EclipseEffect = GetComponent<Eclipse>();
            GPUIDisplayManager = GetComponentInChildren<GPUIDisplayManager>();
            GradientFogComponent = GetComponentInChildren<GradientFog>();
            PostProcessingBehaviourComponent = GetComponentInChildren<PostProcessingBehaviour>();

			defaultGradient = GradientFogComponent.gradient;
			defaultStart = GradientFogComponent.startDistance;
			defaultEnd = GradientFogComponent.endDistance;

			PoS_Camera.Initialize(gameController);
            GPUIDisplayManager.Initialize(gameController);
        }

        //########################################################################

        // -- OPERATIONS

    }
} // end of namespace