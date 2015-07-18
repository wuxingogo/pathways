/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 18:13 * Import Defaults */

using UnityEngine;
using UnityEditor;
using System;

public class ImportDefaults : AssetPostprocessor {
	public void OnPreprocessModel() {
		ModelImporter importer = assetImporter as ModelImporter;
		//importer.generateAnimations = ModelImporterGenerateAnimations.None;
		//importer.importAnimation = false;
		importer.importMaterials = false;
		importer.normalImportMode = ModelImporterTangentSpaceMode.Calculate;
	}
}
