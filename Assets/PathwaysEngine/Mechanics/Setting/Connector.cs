/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-09 * Connector */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Mechanics.Setting {
	public class Connector : MonoBehaviour {
		public Room src, tgt;

		public string uuid {get;set;}

		public string desc {
			get { return string.Format("{0} {1}",_desc,(src!=null && tgt!=null)
				? string.Format("It goes between {0} and {1}.",src,tgt)
				: "It doesn't seem to go anywhere."); }
			set { _desc = value; }
		} string _desc;

		//public Connector() { this.desc = Pathways.GetYAML(uuid); }

		public void OnTriggerEnter(Collider other) {
			if (other.GetComponent<Player>()!=null) this.Log(); }
	}
}
