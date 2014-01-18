package main.kazgarsrevenge.ui.listeners;

import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.SwingUtilities;

import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;
import main.kazgarsrevenge.ui.panels.MainPanel;

public class SelectRoomListener extends MouseAdapter {
	
	// MainPanel to act on
	private final MainPanel mp;
	
	// The room to select
	private final DefaultRoom room;
	
	public SelectRoomListener(MainPanel mp, DefaultRoom room) {
		this.mp = mp;
		this.room = room;
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		new Thread() {
			@Override
			public void run() {
				mp.setSelectedRoom(room);
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
