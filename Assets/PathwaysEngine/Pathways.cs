/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Pathways */

using UnityEngine; // Well, here we are! The main file! The big one!
using ui=UnityEngine.UI; // The Pathways Engine, just like I pictured it!
using System.Linq; // This file contains most of the enums, interfaces,
using System.Collections; // and other loose bits that shouldn't be put
using System.Collections.Generic; // in the individual class files.
using System.IO; // if you can't find some global variable or you want
using System.Text.RegularExpressions; // a direct reference to the
using type=System.Type; // player, it's probably in here.
using Flags=System.FlagsAttribute;
using Buffer=System.Text.StringBuilder;
using DateTime=System.DateTime;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using invt=PathwaysEngine.Inventory;
using intf=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine { //[Flags]
	public enum GameStates { None, Game, Term, Msgs, Menu }

	public enum Formats : int {
		Default=0xFFFFFF, State=0x2A98AA, Change=0xFFAE10,
		Alert=0xFC0000, Command=0x999999, h1=36, h2=28, h3=24,
		Newline=0, Inline=1, Refresh=2 };

	public delegate void StateHandler(
		object sender,System.EventArgs e,GameStates gameState);

 	public static class Pathways {
		public static DateTime gameDate, finalDate;
		public static Camera mainCamera;
		public static Player player;
		public static MessageWindow messageWindow;
		public static Terminal terminal;
		public static event StateHandler StateChange;

		public static GameStates gameState {
			get { return _gameState; }
			set { if (_gameState!=value) _lastState = _gameState;
				_gameState = value;
				OnStateChange(System.EventArgs.Empty,_gameState);
			}
		} static GameStates _gameState = GameStates.Game;

		public static GameStates lastState {
			get { return _lastState; }
		} static GameStates _lastState = GameStates.Game;

		static Pathways() {
			mainCamera = Camera.main;
			gameDate = new DateTime(1994,5,8,2,0,0);
			finalDate = new DateTime(1994,5,13,14,0,0);
			StateChange += new StateHandler(EventListener);
		} /*(Application.LoadLevel(1);*/

		public static void OnStateChange( // master event
		System.EventArgs e,GameStates gameState) {
			if (StateChange!=null)
				StateChange(default (object),e,gameState);
		}

		public static void EventListener(
		object sender, System.EventArgs e,GameStates gameState) {
			Cursor.visible = (gameState==GameStates.Msgs
				|| gameState==GameStates.Menu);
		}

		public static void Log(string s) {
			Terminal.Log((intf::Message) yaml.data[s]); }

		public static void Redo(command cmd) { intf::Parser.eval(cmd.input); }

		public static void Quit(command cmd) {
			Terminal.Alert("Quit Pathways into Darkness?",Formats.Alert); }

		public static void Load(command cmd) { Terminal.Log("I/O Disabled."); }

		public static void Save(command cmd) {
			if (File.Exists(cmd.input)) Terminal.Alert("Overwriting file...");
			using (StreamWriter file = new StreamWriter(cmd.input)) {
				file.WriteLine("%YAML 1.1");
				file.WriteLine("%TAG !invt! _PathwaysEngine.Inventory.");
				file.WriteLine("%TAG !maps! _PathwaysEngine.Adventure.Setting.");
				file.WriteLine(string.Format("---  # {0}\n",
					new P_ID("Saved_Game_00000",DateTime.Now)));
				file.WriteLine("player:");
				file.WriteLine(string.Format("  position: {0}",
					new vect_3(Pathways.player.position)));
				file.WriteLine(string.Format(
					"  area: {0}",Player.area));
				file.WriteLine(string.Format(
					"  room: {0}",Player.room));
				file.WriteLine("  holdall:\n");
				foreach (var elem in Player.holdall)
					file.WriteLine(string.Format("  - {0}",elem.uuid));
				file.WriteLine("\n...\n");
			}
		} /*(s) => (s.Length>100)?(s.Substring(0,100)+"&hellip;"):(s); */

		public static void Help(command cmd) {
			Terminal.Display((intf::Message) yaml.data["help"]); }

		public static class yaml {
			public static Dictionary<string,object> data;

			static yaml() {
				data = new Dictionary<string,object>();
				var deserializer = new Deserializer();
				foreach (var elem in Directory.GetFiles(
						Directory.GetCurrentDirectory()
						+"/Assets/PathwaysEngine/Data/","*.yml")) {
					if (!File.Exists(elem))
						throw new System.Exception("YAML: 404");
					var buffer = new Buffer();
					foreach (var line in File.ReadAllLines(elem))
						buffer.AppendLine(line);
					foreach (var kvp in deserializer
							.Deserialize<Dictionary<string,object>>(
								new StringReader(buffer.ToString())))
						data[kvp.Key] = kvp.Value;
				}
			}

			public static T GetYAML<T>(string s) {
				object temp;
				if (data.TryGetValue(s,out temp)) return ((T) temp);
				else throw new System.Exception("404 : ");
			}
		}
	}

	public class RandList<T> : List<T> {
		System.Random random = new System.Random();
		public T GetRandom() {
			return this[random.Next(this.Count())]; }
	}

	public static class Extension {
		public static string title(this string s) {
			return s.Replace('_',' '); }

		public static string md(this string s) {
			var buffer = new Buffer(Markdown.Transform(s));
			return buffer
				.Replace("<em>","<i>").Replace("</em>","</i>")
				.Replace("<strong>","<b>").Replace("</strong>","</b>")
				.Replace("<h1>","<size=36>").Replace("</h1>","</size>")
				.Replace("<h2>","<size=24>").Replace("</h2>","</size>")
				.Replace("<h3>","<size=16>").Replace("</h3>","</size>")
				.Replace("<ul>","").Replace("</ul>","")
				.Replace("<li>","").Replace("</li>","")
				.Replace("<p>","").Replace("</p>","").ToString();
		}

		public static void Log(this invt::IItemSet itemSet) {
			Terminal.Log(string.Format("It contains: ",itemSet));
			foreach (var item in itemSet)
				Terminal.Log(string.Format("\n- {0}",item)); }
		public static void Log(this Player.Holdall holdall) {
			Terminal.Log("You are holding: ");
			foreach (var item in holdall)
				Terminal.Log(string.Format("\n- {0}",item)); }

		public static string Replace(this string s, string newValue) {
			return s.Replace(newValue,""); }
		public static string Strip(this string s) {
			return s.Trim().ToLower().Replace("\bthe\b").Replace("\ba\b"); }
		public static List<string> Process(this string s) {
			return new List<string>(s.Strip().Split('.')); }

		public static void GetYAML(this intf::Encounter o) {
			Pathways.yaml.GetYAML<intf::Encounter.yml>(o.uuid).Deserialize(o); }
		public static void GetYAML(this invt::Lamp o) {
			Pathways.yaml.GetYAML<invt::Lamp.yml>(o.uuid).Deserialize(o); }
		public static void GetYAML(this maps::Area o) {
			Pathways.yaml.GetYAML<maps::Area.yml>(o.uuid).Deserialize(o); }
		public static void GetYAML(this maps::Room o) {
			Pathways.yaml.GetYAML<maps::Room.yml>(o.uuid).Deserialize(o); }
		public static void GetYAML(this intf::Thing o) {
			Pathways.yaml.GetYAML<intf::Thing.yml>(o.uuid).Deserialize(o); }
	}

	public struct P_ID { // hehe, get it? PiD!
		public string @name { // ! enforce length, not sure if I care
			get { return string.Format(
				"pathways-{1:yyyy-mm-dd}-{0}",_name,date); }
			private set { _name = value; }
		} private string _name;
		public DateTime date;
		public P_ID(string name) {
			this.@name = name; date = new DateTime(1994,5,8); }
		public P_ID(string name, DateTime date)
			: this(name) { this.date = date; }
		public override string ToString() { return @name; }
	}

	public struct proper_name {
		public string first, last;
		public proper_name(string first, string last) {
			this.first = first; this.last = last; } }

	public struct command {
		public string name,input;
		public Regex regex;
		public intf::Parse parse;
		public command(string name,Regex regex,intf::Parse parse) {
			this.name = name; this.regex = regex;
			this.parse = parse; this.input = ""; }
		public command(	string name,Regex regex,
						intf::Parse parse,string input)
			: this(name,regex,parse) { this.input = input; }
		public command(string name,string regex,intf::Parse parse)
			: this(name,new Regex(regex),parse) { }
		public command(	string name,string regex,
						intf::Parse parse,string input)
			: this(name,new Regex(regex),parse) { this.input = input; }
		public command(string name,intf::Parse parse,string regex)
			: this(name,new Regex(regex),parse) { }
		public command(	string name,intf::Parse parse,
						string regex,string input)
			: this(name,new Regex(regex),parse) { this.input = input; }
	}

	public struct vect_3 {
		public float x,y,z;
		public vect_3(float x,float y,float z) {
			this.x = x; this.y = y; this.z = z; }
		public vect_3(Vector3 vector) {
			this.x = vector.x; this.y = vector.y; this.z = vector.z; }
		public override string ToString() {
			return string.Format("<{0},{1},{2}>",x,y,z); } }


	namespace Adventure {

		public enum Corpus : byte {
			Head  = 0x0, Neck  = 0x1,
			Chest = 0x2, Back  = 0x3,
			Waist = 0x4, Frock = 0x5,
			Arms  = 0x6, Legs  = 0x7,
			Hands = 0x8, Feet  = 0x9,
			HandL = 0xA, HandR = 0xB,
			Other = 0xE, All   = 0xF};

		public delegate void Parse(command cmd);
		public delegate string Desc_Style(Desc<Thing> desc);

		public interface IStorable { string uuid {get;} }

		public interface ILiving : IStorable {
			bool dead {get;set;}
			stat::Set stats {get;set;}
			void ApplyDamage(float damage);
		}

		public static class Parser {
			public enum Kinds { All, Item, Examine, Move, Room }

			public static readonly Dictionary<Kinds,Regex> sentences;
			public static readonly Dictionary<string,command> commands;

			static Parser() {
				var temp = new List<command> {
					new command("quit", Pathways.Quit,@"\b(quit|restart)\b"),
					new command("again", Pathways.Redo,@"\b(again|redo)\b"),
					new command("load", Pathways.Load,@"\b(load|restore)\b"),
					new command("save", Pathways.Save,@"\b(save)\b"),
					new command("help", Pathways.Help,
						@"\b(help|info|about|idk)\b"),
					new command("status", Player.View,
						@"\b(c|diagnos(e|tic)|status)\b"),
					new command("invt", Player.View,
						@"\b(i(nvt|nventory|tems)?)\b"),
					new command("look", Player.View,
						@"\b(x|examine|l(ook)?|view)\b"),
					/*new command("move", Player.Move,
						@"\b(move|run|walk|jump|go)\b"),
					new command("travel", Player.Travel,
						@"\b(e|ne|n|nw|w|sw|s|se|u|d)\b"),*/
					new command("take", Player.Take,
						@"\b(take|get|pick\s+up|grab)\b"),
					new command("drop", Player.Drop,
						@"\b(drop|put(\s+down)?|throw)\b"),
					new command("wear", Player.Wear,
						@"\b(equip|don|wear|put\s+(on)?|sport)\b"),
					new command("stow", Player.Stow,
						@"\b(stow|put(\s+away)|remove|un(equip|load))\b"),
					new command("read", Player.Read,
						@"\b(read|watch|glance)\b")};
				commands = new Dictionary<string,command>();
				foreach (var elem in temp) commands.Add(elem.name,elem);
			}

			public static void eval(string s) {
				foreach (var elem in s.Process()) {
					if (string.IsNullOrEmpty(elem)) continue;
					foreach (var kvp in commands.Values) {
						if (kvp.regex.IsMatch(elem)) {
							Pathways.gameState = GameStates.Game;
							kvp.parse(new command(
								kvp.name,kvp.regex,kvp.parse,elem));
							return;
						}
					} Terminal.Log(" > "+s+": Do what, exactly?\n",Formats.Command);
					Pathways.gameState = GameStates.Game;
				}
			}
		}

		public partial class Creature : Thing, ILiving {
			public new class yml : Thing.yml {
				public bool dead {get;set;}

				public void Deserialize(Creature o) {
					base.Deserialize((Thing) o);
					o.dead = dead;
				}
			}
		}

		public partial class Encounter : Thing {
			public new class yml : Thing.yml {
				public bool reuse {get;set;}
				public float time {get;set;}
				public Inputs input {get;set;}

				public void Deserialize(Encounter o) {
					base.Deserialize((Thing) o);
					o.reuse = this.reuse;  o.time = this.time;
					o.input = this.input;
				}
			}
		}

		public partial class Thing : MonoBehaviour, IStorable {
			public class yml : IStorable {
				public string uuid {get;set;}
				public Desc<Thing> desc {get;set;}
				public void Deserialize(Thing o) {
					o.desc = this.desc;
				}
			}
		}

		public class Desc<T> where T : Thing {
			bool seen = false;
			public Formats[] formats {get;set;}

			[YamlMember(Alias="description")]
			public string desc {
				get { return _desc; }
				set { _desc_base = value; } //Refresh(); }
			} protected string _desc, _desc_base;

			[YamlMember(Alias="init_description")]
			public string init {get;set;}
			public string nouns {get;set;} // wants regex
			[YamlMember(Alias="rand_descriptions")]
			public RandList<string> rand {get;set;}

			public bool IsSynonymousWith(string s) {
				return new Regex(nouns).IsMatch(s); }

			public Desc() { desc = " "; init = " "; }
			public Desc(string desc)
				: this() { this.desc = desc; }
			public Desc(string desc,string init)
				: this(desc) { this.init = init; }
			public Desc(string desc,string init,
			params Formats[] formats) : this(desc,init) {
				this.formats = formats; }

			public virtual void SetFormat(string s) {
				_desc = Terminal.Format(string.Format(s,
					(seen || init!=null)?(_desc_base):(init)).md());
			}

			public void AddNouns(string s) {
				if (string.IsNullOrEmpty(s)) return;
				nouns = new Regex(nouns.ToString()+s).ToString();
			}

			public override string ToString() { return desc; }
		}

		public class Message : IStorable {
			public string uuid {get;set;}
			public string desc {
				get { return _desc; }
				set { _desc = Terminal.Format(value.md(),formats); }
			} string _desc;

			public Formats[] formats {get;set;}

			public Message() { uuid = ""; }
			public Message(string uuid) { this.uuid = uuid; }
			public Message(string uuid,string desc)
				: this(uuid) { this.desc = desc; }
			public Message(string uuid,string desc,params Formats[] format)
				: this(uuid,desc) { this.formats = formats; }
		}




		namespace Setting {
			public partial class Room : Thing {
				public new class yml : Thing.yml {
					public int depth {get;set;}
					public List<invt::Item> items {get;set;}
					public List<maps::Room> nearbyRooms {get;set;}

					public void Deserialize(Room o) {
						base.Deserialize((Thing) o);
						o.items = this.items;
						o.nearbyRooms = this.nearbyRooms;
					}
				}
			}

			public partial class Area : Thing {
				public new class yml : Thing.yml {
					public bool safe {get;set;}
					public int level {get;set;}
					public List<Room.yml> rooms {get;set;}
					public List<Area.yml> areas {get;set;}

					public void Deserialize(Area o) {
						base.Deserialize((Thing) o);
						o.safe = this.safe;  o.level = this.level;
						//o.rooms = this.rooms;  o.areas = this.areas;
					}
				}
			}
		}
	}





	namespace Inventory {
		public enum ItemStates : byte { Unused, Tarnished, Damaged, Broken }

		public interface IItem : intf::IStorable {
			bool held {get;set;}
			void Take(); void Drop();
		}

		public interface IItemGroup<T> : IItem {
			uint count {get;set;}
			void Group();
			IItemGroup<T> Split(uint n);
		}

		public interface IUsable : IItem {
			uint uses {get;set;}
			void Use();
		}

		public interface IGainful : IItem {
			int cost {get;set;}
			void Buy(); void Sell();
		}

		public interface IReadable : IUsable {
			bool read {get;set;}
			void Read();
		}

		public interface IWearable : IItem {
			bool worn {get;set;}
			void Wear(); void Stow();
		}

		public interface IWieldable : IWearable, IUsable { void Attack(); }

		public interface IItemSet : ICollection<Item>, IEnumerable<Item> {
			List<Item> GetItemsOfType<T>() where T : Item;
			Item GetItemOfType<T>() where T : Item;
		}

		public static class Items {
			public static readonly IItemSet items;

			static Items() {
#if Index_Items
				var dict = new Dictionary<type,Item[]>() {
					{typeof(Item),Object.FindObjectsOfType<Item>() as Item[]}};
				foreach (var elem in dict[typeof(Item)]) {
					if (elem.GetType()!=typeof(Item))
						dict[elem.GetType()] = GetItemsOfType(
							elem.GetType(),dict);
				} items=dict; // readonlys must be constructed here
#endif
			}
#if TODO
			public static Item GetItem<T>() where T : Item {
				return GetItem<T>(items); }

			public static Item GetItem<T>(IItemSet dict) where T : Item {
				List<Item> temp;
				if (dict.TryGetValue(typeof(T),out temp)
				&& temp!=null && temp.Count>0 && temp[0])
					return temp[0];
				else return default (Item);
			}

			public static List<Item> GetItemsOfType<T>() where T : Item {
				return GetItemsOfType(typeof(T),items); }

			public static List<Item> GetItems<T>(IItemSet dict) {
				List<Item> temp = new List<Item>();
				if (typeof(T)==typeof(Item)
				&& dict.ContainsKey(typeof(T)))
					return dict[typeof(Item)];
				if (typeof(T).IsSubclassOf(typeof(Item))
				&& dict.ContainsKey(typeof(T)))
					return dict[typeof (T)];
				foreach (var elem in dict[typeof(Item)])
					if (elem.GetType()==typeof (T)) temp.Add(elem);
				return temp;
			}

			static List<Item> GetItemsOfType(type T, IItemSet dict) {
				List<Item> temp = new List<Item>();
				if (T==typeof(Item) && dict.ContainsKey(T))
					return dict[typeof(Item)];
				if (T.IsSubclassOf(typeof(Item))
				&& dict.ContainsKey(T)) return dict[T];
				foreach (var elem in dict[typeof(Item)])
					if (elem.GetType()==T) temp.Add(elem);
				return temp;
			}

			static List<Item> GetItemsOfType(type T) {
				return GetItemsOfType(T,items); }
#endif
		}

		public class ItemSet : IItemSet {
			public bool IsReadOnly { get { return false; } }
			public int Count {
				get { return items[typeof(Item)].Count; } }

			public Dictionary<type,List<Item>> items {
				get { return _items; }
			} Dictionary<type,List<Item>> _items;
			public ICollection<type> Keys {
				get { return items.Keys; } }
			public ICollection<List<Item>> Values {
				get { return items.Values; } }

			public List<Item> this[type T] {
				get { return items[T]; }
				set { items[T] = (List<Item>) value; } }

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

			/* begin engine specific */
			public void Add(Item item) {
				if (!items.ContainsKey(item.GetType()))
					items[item.GetType()] = new List<Item>();
				items[item.GetType()].Add(item); }

			public void Add(type T,List<Item> list) {
				items[T].AddRange(list); }

			public bool Contains(Item item) {
				List<Item> temp;
				return (TryGetValue<Item>(out temp)
					&& temp!=null && temp.Contains(item));
			}

			public int IndexOf(Item item) {
				return _items[typeof(Item)].IndexOf(item); }

			public bool Remove(Item item) {
				return items[item.GetType()].Remove(item); }

			public void Remove(List<Item> list) {
				if (list==null) return;
				if (ContainsKey(list[0].GetType()))
					items[list[0].GetType()] = null;
			}
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
			public void CopyTo(Item[] list, int n) {
				items[typeof(Item)].CopyTo(list,n); }
			public bool Remove<T>() { return Remove(typeof (T)); }
			public bool Remove(type T) { return items.Remove(T); }
			public bool Remove(KeyValuePair<type,List<Item>> kvp) {
				return items.Remove(kvp.Key); }

			public bool TryGetValue<T>(out List<Item> list) where T : Item {
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

			public IEnumerator<Item> GetEnumerator() {
				return items[typeof(Item)].GetEnumerator(); }

			public Item GetItemOfType<T>() where T : Item {
				var temp = GetItemsOfType<T>();
				if (temp!=null) return temp[0];
				else return default(Item); }

			public List<Item> GetItemsOfType<T>() where T : Item {
				var temp = new List<Item>();
				if (items.TryGetValue(typeof (T), out temp))
					return items[typeof (T)];
				return default(List<Item>); }

			public class ItemSetEnum {
				List<Item> _items;
				int position = -1;

				public Item Current {
					get {
						try { return _items[position]; }
						catch (System.IndexOutOfRangeException) {
							throw new System.Exception();
						}
					}
				}

				public ItemSetEnum(Item[] list) { this._items = new List<Item>(list); }
				public ItemSetEnum(List<Item> list) { this._items = list; }
				//public ItemSetEnum(List<List<Item>> lists) {
				//	this._items = new List<Item>(lists); }
				public bool MoveNext() { position++; return (position<_items.Count); }
				public void Reset() { position = -1; }
			}
		}

		public partial class Item : intf::Thing, IGainful {
			public new class yml : intf::Thing.yml {
				public int cost {get;set;}
				public float mass {get;set;}
				public void Deserialize(Item o) {
					base.Deserialize((intf::Thing) o);
					o.mass = this.mass;  o.cost = this.cost;
				}
			}
		}

		public partial class Lamp : Item, IWearable {
			public new class yml : Item.yml {
				public float time {get;set;}
				public void Deserialize(Lamp o) {
					base.Deserialize((Item) o);
					o.time = time;
				}
			}
		}

		public partial class Crystal : Item, IWieldable {
			public new class yml : Item.yml {
				public uint shards {get;set;}
				public void Deserialize(Crystal o) {
					base.Deserialize((Item) o);
					o.shards = shards;
				}
			}
		}

		public partial class Weapon : Item, IWieldable {
			public new class yml : Item.yml {
				public float rate {get;set;}
				public void Deserialize(Weapon o) {
					base.Deserialize((Item) o);
					o.rate = rate;
				}
			}
		}
	}




	namespace Movement { public enum Hands : byte { Left, Right }; }




	namespace Statistics {
		public class Stat {
			public StatTypes statType;
			public bool Check() { return true; }
			public bool Check(Stat stat) { return true; }
			protected uint @value {get;set;}

			public Stat() {  }
			public Stat(StatTypes statType) {
				this.statType = statType; }
			public Stat(StatTypes statType, uint @value)
				: this(statType) { this.@value = @value; }
		}

		public class Set : Stat, ICollection {
			List<Stat> stats;
			public bool IsSynchronized { get { return false; } }
			public int Count { get { return stats.Count; } }
			public object SyncRoot { get { return default (object); } }

			public Set() { }
			public Set(Stat[] stats) {
				this.stats = new List<Stat>(stats); }
			public Set(List<Stat> stats) {
				this.stats = stats; }

			public Stat this[string s] {
				get { foreach (var elem in stats)
						if (s==elem.GetType().ToString()) return elem;
					return default (Stat); } }

			public Stat this[type T] {
				get { foreach (var elem in stats)
						if (T==elem.GetType()) return elem;
					return default (Stat); } }

			public void CopyTo(System.Array arr, int i) {
				arr = stats.ToArray(); }

			public IEnumerator GetEnumerator() {
				return stats.GetEnumerator(); }
		}

		public class HealthStats : Set {
			Faculties faculties {get;set;}
			Condition condition {get;set;}
			Diagnosis diagnosis {get;set;}
			Prognosis prognosis {get;set;}

			public HealthStats(StatTypes statType) {
				this.statType = statType; }
			public HealthStats(StatTypes statType, uint @value) {
				this.statType = statType; this.@value = @value; }

			public void AddCondition(Condition cond) { }
			public void AddCondition(Condition cond,Severity svrt) { }
			public void AddConditions(params Condition[] conds) {
				foreach (var cond in conds) AddCondition(cond); }
		}

		public struct hit {
			public int damage; public Hits crit;
			public hit(int damage, Hits crit) {
				this.damage = damage; this.crit = crit; }
			public override string ToString() {
				return string.Format(@"~{0}: |{1}|~",crit,damage); } }

		public enum Hits : byte { Miss=0, Graze=1, Hit=2, Crit=3 }

		public enum StatTypes : byte {
			Health 	   = 0x00, Endurance  = 0x01,
			Strength   = 0x02, Agility    = 0x03,
			Dexterity  = 0x04, Perception = 0x05,
			Intellect  = 0x06, Memory	  = 0x07};

		public enum Severity : byte {
			None 	 = 0x00, Mild 	= 0x01,
			Moderate = 0x02, Severe = 0x03};

		[Flags] public enum Faculties : byte {
			Thinking = 0x00, Breathing = 0x04,
			Moving	 = 0x08, Seeing	   = 0x0C,
			Walking	 = 0x10, Jumping   = 0x14};

		[Flags] public enum Condition : byte {
			Dead	  = 0x00, Wounded  = 0x04,
			Shocked	  = 0x08, Poisoned = 0x0C,
			Psychotic = 0x10, Stunned  = 0x14,
			Injured   = 0x18, Healthy  = 0x1C};

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
			Psychosis	 = 0x48, Depression   = 0x4C};

		public enum Prognosis : byte {
			None 	   = 0x00, Unknown	= 0x04,
			Fatal	   = 0x08, Mortal	= 0x0C,
			Grievous   = 0x10, Critical = 0x14,
			Survivable = 0x18, Livable	= 0x1C};
	}




	namespace Utilities {
		[System.AttributeUsage(System.AttributeTargets.Field)]
		public class InputKeyAttribute : System.Attribute {
			public string button {get;set;}
			public InputKeyAttribute() { }
			public InputKeyAttribute(string button) {
				this.button = button; }
		}

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
		public float x {get;set;}
		public float y {get;set;}
		public float z {get;set;}

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

		public static void Line(Vector3 x,Vector3 y) { Line(x,y,Color.white); }
		public static void Line(Vector3 x,Vector3 y,Color c) { Debug.DrawLine(x,y,c); }

		//public static void Point(Vector3 p) { new Point(p); }

		public static void Circle(Vector3 c, float r) {
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

		public static void Cylinder(Vector3 c, float r, float h) {
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
			Circle(c,r); Circle(new Vector3(c.x,c.y+h,c.z), r);
			Line(new Vector3(c.x+r,c.y,c.z),new Vector3(c.x+r,c.y+h,c.z));
			Line(new Vector3(c.x-r,c.y,c.z),new Vector3(c.x-r,c.y+h,c.z));
			Line(new Vector3(c.x,c.y,c.z+r),new Vector3(c.x,c.y+h,c.z+r));
			Line(new Vector3(c.x,c.y,c.z-r),new Vector3(c.x,c.y+h,c.z-r));
		}
	}
}


