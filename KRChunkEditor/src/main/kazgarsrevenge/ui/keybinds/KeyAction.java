package main.kazgarsrevenge.ui.keybinds;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

import javax.swing.AbstractAction;

import main.kazgarsrevenge.util.managers.InputManager;

public class KeyAction extends AbstractAction implements ActionListener {

	// Whether this is for keyPresses or releases
	private boolean pressed;
	
	// keyCode for this moveAction
	private int keyCode;
	
	public KeyAction(int keyCode, boolean pressed) {
		this.pressed = pressed;
		this.keyCode = keyCode;
	}
	
	public void actionPerformed(ActionEvent e) {
		InputManager.getInstance().updateValue(keyCode, pressed);
	}
}
