using Game.GameControl;
using System.Collections;
using UnityEngine;

namespace Game.UI {
	public class EndCreditsController : MonoBehaviour, IUiMenu {

		//###########################################################

		// -- ATTRIBUTES

		public bool IsActive { get; private set; }
		private GameController GameController;

		[SerializeField] CanvasGroup logo;
		[SerializeField] CanvasGroup[] team;
		[SerializeField] CanvasGroup[] endMsg;

		float startWait = 1.6f,
			  fadeDuration = 0.8f, waitTime = 0.4f,
			  logoFade = 2f, logoDuration = 2f,
			  teamNameTime = 0.6f, lastTeamWait = 1f,
		      endMsgTime = 1.4f, endMsgWait = 2f;

		//###########################################################

		// -- INITIALIZATION

		/// <summary>
		/// Initializes the Hud Menu.
		/// </summary>
		/// <param name="gameController"></param>
		/// <param name="ui_controller"></param>
		public void Initialize(GameController gameController, UiController ui_controller) {
			GameController = gameController;
			ClearAll();
		}

		void ClearAll() {
			logo.alpha = 0;

			foreach (var txt in team)
				txt.alpha = 0;

			foreach (var txt in endMsg)
				txt.alpha = 0;
		}

		/// <summary>
		/// Shows the Hud Menu.
		/// </summary>
		public void Activate() {
			if (IsActive) {
				return;
			}

			if (!gameObject.activeSelf) {
				gameObject.SetActive(true);
				StartCoroutine(_RollCredits());
			}

			IsActive = true;
		}

		/// <summary>
		/// Hides the Hud Menu.
		/// </summary>
		public void Deactivate() {
			if (!IsActive) {
				return;
			}

			if (gameObject.activeSelf) {
				StopAllCoroutines();
				gameObject.SetActive(false);
			}

			IsActive = false;
		}

		/// <summary>
		/// Handles Input.
		/// </summary>
		public void HandleInput() {
			if (Input.GetButtonDown("MenuButton")) {
				StopAllCoroutines();
				GameController.SwitchToOpenWorld();
				Deactivate();
			}
		}

		IEnumerator _RollCredits() {
			ClearAll();

			yield return new WaitForSeconds(startWait);

			// LOGO APPEAR
			while (logo.alpha < 1) {
				logo.alpha += Time.deltaTime / logoFade;
				yield return null;
			}
			yield return new WaitForSeconds(logoDuration);

			// LOGO DISAPPEAR
			while (logo.alpha > 0) {
				logo.alpha -= Time.deltaTime / fadeDuration;
				yield return null;
			}
			yield return new WaitForSeconds(waitTime);

			// TEAM MEMBERS APPEAR
			foreach (var guy in team) {
				while (guy.alpha < 1) {
					guy.alpha += Time.deltaTime / fadeDuration;
					yield return null;
				}
				yield return new WaitForSeconds(teamNameTime);
			}
			yield return new WaitForSeconds(lastTeamWait);

			// TEAM DISAPPEARS
			while (team[0].alpha > 0) {
				foreach (var guy in team)
					guy.alpha -= Time.deltaTime / fadeDuration;
				yield return null;
			}
			yield return new WaitForSeconds(waitTime);

			// LAST MESSAGES APPEAR
			foreach (var msg in endMsg) {

				while (msg.alpha < 1) {
					msg.alpha += Time.deltaTime / fadeDuration;
					yield return null;
				}
				yield return new WaitForSeconds(endMsgTime);
			}

			yield return new WaitForSeconds(endMsgWait);

			// LAST MESSAGES DISAPPEARS
			while (endMsg[0].alpha > 0) {
				foreach (var msg in endMsg)
					msg.alpha -= Time.deltaTime / fadeDuration;
				yield return null;
			}

			yield return new WaitForSeconds(waitTime);

			GameController.SwitchToOpenWorld();
			Deactivate();
		}

	}
}