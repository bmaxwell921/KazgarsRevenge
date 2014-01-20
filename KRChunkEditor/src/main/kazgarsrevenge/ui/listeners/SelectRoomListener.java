package main.kazgarsrevenge.ui.listeners;

import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.SwingUtilities;

import main.kazgarsrevenge.ui.panels.KREditorPanel;

public class SelectRoomListener extends MouseAdapter {
	
	// MainPanel to act on
	private final KREditorPanel editorPanel;
	
	// The roomName to select
	private final String roomName;
	
	public SelectRoomListener(KREditorPanel editorPanel, String roomName) {
		this.editorPanel = editorPanel;
		this.roomName = roomName;
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		new Thread() {
			@Override
			public void run() {
				editorPanel.select(roomName);
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
