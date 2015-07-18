/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-01 * Health Metrics */

using UnityEngine;
using F = System.FlagsAttribute;

namespace PathwaysEngine.Statistics {

	[F] public enum Severity : byte {
		None 		= 0x00, Mild 	= 0x01,
		Moderate 	= 0x02, Severe 	= 0x03 };

	[F] public enum Faculties : byte {
		Thinking		= 0x00, Circulating	= 0x04,
		Breathing		= 0x08, Seeing		= 0x0C,
		Working			= 0x10, Walking		= 0x14,
		Jumping			= 0x18, All 		= 0x1C };

	[F] public enum Condition : byte {
		Dead		= 0x00,	Wounded		= 0x04,
		Shocked		= 0x08, Posioned	= 0x0C,
		Psychotic	= 0x10, Stunned		= 0x14,
		Injured 	= 0x18, Healthy 	= 0x1C };

	[F] public enum Diagnosis : byte {
		None 			= 0x00, Unknown			= 0x04,
		Polytrauma		= 0x08, Paralysis		= 0x0C,
		Necrosis		= 0x10,	Infection		= 0x14,
		Fracture		= 0x18,	Ligamentitis	= 0x1C,
		Radiation		= 0x20, Enterotoxia		= 0x24,
		Hypovolemia		= 0x28,	Hemorrhage		= 0x2C,
		Frostbite		= 0x30, Thermosis		= 0x34,
		Hypothermia		= 0x38, Hyperthermia	= 0x3C,
		Hypohydratia	= 0x40, Inanition		= 0x44,
		Psychosis		= 0x48,	Depression 		= 0x4C };

	public enum Prognosis : byte {
		None 		= 0x00,	Unknown		= 0x04,
		Fatal		= 0x08, Mortal		= 0x0C,
		Grievous	= 0x10, Critical 	= 0x14,
		Survivable	= 0x18, Livable		= 0x1C };

	public interface ILiving {
		Faculties faculties { get; set; }
		Condition condition { get; set; }
		Diagnosis diagnosis { get; set; }
		Prognosis prognosis { get; set; }
	}

	public interface IDamageable : ILiving {
		Severity[] severityFaculties { get; set; }
		Severity[] severityCondition { get; set; }
		Severity[] severityDiagnosis { get; set; }
		Severity[] severityPrognosis { get; set; }
		void ApplyDamage(float damage);
		void AddCondition(Condition cond);
		void AddCondition(Condition cond, Severity svrt);
	}
}
