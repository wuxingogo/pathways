#pragma strict
function Update () {
var mainCameraFOV: float = Camera.main.fieldOfView;
GetComponent(Camera).fieldOfView = mainCameraFOV;
}