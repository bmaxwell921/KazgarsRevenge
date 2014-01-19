package main.kazgarsrevenge.ui.listeners;

import java.awt.Frame;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.JOptionPane;

import main.kazgarsrevenge.ui.panels.MainPanel;

public class NewChunkClickListener extends MouseAdapter {

	// The parent frame, used for dialogs
	private final Frame parent;
	
	// MainPanel to act on
	private final MainPanel mp;
	
	public NewChunkClickListener(Frame parent, MainPanel mp) {
		this.parent = parent;
		this.mp = mp;
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		
		new Thread() {
			@Override 
			public void run() {
				int result = JOptionPane.showConfirmDialog(parent, "Creating a new chunk will clear unsaved content, continue?", "", JOptionPane.YES_NO_OPTION);
				if (result == JOptionPane.YES_OPTION) {
					mp.newChunk();
				}
			}
		}.start();
	}

}
