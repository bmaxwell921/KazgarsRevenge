package main.kazgarsrevenge.data;

/**
 * Representation of rotations by 90 degrees.
 * Values should be used like you do in math....
 * 
 * On the unit circle
 * 	ZERO = (1, 0)
 * 	NINETY = (0, 1)
 * 	ONE_EIGHTY = (-1, 0)
 * 	TWO_FORTY = (0, -1)
 * @author Brandon
 *
 */
public enum Rotation {

	ZERO, NINETY, ONE_EIGHTY, TWO_FORTY;
	
	public static double toDegrees(Rotation rot) {
		if (rot == Rotation.ZERO) {
			return 0;
		} else if (rot == Rotation.NINETY) {
			return 90;
		} else if (rot == Rotation.ONE_EIGHTY) {
			return 180;
		} else {
			return 240;
		}
	}
	
	public static double toRadians(Rotation rot) {
		return Math.toRadians(Rotation.toDegrees(rot));
	}
}
