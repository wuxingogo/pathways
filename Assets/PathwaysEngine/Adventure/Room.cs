/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Room */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using invt=PathwaysEngine.Inventory;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.Adventure.Setting {
	public partial class Room : Thing {
		bool wait = false;
		public List<invt::Item> items;
		public List<Room> nearbyRooms;

		public int depth {get;set;}
		public override Desc<Thing> desc {get;set;}

		public void OnTriggerEnter(Collider other) {
			if (!wait && other.tag=="Player"
			&& (Player.room && Player.room.depth<=this.depth) || !Player.room) {
				StartCoroutine(LogRoom());
			} else if (other.tag=="Item")
				items.Add(other.attachedRigidbody.GetComponent<invt::Item>());
		}

		public void OnTriggerExit(Collider other) {
			if (other.tag=="Player" && Player.room==this) Player.room = null; }

		IEnumerator LogRoom() {
			wait = true;
			yield return new WaitForSeconds(0.5f);
			seen = true;
			Terminal.Log(this);
			Player.room = this;
			yield return new WaitForSeconds(3f);
			wait = false;
		}

		public string descItems() {
			if (items==null || items.Count<1) return "";
			if (items.Count==1)
				return string.Format("You see a {0} here.",items[0]);
			var buffer = new Buffer("You see a ");
			foreach (var item in items) buffer.Append(item.name+", ");
			return buffer.ToString();
		}

		public override void Awake() { base.Awake();
			this.GetYAML(); desc.AddNouns(@"room|floor|walls");
			FormatDescription();
		}

		public override void FormatDescription() {
			this.desc.SetFormat(string.Format("## {0} ##\n{{0}}\n\n{1}",
				uuid.title(),descItems()));
		}
	}
}
