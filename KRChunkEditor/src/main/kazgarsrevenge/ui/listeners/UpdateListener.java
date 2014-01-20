package main.kazgarsrevenge.ui.listeners;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.KeyEvent;

import javax.swing.SwingUtilities;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.ui.panels.KREditorPanel;
import main.kazgarsrevenge.util.ImageLoader;
import main.kazgarsrevenge.util.managers.InputManager;

/**
 * Used in conjunction with the InputManager to allow multiple key presses
 * @author Brandon
 *
 */
public class UpdateListener implements ActionListener {

	private static final int MOVE_AMT = ImageLoader.IMAGE_SIZE;
	
	// The main panel to update
	private final KREditorPanel editorPanel;
	
	public UpdateListener(KREditorPanel editorPanel) {
		this.editorPanel = editorPanel;
	}

	@Override
	public void actionPerformed(ActionEvent e) {
		handleMovement();
		handlePlacement();
		handleRotation();
	}
	
	// Handles checking and moving as needed
	private void handleMovement() {
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
		
		if (move.equals(new Location())) {
			// Don't bother if it's no movement
			return;
		}
		
		// Do it!
		new Thread() {
			@Override
			public void run() {
				editorPanel.moveSelection(move);
				SwingUtilities.invokeLater(new Runnable() {
					@Override
					public void run() {
						editorPanel.repaint();
					}
				});
			}
		}.start();
	}
	
	// Handles placing a room
	private void handlePlacement() {
		InputManager im = InputManager.getInstance();
		if (im.isPressed(KeyEvent.VK_ENTER)) {
			// consume!
			im.updateValue(KeyEvent.VK_ENTER, false);
			new Thread() {
				@Override
				public void run() {
					editorPanel.placeSelection();
					SwingUtilities.invokeLater(new Runnable() {
						@Override
						public void run() {
							editorPanel.repaint();
						}
					});
				}
			}.start();
		}
	}
	
	private void handleRotation() {
		InputManager im = InputManager.getInstance();
		if (im.isPressed(KeyEvent.VK_R)) {
			// consume!
			im.updateValue(KeyEvent.VK_R, false);
			new Thread() {
				@Override
				public void run() {
					editorPanel.rotateSelection();
					SwingUtilities.invokeLater(new Runnable() {
						@Override
						public void run() {
							editorPanel.repaint();
						}
					});
				}
			}.start();
		}
	}

}
