/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Creature */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Mechanics {
	public class Creature : MonoBehaviour, ILiving {
		public StatSet stats {get;set;}
		public bool dead {get;set;}

		public Creature() {

		}

		public void ApplyDamage(float n) {

		}
	}
}