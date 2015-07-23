/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-13 * Controls */

using UnityEngine;
using UnityEngine.UI;
using invt=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.UserInterface {
	public class Controls : MonoBehaviour {
		static key menu, term, invt, lamp, jump, dash, duck;
		static axis mouseX, mouseY, axisX, axisY, roll;

		public delegate void InputKey(bool value);
		public delegate void InputAxis(float value);

		public Controls() { // why the how does it
			menu 	= new key((n)=>menu.input=n);
			term 	= new key((n)=>term.input=n);
			invt 	= new key((n)=>invt.input=n);
			lamp 	= new key((n)=>lamp.input=n);
			jump 	= new key((n)=>jump.input=n);
			dash 	= new key((n)=>dash.input=n);
			duck 	= new key((n)=>duck.input=n);
			mouseX 	= new axis((n)=>mouseX.input=n);
			mouseY 	= new axis((n)=>mouseY.input=n);
			axisX 	= new axis((n)=>axisX.input=n);
			axisY 	= new axis((n)=>axisY.input=n);
			roll 	= new axis((n)=>roll.input=n);
		} // there is no reason for this to compile

		void Awake() { AddAllInputListeners(); }

		void Update() {
			lamp.@get = true;  jump.@get = true;   dash.@get = true;
			duck.@get = true;  mouseX.@get = true; mouseY.@get = true;
			axisX.@get = true; axisY.@get = true;  roll.@get = true;
			term.@get = true;  invt.@get = true;   menu.@get = true;
			switch (Pathways.GameState) {
				case GameStates.Game: break;
				case GameStates.Term:
					lamp.@get = false;   lamp.f(false);
					jump.@get = false;   jump.f(false);
					dash.@get = false;   dash.f(false);
					duck.@get = false;   duck.f(false);
					mouseX.@get = false; mouseX.f(0f);
					mouseY.@get = false; mouseY.f(0f);
					axisX.@get = false;  axisX.f(0f);
					axisY.@get = false;  axisY.f(0f);
					roll.@get = false;   roll.f(0f);
					goto case GameStates.Game;
				case GameStates.Menu:
					term.@get = false;   term.f(false);
					invt.@get = false;   invt.f(false);
					goto case GameStates.Term;
				case GameStates.None:
					menu.@get = false;   menu.f(false);
					goto case GameStates.Menu;
			} switch (Pathways.GameState) {
				case GameStates.Game :
					lamp.f(lamp.@get && Input.GetButtonDown("Lamp"));
					jump.f(jump.@get && Input.GetButton("Jump"));
					dash.f(dash.@get && Input.GetButton("Sprint"));
					duck.f(duck.@get && Input.GetButton("Crouch"));
					mouseX.f((mouseX.@get)?Input.GetAxisRaw("MouseX"):0f);
					mouseY.f((mouseY.@get)?Input.GetAxisRaw("MouseY"):0f);
					axisX.f((axisX.@get)?Input.GetAxis("Horizontal"):0f);
					axisY.f((axisY.@get)?Input.GetAxis("Vertical"):0f);
					roll.f((roll.@get)?Input.GetAxis("Roll"):0f);
					goto case GameStates.Term;
				case GameStates.Term :
					term.f(term.@get && Input.GetButton("Console"));
					invt.f(invt.@get && Input.GetButton("Invt"));
					goto case GameStates.Menu;
				case GameStates.Menu :
					menu.f(menu.@get && Input.GetButtonDown("Menu"));
					goto case GameStates.None;
				case GameStates.None : break;
			}
		}

		public static void AddInputListener(Object c) {
			foreach (var elem in c.GetType().GetFields()) {
				if (elem.FieldType==typeof (key))
					AddInput((key) elem.GetValue(c), elem.Name);
				else if (elem.FieldType==typeof (axis))
					AddInput((axis) elem.GetValue(c), elem.Name);
			}
		}

		static void AddAllInputListeners() {
			foreach (var elem in Object.FindObjectsOfType<MonoBehaviour>())
				AddInputListener(elem); /* automatic via reflection! */
		}

		static void AddInput(key k, string s) {
			switch (s) {
				case "menu": menu.f += k.f; break;
				case "term": term.f += k.f; break;
				case "invt": invt.f += k.f; break;
				case "lamp": lamp.f += k.f; break;
				case "jump": jump.f += k.f; break;
				case "duck": duck.f += k.f; break;
				case "dash": dash.f += k.f; break;
			}
		}

		static void AddInput(axis k, string s) {
			switch (s) {
				case "axisX":  axisX.f	+= k.f; break;
				case "axisY":  axisY.f  += k.f; break;
				case "mouseX": mouseX.f += k.f; break;
				case "mouseY": mouseY.f += k.f; break;
				case "roll":   roll.f   += k.f; break;
			}
		}
	}

	public struct key {
		public bool @get;
		public bool input;
		public Controls.InputKey f;
		public key (Controls.InputKey _f) {
			@get = true;
			input = false;
			f = _f;
		}
	}

	public struct axis {
		public bool @get;
		public float input;
		public Controls.InputAxis f;
		public axis (Controls.InputAxis _f) {
			@get = true;
			input = 0f;
			f = _f;
		}
	}
}


