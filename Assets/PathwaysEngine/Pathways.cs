/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-22 * Pathways */

using UnityEngine; // Well, here we are! The main file! The big one!
using UnityEngine.UI; // The Pathways Engine, just like I pictured it!
using System.Linq; // This file contains most of the enums, interfaces,
using System.Collections; // and other loose bits that shouldn't be put
using System.Collections.Generic; // in the individual class files.
using System.IO; // if you can't find some global variable or you want
using System.Text.RegularExpressions; // a direct reference to the
using type=System.Type; // player, it's probably in here.
using Flags=System.FlagsAttribute;
using Markdown;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Buffer=System.Text.StringBuilder;
using invt=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;
using mech=PathwaysEngine.Mechanics;
using maps=PathwaysEngine.Mechanics.Setting;

namespace PathwaysEngine {
	public enum GameStates : byte { None=0, Game=1, Term=2, Menu=3 };
	[Flags] public enum UIStates : byte { Pack, Note, Map, Log };
	[Flags] public enum Formats : int {
		Default=0xFFFFFF, State=0x2A98AA, Change=0xFFAE10,
		Alert=0xFC0000, Command=0x999999, h1=36, h2=28, h3=24,
		Emphasis, Strong, Newline };

 	public static class Pathways {
		public static GameStates GameState = GameStates.Game;
		public static readonly Dictionary<string,string> paths_yaml;
		public static readonly Dictionary<string,mech::Encounter.yml> story_yaml;
		public static readonly Dictionary<string,maps::Room.yml> rooms_yaml;
		public static readonly Dictionary<string,maps::Area.yml> areas_yaml;
		public static readonly Dictionary<string,invt::Item.yml> items_yaml;
		public static Camera mainCamera;
		public static Player player;
		public static Terminal terminal;

		static Pathways() {
			mainCamera = Camera.main;
			paths_yaml = DeserializeYAML(
				"/Assets/PathwaysEngine/pathways.yml");
			story_yaml = DeserializeYAML<mech::Encounter.yml>(
				"/Assets/PathwaysEngine/Mechanics/encounters.yml");
			rooms_yaml = DeserializeYAML<maps::Room.yml>(
				"/Assets/PathwaysEngine/Mechanics/Setting/rooms.yml");
			areas_yaml = DeserializeYAML<maps::Area.yml>(
				"/Assets/PathwaysEngine/Mechanics/Setting/areas.yml");
			items_yaml = DeserializeYAML<invt::Item.yml>(
				"/Assets/PathwaysEngine/Inventory/items.yml");
		}

		public static void Restart() { /*(Application.LoadLevel(1);*/
			Terminal.Log("Restart Disabled",Formats.Command); }

		public static void Restore() {
			Terminal.LogAndWaitForInput(
				"Please enter a filename:", Formats.Alert); }

		public static bool Save(string file_name) {
			if (File.Exists(file_name)) return false;
			return true; /* do writing file part here */ }

		public static Dictionary<string,string> DeserializeYAML(string s) {
			s = Directory.GetCurrentDirectory()+s;
			if (!File.Exists(s)) throw new System.Exception("YAML: 404");
			return new Deserializer().Deserialize<Dictionary<string,string>>(
				new StringReader(string.Join("\n",File.ReadAllLines(s))));
		}

		public static Dictionary<string,T> DeserializeYAML<T>(string s) {
			s = Directory.GetCurrentDirectory()+s;
			if (!File.Exists(s)) throw new System.Exception("YAML: 404");
			return new Deserializer().Deserialize<Dictionary<string,T>>(
				new StringReader(string.Join("\n",File.ReadAllLines(s))));
		}

		public static string GetYAML(string s) {
			return HTML_Unity(paths_yaml[s].md()); }
		public static string GetYAML<T>(string s) {
			if (typeof(T)==typeof(mech::Encounter))
				return HTML_Unity(story_yaml[s].desc.md());
			if (typeof(T)==typeof(maps::Room))
				return HTML_Unity(rooms_yaml[s].desc.md());
			if (typeof(T)==typeof(maps::Area))
				return HTML_Unity(areas_yaml[s].desc.md());
			if (typeof(T)==typeof(invt::Item))
				return HTML_Unity(items_yaml[s].desc.md());
			else return default (string);
		}

		public static string HTML_Unity(string s) {
			return s
				.Replace("<p>","").Replace("</p>","")
				.Replace("<em>","<i>").Replace("</em>","</i>")
				.Replace("<strong>","<b>").Replace("</strong>","</b>")
				.Replace("<h1>","<size=36>").Replace("</h1>","</size>")
				.Replace("<h2>","<size=24>").Replace("</h2>","</size>")
				.Replace("<h3>","<size=16>").Replace("</h3>","</size>");
			}
		}

	public class Message {
		public string uuid { get; set; }
		public string desc { get; set; }
		public Formats format;

		public Message(string uuid) { this.uuid = uuid; }
		public Message(string uuid,string desc) : this(uuid) {
			this.desc = desc; }
		public Message(string uuid,string desc,Formats format) : this(uuid,desc) {
		this.format = format; }
	}

	public struct proper_name {
		public string first, last;
		public proper_name(string first, string last) {
			this.first = first; this.last = last;
		}
	}

	public static class Parser {
		public delegate void Parse(string s);
		public static UIStates UIState = UIStates.Log;
		public static readonly Dictionary<Parse,List<Regex>> sentences;
		public static List<Regex> commands = new List<Regex>() {
			new Regex(@"(help|info|about|idk|wtf)"),
			new Regex(@"(q|quit|restart|restore)"),
			new Regex(@"(c|f|g|i|l|x)"),
			new Regex(@"(e|ne|n|nw|w|sw|s|se|u|d)"),
			new Regex(@"(diagnose|diagnostic|status|invt|inventory|items)")};
		public static List<Regex> verbs = new List<Regex>() {
			new Regex(@"(get|take|pick up|grab)"),
			new Regex(@"(drop|throw|put|put down)"),
			new Regex(@"(look|see|view|examine|read|watch)"),
			new Regex(@"(move|run|walk|jump|slide|crouch|duck|go)")};
		public static List<Regex> item_sentences = new List<Regex>() {
			new Regex(@"^(take|get|pick\s+up|grab)\s+(all)?"),
			new Regex(@"^(drop|put|throw)\s+(all)?")};

		static Parser() {
			var dict = new Dictionary<Parse,List<Regex>>(){
				{Take,item_sentences}}; sentences = dict;}

		public static string Strip(string s) {
			return s.ToLower().Replace("\bthe\b").Replace("\ba\b"); }

		static void Take() {
			foreach (var item in Player.nearbyItems) Player.Take(item); }
		public static void Take(string s) {
			if ((new Regex(@"all")).IsMatch(s)) Take();
			else foreach (var item in Player.nearbyItems)
				if (s==item.name) { Player.Take(item); return;
			}
		}

		public static void eval(string s) {
			s = Strip(s);
			foreach (var kvp in sentences)
				foreach (var sentence in kvp.Value)
					if (sentence.IsMatch(s)) { kvp.Key(s); return; }
			switch (s) {
				case "help": case "info":
				case "init": case "welcome":
					Terminal.Log(Pathways.GetYAML(s)); break;
				case "quit": Application.Quit(); break;
				case "restart": Pathways.Restart(); break;
				case "restore": Pathways.Restore(); break;
				case "i": case "invt": case "inventory":
					Pathways.player.holdall.Log(); break;
			}
		}
	}

	public static class Extension {
		public static void Log(this mech::Encounter e) {
			Terminal.Log(e.desc.md()); }
		public static void Log(this invt::IItemSet itemSet) {
			Terminal.Log(string.Format("It contains: ",itemSet));
			foreach (var item in itemSet)
				Terminal.Log(string.Format("\n- {0}",item)); }
		public static void Log(this Player.Holdall holdall) {
			Terminal.Log("You are holding: ");
			foreach (var item in holdall)
				Terminal.Log(string.Format("\n- {0}",item)); }
		public static void Log(this maps::Room r) { Terminal.Log(r.desc); }
		public static void Log(this maps::Connector rc) { Terminal.Log(rc.desc); }
		public static string Replace(this string s, string newValue) {
			return s.Replace(newValue,""); }
		public static void Log(this Message m) { Terminal.Log(m.desc,m.format); }

		public static void GetYAML(this mech::Encounter e) {
			mech::Encounter.yml temp;// = new mech::Encounter.yml();
			if (Pathways.story_yaml.TryGetValue(e.uuid,out temp))
				temp.Deserialize(e);
			else throw new System.Exception("Encounter not found.");
		}
	}

	namespace Inventory {
		public enum ItemStates : byte { Unused, Tarnished, Damaged, Broken }

		public interface IItem {
			bool seen { get; set; }
			bool held { get; set; }
			string name { get; set; }
			string desc { get; set; }
			void Find(); void View();
			void Take(); void Drop();
		}

		public interface IItemGroup<T> : IItem {
			uint count { get; set; }
			void Group();
			IItemGroup<T> Split(uint n);
		}

		public interface IUsable : IItem {
			uint uses { get; set; }
			void Use();
		}

		public interface IGainful : IItem {
			int cost { get; set; }
			void Buy(); void Sell();
		}

		public interface IReadable : IUsable {
			bool read { get; set; }
			void Read();
		}

		public interface IEquippable : IUsable {
			bool worn { get; set; }
			void Equip(); void Stow();
		}

		public interface IWieldable : IEquippable { void Attack(); }

		public interface IItemSet : IDictionary<type,List<Item>> {
			List<Item> GetItemsOfType<T>() where T : Item;
			bool Contains(Item item);
			Item GetItemOfType<T>() where T : Item;
			void Add(Item item); void Remove(Item item);
		}

		public interface IItemCollection : ICollection {
			ItemSet items { get; } //<Item> {
			Item GetItemOfType<T>() where T : Item;
			List<Item> GetItemsOfType<T>() where T : Item;
		}
#if ITEMS
		public static class Items {
			public static readonly IItemSet items;

			static Items() {
				var dict = new Dictionary<type,Item[]>() {
					{typeof(Item),Object.FindObjectsOfType<Item>() as Item[]}};
				foreach (var elem in dict[typeof(Item)]) {
					if (elem.GetType()!=typeof(Item))
						dict[elem.GetType()] = GetItemsOfType(
							elem.GetType(),dict);
				} items=dict; // readonlys must be constructed here
			}

			public static Item GetItemOfType<T>() where T : Item {
				return GetItemOfType<T>(items);
			}

			public static Item GetItem<T>(IItemSet dict) where T : Item {
				Item[] temp;
				if (dict.TryGetValue(typeof(T),out temp)
				&& temp!=null && temp.Length>0 && temp[0])
					return temp[0];
				else return default (Item);
			}

			public static Item[] GetItemsOfType<T>() where T : Item {
				return GetItemsOfType(typeof(T),items);
			}

			public static Item[] GetItems<T>(IItemSet dict) {
				List<Item> temp = new List<Item>();
				if (typeof(T)==typeof(Item)
				&& dict.ContainsKey(typeof(T)))
					return dict[typeof(Item)];
				if (typeof(T).IsSubclassOf(typeof(Item))
				&& dict.ContainsKey(typeof(T)))
					return dict[typeof (T)];
				foreach (var elem in dict[typeof(Item)])
					if (elem.GetType()==typeof (T)) temp.Add(elem);
				return temp.ToArray();
			}

			static Item[] GetItemsOfType(type T, IItemSet dict) {
				List<Item> temp = new List<Item>();
				if (T==typeof(Item) && dict.ContainsKey(T))
					return dict[typeof(Item)];
				if (T.IsSubclassOf(typeof(Item))
				&& dict.ContainsKey(T)) return dict[T];
				foreach (var elem in dict[typeof(Item)])
					if (elem.GetType()==T) temp.Add(elem);
				return temp.ToArray();
			}

			static Item[] GetItemsOfType(type T) {
				return GetItemsOfType(T,items); }
		}
#endif

		public class ItemSet : IItemSet {
			public bool IsReadOnly { get { return false; } }
			public int Count { get { return items.Count; } }
			public Dictionary<type,List<Item>> items {
				get { return _items; }
			} Dictionary<type,List<Item>> _items;
			public ICollection<type> Keys {
				get { return items.Keys; } }
			public ICollection<List<Item>> Values {
				get { return items.Values; } }

			public ItemSet() {
				_items = new Dictionary<type,List<Item>>() {
					{typeof(Item),new List<Item>()}};}
			public ItemSet(List<Item> items) {
				this._items = new Dictionary<type,List<Item>>() {
					{typeof(Item), items}};}
			public ItemSet(Dictionary<type,List<Item>> items) {
				this._items = items;
				if (!_items.ContainsKey(typeof(Item)))
					_items[typeof(Item)] = new List<Item>(); }

			public List<Item> this[type T] {
				get { return items[T]; }
				set { items[T] = (List<Item>) value; } }

			/* begin engine specific */
			public void Add(Item item) {
				if (!items.ContainsKey(item.GetType()))
					items[item.GetType()] = new List<Item>();
				items[item.GetType()].Add(item); }
			public void Add(type T,List<Item> list) {
				items[T].AddRange(list); }

			public bool Contains(Item item) {
				var temp = items[item.GetType()];
				return (temp!=null && temp.Contains(item)); }

			public void Remove(Item item) {
				items[item.GetType()].Remove(item); }
			public void Remove(List<Item> list) {
				if (list==null) return;
				if (ContainsKey(list[0].GetType()))
					items[list[0].GetType()] = null; }
			/* end engine specific */

			public void Add(KeyValuePair<type,List<Item>> kvp) {
				items.Add(kvp.Key,kvp.Value); }
			public void Clear() { items.Clear(); }
			public bool Contains(KeyValuePair<type,List<Item>> kvp) {
				foreach (var elem in items)
					if (elem.Key==kvp.Key && elem.Value==kvp.Value)
						return true;
				return false;
			}

			public bool ContainsKey(type T) { return items.ContainsKey(T); }
			public bool ContainsValue(List<Item> list) {
				return items.ContainsValue(list); } //items.CopyTo(kvps,n); }
			public void CopyTo(KeyValuePair<type,List<Item>>[] kvps, int n) { }
			public bool Remove<T>() { return Remove(typeof (T)); }
			public bool Remove(type T) { return items.Remove(T); }
			public bool Remove(KeyValuePair<type,List<Item>> kvp) {
				return items.Remove(kvp.Key); }

			public bool TryGetValue<T>(out List<Item> list) where T : type {
				foreach (var elem in items.Keys) {
					if (elem==typeof (T)) {
						list = items[typeof (T)];
						return true; }
				} list = default(List<Item>); return false;
			}

			public bool TryGetValue(type T, out List<Item> list) {
				if (items.ContainsKey(T)) { list = items[T]; return true; }
				else { list = default(List<Item>); return false; } }

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator(); }

			public IEnumerator<KeyValuePair<type,List<Item>>> GetEnumerator() {
				return GetEnumerator(); }

			public Item GetItemOfType<T>() where T : Item {
				var temp = GetItemsOfType<T>();
				if (temp!=null) return temp[0];
				else return default(Item); }

			public List<Item> GetItemsOfType<T>() where T : Item {
				var temp = new List<Item>();
				if (items.TryGetValue(typeof (T), out temp))
					return items[typeof (T)];
				return default(List<Item>); }

			public class ItemSetEnum : IEnumerator {
				List<Item> _items;
				int position = -1;

				public object Current {
					get {
						try { return _items[position]; }
						catch (System.IndexOutOfRangeException) {
							throw new System.Exception();
						}
					}
				}

				public ItemSetEnum(Item[] list) { this._items = new List<Item>(list); }
				public ItemSetEnum(List<Item> list) { this._items = list; }
				public bool MoveNext() { position++; return (position<_items.Count); }
				public void Reset() { position = -1; }
			}
		}

		public partial class Item : MonoBehaviour {
			public class yml {
				/* base properties */
				public int cost {get;set;}
				public float mass {get;set;}
				public string uuid {get;set;}
				public string desc {get;set;}

				/* crystal properties */
				public uint shards {get;set;}

				public void Deserialize(Item o) {
					o.desc = this.desc; //o.uuid = this.uuid;
					o.cost = this.cost; o.mass = this.mass;
				}
			}
		}
	}

	namespace Mechanics {
		struct hit { public int @value; public Hits crit;
			public hit(int @value, Hits crit) {
				this.@value = @value; this.crit = crit; } }

		public enum Hits : byte { Miss, Graze, Hit, Crit }

		public enum Corpus : byte {
			Head  = 0x0, Neck  = 0x1,
			Chest = 0x2, Back  = 0x3,
			Waist = 0x4, Frock = 0x5,
			Arms  = 0x6, Legs  = 0x7,
			Hands = 0x8, Feet  = 0x9,
			HandL = 0xA, HandR = 0xB,
			Other = 0xE, All   = 0xF };

		public enum StatTypes : byte {
			Endurance  = 0x00, Strength   = 0x01,
			Agility    = 0x02, Dexterity  = 0x03,
			Charisma   = 0x04, Perception = 0x05,
			Intellect  = 0x06, Memory	  = 0x07 };

		[Flags] public enum Severity : byte {
			None 	 = 0x00, Mild 	= 0x01,
			Moderate = 0x02, Severe = 0x03 };

		[Flags] public enum Faculties : byte {
			Thinking = 0x00, Breathing = 0x04,
			Moving	 = 0x08, Seeing	   = 0x0C,
			Walking	 = 0x10, Jumping   = 0x14 };

		[Flags] public enum Condition : byte {
			Dead	  = 0x00, Wounded  = 0x04,
			Shocked	  = 0x08, Poisoned = 0x0C,
			Psychotic = 0x10, Stunned  = 0x14,
			Injured   = 0x18, Healthy  = 0x1C };

		[Flags] public enum Diagnosis : byte {
			None 	  	 = 0x00, Unknown 	  = 0x04,
			Polytrauma 	 = 0x08, Paralysis	  = 0x0C,
			Necrosis 	 = 0x10, Infection	  = 0x14,
			Fracture	 = 0x18, Ligamentitis = 0x1C,
			Radiation	 = 0x20, Enterotoxia  = 0x24,
			Hypovolemia	 = 0x28, Hemorrhage	  = 0x2C,
			Frostbite	 = 0x30, Thermosis	  = 0x34,
			Hypothermia	 = 0x38, Hyperthermia = 0x3C,
			Hypohydratia = 0x40, Inanition	  = 0x44,
			Psychosis	 = 0x48, Depression   = 0x4C };

		public enum Prognosis : byte {
			None 	   = 0x00, Unknown	= 0x04,
			Fatal	   = 0x08, Mortal	= 0x0C,
			Grievous   = 0x10, Critical = 0x14,
			Survivable = 0x18, Livable	= 0x1C };

		public interface ILiving {
			bool dead { get; set; }
			StatSet stats { get; set; }
			void ApplyDamage(float damage);
		}

		public class Stat {
			public StatTypes statType;
			public bool Check() { return true; }
			public bool Check(Stat stat) { return true; }
			protected uint @value { get; set; }

			public Stat() { }
			public Stat(StatTypes statType) { this.statType = statType; }
			public Stat(StatTypes statType, uint @value) {
				this.statType = statType; this.@value = @value; }
		}

		public class StatSet : Stat, ICollection {
			List<Stat> stats;
			public bool IsSynchronized { get { return false; } }
			public int Count { get { return stats.Count; } }
			public object SyncRoot { get { return default (object); } }

			public StatSet() { }
			public StatSet(Stat[] stats) { this.stats = new List<Stat>(stats); }
			public StatSet(List<Stat> stats) { this.stats = stats; }

			public Stat this[string s] {
				get { foreach (var elem in stats)
						if (s==elem.GetType().ToString()) return elem;
					return default (Stat);
				}
			}

			public Stat this[type T] {
				get { foreach (var elem in stats)
						if (T==elem.GetType()) return elem;
					return default (Stat);
				}
			}

			public void CopyTo(System.Array arr, int i) { arr = stats.ToArray(); }

			public IEnumerator GetEnumerator() { return stats.GetEnumerator(); }
		}

		public class HealthStats : StatSet {
			Faculties faculties { get; set; }
			Condition condition { get; set; }
			Diagnosis diagnosis { get; set; }
			Prognosis prognosis { get; set; }
			Severity[] severityFaculties { get; set; }
			Severity[] severityCondition { get; set; }
			Severity[] severityDiagnosis { get; set; }
			Severity[] severityPrognosis { get; set; }

			public HealthStats(StatTypes statType) {
				this.statType = statType; }
			public HealthStats(StatTypes statType, uint @value) {
				this.statType = statType; this.@value = @value; }

			public void AddCondition(Condition cond) { }
			public void AddCondition(Condition cond,Severity svrt) { }
			public void AddConditions(params Condition[] conds) {
				foreach (var cond in conds) AddCondition(cond); }
		}

		public partial class Encounter : MonoBehaviour {
			public class yml {
				public bool reuse {get;set;}
				public float time {get;set;}
				public string uuid {get;set;}
				public string desc {get;set;}
				public Inputs input {get;set;}
				public Outputs output {get;set;}

				public void Deserialize(Encounter o) {
					o.reuse = this.reuse;  o.time   = this.time;
					o.desc  = this.desc;   //o.uuid  = this.uuid;
					o.input = this.input;  o.output = this.output;
				}
			}
		}
	}

	namespace Mechanics.Setting {
		public partial class Room : MonoBehaviour {
			public class yml {
				public string uuid {get;set;}
				public string desc {get;set;}
				public List<invt::Item> items {get;set;}
				public List<Connector> adjacents {get;set;}

				public void Deserialize(Room o) {
					o.uuid  = this.uuid;   o.desc      = this.desc;
					o.items = this.items;  o.adjacents = this.adjacents;
				}
			}
		}

		public partial class Area : MonoBehaviour {
			public class yml {
				public string uuid {get;set;}
				public string desc {get;set;}
				public bool safe {get;set;}
				public int level {get;set;}
				public List<Room> rooms {get;set;}
				public List<Area> areas {get;set;}

				public void Deserialize(Area o) {
					o.safe  = this.safe;   o.level = this.level;
					o.desc  = this.desc;   //o.uuid  = this.uuid;
					o.rooms = this.rooms;  o.areas = this.areas;
				}
			}
		}
	}


	namespace Movement {
		public enum Hands : byte { Left, Right };
	}

	namespace Utilities {
		public delegate void InputKey(bool value);
		public struct key {
			public bool @get;
			public bool input;
			public InputKey f;
			public key (InputKey _f) {
				@get = true; input = false; f = _f; }
		}

		public delegate void InputAxis(float value);
		public struct axis {
			public bool @get;
			public float input;
			public InputAxis f;
			public axis (InputAxis _f) {
				@get = true; input = 0f; f = _f; }
		}
	}
}

namespace dev {
	public class Point {
		public float x { get; set; }
		public float y { get; set; }
		public float z { get; set; }

		public Point() { x = 0; y = 0; z = 0; }
		public Point(float x,float y,float z) {
			this.x = x; this.y = y; this.z = z; }

		public void draw() {
			Draw.Line(
				new Vector3(x-0.1f,y+0.1f,z-0.1f),
				new Vector3(x+0.1f,y-0.1f,z+0.1f));
			Draw.Line(
				new Vector3(x-0.1f,y-0.1f,z-0.1f),
				new Vector3(x+0.1f,y+0.1f,z+0.1f));
		}
	}

	public static class Draw {
		static Draw() { }

		static public void Line(Vector3 x,Vector3 y) { Line(x,y,Color.white); }
		static public void Line(Vector3 x,Vector3 y,Color c) { Debug.DrawLine(x,y,c); }

		//static public void Point(Vector3 p) { new Point(p); }

		static public void Circle(Vector3 c, float r) {
			for (int i=0;i<36;++i) {
				var s0 = new Vector3(
					Mathf.Cos((i*10*r)/Mathf.Rad2Deg)+c.x,c.y,
					Mathf.Sin((i*10*r)/Mathf.Rad2Deg)+c.z);
				var s1 = new Vector3(
					Mathf.Cos(((i+1)*10*r)/Mathf.Rad2Deg)+c.x,c.y,
					Mathf.Sin(((i+1)*10*r)/Mathf.Rad2Deg)+c.z);
				Line(s1,s0,Color.white);
			}
		}

		static public void Cylinder(Vector3 c, float r, float h) {
			Circle(c,r); Circle(new Vector3(c.x,c.y+h,c.z), r);
			Line(new Vector3(c.x+r,c.y,c.z),new Vector3(c.x+r,c.y+h,c.z));
			Line(new Vector3(c.x-r,c.y,c.z),new Vector3(c.x-r,c.y+h,c.z));
			Line(new Vector3(c.x,c.y,c.z+r),new Vector3(c.x,c.y+h,c.z+r));
			Line(new Vector3(c.x,c.y,c.z-r),new Vector3(c.x,c.y+h,c.z-r));
		}
	}
}

#if Advanced
		int i;
		for(i=0;i<vertP;i++) {
			fv[i]=newVertex[i];fn[i]=newNormal[i];
			fuv[i]=tada2[tadac2++];
			Vector3 fuvt=transform.TransformPoint(fn[i]).normalized;
			fuv[i].x=(fuvt.x+1f)*.5f;fuv[i].y=(fuvt.y+1f)*.5f;}
//			fuv[i].x=fn[i].x;fuv[i].y=fn[i].y;}

		for(i=vertP;i<fv.Length;i++) {
			fv[i][0]=0;fn[i][0]=0;fuv[i][0]=0;
			fv[i][1]=0;fn[i][1]=0;fuv[i][1]=0;
			fv[i][2]=0;}

		for(i=0;i<triP;i++) {ft[i]=newTri[i];}
		for(i=triP;i<ft.Length;i++) {ft[i]=0;}

		Mesh mesh=((MeshFilter) GetComponent("MeshFilter")).mesh;
	    mesh.vertices = fv ;
	    mesh.uv = fuv;
		mesh.triangles = ft;
	    mesh.normals = fn;

	    /*For Disco Ball Effect*/
	    //mesh.RecalculateNormals();\
#endif


