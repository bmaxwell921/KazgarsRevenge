package main.kazgarsrevenge.util;

import java.awt.Point;
import java.awt.Rectangle;
import java.awt.geom.AffineTransform;
import java.awt.image.AffineTransformOp;
import java.awt.image.BufferedImage;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

public class Rotater {
	
	public static AffineTransform getRotationTransform(Rectangle src, Rotation rot) {
		int w = (int) src.getWidth();
        int h = (int) src.getHeight();

        AffineTransform t = new AffineTransform();
        double ang = rot.getRadians();
        t.setToRotation(ang, w / 2d, h / 2d);

        // source image rectangle
        Point[] points = {
            new Point(0, 0),
            new Point(w, 0),
            new Point(w, h),
            new Point(0, h)
        };

        // transform to destination rectangle
        t.transform(points, 0, points, 0, 4);

        // get destination rectangle bounding box
        Point min = new Point(points[0]);
        Point max = new Point(points[0]);
        for (int i = 1, n = points.length; i < n; i ++) {
            Point p = points[i];
            double pX = p.getX(), pY = p.getY();

            // update min/max x
            if (pX < min.getX()) min.setLocation(pX, min.getY());
            if (pX > max.getX()) max.setLocation(pX, max.getY());

            // update min/max y
            if (pY < min.getY()) min.setLocation(min.getX(), pY);
            if (pY > max.getY()) max.setLocation(max.getX(), pY);
        }

        // determine new width, height
        w = (int) (max.getX() - min.getX());
        h = (int) (max.getY() - min.getY());

        // determine required translation
        double tx = min.getX();
        double ty = min.getY();

        // append required translation
        AffineTransform translation = new AffineTransform();
        translation.translate(-tx, -ty);
        t.preConcatenate(translation);

//        return new AffineTransformOp(t, null);
        return t;
	}
	
	public static BufferedImage otherRotateImage(BufferedImage src, Rotation rot) {
		int w = src.getWidth();
        int h = src.getHeight();

        AffineTransform t = new AffineTransform();
        double ang = rot.getRadians();
        t.setToRotation(ang, w / 2d, h / 2d);

        // source image rectangle
        Point[] points = {
            new Point(0, 0),
            new Point(w, 0),
            new Point(w, h),
            new Point(0, h)
        };

        // transform to destination rectangle
        t.transform(points, 0, points, 0, 4);

        // get destination rectangle bounding box
        Point min = new Point(points[0]);
        Point max = new Point(points[0]);
        for (int i = 1, n = points.length; i < n; i ++) {
            Point p = points[i];
            double pX = p.getX(), pY = p.getY();

            // update min/max x
            if (pX < min.getX()) min.setLocation(pX, min.getY());
            if (pX > max.getX()) max.setLocation(pX, max.getY());

            // update min/max y
            if (pY < min.getY()) min.setLocation(min.getX(), pY);
            if (pY > max.getY()) max.setLocation(max.getX(), pY);
        }

        // determine new width, height
        w = (int) (max.getX() - min.getX());
        h = (int) (max.getY() - min.getY());

        // determine required translation
        double tx = min.getX();
        double ty = min.getY();

        // append required translation
        AffineTransform translation = new AffineTransform();
        translation.translate(-tx, -ty);
        t.preConcatenate(translation);

        AffineTransformOp op = new AffineTransformOp(t, null);
        BufferedImage dst = new BufferedImage(w, h, src.getType());
        return op.filter(src, dst);
	}
	
	public static AffineTransform getSimpleRotation(Rectangle drawLoc, Rotation rot) {
		AffineTransform tx = new AffineTransform();

		// last, width = height and height = width
		tx.translate(drawLoc.getHeight() / 2,drawLoc.getWidth() / 2);
		tx.rotate(Math.PI / 2);
		// first - center image at the origin so rotate works OK
		tx.translate(-drawLoc.getWidth() / 2,-drawLoc.getHeight() / 2);
		return tx;
	}
}
