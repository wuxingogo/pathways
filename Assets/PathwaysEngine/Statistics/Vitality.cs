/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Vitality */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using F = System.FlagsAttribute;

namespace PathwaysEngine.Statistics {
	public class Vitality : MonoBehaviour, IDamageable {
		public enum DamageStates : byte { Heal, Hurt, Crit, Dead};
		public int weightCurrent, weightCritical;
		public float healthCurrent, healthCritical, RUL, TTF;
		public float bodyMass, bodyTemp, bodyWater;
		public double staminaCurrent, staminaCritical;
		public Faculties faculties { get; set; }
		public Condition condition { get; set; }
		public Diagnosis diagnosis { get; set; }
		public Prognosis prognosis { get; set; }
		public Severity[] severityFaculties { get; set; }
		public Severity[] severityCondition { get; set; }
		public Severity[] severityDiagnosis { get; set; }
		public Severity[] severityPrognosis { get; set; }
		public AudioSource au;
		public AudioClip[] soundsHurt, soundsHeal;

		public Vitality() {
			bodyMass 			= 80;
			weightCurrent 		= 0;					weightCritical		= 40;
			healthCurrent		= 128.0f;				healthCritical		= 128.0f;
			staminaCurrent		= 64.0;					staminaCritical		= 64.0;
			condition 			= Condition.Healthy;	faculties 			= Faculties.All;
			diagnosis 			= Diagnosis.None;		prognosis 			= Prognosis.None;
			severityFaculties 	= new Severity[8];		severityCondition 	= new Severity[8];
			severityDiagnosis	= new Severity[20];		severityPrognosis 	= new Severity[8];
		}

		public void Awake() { au = (GetComponent<AudioSource>()) ?? (gameObject.AddComponent<AudioSource>()); }

		public void ApplyDamage(float damage) {
			if (damage==0) return;
			DamageStates state = (damage>0) ? (DamageStates.Hurt):(DamageStates.Heal);
			byte ind = 0;
			healthCurrent -= damage;
			state = (healthCurrent>0) ? (state):(DamageStates.Dead);
			switch (state) {
				case DamageStates.Heal :
					ind = (byte) Random.Range(0,soundsHeal.Length); break;
				case DamageStates.Hurt :
					ind = (byte) Random.Range(0,soundsHurt.Length>>1); break;
				case DamageStates.Crit :
					ind = (byte) Random.Range(soundsHurt.Length>>1,soundsHurt.Length); break;
				case DamageStates.Dead :
					ind = (byte) soundsHurt.Length; break;
			} au.PlayOneShot(soundsHurt[(int)ind], (Mathf.Abs((int) damage)>>2)/au.volume);
		}

		public void AddCondition(Condition cond) { severityFaculties[((byte) cond)/4]++; }

		public void AddCondition(Condition cond, Severity srvt) { severityFaculties[((byte) cond)/4]+=(byte) srvt; }
	}
}
