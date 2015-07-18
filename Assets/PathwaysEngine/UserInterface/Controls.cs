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

		public Controls() { // why the fuck how does it
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
		} // by all accounts, this should not compile

		void Awake() { AddAllInputListeners(); }

		void Update() {
			switch (Pathways.GameState) {
				case GameStates.None : break;
				case GameStates.Game :
					if (lamp.@get)		lamp.f(Input.GetButtonDown("Lamp"));
					if (jump.@get) 		jump.f(Input.GetButton("Jump"));
					if (dash.@get) 		dash.f(Input.GetButton("Sprint"));
					if (duck.@get) 		duck.f(Input.GetButton("Crouch"));
					if (mouseX.@get)	mouseX.f(Input.GetAxisRaw("MouseX"));
					if (mouseY.@get)	mouseY.f(Input.GetAxisRaw("MouseY"));
					if (axisX.@get)		axisX.f(Input.GetAxis("Horizontal"));
					if (axisY.@get)		axisY.f(Input.GetAxis("Vertical"));
					if (roll.@get)		roll.f(Input.GetAxis("Roll"));
					goto case GameStates.Term;
				case GameStates.Term :
					if (term.@get) term.f(Input.GetButtonDown("Console"));
					if (invt.@get) invt.f(Input.GetButtonDown("Inventory"));
					goto case GameStates.Menu;
				case GameStates.Menu :
					if (menu.@get) menu.f(Input.GetButtonDown("Menu")); break;
			}
		}

		static void AddAllInputListeners() {
			foreach (var elem in Object.FindObjectsOfType<MonoBehaviour>())
				AddInputListener(elem); /* automatic via reflection! */
		}

		public static void AddInputListener(MonoBehaviour c) {
			foreach (var elem in c.GetType().GetFields()) {
				if (elem.FieldType==typeof (InputAxis))
					AddAxis((InputAxis) elem.GetValue(c),elem.Name);
				else if (elem.FieldType==typeof (InputKey))
					AddKey((InputKey) elem.GetValue(c),elem.Name);
			}
		}

		static void AddKey(InputKey f, string s) {
			switch (s) {
				case "OnMenu" : menu.f	+= f; menu.@get = true; break;
				case "OnJump" : jump.f 	+= f; jump.@get	= true; break;
				case "OnDuck" : duck.f	+= f; duck.@get	= true; break;
				case "OnDash" : dash.f	+= f; dash.@get	= true; break;
				case "OnLamp" : lamp.f	+= f; lamp.@get	= true; break;
				case "OnInvt" : invt.f	+= f; invt.@get = true; break;
				case "OnTerm" : term.f	+= f; term.@get = true; break;
			}
		}

		static void AddAxis(InputAxis f, string s) {
			switch (s) {
				case "OnAxisX"	: axisX.f 	+= f; axisX.@get	= true; break;
				case "OnAxisY" 	: axisY.f 	+= f; axisY.@get	= true; break;
				case "OnMouseX" : mouseX.f 	+= f; mouseX.@get	= true; break;
				case "OnMouseY" : mouseY.f  += f; mouseY.@get 	= true; break;
				case "OnRoll" 	: roll.f 	+= f; roll.@get 	= true; break;
			}
		}

		struct key {
			public bool @get;
			public bool input;
			public InputKey f;
			public key (InputKey _f) {
				@get = false;
				input = false;
				f = _f;
			}
		}

		struct axis {
			public bool @get;
			public float input;
			public InputAxis f;
			public axis (InputAxis _f) {
				@get = false;
				input = 0f;
				f = _f;
			}
		}
	}
}


