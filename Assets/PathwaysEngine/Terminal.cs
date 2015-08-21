/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-11 * Terminal */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Flags=System.FlagsAttribute;
using Buffer=System.Text.StringBuilder;
using invt=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {
	public class Terminal : MonoBehaviour {
		static bool wait = false;
		static InputField inputField;
		static Text log;
		static Buffer buffer = new Buffer(65535);
		public util::key term;

		public static bool focus {
			get { return _focus; }
			set { _focus = value;
				inputField.interactable = _focus;
				Cursor.visible = _focus;
				if (_focus) {
					inputField.ActivateInputField();
					inputField.Select(); }}
		} static bool _focus = false;

		public Terminal() { term = new util::key((n)=>{
			if (!wait && n) StartCoroutine(Term());}); }

		IEnumerator Term() {
			wait = true;
			yield return new WaitForSeconds(0.125f);
			SetTerm(!term.input);
			yield return new WaitForSeconds(0.25f);
			wait = false; }

		static void SetTerm(bool value) {
			focus = value;
			Pathways.GameState = (value)?(GameStates.Term):(GameStates.Game);}

		void Awake() {
			Pathways.terminal = this;
			inputField = gameObject.GetComponentInChildren<InputField>();
			log = GetComponentInChildren<Text>();
			Log(Pathways.GetYAML("init")); }

		public static void Clear() { buffer = new Buffer(65535); }

		public static void Log() {
			buffer.AppendLine("\n"); log.text = buffer.ToString(); }

		public static void Log(string s) {
			buffer.Append(s); log.text = buffer.ToString(); }

		public static void Log(string s,Formats f) {
			switch (f) {
				case Formats.Emphasis: case Formats.Strong:
					buffer.AppendLine(string.Format("\n<{1}>{0}</{1}>",s,
						string.Format((f==Formats.Emphasis)?"i":"b"))); break;
				case Formats.h1: case Formats.h2: case Formats.h3:
					buffer.AppendLine(string.Format(
						"\n<size={0}>{1}</size>",f,s)); break;
				case Formats.Newline:
					Log(string.Format("\n{0}",s)); break;
				default: buffer.AppendLine(string.Format(
					"\n<color=#{0:X}>{1}</color>",(int) f,s)); break;
			} log.text = buffer.ToString(); }

		public static void Log(params string[] lines) {
			buffer.AppendLine("\n");
			foreach (var elem in lines) buffer.AppendLine(elem);
			log.text = buffer.ToString(); }

		public static void Log(TextAsset s) { buffer.AppendLine(s.text); }

		public static void LogAndWaitForInput(string s, Formats f=Formats.Default) {
			buffer.AppendLine(s); SetTerm(true); }

		public void CommandInput() {
			Log(" > "+inputField.text,Formats.Command);
			Parser.eval(inputField.text);
			inputField.text = "";
			Pathways.GameState = GameStates.Game;
		}

		public static void WaitForInput() { focus = true; }
		/* pause game somehow */
	}
}
