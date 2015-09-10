/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-31 * Creature */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using stat=PathwaysEngine.Statistics;

namespace PathwaysEngine.Adventure {
	public partial class Creature : Thing, ILiving {
		public virtual stat::Set stats {get;set;}
		public virtual bool dead {
			get { return _dead; }
			set { _dead = value; if (_dead) Kill(); }
		} protected bool _dead = false;

		public virtual void ApplyDamage(float n) {

		}

		public virtual void Kill() {
			Terminal.Log(uuid.title()+" has died.",Formats.Command); }

		public override void Awake() { this.GetYAML(); }
	}
}