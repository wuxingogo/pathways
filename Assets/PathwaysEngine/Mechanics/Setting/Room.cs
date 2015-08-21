/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-02 * Room */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using invt=PathwaysEngine.Inventory;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.Mechanics.Setting {
	public partial class Room : MonoBehaviour {
		public List<invt::Item> items;
		public List<Connector> adjacents;

		public string uuid { get; private set; }
		public string desc {
			get { return string.Format("{0}{1}",_desc,descItems()); }
			set { _desc = value; }
		} string _desc = "This is a room.";

		public void OnTriggerEnter(Collider other) {
			if (other.GetComponent<Player>()!=null) this.Log(); }

		public string descItems() {
			if (items==null || items.Count<1) return "";
			if (items.Count==1)
				return string.Format("You see a {0} here.", items[0]);
			var buffer = new Buffer("You see a ");
			foreach (var item in items) buffer.Append(item.name+", ");
			return buffer.ToString();
		}
	}
}
