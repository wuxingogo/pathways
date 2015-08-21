/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-09 * Area */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Mechanics.Setting {
	public partial class Area : MonoBehaviour {
		public List<Room> rooms;
		public List<Area> areas;

		public bool safe {get;set;}
		public int level {get;set;}
		public string uuid { get; private set; }
		public string desc {
			get { return string.Format("{0}{1}",_desc,
				@"\*\*\*List Nearby Rooms\*\*\*"); }
			set { _desc = value; }
		} string _desc;
	}
}
