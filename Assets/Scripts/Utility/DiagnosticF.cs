using System;
using System.Reflection;
 
public static class DiagnosticF {
    public static bool IsOverride(this MethodInfo m) {
		return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
	}
}