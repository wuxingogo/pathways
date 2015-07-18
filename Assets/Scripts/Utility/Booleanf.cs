using UnityEngine;
using System;
using System.Collections;

public class Set : Hashtable {
	/*//				// Based on outdated hashtable code, will fix later
	public Set ( ) : base() { }
	public Set (Set otherSet) : base(otherSet) { }
	public Set (int capacity) : base(capacity) { }
	public Set (Set otherSet, float loadFactor) : base(otherSet, loadFactor) { }
	public Set (IHashCodeProvider iHashCodeProvider, IComparer iComparer) : base(iHashCodeProvider, iComparer) { }
	public Set (int capacity, float loadFactor) : base(capacity, loadFactor) { }
	public Set (Set otherSet, IHashCodeProvider iHashCodeProvider, IComparer iComparer) : base(otherSet, iHashCodeProvider, iComparer) { }
	public Set(int capacity, IHashCodeProvider iHashCodeProvider, IComparer iComparer) : base(capacity, iHashCodeProvider, iComparer) { }
	public Set(Set otherSet, float loadFactor, IHashCodeProvider iHashCodeProvider, IComparer iComparer) : base(otherSet, loadFactor, iHashCodeProvider, iComparer) { }
	public Set(int capacity, float loadFactor, IHashCodeProvider iHashCodeProvider, IComparer iComparer) : base(capacity, loadFactor, iHashCodeProvider, iComparer) { }
	
	public void Add (System.Object entry) { base.Add(entry, null); }
	
	private static Set Generate( Set iterSet, Set containsSet, Set startingSet, bool containment) {
		// Returned set either starts out empty or as copy of the starting set.
		Set returnSet = startingSet == null ? new Set(iterSet.hcp, iterSet.comparer) : startingSet;
 		foreach(object key in iterSet.Keys) {
			// (!containment && !containSet.ContainsKey) ||
			//  (containment &&  containSet.ContainsKey)
			if (!(containment ^ containsSet.ContainsKey(key))) returnSet.Add(key);
		} return returnSet;
	}
	
	public static Set operator | (Set set1, Set set2) {
		// Copy set1, then add items from set2 not already in set 1.
		Set unionSet = new Set(set1, set1.hcp, set1.comparer);
		return Generate(set2, unionSet, unionSet, false);
	}
 
	public Set Union(Set otherSet) { return this | otherSet; }
 
	public static Set operator & (Set set1, Set set2) {
		return Generate( set1.Count > set2.Count ? set2 : set1, set1.Count > set2.Count ? set1 : set2, null, true);
	}
 
	public Set Intersection (Set otherSet)  { return this & otherSet; }
	// Find items in set1 that aren't in set2. Then find
	// items in set2 that aren't in set1. Return combination of those two subresults.
	public static Set operator ^ (Set set1, Set set2)  { return Generate(set2, set1, Generate(set1, set2, null, false), false); }

	public Set ExclusiveOr(Set otherSet) { return this ^ otherSet; }
 
	public static Set operator - (Set set1, Set set2)  { return Generate(set1, set2, null, false); }
 
	public Set Difference(Set otherSet) { return this - otherSet; }
	//*/
}