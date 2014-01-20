package main.kazgarsrevenge.ui.listeners;

import java.awt.Frame;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.JOptionPane;
import javax.swing.SwingUtilities;

import main.kazgarsrevenge.ui.panels.KREditorPanel;

public class NewClickListener extends MouseAdapter {

	// The parent frame, used for dialogs
	private final Frame parent;
	
	// MainPanel to act on
	private final KREditorPanel editorPanel;
	
	public NewClickListener(Frame parent, KREditorPanel editorPanel) {
		this.parent = parent;
		this.editorPanel = editorPanel;
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		
		new Thread() {
			@Override 
			public void run() {
				int result = JOptionPane.showConfirmDialog(parent, "Creating a new item will clear unsaved content, continue?", "", JOptionPane.YES_NO_OPTION);
				if (result == JOptionPane.YES_OPTION) {
					editorPanel.newEditable();
				}
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
