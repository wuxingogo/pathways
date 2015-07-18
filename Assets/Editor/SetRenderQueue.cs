/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Render Queue */

using UnityEngine;

public class SetRenderQueue : MonoBehaviour {
	[SerializeField] protected int[] m_queues = new int[] { 3000 };

	protected void Awake() {
		Material[] materials = GetComponent<Renderer>().materials;
		for (int i=0;i<materials.Length&&i<m_queues.Length;++i)
			materials[i].renderQueue = m_queues[i];
	}
}