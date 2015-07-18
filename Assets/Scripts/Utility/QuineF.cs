using UnityEngine;
using System.Collections;

public class QuineF : MonoBehaviour {
  public static void main( string[] Arguments ) {
    string q = "'";
    string[] l = {
    " ",
	"public class Quinef : MonoBehaviour {",
    "  public static void main ( String[] args ) {",
    "    char q = 34;",
    "    String[] l = {",
    "    };",
    "    for ( int i = 0; i < 6; i++ ) print ( l[i] );",
    "    for ( int i = 0; i < l.length; i++ ) print ( l[6] + q + l[i] + q + ',' );",
    "    for ( int i = 7; i < l.length; i++ ) print ( l[i] );",
    "  }",
    "}",
    };
    for ( int i = 0; i < 6; i++ ) print( l[i] );
    for ( int i = 0; i < l.Length; i++ ) print( l[6] + q + l[i] + q + ',' );
    for ( int i = 7; i < l.Length; i++ ) print( l[i] );
  }
}