using Game.GameControl;
using Game.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using TMPro;

namespace Game.UI
{
    public class MainMenuController : MonoBehaviour, IUiMenu
    {
        //##################################################################

        // -- CONSTANTS

        [SerializeField] private CanvasGroup buttonsGroup;
        [SerializeField] private Button newGameButton, continueButton;

        //##################################################################

        // -- ATTRIBUTES

        public bool IsActive { get; private set; }

        private GameController GameController;

        //##################################################################

        // -- INITIALIZATION

        void IUiMenu.Initialize(GameController gameController, UiController ui_controller)
        {
            this.GameController = gameController;
        }
        
        void IUiMenu.Activate()
        {

            if (IsActive)
            {
                return;
            }

            IsActive = true;
            gameObject.SetActive(true);

            StartCoroutine(_AppearLeBouton());

            if (GameController.PlayerModel.ContinuedGame)
				continueButton.Select();
			else {
				continueButton.gameObject.SetActive(false);
				newGameButton.Select();
			}
        }

        IEnumerator _AppearLeBouton()
        {
            float duration = 10.0f;

            for(float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                buttonsGroup.alpha = elapsed / duration;
                yield return null;
            }
        }

        void IUiMenu.Deactivate()
        {
            IsActive = false;
            gameObject.SetActive(false);

            EventSystem.current.SetSelectedGameObject(null);
        }

        //##################################################################

        // -- OPERATIONS

        public void HandleInput()
        {
            if (Input.GetButtonDown("Interact"))
            {
				StartGame();
            }
        }

		public void StartGame() {
			GameController.StartGame();
		}

		public void NewGame() {
			GameController.PlayerModel.DeleteSave();
			GameController.StartGame();
		}
    }
} //end of namespace