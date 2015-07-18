/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-16 * Markdown */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Buffer=System.Text.StringBuilder;

namespace Markdown { // renders *.md to basic html
	public static class MarkdownExtension {
		static string marks = "*_-=#";
		public static string md(this string s) {
			bool e = false, b = false;
	//		var emphases = new List<int,string>();
			Buffer sb = new Buffer(s);
			for (int i=1;i<s.Length;++i) {
				var t = s[i];
				if (marks.IndexOf(t)<0) continue;
				var r = s[i-1];
				if (r=='\\') continue; //'
				if (t!='*' || t!='_') continue; // for now
				if ((r=='*' && t=='*')) {
				//	strongs.Add(i-1,String.Format("<{0}b>",(b)?"/":""));
					b = !b;
				}
				else if (t=='*') e = !e;
			} return sb.ToString();
		}
	}
}
/*
sb.Insert(i,"<i>");
sb.Replace('*','_');
sb.Replace("<i>","</i>");

String.Format("{0:yyyy-MM-dd}",System.DateTime.Now);*/