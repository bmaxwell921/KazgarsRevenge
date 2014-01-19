package main.kazgarsrevenge.data;


/**
 * Representation of rotations by 90 degrees.
 * Values should be used like you do in math....
 * 
 * On the unit circle
 * 	ZERO = (1, 0)
 * 	NINETY = (0, 1)
 * 	ONE_EIGHTY = (-1, 0)
 * 	TWO_SEVENTY = (0, -1)
 * @author Brandon
 *
 */
public enum Rotation {
	
	ZERO(0), NINETY(90), ONE_EIGHTY(180), TWO_SEVENTY(270);
	
	private static final int ROTATE_AMT = 90;
	private static final int FULL_CIRCLE = 360;
	
	private int degrees;
	
	private Rotation(int degrees) {
		this.degrees = degrees;
	}
	
	public int getDegrees() {
		return degrees;
	}
	
	public double getRadians() {
		return Math.toRadians(this.getDegrees());
	}
	
	public static Rotation rotateCounter(Rotation first) {
		int newDegrees = (first.getDegrees() + ROTATE_AMT) % FULL_CIRCLE;
		for (Rotation rot : Rotation.values()) {
			if (rot.getDegrees() == newDegrees) {
				return rot;
			}
		}
		return null;
	}
}
