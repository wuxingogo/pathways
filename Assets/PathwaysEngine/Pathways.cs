/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-22 * Pathways */

using UnityEngine;						using UnityEngine.UI;
using System.Collections;				using System.Collections.Generic;
using System.IO; 						using type=System.Type;
using Flags=System.FlagsAttribute; 		using System.Text.RegularExpressions;
using term=PathwaysEngine.UserInterface;using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {
	public enum GameStates : byte { None=0, Game=1, Term=2, Menu=3 };

	public static class Pathways {
		public static string[] yaml_files = {"pathways.yml","Encounters.yml"};
		public static GameStates GameState = GameStates.Game;
		public static Camera mainCamera;
		public static Player player;
		public static readonly Dictionary<string,string> encounters;
		public static readonly Dictionary<string,string> pathways;
		public static term::Controls controls;
		public static term::Terminal terminal;

		static Pathways() {
			player = Object.FindObjectOfType<Player>();
			terminal = Object.FindObjectOfType<term::Terminal>();
			mainCamera = Camera.main;
			pathways = SimpleYAML("/Assets/PathwaysEngine/Manuscript/pathways.yml");
		}

		public static string GetTextYAML(string s) { return pathways[s]; }

		public static Dictionary<string,string> SimpleYAML(string file_name) {
			if (!File.Exists(Directory.GetCurrentDirectory()+file_name))
				throw new System.Exception(file_name);
			var dict = new Dictionary<string,string>();
			var map = new Regex(@"(\w+):\s*");
			using (TextReader reader=File.OpenText(Directory.GetCurrentDirectory()+file_name)) {
				var line = reader.ReadLine();
				var i = 100;
				while (line!=null && i>0) {
					i--;
					var temp = new List<string>();
					string name;
					var match = map.Match(line);
					if (match.Success) {
						name = match.Groups[1].Value;
						while (line!=null && !(new Regex(@"\n").Match(line).Success) && i>0) {
							i--;
							line = reader.ReadLine();
							temp.Add(line);
						} dict[name] = string.Join("\n",temp.ToArray());
						Debug.Log(dict[name]);
					} else line = reader.ReadLine();
				} //fileLines = temp.ToArray();
			//Debug.Log(dict["init"]);
			dict["init"] = new util::Markdown().Transform(dict["init"]);
			} return dict;
		}

		public static void ParseYAML(string file_name) {
			if (File.Exists(file_name)) { // parse, compose, construct
				using (TextReader reader = File.OpenText(file_name)) {
					var line = reader.ReadLine();
					var temp = new List<string>();
					while (line!=null) {
						temp.Add(line);
						line = reader.ReadLine();
					} //fileLines = temp.ToArray();
				}
			} else throw new System.Exception(file_name);
		}

		public struct name {
			public string first;
			public string last;
			public name(string f, string l) { first = f; last = l; }
		}
	}

	namespace Inventory {
		public enum ItemStates : byte { Unused, Tarnished, Damaged, Broken }

		public interface IItem {
			bool seen { get; set; }
			bool held { get; set; }
			string name { get; set; }
			string desc { get; set; }
			void Find();  void View();
			void Take();  void Drop();
		}

		public interface IStack : IItem {
			uint count { get; set; }
			void Stack();
			ItemStack Split(uint n);
		}

		public interface IGainful : IItem {
			bool sold { get; set; }
			int cost { get; set; }
			void Buy();  void Sell();
		}

		public interface IStorage : IItem, ICollection<Item> {
			int capacity { get; set; }
			List<Item> items { get; set; }
			void Insert();  void Remove();
			bool Holds(Item item);
		}

		public interface IUsable : IItem {
	//		bool used { get; set; }
			uint uses { get; set; }
			void Use();
		}

		public interface IReadable : IUsable {
			bool read { get; set; }
			void Read();
		}

		public interface IEquippable : IUsable {
			bool worn { get; set; }
			void Equip();  void Stow();
		}

		public interface IWieldable : IEquippable {
			void Attack();
		}

		public static class Items {
			public static readonly Dictionary<type,Item[]> items;
			static Dictionary<type,Item[]> _items;

			static Items () {
				_items = new Dictionary<type,Item[]>();
				var temp = Object.FindObjectsOfType<Item>() as Item[];
				_items[typeof (Item)] = temp;
				var dict = new Dictionary<type,Item[]>() {
					{typeof (Item), temp}};
				foreach (var elem in dict) {
					if (elem.Key!=typeof(Item))
						dict[elem.Key] = GetItemsOfType(elem.Key);
				} items=dict;
			}

			public static Item GetItemOfType<T>() where T : Item {
				Item[] temp = new Item[0];
				if (_items.TryGetValue(typeof (T),out temp)
				&& temp!=null && temp.Length>0 && temp[0]) return temp[0];
				else return default(Item);
			}

			static Item[] GetItemsOfType(type T) {
				List<Item> temp = new List<Item>();
				if (T==typeof (Item) && _items.ContainsKey(T))
					return _items[typeof (Item)];
				if (T.IsSubclassOf(typeof (Item))
				&& _items.ContainsKey(T)) return _items[T];
				foreach (var elem in _items[typeof (Item)])
					if (elem.GetType()==T) temp.Add(elem);
				return temp.ToArray();
			}

			public static Item[] GetItemsOfType<T>() where T : Item {
				return GetItemsOfType(typeof (T));
			}
		}
	}

	namespace UserInterface {
		[Flags] public enum UIStates : byte { Pack, Note, Map, Log };
		public static class Manager {
			public static UIStates UIState = UIStates.Pack|UIStates.Map;
		}
	}
}











