/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-17 * Pathways */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using type=System.Type;
using term=PathwaysEngine.UserInterface;
using Flags=System.FlagsAttribute;

namespace PathwaysEngine {
	public enum GameStates : byte { None, Game, Term, Menu };

	public static class Pathways {
		public static GameStates GameState = GameStates.Game;
		public static Camera mainCamera;
		public static Player player;
		public static term::Controls controls;
		public static term::Terminal terminal;
		public static readonly Dictionary<string,TextAsset> encounters;

		static Pathways () {
			player = Object.FindObjectOfType<Player>();
			terminal = Object.FindObjectOfType<term::Terminal>();
			mainCamera = Camera.main;
			LoadYAML();
		}

		public static string GetEncounterYAML(term::Encounter e) {
			return "thing";
		}

		public static string GetTextYAML(string key) {
			// get the mapping thing and stuff, e.nameMessage
			return "<i>hai</i> that should be italic, this strong <b>asdf </b>";
		}

		static void LoadYAML() {
			// parse
			// compose
			// construct
		}
	}

	namespace Inventory {
		public enum ItemStates : byte { Unused, Tarnished, Damaged, Broken }

		public interface IItem {
			bool seen { get; set; }
			bool held { get; set; }
			string name { get; set; }
			string desc { get; set; }
			void Find();
			void View();
			void Take();
			void Drop();
		}

		public interface IStack : IItem {
			uint count { get; set; }
			void Stack();
			ItemStack Split(uint n);
		}

		public interface IGainful : IItem {
			bool sold { get; set; }
			int cost { get; set; }
			void Buy();
			void Sell();
		}

		public interface IStorage : IItem, ICollection<Item> {
			int capacity { get; set; }
			List<Item> items { get; set; }
			void Insert();
			void Remove();
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
			void Equip();
			void Stow();
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











