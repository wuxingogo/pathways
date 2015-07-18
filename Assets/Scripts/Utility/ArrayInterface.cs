using System;

namespace AssemblyCSharp {
	public interface ArrayInterface {
		// Insert an element at a given index
	T[] InsertAt<T>( T[] array, T value, int index );
 
	// Insert an array of elements at a given index
	T[] InsertAt<T>( T[] array, T[] value, int index );
 
	// Insert an element at the first index
	T[] Push<T>( T[] array, T value );
 
	// Insert an element at the last index
	T[] PushLast<T>( T[] array, T value );
 
	// Remove all elements between start and end indexes
	T[] RemoveRange<T>( T[] array, int start, int end );
 
	// Remove an element at a given index
	T[] RemoveAt<T>( T[] array, int index );
 
	// Remove all elements from start to start+count indexes
	T[] RemoveAt<T>( T[] array, int start, int count );
 
	// Remove first element
	T[] Pop<T>( T[] array );
 
	// Remove count elements at the beginning
	T[] Pop<T>( T[] array, int count );
 
	// Remove last element
	T[] PopLast<T>( T[] array );
 
	// Remove count elements at the end
	T[] PopLast<T>( T[] array, int count );
 
	// Find and remove an element
	T[] Remove<T>( T[] array, T value );
 
	// Find and remove all occurrences of the element
	T[] RemoveAll<T>( T[] array, T value );
 
	// Move an element inside the array, from the index indice to the index indice+decalage
	// move count elements.
	T[] Shift<T>( T[] array, int indice, int count, int decalage );
 
	// Move one element to the right
	T[] Shr<T>( T[] array, int indice );
 
	// Move one element to the left
	T[] Shl<T>( T[] array, int indice );
 
	// Concats all the array in parameters.
	T[] Concat<T>( params T[][] arrays );
 
	// Return a subarray of array within the specified bounds.
	T[] SubArray<T>( T[] source, int start, int end );
 
	// Change randomly the order of the array.
	T[] Shuffle<T>( T[] array );
 
	// Change randomly the order of a part of the array.
	T[] Shuffle<T>( T[] array, int start, int end );
 
	// Insert count elements randomly all over the array.
	T[] Sow<T>( T[] array, T value, int count );
 
	// Insert count elements randomly between the specified bounds.
	T[] Sow<T>( T[] array, T value, int count, int lowerBound, int upperBound, bool includeBounds );
 
	// Create an array of size count with every element == value.
	T[] CreateRepeat<T>( T value, int count );
 
	// Create an array of random integer and of size count. The numbers are between min and max.
	int[] CreateRandom( int count, int min, int max );
 
	T[] Create<T>( int count, Func<T> constructor );
 
	T[] Create<T>( int count, Func<int, T> constructor );
 
	// Apply a function upon all members of the array. The function take a T in input and return a T
	T[] Update<T>( T[] array, Func<T, T> selectFunc );
 
	// Apply a function upon all members of the array between start and end included. The function take a T in input and return a T
	T[] Update<T>( T[] array, Func<T, T> selectFunc, int start, int end );
 
	// Apply a function upon all members of the array. The function take a T and the index in input and return a T
	T[] Update<T>( T[] array, Func<T, int, T> selectFunc );
 
	// Apply a function upon all members of the array between start and end included. The function take a T and the index in input and return a T
	T[] Update<T>( T[] array, Func<T, int, T> selectFunc, int start, int end );
	}
}