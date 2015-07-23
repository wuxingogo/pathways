/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-11 * YAML */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PathwaysEngine.Utilities {
	public class YAML {
		public TextReader stream;
		uint line = 1; // ind from 1
		string fileName;
		public string[] fileLines;

		public bool EOF { get { return stream.Peek()==-1; } }
		public char c { get {
			return ((EOF)?('\0'):((char) stream.Peek()));} }
		public bool literal {
			get { return _literal; }
			set { if (!_literal && !value)
				throw new System.Exception("literal");
				_literal = value;
			}
		} bool _literal = false;

		public void Next() {
			if (c=='\n') line++;
			if (!EOF) {
				stream.Read();
				while (stream.Peek()=='\0') stream.Read();
			}
		}

		public static void LoadYAML(string file_name) {
			if (File.Exists(file_name)) {
				using (TextReader reader = File.OpenText(file_name)) {
					var line = reader.ReadLine();
					var temp = new List<string>();
					while (line!=null) {
						temp.Add(line);
						line = reader.ReadLine();
					} //fileLines = temp.ToArray();
				}
			} else throw new System.Exception("404");
		}
	}
}

#if LAME
	public static class YAML {
		static Dictionary<string,string> tokens = {
			{"comment", @"#[^\n]*"},
			{"indent", @"\n( *)"},
			{"space", @" +"},
			{"true", @"\b(enabled|true|yes|on)\b"},
			{"false", @"\b(disabled|false|no|off)\b"},
			{"null", @"\b(null|Null|NULL|~)\b"},
			{"string", @"""(.*?)"""},
			{"string", @"'(.*?)'"},
			{"timestamp", @"((\d{4})-(\d\d?)-(\d\d?)(?:(?:[ \t]+)(\d\d?):(\d\d)(?::(\d\d))?)?)"},
			{"float", @"(\d+\.\d+)"},
			{"int", @"(\d+)"},
			{"doc", @"---"},
			{",", @","},
			{"{", @"\{(?![^\n\}]*\}[^\n]*[^\s\n\}])"},
			{"}", @"\}"},
			{"[", @"\[(?![^\n\]]*\][^\n]*[^\s\n\]])"},
			{"]", @"\]"},
			{"-", @"\-"},
			{":", @"[:]"},
			{"string", @"(?![^:\n\s]*:[^\/]{2})(([^:,\]\}\n\s]|(?!\n)\s(?!\s*?\n)|:\/\/|,(?=[^\n]*\s*[^\]\}\s\n]\s*\n)|[\]\}](?=[^\n]*\s*[^\]\}\s\n]\s*\n))*)(?=[,:\]\}\s\n]|$)"},
			{"id", @"([\w][\w -]*)"}};

		public static Stack<string> Tokenize(string s) {
			string[] token = new string[1];
			bool ignore = false;
			//var input;
			string captures;
			int l = s.Length, indents = 0, lastIndents = 0, indentAmount = -1;
			Stack<string> stack = new Stack<string>();
			while (l>0) {
				foreach (var elem in tokens) {
					if (elem.Value.Contains(s)) {
						captures = elem.Value;
						token = new string[2] { elem.Key,"" };
						s = s.Replace(elem.Value,"");
						switch (elem.Key) {
							case "comment": ignore = true; break;
							case "indent":
								lastIndents = indents;
								if (indentAmount==-1)
									indentAmount = token[1].Length;
								indents = token[1].Length/indentAmount;
								if (indents==lastIndents) ignore = true;
								else if (indents>lastIndents+1) Debug.Log("indent failll");
								else if (indents<lastIndents) {
									//input = token[1].input;
									token = new string[1] { "dedent" };
									//token.input = input;
									while (--lastIndents>indents)
										stack.Push(token[1]);
								} break;
						}
					} if (!ignore)
						if (token!=null) {
							stack.Push(token[1]);
							token = null;
						} else Debug.Log("faill");
					ignore = false;
				}
			} return stack;
		}

		static string[] Peek() { return tokens[0]; }
		static string[] Advance() { return tokens.shift(); }
		static string[] AdvanceValue() { return this.advance()[1][1]; }
		static bool Accept(string s) { if (PeekType(s)) return this.advance(); }
		static void Expect(string s) { if (!this.accept(s)) Debug.Log("sheeit"); }
		static bool PeekType(string val) {
			return (this.tokens[0] && this.tokens[0][0]==val);
		}

		static void IgnoreSpace() { while (PeekType("space")) Advance(); }
		static void IgnoreWhiteSpace() {
			while (PeekType("space")
			|| PeekType("indent") || PeekType("Dedent")) Advance();
		}

		static void Parse() {
			switch (Peek()) {
				case "doc": // parse doc
					Accept("doc");
					Expect("indent");
					var temp = Parse();
					Expect("dedent");
					return temp;
					break;
				case "-": ParseList(); break;
				case "{": ParseInLineHash(); break;
				case "[": ParseInLineList(); break;
				case "id": ParseHash(); break;
				case "string": AdvanceValue(); break;
				case "timestamp": ParseTimeStamp(); break;
				case "float": ParseFloat(advanceValue()); break;
				case "int": ParseInt(advanceValue()); break;
				case "true": advanceValue(); break;
				case "false": advanceValue(); break;
				case "null": advanceValue(); break;
			}
		}

#endif
