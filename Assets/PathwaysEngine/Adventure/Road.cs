/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-13 * Road */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Adventure.Setting {
	public class Road : Connector {
		public new Area src, tgt;

		public new void OnTriggerEnter(Collider other) {
			base.OnTriggerEnter(other);
			if (other.tag=="Player") Player.Goto(tgt);
		}
	}
}
