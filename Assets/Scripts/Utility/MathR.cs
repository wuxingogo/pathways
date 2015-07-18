using UnityEngine;
 
public static class MathR {
	
	public const float EulerF = 2.718281828459045F;
	public const double EulerD = 2.7182818284590452353602874713526D;
	public const decimal EulerM = 2.7182818284590452353602874713526624977572470936999595749669676277M;
	public const float ArchimedesF = 3.141592653589793F;
	public const double ArchimedesD = 3.1415926535897932384626433832795D;
	public const decimal ArchimedesM = 3.141592653589793238462643383279502884197169399375105820974944592M;
	public const float FibonacciF = 1.618033988749894F;
	public const double FibonacciD = 1.6180339887498948482045868343656D;
	public const decimal FibonacciM = 1.618033988749894848204586834365638117720309179805762862135448622M;
	public const float PellF = 2.414213562373095F;
	public const double PellD = 2.4142135623730950488016887242096D;
	public const decimal PellM = 2.41421356237309504880168872420969807856967187537694807317667973799M;
	public const float TheodorusF = 1.732050807568877F;
	public const double TheodorusD = 1.7320508075688772935274463415058D;
	public const decimal TheodorusM = 1.732055080756887729352744634150587236694280525381038062805580M;
	public const float RamanuajanF = 2.236067977499789F;
	public const double RamanuajanD = 2.2360679774997896964091736687312D;
	public const decimal RamanuajanM = 2.23606797749978969640917366873127623544061835961152572427089M;
	public const float PadovanF = 1.324717957244746F;
	public const double PadovanD = 1.3247179572447460259609088541234D;
	public const float GaussF = 0.8346268416740731F;
	public const double GaussD = 0.83462684167407318628142973279904D;
	public const decimal GaussM = 0.834626841674073186281429732799046808994M;
	public const float FeigenbaumF = 4.669201609102990F;
	public const double FeigenbaumD = 4.6692016091029906718532038204662D;
	public const decimal FeigenbaumM = 4.669201609102990671853203820466201617258185577475768632745651343M;

	public const float EinsteinF = 2.99792458F;							// e+9 m/s
	public const float NewtonF = 6.6738480F;							// e-11 m^3/(kg*s^2)
	public const float PlanckF = 6.6260695729F; 						//	e-34 (kg*m^2)/s^3
	public const float FaradF = 1.60217656535F;						//	e-19 A*s
	public const float DaltonF = 1.66053892173F;						// e-27 kg
	public const float AvogadroF = 6.0221412927F;					// e+23 mol^-1
	public const float BoltzmannF = 8.314462175F;					// e+0 (kg*m^2)/(mol*k)
		
	public static float Hermite (float start, float end, float tValue) { return Mathf.Lerp(start, end, tValue * tValue * (3.0f - 2.0f * tValue)); }
	
    public static float Sinerp (float start, float end, float tValue) { return Mathf.Lerp(start, end, Mathf.Sin(tValue * Mathf.PI * 0.5f)); }
	
    public static float Coserp (float start, float end, float tValue) { return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(tValue * Mathf.PI * 0.5f)); }
	
    public static float Berp (float start, float end, float tValue) {
        tValue = Mathf.Clamp01(tValue);
        tValue = (Mathf.Sin(tValue * Mathf.PI * (0.2f + 2.5f * tValue * tValue * tValue)) * Mathf.Pow(1f - tValue, 2.2f) + tValue) * (1f + (1.2f * (1f - tValue)));
        return start + (end - start) * tValue;
    }
	
    public static float SmoothStep (float x, float min, float max) {
        x = Mathf.Clamp (x, min, max);
        float v1 = (x-min)/(max-min);
        float v2 = (x-min)/(max-min);
        return -2*v1 * v1 *v1 + 3*v2 * v2;
    }
 
    public static float Lerp (float start, float end, float tValue) { return ((1.0f - tValue) * start) + (tValue * end); }
 
    public static Vector3 NearestPoint (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
        Vector3 lineDirection = Vector3.Normalize(lineEnd-lineStart);
        float closestPoint = Vector3.Dot((point-lineStart),lineDirection)/Vector3.Dot(lineDirection,lineDirection);
        return lineStart+(closestPoint*lineDirection);
    }
 
    public static Vector3 NearestPointStrict (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
        Vector3 fullDirection = lineEnd-lineStart;
        Vector3 lineDirection = Vector3.Normalize (fullDirection);
        float closestPoint = Vector3.Dot((point-lineStart),lineDirection)/Vector3.Dot(lineDirection,lineDirection);
        return lineStart+(Mathf.Clamp(closestPoint,0.0f,Vector3.Magnitude(fullDirection))*lineDirection);
    }
	
    public static float Bounce(float x) { return Mathf.Abs(Mathf.Sin(6.28f*(x+1f)*(x+1f)) * (1f-x)); }
	
    public static bool Approx (float val, float about, float range) { return ( ( Mathf.Abs(val - about) < range) ); }
	
	public static bool Approx(Vector3 val, Vector3 about, float range) { return ( (val - about).sqrMagnitude < range*range); }

    public static float Clerp (float start , float end, float tValue) {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min)/2.0f);//half the distance between min and max
        float retval = 0.0f;
        float diff = 0.0f;
        if ((end - start) < -half) {
            diff = ((max - start)+end)*tValue;
			retval =  start+diff;
            } else if ((end - start) > half) {
                diff = -((max - end)+start)*tValue;
                retval =  start+diff;
            } else retval =  start+(end-start)*tValue;
		return retval;
	}
}