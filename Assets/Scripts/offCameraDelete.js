#pragma strict
var updateInterval : float = 5;

function OnBecameInvisible () {
yield WaitForSeconds(updateInterval);
if (GetComponent(Renderer).isVisible == false){
DestroyObject (gameObject);
	}
}
