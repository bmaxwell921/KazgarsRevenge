package main.kazgarsrevenge.ui.listeners;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.KeyEvent;

import javax.swing.SwingUtilities;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.ui.panels.MainPanel;
import main.kazgarsrevenge.util.InputManager;

/**
 * Used in conjunction with the InputManager to allow multiple key presses
 * @author Brandon
 *
 */
public class UpdateListener implements ActionListener {

	private static final int MOVE_AMT = 2;
	
	// The main panel to update
	private final MainPanel mp;
	
	public UpdateListener(MainPanel mp) {
		this.mp = mp;
	}

	@Override
	public void actionPerformed(ActionEvent e) {
		final Location move = new Location();
		InputManager im = InputManager.getInstance();
		if (im.isPressed(KeyEvent.VK_LEFT)) {
			move.setX(-MOVE_AMT);
		} else if (im.isPressed(KeyEvent.VK_RIGHT)) {
			move.setX(MOVE_AMT);
		}
		
		if (im.isPressed(KeyEvent.VK_UP)) {
			move.setY(-MOVE_AMT);
		} else if (im.isPressed(KeyEvent.VK_DOWN)) {
			move.setY(MOVE_AMT);
		}
		
		// Do it!
		new Thread() {
			@Override
			public void run() {
				mp.moveSelection(move);
				SwingUtilities.invokeLater(new Runnable() {
					@Override
					public void run() {
						mp.repaint();
					}
				});
			}
		}.start();
	}

}
