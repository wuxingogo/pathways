/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-11 * Terminal */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using invt=PathwaysEngine.Inventory;
using Flags=System.FlagsAttribute;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.UserInterface {
	[Flags] public enum Formats : int {
			Default=0xFFFFFF, State=0x2A98AA, Change=0xFFAE10,
			Alert=0xFC0000, Command=0x999999, h1=48, h2=36,
			Emphasis, Strong, Newline };

	public class Terminal : MonoBehaviour {
		static bool wait = false;
		static InputField inputField;
		static Text log;
		static Buffer buffer = new Buffer(2048);
		public key term;

		public static bool focus {
			get { return _focus; }
			set { _focus = value;
				inputField.interactable = _focus;
				Cursor.visible = _focus;
				if (_focus) {
					inputField.ActivateInputField();
					inputField.Select();
				}
			}
		} static bool _focus = false;
#if OLD
		public bool term {
			get { return _term; }
			set { _term = value;
				//StartCoroutine(Term());
			}
		} bool _term = false;
#endif
		public Terminal() {
			term = new key((n)=>{if (!wait && n) StartCoroutine(Term()); });
		}

		IEnumerator Term() {
			wait = true;
			yield return new WaitForSeconds(0.125f);
			term.input = !term.input;
			focus = term.input;
			Pathways.GameState = (term.input)?(GameStates.Term):(GameStates.Game);
			yield return new WaitForSeconds(0.25f);
			wait = false;
		}

		private void Awake() {
			inputField = gameObject.GetComponentInChildren<InputField>();
			log = GetComponentInChildren<Text>();
			Log(Pathways.GetTextYAML("init"),Formats.State);
		}

		public static void Log() {
			buffer.AppendLine("\n");
			log.text = buffer.ToString();
		}

		public static void Log(string s) {
			buffer.AppendLine(s);
			log.text = buffer.ToString();
		}

		public static void Log(string s, Formats f) {
			switch (f) {
				case Formats.Emphasis:
					buffer.AppendLine(string.Format("\n<i>{0}</i>",s));
					break;
				case Formats.Strong:
					buffer.AppendLine(string.Format("\n<b>{0}</b>",s));
					break;
				case Formats.h1:
				case Formats.h2:
					buffer.AppendLine(string.Format(
						"\n<size={0}>{1}</size>", f,s));
					break;
				case Formats.Newline:
					Log(string.Format("\n{0}",s));
					break;
				default:
					buffer.AppendLine(string.Format(
						"\n<color=#{0:X}>{1}</color>",(int) f,s));
					break;
			} log.text = buffer.ToString();
		}

		public static void Log(params string[] lines) {
			buffer.AppendLine("\n");
			foreach (var elem in lines) buffer.AppendLine(elem);
			log.text = buffer.ToString();
		}

		public static void Log(TextAsset s) {
			buffer.AppendLine(s.text);
		}

		public static void Log(Encounter e) {
			//Log(e.message); // and then should wait for return
		}

		public void CommandInput() {
			eval(inputField.text);
			inputField.text = "";
		}

		static public void CommandInput(string s) { eval(s); }

		static void eval(string s) {
			Log(" > "+s,Formats.Command);
//			foreach (var elem in s.Split(' ')) break;
		}
	}
}
