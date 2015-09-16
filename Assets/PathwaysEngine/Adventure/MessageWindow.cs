/* Ben Scott * bescott@andrew.cmu.edu * 2015-09-02 * Message Window */

using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using intf=PathwaysEngine.Adventure;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {
	public class MessageWindow : MonoBehaviour {
		static ui::Text message_title, message_body;

		void Awake() {
			Pathways.StateChange += new StateHandler(EventListener);
			Pathways.messageWindow = this;
			foreach (var child in GetComponentsInChildren<ui::Text>())
				if (child.name=="Title") message_title = child;
				else if (child.name=="Body") message_body = child;
			if (!message_title || !message_body)
				Debug.LogError("missing title / body");
		}

		public static void EventListener(
		object sender,System.EventArgs e,GameStates gameState) {
			if (Pathways.messageWindow.gameObject)
				Pathways.messageWindow.gameObject.SetActive(gameState==GameStates.Msgs); }

		public static void Display(intf::Message m) {
			message_title.text = m.uuid;
			message_body.text = m.desc;
			Pathways.gameState = GameStates.Msgs;
		}

		public void Disable() { Pathways.gameState = GameStates.Game; }
	}
}
