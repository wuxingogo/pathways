/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-11 * Terminal */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using invt=PathwaysEngine.Inventory;
using Flags=System.FlagsAttribute;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.UserInterface {
	public class Terminal : MonoBehaviour {
		[Flags] public enum Format : byte {
			Default=0, State=1, Change=2, Alert=3 };
		static bool wait = false;
		static int[] colorsText = new int[] {
			0xFFFFFF, 0x2A98AA, 0xFFAE10, 0xFC0000 };
		static InputField inputField;
		static Text log;
		static Buffer buffer = new Buffer(2048);
		public Controls.InputKey OnTerm;

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

		public bool term {
			get { return _term; }
			set { _term = value;
				//StartCoroutine(Term());
			}
		} bool _term = false;

		public Terminal() {
			OnTerm = new Controls.InputKey(
				(n)=>{ if (!wait && n) StartCoroutine(Term()); });
		}

		IEnumerator Term() {
			wait = true;
			yield return new WaitForSeconds(0.125f);
			term = !term;
			focus = term;
			yield return new WaitForSeconds(0.25f);
			wait = false;
		}

		private void Awake() {
			inputField = gameObject.GetComponentInChildren<InputField>();
			log = GetComponentInChildren<Text>();
			Log(Pathways.GetTextYAML("init"), Format.State);
		}

		public static void Log() {
			buffer.AppendLine("\n");
			log.text = buffer.ToString();
		}

		public static void Log(string s) {
			buffer.AppendLine("\n"+s);
			log.text = buffer.ToString();
		}

		public static void Log(string s, Format f) {
				buffer.AppendLine(string.Format(
					"\n<color=#{0:X}>{1}</color>",colorsText[(int)f],s));
			log.text = buffer.ToString();
		}

		public static void Log(params string[] lines) {
			buffer.AppendLine("\n");
			foreach (var elem in lines) buffer.AppendLine(elem);
			log.text = buffer.ToString();
		}

		public static void Log(TextAsset s) {
			buffer.AppendLine(s.text);
		}

		public static void LogEvent(Encounter e) {
			//Log(e.message); // and then should wait for return
		}

		public static void CommandInput() {
//			eval(inputField.TextComponent.text);
		}

		public static void CommandInput(string s) {
			eval(s);
		}

		static void eval(string s) {
			foreach (var elem in s.Split(' '))
				Log("wow!"+elem);
		}
	}
}
