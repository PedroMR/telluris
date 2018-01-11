namespace com.pedromr.telluris.model
{
	public class Util
	{
		public static int mod(int k, int n) { return ((k %= n) < 0) ? k + n : k; }

		public static float Clamp(float v, float min, float max)
		{
			if (min > v) return min;
			else if (max < v) return max;
			return v;
		}
	}
}