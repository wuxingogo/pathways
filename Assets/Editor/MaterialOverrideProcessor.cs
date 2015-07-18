/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Material Override */

using UnityEngine;
using UnityEditor;

class MaterialOverrideProcessor : AssetPostprocessor {
	string materialsPath = "Trees/Materials";

	Material OnAssignMaterialModel(Material m, Renderer r) {
#if DEBUG
		Debug.Log(m.name);
#endif
    	string materialPath = "Assets/"+materialsPath+"/"+m.name+".mat";
    	return AssetDatabase.LoadAssetAtPath<Material>(materialPath);
	}
}

