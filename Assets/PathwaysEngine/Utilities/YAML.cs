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