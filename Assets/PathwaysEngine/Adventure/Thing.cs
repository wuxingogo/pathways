/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Thing */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Adventure {
	public partial class Thing : MonoBehaviour, IStorable {
		public virtual bool seen {get;set;}
		public string uuid { get { return gameObject.name; } }
		public virtual Desc<Thing> desc {get;set;}

		public virtual void Awake() { this.GetYAML();
			FormatDescription();
		}

		public virtual void Find() { }
		public virtual void View() { Terminal.Log(desc); }

		public virtual void FormatDescription() { this.desc.SetFormat("{0}"); }

		public override string ToString() { return this.uuid.title(); }

		public static bool operator !(Thing i) { return (i==null); }
	}
}
